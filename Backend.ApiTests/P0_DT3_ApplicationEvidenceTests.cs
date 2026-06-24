using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT3_ApplicationEvidenceTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string OtherStudentEmail = "student.tkdh01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string TestPrefix = "NUnit P0-DT3";

    [OneTimeSetUp]
    public void ValidateP0Dt3Environment()
    {
        _ = GetConnectionString();
        _ = GetP0Dt3TestPassword();
        _ = GetStorageRoot();
    }

    [Test]
    public async Task Anonymous_UploadDownloadDelete_ShouldReturn401()
    {
        using var client = new HttpClient { BaseAddress = BaseUri };
        using var upload = await UploadAsync(client, 1, "abc", [PdfFile("anonymous.pdf")]);
        using var download = await client.GetAsync("api/student/applications/1/attachments/1/download");
        using var delete = await DeleteAsync(client, 1, 1, "abc");

        Assert.Multiple(() =>
        {
            Assert.That(upload.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), "upload");
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), "download");
            Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), "delete");
        });
    }

    [Test]
    public async Task NonStudent_Upload_ShouldReturnForbidden()
    {
        using var teacherClient = await CreateAuthenticatedClientAsync(TeacherEmail);
        using var response = await UploadAsync(teacherClient, 1, "abc", [PdfFile("teacher.pdf")]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_ValidPdf_ShouldPersistSafeMetadataAndHiddenLog()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} valid pdf");
        var file = PdfFile("bang-chung.pdf");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion, [file]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        var upload = await ReadUploadAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(upload.UploadedFiles, Has.Count.EqualTo(1));
            Assert.That(upload.RowVersion, Is.Not.EqualTo(created.RowVersion));
            Assert.That(upload.ActiveFileCount, Is.EqualTo(1));
            Assert.That(upload.TotalSizeBytes, Is.EqualTo(file.Bytes.Length));
            Assert.That(upload.UploadedFiles[0].TenFileGoc, Is.EqualTo("bang-chung.pdf"));
            Assert.That(upload.UploadedFiles[0].ContentType, Is.EqualTo("application/pdf"));
            Assert.That(upload.UploadedFiles[0].KichThuocByte, Is.EqualTo(file.Bytes.Length));
        });

        await using var db = CreateDbContext();
        var attachment = await db.TepDinhKemDonTus.AsNoTracking()
            .SingleAsync(x => x.MaTep == upload.UploadedFiles[0].MaTep);
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == created.MaDonTu && x.HanhDong == ApplicationActions.Update)
            .OrderByDescending(x => x.NgayTao)
            .FirstAsync();

        var detail = await GetApplicationRawAsync(studentClient, created.MaDonTu);
        var lowerDetail = detail.ToLowerInvariant();
        Assert.Multiple(() =>
        {
            Assert.That(attachment.StorageKey, Is.Not.Empty);
            Assert.That(attachment.StorageKey, Does.Not.Contain("bang-chung.pdf"));
            Assert.That(attachment.FileHash, Is.EqualTo(Sha256Hex(file.Bytes)));
            Assert.That(attachment.DaXoa, Is.False);
            Assert.That(File.Exists(StoragePath(attachment.StorageKey)), Is.True);
            Assert.That(log.HienThiChoHocSinh, Is.False);
            Assert.That(log.SnapshotJson, Does.Contain("attachmentAction"));
            Assert.That(lowerDetail, Does.Not.Contain("storagekey"));
            Assert.That(lowerDetail, Does.Not.Contain("filehash"));
            Assert.That(lowerDetail, Does.Not.Contain("tenfileluu"));
        });
    }

    [Test]
    public async Task Upload_ValidImageTypes_ShouldReturnCreated()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} valid images");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            [JpegFile("anh.jpg"), PngFile("scan.png"), WebpFile("web.webp")]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        var upload = await ReadUploadAsync(response);
        Assert.That(upload.UploadedFiles.Select(x => x.ContentType), Is.EquivalentTo(new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        }));
    }

    [Test]
    public async Task Upload_NoFiles_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} no files");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion, []);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_MoreThanFiveFiles_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} too many files");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            Enumerable.Range(1, 6)
                .Select(x => new TestFile($"file-{x}.pdf", "application/pdf", PdfBytes($"file-{x}")))
                .ToList());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_FileLargerThanTenMb_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} large file");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            [new TestFile("large.pdf", "application/pdf", LargePdfBytes())]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [TestCase("bad.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [TestCase("bad.svg", "image/svg+xml")]
    [TestCase("bad.zip", "application/zip")]
    [TestCase("bad.html", "text/html")]
    [TestCase("bad.exe", "application/octet-stream")]
    public async Task Upload_DisallowedTypes_ShouldReturn400(string fileName, string contentType)
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} bad type {fileName}");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            [new TestFile(fileName, contentType, Encoding.UTF8.GetBytes("bad"))]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_SpoofedExtensionOrMagic_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var extension = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} spoof extension");
        var magic = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} spoof magic");

        using var extensionResponse = await UploadAsync(studentClient, extension.MaDonTu, extension.RowVersion,
            [new TestFile("fake.pdf", "image/png", PngBytes())]);
        using var magicResponse = await UploadAsync(studentClient, magic.MaDonTu, magic.RowVersion,
            [new TestFile("fake.pdf", "application/pdf", Encoding.UTF8.GetBytes("not a real pdf"))]);
        var extensionDescription = await DescribeResponseAsync(extensionResponse);
        var magicDescription = await DescribeResponseAsync(magicResponse);

        Assert.Multiple(() =>
        {
            Assert.That(extensionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), extensionDescription);
            Assert.That(magicResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), magicDescription);
        });
    }

    [Test]
    public async Task Upload_DuplicateHashInSameRequest_ShouldReturn409()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} duplicate same");
        var bytes = PdfBytes("same");

        using var response = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            [
                new TestFile("one.pdf", "application/pdf", bytes),
                new TestFile("two.pdf", "application/pdf", bytes)
            ]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_DuplicateExistingHash_ShouldReturn409()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} duplicate existing");
        var bytes = PdfBytes("existing");

        using var first = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion,
            [new TestFile("first.pdf", "application/pdf", bytes)]);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(first));
        var upload = await ReadUploadAsync(first);

        using var duplicate = await UploadAsync(studentClient, created.MaDonTu, upload.RowVersion,
            [new TestFile("second.pdf", "application/pdf", bytes)]);

        Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(duplicate));
    }

    [Test]
    public async Task Upload_InvalidOrStaleRowVersion_ShouldReturn400And409()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} stale rowversion");

        using var bad = await UploadAsync(studentClient, created.MaDonTu, "not-base64", [PdfFile("bad-rowversion.pdf")]);
        using var first = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion, [PdfFile("first-stale.pdf")]);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(first));
        using var stale = await UploadAsync(studentClient, created.MaDonTu, created.RowVersion, [PdfFile("second-stale.pdf")]);
        var badDescription = await DescribeResponseAsync(bad);
        var staleDescription = await DescribeResponseAsync(stale);

        Assert.Multiple(() =>
        {
            Assert.That(bad.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), badDescription);
            Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), staleDescription);
        });
    }

    [Test]
    public async Task Upload_EditableNeedSupplement_ShouldReturnCreated()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} supplement upload");
        await SetStatusAsync(created.MaDonTu, ApplicationStatuses.NeedSupplement);
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await UploadAsync(studentClient, created.MaDonTu, refreshed.RowVersion, [PdfFile("supplement.pdf")]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Upload_NonEditableStatuses_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var submitted = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} submitted upload");
        var cancelled = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} cancelled upload");
        await SubmitApplicationAsync(studentClient, submitted.MaDonTu, submitted.RowVersion);
        await SetStatusAsync(cancelled.MaDonTu, ApplicationStatuses.Cancelled);
        var cancelledFresh = await GetApplicationAsync(studentClient, cancelled.MaDonTu);

        using var submittedResponse = await UploadAsync(studentClient, submitted.MaDonTu, submitted.RowVersion,
            [PdfFile("submitted.pdf")]);
        using var cancelledResponse = await UploadAsync(studentClient, cancelled.MaDonTu, cancelledFresh.RowVersion,
            [PdfFile("cancelled.pdf")]);
        var submittedDescription = await DescribeResponseAsync(submittedResponse);
        var cancelledDescription = await DescribeResponseAsync(cancelledResponse);

        Assert.Multiple(() =>
        {
            Assert.That(submittedResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict).Or.EqualTo(HttpStatusCode.BadRequest), submittedDescription);
            Assert.That(cancelledResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), cancelledDescription);
        });
    }

    [Test]
    public async Task OtherStudent_CannotUploadDownloadOrDelete_ShouldReturn404()
    {
        using var ownerClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var otherClient = await CreateAuthenticatedClientAsync(OtherStudentEmail);
        var created = await CreateDraftAndReadAsync(ownerClient, $"{TestPrefix} ownership");
        var upload = await UploadOneAndReadAsync(ownerClient, created.MaDonTu, created.RowVersion, PdfFile("owner.pdf"));

        using var otherUpload = await UploadAsync(otherClient, created.MaDonTu, created.RowVersion, [PdfFile("other.pdf")]);
        using var otherDownload = await otherClient.GetAsync($"api/student/applications/{created.MaDonTu}/attachments/{upload.Attachment.MaTep}/download");
        using var otherDelete = await DeleteAsync(otherClient, created.MaDonTu, upload.Attachment.MaTep, upload.RowVersion);
        var otherUploadDescription = await DescribeResponseAsync(otherUpload);
        var otherDownloadDescription = await DescribeResponseAsync(otherDownload);
        var otherDeleteDescription = await DescribeResponseAsync(otherDelete);

        Assert.Multiple(() =>
        {
            Assert.That(otherUpload.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), otherUploadDescription);
            Assert.That(otherDownload.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), otherDownloadDescription);
            Assert.That(otherDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), otherDeleteDescription);
        });
    }

    [Test]
    public async Task Download_ShouldReturnBinaryWithSafeHeaders_AndSubmittedStillAllowed()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} download");
        var file = PdfFile("download.pdf");
        var upload = await UploadOneAndReadAsync(studentClient, created.MaDonTu, created.RowVersion, file);
        await SubmitApplicationAsync(studentClient, created.MaDonTu, upload.RowVersion);

        using var response = await studentClient.GetAsync($"api/student/applications/{created.MaDonTu}/attachments/{upload.Attachment.MaTep}/download");
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(response.Headers.TryGetValues("X-Content-Type-Options", out var nosniff), Is.True);
            Assert.That(nosniff!.Single(), Is.EqualTo("nosniff"));
            Assert.That(response.Headers.CacheControl?.Private, Is.True);
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(response.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));
            Assert.That(bytes, Is.EqualTo(file.Bytes));
        });
    }

    [Test]
    public async Task Download_DeletedOrMissingObject_ShouldReturn404()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var deletedApp = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} deleted download");
        var missingApp = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} missing object");
        var deletedUpload = await UploadOneAndReadAsync(studentClient, deletedApp.MaDonTu, deletedApp.RowVersion, PdfFile("deleted.pdf"));
        var missingUpload = await UploadOneAndReadAsync(studentClient, missingApp.MaDonTu, missingApp.RowVersion, PdfFile("missing.pdf"));
        await DeleteAttachmentAsync(studentClient, deletedApp.MaDonTu, deletedUpload.Attachment.MaTep, deletedUpload.RowVersion);
        var missing = await GetAttachmentAsync(missingUpload.Attachment.MaTep);
        File.Delete(StoragePath(missing.StorageKey));

        using var deletedResponse = await studentClient.GetAsync($"api/student/applications/{deletedApp.MaDonTu}/attachments/{deletedUpload.Attachment.MaTep}/download");
        using var missingResponse = await studentClient.GetAsync($"api/student/applications/{missingApp.MaDonTu}/attachments/{missingUpload.Attachment.MaTep}/download");
        var deletedDescription = await DescribeResponseAsync(deletedResponse);
        var missingDescription = await DescribeResponseAsync(missingResponse);

        Assert.Multiple(() =>
        {
            Assert.That(deletedResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), deletedDescription);
            Assert.That(missingResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), missingDescription);
        });
    }

    [Test]
    public async Task Delete_ShouldSoftDeleteHideFromDetailAndRemovePhysicalObject()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} delete");
        var upload = await UploadOneAndReadAsync(studentClient, created.MaDonTu, created.RowVersion, PdfFile("delete.pdf"));
        var before = await GetAttachmentAsync(upload.Attachment.MaTep);
        Assert.That(File.Exists(StoragePath(before.StorageKey)), Is.True);

        using var response = await DeleteAsync(studentClient, created.MaDonTu, upload.Attachment.MaTep, upload.RowVersion);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var result = await ReadDeleteAsync(response);
        var after = await GetAttachmentAsync(upload.Attachment.MaTep);
        var detail = await GetApplicationAsync(studentClient, created.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(result.ActiveFileCount, Is.EqualTo(0));
            Assert.That(result.RowVersion, Is.Not.EqualTo(upload.RowVersion));
            Assert.That(after.DaXoa, Is.True);
            Assert.That(after.NgayXoa, Is.Not.Null);
            Assert.That(File.Exists(StoragePath(before.StorageKey)), Is.False);
            Assert.That(detail.AttachmentIds, Does.Not.Contain(upload.Attachment.MaTep));
        });
    }

    [Test]
    public async Task Delete_StaleRowVersionOrSubmittedStatus_ShouldReturn409And400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var staleApp = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} delete stale");
        var submittedApp = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} delete submitted");
        var staleUpload = await UploadOneAndReadAsync(studentClient, staleApp.MaDonTu, staleApp.RowVersion, PdfFile("stale-delete.pdf"));
        var submittedUpload = await UploadOneAndReadAsync(studentClient, submittedApp.MaDonTu, submittedApp.RowVersion, PdfFile("submitted-delete.pdf"));
        await SubmitApplicationAsync(studentClient, submittedApp.MaDonTu, submittedUpload.RowVersion);
        var submittedFresh = await GetApplicationAsync(studentClient, submittedApp.MaDonTu);

        using var staleResponse = await DeleteAsync(studentClient, staleApp.MaDonTu, staleUpload.Attachment.MaTep, staleApp.RowVersion);
        using var submittedResponse = await DeleteAsync(studentClient, submittedApp.MaDonTu, submittedUpload.Attachment.MaTep, submittedFresh.RowVersion);
        var staleDescription = await DescribeResponseAsync(staleResponse);
        var submittedDescription = await DescribeResponseAsync(submittedResponse);

        Assert.Multiple(() =>
        {
            Assert.That(staleResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), staleDescription);
            Assert.That(submittedResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), submittedDescription);
        });
    }

    [Test]
    public async Task LegacyDraft_Upload_ShouldAssignActiveTemplate()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} legacy template");
        await ClearApplicationTemplateAsync(created.MaDonTu);
        var legacy = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await UploadAsync(studentClient, created.MaDonTu, legacy.RowVersion, [PdfFile("legacy.pdf")]);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == created.MaDonTu);
        Assert.That(application.MaMauDon, Is.Not.Null);
    }

    [Test]
    public async Task ConcurrentUpload_SameRowVersion_ShouldHaveOneCreatedOneConflictAndCleanupLoser()
    {
        using var setupClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var firstClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(setupClient, $"{TestPrefix} concurrency");

        var firstTask = UploadAsync(firstClient, created.MaDonTu, created.RowVersion, [new TestFile("first.pdf", "application/pdf", PdfBytes("first"))]);
        var secondTask = UploadAsync(secondClient, created.MaDonTu, created.RowVersion, [new TestFile("second.pdf", "application/pdf", PdfBytes("second"))]);
        var responses = await Task.WhenAll(firstTask, secondTask);

        try
        {
            var codes = responses.Select(x => x.StatusCode).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(codes.Count(x => x == HttpStatusCode.Created), Is.EqualTo(1), string.Join(", ", codes));
                Assert.That(codes.Count(x => x == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", codes));
            });
            Assert.That(await CountActiveAttachmentsAsync(created.MaDonTu), Is.EqualTo(1));
            Assert.That(await CountStoredObjectsForApplicationAsync(created.MaDonTu), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private static async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var db = CreateDbContext();
        await db.Database.OpenConnectionAsync();
        return db;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("P0_DT3_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail("Thiếu env var P0_DT3_TEST_CONNECTION_STRING cho P0-DT3 tests.");
        }

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException exception)
        {
            Assert.Fail($"P0_DT3_TEST_CONNECTION_STRING không hợp lệ: {exception.Message}");
            throw;
        }

        if (string.IsNullOrWhiteSpace(builder.InitialCatalog) ||
            !builder.InitialCatalog.StartsWith("LMS_DT3_", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("P0-DT3 tests chỉ được chạy trên database có tên bắt đầu bằng 'LMS_DT3_'.");
        }

        var backendConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(backendConnectionString))
        {
            Assert.Fail("Thiếu env var ConnectionStrings__DefaultConnection cho backend test server.");
        }

        var backendBuilder = new SqlConnectionStringBuilder(backendConnectionString);
        if (!string.Equals(backendBuilder.InitialCatalog, builder.InitialCatalog, StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("Backend test server phải dùng cùng isolated DT3 database với P0_DT3_TEST_CONNECTION_STRING.");
        }

        return connectionString!;
    }

    private static string GetP0Dt3TestPassword()
    {
        var password = Environment.GetEnvironmentVariable("P0_DT3_TEST_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            Assert.Fail("Thiếu env var P0_DT3_TEST_PASSWORD cho P0-DT3 tests.");
        }

        return password!;
    }

    private static string GetStorageRoot()
    {
        var root = Environment.GetEnvironmentVariable("P0_DT3_TEST_STORAGE_ROOT");
        if (string.IsNullOrWhiteSpace(root))
        {
            Assert.Fail("Thiếu env var P0_DT3_TEST_STORAGE_ROOT cho P0-DT3 tests.");
        }

        var fullPath = Path.GetFullPath(root!);
        var leaf = new DirectoryInfo(fullPath).Name;
        if (!leaf.Contains("LMS_DT3_", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("P0-DT3 storage root phải là thư mục isolated có tên chứa 'LMS_DT3_'.");
        }

        Directory.CreateDirectory(fullPath);

        var configuredProvider = Environment.GetEnvironmentVariable("ApplicationEvidenceStorage__Provider");
        var configuredRoot = Environment.GetEnvironmentVariable("ApplicationEvidenceStorage__LocalRoot");
        if (string.IsNullOrWhiteSpace(configuredRoot))
        {
            Assert.Fail("Thiếu env var ApplicationEvidenceStorage__LocalRoot cho P0-DT3 tests.");
        }

        if (!string.Equals(configuredProvider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("P0-DT3 tests yêu cầu ApplicationEvidenceStorage__Provider=Local.");
        }

        if (!string.Equals(Path.GetFullPath(configuredRoot!), fullPath, StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("ApplicationEvidenceStorage__LocalRoot phải trỏ cùng P0_DT3_TEST_STORAGE_ROOT.");
        }

        return fullPath;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = new HttpClient { BaseAddress = BaseUri };
        using var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetP0Dt3TestPassword()
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            Assert.Fail($"Login {email} thất bại. {await DescribeResponseAsync(loginResponse)}");
        }

        using var root = await GetRootAsync(loginResponse);
        var token = GetRequiredString(root.RootElement, "accessToken");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<ApplicationSnapshot> CreateDraftAndReadAsync(HttpClient client, string title)
    {
        using var response = await client.PostAsJsonAsync("api/student/applications", new
        {
            loaiDon = ApplicationTypes.Confirmation,
            tieuDe = title,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit",
                so_ban = 1
            }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private async Task<ApplicationSnapshot> GetApplicationAsync(HttpClient client, int applicationId)
    {
        using var response = await client.GetAsync($"api/student/applications/{applicationId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private static async Task<string> GetApplicationRawAsync(HttpClient client, int applicationId)
    {
        using var response = await client.GetAsync($"api/student/applications/{applicationId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return await response.Content.ReadAsStringAsync();
    }

    private static ApplicationSnapshot ReadApplication(JsonElement data)
    {
        var attachments = new List<int>();
        if (TryGetPropertyIgnoreCase(data, "attachments", out var attachmentElement) &&
            attachmentElement.ValueKind == JsonValueKind.Array)
        {
            attachments.AddRange(attachmentElement.EnumerateArray().Select(x => GetInt32(x, "maTep")));
        }

        return new ApplicationSnapshot(
            GetInt32(data, "maDonTu"),
            GetRequiredString(data, "trangThai"),
            GetRequiredString(data, "rowVersion"),
            attachments);
    }

    private static async Task<HttpResponseMessage> UploadAsync(
        HttpClient client,
        int applicationId,
        string rowVersion,
        IReadOnlyList<TestFile> files)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(rowVersion), "rowVersion");
        foreach (var file in files)
        {
            var content = new ByteArrayContent(file.Bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(content, "files", file.FileName);
        }

        return await client.PostAsync($"api/student/applications/{applicationId}/attachments", form);
    }

    private static async Task<HttpResponseMessage> DeleteAsync(
        HttpClient client,
        int applicationId,
        int attachmentId,
        string rowVersion)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/student/applications/{applicationId}/attachments/{attachmentId}")
        {
            Content = JsonContent.Create(new DeleteApplicationEvidenceRequest { RowVersion = rowVersion })
        };

        return await client.SendAsync(request);
    }

    private async Task<UploadResult> UploadOneAndReadAsync(
        HttpClient client,
        int applicationId,
        string rowVersion,
        TestFile file)
    {
        using var response = await UploadAsync(client, applicationId, rowVersion, [file]);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        var upload = await ReadUploadAsync(response);
        Assert.That(upload.UploadedFiles, Has.Count.EqualTo(1));
        return new UploadResult(upload.UploadedFiles[0], upload.RowVersion);
    }

    private async Task<ApplicationEvidenceDeleteResponseDto> DeleteAttachmentAsync(
        HttpClient client,
        int applicationId,
        int attachmentId,
        string rowVersion)
    {
        using var response = await DeleteAsync(client, applicationId, attachmentId, rowVersion);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        return await ReadDeleteAsync(response);
    }

    private static async Task<ApplicationEvidenceUploadResponseDto> ReadUploadAsync(HttpResponseMessage response)
    {
        using var root = await GetRootAsync(response);
        return JsonSerializer.Deserialize<ApplicationEvidenceUploadResponseDto>(
            GetRequiredProperty(root.RootElement, "data").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static async Task<ApplicationEvidenceDeleteResponseDto> ReadDeleteAsync(HttpResponseMessage response)
    {
        using var root = await GetRootAsync(response);
        return JsonSerializer.Deserialize<ApplicationEvidenceDeleteResponseDto>(
            GetRequiredProperty(root.RootElement, "data").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static async Task SubmitApplicationAsync(HttpClient client, int applicationId, string rowVersion)
    {
        using var response = await client.PostAsJsonAsync($"api/student/applications/{applicationId}/submit", new
        {
            rowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
    }

    private static async Task SetStatusAsync(int applicationId, string status)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.TrangThai = status;
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task ClearApplicationTemplateAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.MaMauDon = null;
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task<TepDinhKemDonTu> GetAttachmentAsync(int attachmentId)
    {
        await using var db = CreateDbContext();
        return await db.TepDinhKemDonTus.AsNoTracking().SingleAsync(x => x.MaTep == attachmentId);
    }

    private static async Task<int> CountActiveAttachmentsAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        return await db.TepDinhKemDonTus.AsNoTracking()
            .CountAsync(x => x.MaDonTu == applicationId && !x.DaXoa);
    }

    private static async Task<int> CountStoredObjectsForApplicationAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var keys = await db.TepDinhKemDonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId && !x.DaXoa)
            .Select(x => x.StorageKey)
            .ToListAsync();
        return keys.Count(key => File.Exists(StoragePath(key)));
    }

    private static string StoragePath(string storageKey)
    {
        return Path.Combine(GetStorageRoot(), storageKey.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string Sha256Hex(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static TestFile PdfFile(string fileName)
    {
        return new TestFile(fileName, "application/pdf", PdfBytes(fileName));
    }

    private static TestFile JpegFile(string fileName)
    {
        return new TestFile(fileName, "image/jpeg", [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0xFF, 0xD9]);
    }

    private static TestFile PngFile(string fileName)
    {
        return new TestFile(fileName, "image/png", PngBytes());
    }

    private static TestFile WebpFile(string fileName)
    {
        var payload = Encoding.ASCII.GetBytes("RIFF\x0A\0\0\0WEBPVP8 test");
        return new TestFile(fileName, "image/webp", payload);
    }

    private static byte[] PdfBytes(string marker)
    {
        return Encoding.ASCII.GetBytes($"%PDF-1.7\n% {marker}\n1 0 obj\n<<>>\nendobj\n%%EOF");
    }

    private static byte[] LargePdfBytes()
    {
        var bytes = new byte[(10 * 1024 * 1024) + 1];
        Encoding.ASCII.GetBytes("%PDF-1.7\n").CopyTo(bytes, 0);
        return bytes;
    }

    private static byte[] PngBytes()
    {
        return [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private sealed record TestFile(string FileName, string ContentType, byte[] Bytes);

    private sealed record UploadResult(StudentApplicationAttachmentDto Attachment, string RowVersion);

    private sealed record ApplicationSnapshot(
        int MaDonTu,
        string TrangThai,
        string RowVersion,
        IReadOnlyList<int> AttachmentIds);
}

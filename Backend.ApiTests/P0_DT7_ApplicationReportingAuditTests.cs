using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.DTOs.Auth;
using Backend.Helpers;
using Backend.Models;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT7_ApplicationReportingAuditTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string CampusAdminEmail = "campusadmin.hcm@lms.local";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string TestPrefix = "NUnit P0-DT7";

    [OneTimeSetUp]
    public void ValidateP0Dt7Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
        ValidateSharedStorageRoot();
    }

    [Test]
    public async Task EmptyReport_ShouldReturnZeroBucketsAndNullAverage()
    {
        var campusId = await CreateCampusAsync();

        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?maDonVi={campusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var summary = GetRequiredProperty(data, "summary");
        var review = GetRequiredProperty(data, "reviewPerformance");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(summary, "totalApplications"), Is.EqualTo(0));
            Assert.That(GetRequiredProperty(data, "statusBreakdown").GetArrayLength(), Is.EqualTo(7));
            Assert.That(GetRequiredProperty(data, "processingStatusBreakdown").GetArrayLength(), Is.EqualTo(6));
            Assert.That(GetRequiredProperty(data, "typeBreakdown").GetArrayLength(), Is.GreaterThanOrEqualTo(11));
            Assert.That(GetRequiredProperty(data, "campusBreakdown").GetArrayLength(), Is.EqualTo(0));
            Assert.That(GetRequiredProperty(review, "averageReviewHours").ValueKind, Is.EqualTo(JsonValueKind.Null));
        });
    }

    [Test]
    public async Task Overview_ShouldCountStatusesProcessingRatesAndAverage()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        var submittedAt = DateTime.UtcNow.AddHours(-6);
        var approvedAt = submittedAt.AddHours(2);
        var rejectedAt = submittedAt.AddHours(4);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Draft, ApplicationProcessingStatuses.NotProcessed, submittedAt: null);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.Pending);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.InReview, ApplicationProcessingStatuses.ManualRequired);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.NeedSupplement, ApplicationProcessingStatuses.Recorded);
        var approved = await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Succeeded, submittedAt: submittedAt, decidedAt: approvedAt);
        var rejected = await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Rejected, ApplicationProcessingStatuses.Failed, submittedAt: submittedAt, decidedAt: rejectedAt);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Cancelled, ApplicationProcessingStatuses.NotProcessed);
        await AddDecisionLogAsync(approved, ApplicationActions.Approve, approvedAt);
        await AddDecisionLogAsync(rejected, ApplicationActions.Reject, rejectedAt);

        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?campusId={campusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var summary = GetRequiredProperty(data, "summary");
        var review = GetRequiredProperty(data, "reviewPerformance");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(summary, "totalApplications"), Is.EqualTo(7));
            Assert.That(GetInt32(summary, "pendingReview"), Is.EqualTo(2));
            Assert.That(GetInt32(summary, "waitingForSupplement"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "approved"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "rejected"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "cancelled"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "pendingProcessing"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "manualRequired"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "processingRecorded"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "processingSucceeded"), Is.EqualTo(1));
            Assert.That(GetInt32(summary, "processingFailed"), Is.EqualTo(1));
            Assert.That(GetInt32(review, "decidedCount"), Is.EqualTo(2));
            Assert.That(GetDecimal(review, "approvalRate"), Is.EqualTo(50m));
            Assert.That(GetDecimal(review, "rejectionRate"), Is.EqualTo(50m));
            Assert.That(GetDecimal(review, "averageReviewHours"), Is.EqualTo(3m));
            Assert.That(FindBucket(data, "statusBreakdown", ApplicationStatuses.Draft), Is.EqualTo(1));
            Assert.That(FindBucket(data, "processingStatusBreakdown", ApplicationProcessingStatuses.ManualRequired), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Overdue_ShouldExcludeNeedSupplementAndTerminalApplications()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        var overdueDeadline = DateTime.UtcNow.AddHours(-2);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.InReview, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.NeedSupplement, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Rejected, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Cancelled, ApplicationProcessingStatuses.NotProcessed, deadline: overdueDeadline);

        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?maDonVi={campusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var summary = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "summary");
        Assert.That(GetInt32(summary, "overdue"), Is.EqualTo(2));
    }

    [Test]
    public async Task TypeFilter_ShouldRestrictAllBreakdowns()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Succeeded);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Other, ApplicationStatuses.Rejected, ApplicationProcessingStatuses.ManualRequired);

        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?maDonVi={campusId}&type={ApplicationTypes.Other}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(GetRequiredProperty(data, "summary"), "totalApplications"), Is.EqualTo(1));
            Assert.That(FindTypeCount(data, ApplicationTypes.Other), Is.EqualTo(1));
            Assert.That(FindTypeCount(data, ApplicationTypes.Confirmation), Is.EqualTo(0));
        });
    }

    [Test]
    public async Task CampusScope_ShouldExcludeSiblingCampus()
    {
        var visibleCampus = await CreateCampusAsync();
        var siblingCampus = await CreateCampusAsync();
        var visibleStudent = await CreateStudentInCampusAsync(visibleCampus);
        var siblingStudent = await CreateStudentInCampusAsync(siblingCampus);
        await CreateApplicationAsync(visibleCampus, visibleStudent, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.NotProcessed);
        await CreateApplicationAsync(siblingCampus, siblingStudent, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.NotProcessed);
        var academicStaffEmail = await CreateUserInCampusAsync(visibleCampus, AuthRoles.AcademicStaff);
        using var staffClient = await CreateAuthenticatedClientAsync(academicStaffEmail);

        using var response = await staffClient.GetAsync("api/admin/applications/reports/overview");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var campusBreakdown = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "campusBreakdown");
        var campusIds = campusBreakdown.EnumerateArray().Select(x => GetInt32(x, "campusId")).ToHashSet();
        Assert.Multiple(() =>
        {
            Assert.That(campusIds, Does.Contain(visibleCampus));
            Assert.That(campusIds, Does.Not.Contain(siblingCampus));
        });
    }

    [Test]
    public async Task CampusAdmin_ShouldSeeDescendantCampus()
    {
        var parentCampus = await CreateCampusAsync();
        var childCampus = await CreateCampusAsync(parentCampus);
        var studentId = await CreateStudentInCampusAsync(childCampus);
        await CreateApplicationAsync(childCampus, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.NotProcessed);
        var campusAdminEmail = await CreateUserInCampusAsync(parentCampus, AuthRoles.CampusAdmin);
        using var campusAdmin = await CreateAuthenticatedClientAsync(campusAdminEmail);

        using var response = await campusAdmin.GetAsync("api/admin/applications/reports/overview");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var campusBreakdown = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "campusBreakdown");
        Assert.That(campusBreakdown.EnumerateArray().Select(x => GetInt32(x, "campusId")), Does.Contain(childCampus));
    }

    [Test]
    public async Task CampusFilterOutsideScope_ShouldReturn403()
    {
        var ownCampus = await CreateCampusAsync();
        var otherCampus = await CreateCampusAsync();
        var campusAdminEmail = await CreateUserInCampusAsync(ownCampus, AuthRoles.CampusAdmin);
        using var campusAdmin = await CreateAuthenticatedClientAsync(campusAdminEmail);

        using var response = await campusAdmin.GetAsync($"api/admin/applications/reports/overview?campusId={otherCampus}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AssigneeAndProcessorFilters_ShouldUseCorrectFields()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        var assignee = await CreateUserInCampusAndReturnIdAsync(campusId, AuthRoles.AcademicStaff);
        var processor = await CreateUserInCampusAndReturnIdAsync(campusId, AuthRoles.AcademicStaff);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.Pending, assigneeId: assignee);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Recorded, processorId: processor);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.Pending);

        using var assigneeResponse = await Client.GetAsync($"api/admin/applications/reports/overview?campusId={campusId}&assigneeId={assignee}");
        using var processorResponse = await Client.GetAsync($"api/admin/applications/reports/overview?campusId={campusId}&processorId={processor}");

        Assert.That(assigneeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(assigneeResponse));
        Assert.That(processorResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(processorResponse));
        using var assigneeRoot = await GetRootAsync(assigneeResponse);
        using var processorRoot = await GetRootAsync(processorResponse);
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(GetRequiredProperty(GetRequiredProperty(assigneeRoot.RootElement, "data"), "summary"), "totalApplications"), Is.EqualTo(1));
            Assert.That(GetInt32(GetRequiredProperty(GetRequiredProperty(processorRoot.RootElement, "data"), "summary"), "totalApplications"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SubmittedDateFilter_ShouldExcludeDraftWithoutSubmissionDate()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        var from = DateTime.UtcNow.AddDays(-1);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Draft, ApplicationProcessingStatuses.NotProcessed, submittedAt: null);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.NotProcessed, submittedAt: DateTime.UtcNow.AddHours(-1));

        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?campusId={campusId}&submittedFrom={Uri.EscapeDataString(from.ToString("O"))}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetInt32(GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "summary"), "totalApplications"), Is.EqualTo(1));
    }

    [Test]
    public async Task AliasConflict_ShouldReturn400()
    {
        using var response = await Client.GetAsync($"api/admin/applications/reports/overview?status={ApplicationStatuses.Submitted}&trangThai={ApplicationStatuses.Approved}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ReportSummary_ShouldUseSingleTaggedSqlCommand()
    {
        var campusId = await CreateCampusAsync();
        var studentId = await CreateStudentInCampusAsync(campusId);
        await CreateApplicationAsync(campusId, studentId, ApplicationTypes.Confirmation, ApplicationStatuses.Submitted, ApplicationProcessingStatuses.Pending);
        var interceptor = new TaggedCommandCounterInterceptor("P0-DT7 ReportSummary");
        await using var db = CreateDbContext(interceptor);
        var superAdmin = await db.NguoiDungs.AsNoTracking().FirstAsync(x => x.Email == SuperAdminEmail);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = superAdmin.MaNguoiDung,
            Email = superAdmin.Email,
            Role = AuthRoles.SuperAdmin,
            CampusId = superAdmin.MaDonVi,
            Status = UserStatuses.DbActive
        };
        var scope = new ApplicationCampusScopeService(db, accessor);
        var service = new ApplicationReportService(db, scope, Options.Create(new ApplicationQueueOptions()));

        await service.GetOverviewAsync(new ApplicationReportQueryParameters { CampusId = campusId });

        Assert.That(interceptor.TaggedCommandCount, Is.EqualTo(1));
    }

    [Test]
    public async Task UploadAndDeleteEvidence_ShouldExposeTypedMetadataAndCreateMinimalAudit()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} evidence audit");

        var upload = await UploadOneAndReadAsync(studentClient, created.MaDonTu, created.RowVersion, PdfFile("dt7-evidence.pdf"));
        using var uploadDetail = await Client.GetAsync($"api/admin/applications/{created.MaDonTu}");
        Assert.That(uploadDetail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(uploadDetail));
        var uploadBody = await uploadDetail.Content.ReadAsStringAsync();
        using var uploadRoot = JsonDocument.Parse(uploadBody);
        var uploadMetadata = FindTimelineMetadata(uploadRoot.RootElement, "upload_evidence");
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(uploadMetadata, "operation"), Is.EqualTo("upload_evidence"));
            Assert.That(GetInt32(uploadMetadata, "fileCount"), Is.EqualTo(1));
            Assert.That(uploadBody.ToLowerInvariant(), Does.Not.Contain("storagekey"));
            Assert.That(uploadBody.ToLowerInvariant(), Does.Not.Contain("filehash"));
            Assert.That(uploadBody.ToLowerInvariant(), Does.Not.Contain("tenfileluu"));
        });

        var delete = await DeleteAttachmentAsync(studentClient, created.MaDonTu, upload.Attachment.MaTep, upload.RowVersion);
        using var deleteDetail = await Client.GetAsync($"api/admin/applications/{created.MaDonTu}");
        Assert.That(deleteDetail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(deleteDetail));
        var deleteBody = await deleteDetail.Content.ReadAsStringAsync();
        using var deleteRoot = JsonDocument.Parse(deleteBody);
        var deleteMetadata = FindTimelineMetadata(deleteRoot.RootElement, "delete_evidence");
        Assert.Multiple(() =>
        {
            Assert.That(delete.ActiveFileCount, Is.EqualTo(0));
            Assert.That(GetRequiredString(deleteMetadata, "operation"), Is.EqualTo("delete_evidence"));
            Assert.That(GetInt32(deleteMetadata, "attachmentId"), Is.EqualTo(upload.Attachment.MaTep));
        });

        await using var db = CreateDbContext();
        var audits = await db.NhatKyKiemToans.AsNoTracking()
            .Where(x => x.MaDoiTuong == created.MaDonTu.ToString() && x.LoaiDoiTuong == nameof(DonTu))
            .OrderBy(x => x.MaKiemToan)
            .ToListAsync();
        Assert.That(audits.Count(x => x.MoTa == "P0-DT7 application evidence upload"), Is.EqualTo(1));
        Assert.That(audits.Count(x => x.MoTa == "P0-DT7 application evidence delete"), Is.EqualTo(1));
        foreach (var audit in audits.Where(x => x.MoTa?.StartsWith("P0-DT7 application evidence", StringComparison.Ordinal) == true))
        {
            var combined = $"{audit.GiaTriCu} {audit.GiaTriMoi}".ToLowerInvariant();
            Assert.Multiple(() =>
            {
                Assert.That(combined, Does.Contain("activefilecount"));
                Assert.That(combined, Does.Contain("totalsizebytes"));
                Assert.That(combined, Does.Not.Contain("dt7-evidence.pdf"));
                Assert.That(combined, Does.Not.Contain("storagekey"));
                Assert.That(combined, Does.Not.Contain("filehash"));
                Assert.That(combined, Does.Not.Contain("tenfileluu"));
            });
        }
    }

    [Test]
    public void LegacyEvidenceSnapshots_ShouldMapSafely()
    {
        var upload = ApplicationTimelineMetadataSanitizer.Sanitize("{\"attachmentAction\":\"upload\",\"count\":2,\"totalBytes\":12345,\"storageKey\":\"secret\"}");
        var delete = ApplicationTimelineMetadataSanitizer.Sanitize("{\"attachmentAction\":\"delete\",\"maTep\":11,\"fileHash\":\"secret\"}");
        var malformed = ApplicationTimelineMetadataSanitizer.Sanitize("{ bad json");
        var futureWins = ApplicationTimelineMetadataSanitizer.Sanitize("{\"operation\":\"delete_evidence\",\"attachmentId\":9,\"attachmentAction\":\"upload\",\"count\":4}");

        Assert.Multiple(() =>
        {
            Assert.That(upload, Is.Not.Null);
            Assert.That(upload!.Operation, Is.EqualTo("upload_evidence"));
            Assert.That(upload.FileCount, Is.EqualTo(2));
            Assert.That(delete, Is.Not.Null);
            Assert.That(delete!.Operation, Is.EqualTo("delete_evidence"));
            Assert.That(delete.AttachmentId, Is.EqualTo(11));
            Assert.That(malformed, Is.Null);
            Assert.That(futureWins!.Operation, Is.EqualTo("delete_evidence"));
            Assert.That(futureWins.AttachmentId, Is.EqualTo(9));
            Assert.That(futureWins.FileCount, Is.Null);
        });
    }

    [Test]
    public async Task StudentDetail_ShouldNotExposeHiddenEvidenceTimeline()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, $"{TestPrefix} hidden student timeline");
        await UploadOneAndReadAsync(studentClient, created.MaDonTu, created.RowVersion, PdfFile("hidden.pdf"));

        var raw = await GetApplicationRawAsync(studentClient, created.MaDonTu);

        Assert.That(raw.ToLowerInvariant(), Does.Not.Contain("upload_evidence"));
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
        var data = GetRequiredProperty(root.RootElement, "data");
        return new ApplicationSnapshot(
            GetInt32(data, "maDonTu"),
            GetRequiredString(data, "rowVersion"));
    }

    private static async Task<string> GetApplicationRawAsync(HttpClient client, int applicationId)
    {
        using var response = await client.GetAsync($"api/student/applications/{applicationId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = new HttpClient { BaseAddress = BaseUri };
        using var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(loginResponse));
        using var root = await GetRootAsync(loginResponse);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetRequiredString(root.RootElement, "accessToken"));
        return client;
    }

    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, int applicationId, string rowVersion, IReadOnlyList<TestFile> files)
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

    private static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, int applicationId, int attachmentId, string rowVersion)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/student/applications/{applicationId}/attachments/{attachmentId}")
        {
            Content = JsonContent.Create(new DeleteApplicationEvidenceRequest { RowVersion = rowVersion })
        };
        return await client.SendAsync(request);
    }

    private async Task<UploadResult> UploadOneAndReadAsync(HttpClient client, int applicationId, string rowVersion, TestFile file)
    {
        using var response = await UploadAsync(client, applicationId, rowVersion, [file]);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var upload = JsonSerializer.Deserialize<ApplicationEvidenceUploadResponseDto>(
            GetRequiredProperty(root.RootElement, "data").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Assert.That(upload.UploadedFiles, Has.Count.EqualTo(1));
        return new UploadResult(upload.UploadedFiles[0], upload.RowVersion);
    }

    private async Task<ApplicationEvidenceDeleteResponseDto> DeleteAttachmentAsync(HttpClient client, int applicationId, int attachmentId, string rowVersion)
    {
        using var response = await DeleteAsync(client, applicationId, attachmentId, rowVersion);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return JsonSerializer.Deserialize<ApplicationEvidenceDeleteResponseDto>(
            GetRequiredProperty(root.RootElement, "data").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static JsonElement FindTimelineMetadata(JsonElement root, string operation)
    {
        var timeline = GetRequiredProperty(GetRequiredProperty(root, "data"), "timeline");
        foreach (var item in timeline.EnumerateArray())
        {
            if (!TryGetPropertyIgnoreCase(item, "metadata", out var metadata) ||
                metadata.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (GetOptionalString(metadata, "operation") == operation)
            {
                return metadata.Clone();
            }
        }

        Assert.Fail($"Không tìm thấy timeline metadata operation={operation}.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }

    private static int FindBucket(JsonElement data, string propertyName, string code)
    {
        foreach (var item in GetRequiredProperty(data, propertyName).EnumerateArray())
        {
            if (string.Equals(GetRequiredString(item, "code"), code, StringComparison.OrdinalIgnoreCase))
            {
                return GetInt32(item, "count");
            }
        }

        Assert.Fail($"Không tìm thấy bucket {code}.");
        return -1;
    }

    private static int FindTypeCount(JsonElement data, string code)
    {
        foreach (var item in GetRequiredProperty(data, "typeBreakdown").EnumerateArray())
        {
            if (string.Equals(GetRequiredString(item, "code"), code, StringComparison.OrdinalIgnoreCase))
            {
                return GetInt32(item, "count");
            }
        }

        Assert.Fail($"Không tìm thấy type bucket {code}.");
        return -1;
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        return GetRequiredProperty(element, propertyName).GetDecimal();
    }

    private static async Task<int> CreateCampusAsync(int? parentId = null)
    {
        await using var db = CreateDbContext();
        var campus = new DonVi
        {
            MaDonViCha = parentId,
            TenDonVi = $"{TestPrefix} Campus {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        return campus.MaDonVi;
    }

    private static async Task<int> CreateStudentInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt7-student-{Guid.NewGuid():N}@lms.local",
            HoTen = $"{TestPrefix} Student",
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task<int> CreateUserInCampusAndReturnIdAsync(int campusId, string role)
    {
        var email = await CreateUserInCampusAsync(campusId, role);
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .SingleAsync();
    }

    private static async Task<string> CreateUserInCampusAsync(int campusId, string role)
    {
        await using var db = CreateDbContext();
        var email = $"p0dt7-{role}-{Guid.NewGuid():N}@lms.local";
        db.NguoiDungs.Add(new NguoiDung
        {
            MaDonVi = campusId,
            Email = email,
            HoTen = $"{TestPrefix} {role}",
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            VaiTroChinh = AuthRoles.ToDatabaseCode(role),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return email;
    }

    private static async Task<int> CreateApplicationAsync(
        int campusId,
        int studentId,
        string type,
        string status,
        string processingStatus,
        int? assigneeId = null,
        int? processorId = null,
        DateTime? submittedAt = null,
        DateTime? decidedAt = null,
        DateTime? deadline = null)
    {
        await using var db = CreateDbContext();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => (int?)x.MaMauDon)
            .FirstOrDefaultAsync();
        templateId ??= await db.MauDonTus.AsNoTracking()
            .Where(x => x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaDonVi = campusId,
            MaHocSinh = studentId,
            MaMauDon = templateId,
            LoaiDon = type,
            TieuDe = $"{TestPrefix} report {Guid.NewGuid():N}",
            DuLieuBieuMau = "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = processingStatus,
            NguoiDuyetHienTai = assigneeId,
            NguoiXuLyCuoi = processorId,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = status == ApplicationStatuses.Draft ? null : submittedAt ?? now.AddHours(-2),
            NgayDuyet = decidedAt,
            HanXuLyLuc = deadline ?? now.AddHours(24)
        };
        db.DonTus.Add(application);
        await db.SaveChangesAsync();
        return application.MaDonTu;
    }

    private static async Task AddDecisionLogAsync(int applicationId, string action, DateTime at)
    {
        await using var db = CreateDbContext();
        db.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = applicationId,
            NguonThucHien = "user",
            HanhDong = action,
            TrangThaiCu = ApplicationStatuses.InReview,
            TrangThaiMoi = action == ApplicationActions.Approve ? ApplicationStatuses.Approved : ApplicationStatuses.Rejected,
            HienThiChoHocSinh = true,
            NgayTao = at
        });
        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDbContext(DbCommandInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString());
        if (interceptor is not null)
        {
            builder.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(builder.Options);
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

    private static TestFile PdfFile(string fileName)
    {
        return new TestFile(fileName, "application/pdf", Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF"));
    }

    private sealed record ApplicationSnapshot(int MaDonTu, string RowVersion);
    private sealed record UploadResult(StudentApplicationAttachmentDto Attachment, string RowVersion);
    private sealed record TestFile(string FileName, string ContentType, byte[] Bytes);

    private sealed class TaggedCommandCounterInterceptor : DbCommandInterceptor
    {
        private readonly string _tag;

        public TaggedCommandCounterInterceptor(string tag)
        {
            _tag = tag;
        }

        public int TaggedCommandCount { get; private set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            CountIfTagged(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            CountIfTagged(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void CountIfTagged(DbCommand command)
        {
            if (command.CommandText.Contains(_tag, StringComparison.Ordinal))
            {
                TaggedCommandCount++;
            }
        }
    }
}

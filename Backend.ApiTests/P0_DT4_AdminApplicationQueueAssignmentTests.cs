using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT4_AdminApplicationQueueAssignmentTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string OtherStudentEmail = "student.tkdh01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string TestPrefix = "NUnit P0-DT4";

    [OneTimeSetUp]
    public void ValidateP0Dt4Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
        ValidateSharedStorageRoot();
    }

    [Test]
    public async Task Anonymous_Queue_ShouldReturn401()
    {
        using var client = new HttpClient { BaseAddress = BaseUri };
        using var response = await client.GetAsync("api/admin/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Student_Queue_ShouldReturn403()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var response = await studentClient.GetAsync("api/admin/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Queue_Default_ShouldIncludeSubmittedAndInReviewOnly()
    {
        var submitted = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var inReview = await CreateApplicationAsync(ApplicationStatuses.InReview);
        var supplement = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement);
        var draft = await CreateApplicationAsync(ApplicationStatuses.Draft);
        var approved = await CreateApplicationAsync(ApplicationStatuses.Approved);

        using var response = await Client.GetAsync($"api/admin/applications?search={Uri.EscapeDataString(TestPrefix)}&pageIndex=1&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
        Assert.Multiple(() =>
        {
            Assert.That(ids, Does.Contain(submitted.MaDonTu));
            Assert.That(ids, Does.Contain(inReview.MaDonTu));
            Assert.That(ids, Does.Not.Contain(supplement.MaDonTu));
            Assert.That(ids, Does.Not.Contain(draft.MaDonTu));
            Assert.That(ids, Does.Not.Contain(approved.MaDonTu));
        });
    }

    [Test]
    public async Task Queue_ExplicitSupplementStatus_ShouldIncludeNeedSupplement()
    {
        var supplement = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement);

        using var response = await Client.GetAsync($"api/admin/applications?trangThai={ApplicationStatuses.NeedSupplement}&search={Uri.EscapeDataString(TestPrefix)}&pageIndex=1&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
        Assert.That(ids, Does.Contain(supplement.MaDonTu));
    }

    [Test]
    public async Task QueueSummary_ShouldReturnSlaCounts()
    {
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(-1));
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(2));
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(48));
        await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-1));

        using var response = await Client.GetAsync($"api/admin/applications/queue-summary?search={Uri.EscapeDataString(TestPrefix)}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "totalActive"), Is.GreaterThanOrEqualTo(3));
            Assert.That(GetInt32(data, "overdue"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "dueSoon"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "unassigned"), Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task Detail_ShouldReturnSafeFieldsAndTimeline()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddAttachmentMetadataAndObjectAsync(application.MaDonTu);

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        var lower = body.ToLowerInvariant();
        using var root = JsonDocument.Parse(body);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "maDonTu"), Is.EqualTo(application.MaDonTu));
            Assert.That(GetRequiredProperty(data, "attachments").GetArrayLength(), Is.EqualTo(1));
            Assert.That(GetRequiredProperty(data, "timeline").GetArrayLength(), Is.GreaterThanOrEqualTo(1));
            Assert.That(lower, Does.Not.Contain("storagekey"));
            Assert.That(lower, Does.Not.Contain("filehash"));
            Assert.That(lower, Does.Not.Contain("tenfileluu"));
            Assert.That(lower, Does.Not.Contain("matkhau"));
            Assert.That(lower, Does.Not.Contain("password"));
        });
    }

    [Test]
    public async Task Receive_SubmittedUnassigned_ShouldSetInReviewAssigneeAndPublicLog()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var updated = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == application.MaDonTu);
        var superAdminId = await GetUserIdAsync("superadmin@lms.local");
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Receive)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.TrangThai, Is.EqualTo(ApplicationStatuses.InReview));
            Assert.That(updated.NguoiDuyetHienTai, Is.EqualTo(superAdminId));
            Assert.That(updated.NguoiXuLyCuoi, Is.Null);
            Assert.That(log.HienThiChoHocSinh, Is.True);
            Assert.That(log.GhiChuCongKhai, Is.EqualTo("Đơn đã được tiếp nhận để xử lý."));
            Assert.That(log.SnapshotJson, Does.Contain("toAssigneeId"));
        });
    }

    [Test]
    public async Task Receive_StaleRowVersion_ShouldReturn409()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await TouchApplicationAsync(application.MaDonTu);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Receive_AlreadyAssigned_ShouldReturn409()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted, assigneeId: assignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Receive_NonSubmitted_ShouldReturn400()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_SubmittedUnassigned_ShouldSetAssigneeInReviewAndPublicLog()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var updated = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == application.MaDonTu);
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Assign)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.TrangThai, Is.EqualTo(ApplicationStatuses.InReview));
            Assert.That(updated.NguoiDuyetHienTai, Is.EqualTo(assignee));
            Assert.That(log.HienThiChoHocSinh, Is.True);
            Assert.That(log.SnapshotJson, Does.Contain("toAssigneeId"));
        });
    }

    [Test]
    public async Task Assign_ReassignRequiresReason()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_ReassignWithReason_ShouldCreateInternalLog()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion,
            lyDo = "NUnit P0-DT4 đổi người xử lý do phân tải công việc."
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Reassign)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(log.HienThiChoHocSinh, Is.False);
            Assert.That(log.GhiChuNoiBo, Does.Contain("NUnit P0-DT4"));
            Assert.That(log.SnapshotJson, Does.Contain("reasonProvided"));
        });
    }

    [Test]
    public async Task Assign_SameAssigneeNoOp_ShouldValidateRowVersionAndNotLog()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: assignee);
        var beforeLogs = await CountLogsAsync(application.MaDonTu);

        using var ok = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(ok));
        Assert.That(await CountLogsAsync(application.MaDonTu), Is.EqualTo(beforeLogs));

        await TouchApplicationAsync(application.MaDonTu);
        using var stale = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(stale));
    }

    [TestCase(ApplicationStatuses.Draft)]
    [TestCase(ApplicationStatuses.Approved)]
    [TestCase(ApplicationStatuses.Rejected)]
    [TestCase(ApplicationStatuses.Cancelled)]
    public async Task Assign_NonAssignableStatuses_ShouldReturn400(string status)
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(status);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AcademicStaff_CanReceiveButCannotAssign()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), async client =>
        {
            using var receive = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
            {
                rowVersion = application.RowVersion
            });
            Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(receive));

            var fresh = await GetApplicationSnapshotFromDbAsync(application.MaDonTu);
            var assignee = await CreateAssignableUserAsync();
            using var assign = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
            {
                assigneeId = assignee,
                rowVersion = fresh.RowVersion
            });
            Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(assign));
        });
    }

    [Test]
    public async Task Principal_CanReadButCannotReceive()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.Principal), async client =>
        {
            using var detail = await client.GetAsync($"api/admin/applications/{application.MaDonTu}");
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detail));

            using var receive = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
            {
                rowVersion = application.RowVersion
            });
            Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(receive));
        });
    }

    [Test]
    public async Task ScopedUser_DetailOutsideCampus_ShouldReturn404_AndListFilterOutsideScope403()
    {
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherCampusApplication = await CreateApplicationForEmailAsync(
            StudentEmail,
            ApplicationStatuses.Submitted,
            campusOverride: otherCampusId);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            using var detail = await client.GetAsync($"api/admin/applications/{otherCampusApplication.MaDonTu}");
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(detail));

            using var list = await client.GetAsync($"api/admin/applications?maDonVi={otherCampusApplication.MaDonVi}");
            Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(list));
        });
    }

    [Test]
    public async Task Assignees_InvalidOutsideScope_ShouldReturnNotFoundOnAssign()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherCampusAssignee = await CreateAssignableUserInCampusAsync(otherCampusId);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            using var response = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
            {
                assigneeId = otherCampusAssignee,
                rowVersion = application.RowVersion
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task AdminDownloadAttachment_ShouldReturnFileWithSafeHeaders()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var attachmentId = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu);

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Multiple(() =>
        {
            Assert.That(bytes, Is.EqualTo(Encoding.UTF8.GetBytes("P0-DT4 evidence")));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(response.Headers.TryGetValues("X-Content-Type-Options", out var values), Is.True);
            Assert.That(values!.Single(), Is.EqualTo("nosniff"));
        });
    }

    [Test]
    public async Task AdminDownload_DeletedOrMissing_ShouldReturn404()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var deleted = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu, deleted: true);

        using var deletedResponse = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{deleted}/download");
        using var missingResponse = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/999999999/download");

        Assert.That(deletedResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(deletedResponse));
        Assert.That(missingResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(missingResponse));
    }

    [Test]
    public async Task Receive_ConcurrentSameRowVersion_ShouldHaveOneSuccessOneConflict()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        using var firstClient = await CreateAuthenticatedClientAsync("superadmin@lms.local");
        using var secondClient = await CreateAuthenticatedClientAsync("superadmin@lms.local");

        var firstTask = firstClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
        var secondTask = secondClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
        var responses = await Task.WhenAll(firstTask, secondTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(application.MaDonTu, ApplicationActions.Receive), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private async Task<ApplicationSnapshot> CreateApplicationAsync(
        string status,
        DateTime? deadline = null,
        int? assigneeId = null)
    {
        return await CreateApplicationForEmailAsync(StudentEmail, status, deadline, assigneeId);
    }

    private static async Task<ApplicationSnapshot> CreateApplicationForEmailAsync(
        string email,
        string status,
        DateTime? deadline = null,
        int? assigneeId = null,
        int? campusOverride = null)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == ApplicationTypes.Confirmation && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = campusOverride ?? student.MaDonVi,
            MaMauDon = templateId,
            LoaiDon = ApplicationTypes.Confirmation,
            TieuDe = $"{TestPrefix} {status} {Guid.NewGuid():N}",
            DuLieuBieuMau = "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed,
            NguoiDuyetHienTai = assigneeId,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = status == ApplicationStatuses.Draft ? null : now.AddMinutes(-5),
            HanXuLyLuc = deadline ?? now.AddHours(48)
        };
        db.DonTus.Add(application);
        await db.SaveChangesAsync();
        db.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = student.MaNguoiDung,
            NguonThucHien = "user",
            HanhDong = ApplicationActions.Submit,
            TrangThaiCu = ApplicationStatuses.Draft,
            TrangThaiMoi = status,
            GhiChuCongKhai = "NUnit P0-DT4 tạo đơn.",
            HienThiChoHocSinh = true,
            NgayTao = now
        });
        await db.SaveChangesAsync();
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion));
    }

    private static async Task<int> CreateAssignableUserAsync()
    {
        return await CreateAssignableUserForEmailCampusAsync(StudentEmail);
    }

    private static async Task<int> CreateAssignableUserForEmailCampusAsync(string email)
    {
        await using var db = CreateDbContext();
        var campusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        return await CreateAssignableUserInCampusAsync(campusId);
    }

    private static async Task<int> CreateAssignableUserInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt4-assignee-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT4 Assignee",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task<int> CreateDifferentCampusAsync()
    {
        await using var db = CreateDbContext();
        var baseCampusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == StudentEmail)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        var campus = new DonVi
        {
            TenDonVi = $"NUnit P0-DT4 campus {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        Assert.That(campus.MaDonVi, Is.Not.EqualTo(baseCampusId));
        return campus.MaDonVi;
    }

    private static async Task<int> AddAttachmentMetadataAndObjectAsync(int applicationId, bool deleted = false)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == applicationId);
        var storageKey = $"applications/evidence/{application.MaDonVi}/{applicationId}/p0dt4-{Guid.NewGuid():N}.pdf";
        var root = GetSharedTestStorageRoot();
        var path = Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("P0-DT4 evidence"));
        var attachment = new TepDinhKemDonTu
        {
            MaDonTu = applicationId,
            StorageKey = storageKey,
            TenFileGoc = "p0dt4.pdf",
            TenFileLuu = Path.GetFileName(path),
            ContentType = "application/pdf",
            KichThuocByte = Encoding.UTF8.GetByteCount("P0-DT4 evidence"),
            FileHash = "hidden",
            NguoiTaiLen = application.MaHocSinh,
            NgayTao = DateTime.UtcNow,
            DaXoa = deleted,
            NguoiXoa = deleted ? application.MaHocSinh : null,
            NgayXoa = deleted ? DateTime.UtcNow : null
        };
        db.TepDinhKemDonTus.Add(attachment);
        await db.SaveChangesAsync();
        return attachment.MaTep;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await LoginAsync(email);
        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> LoginAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"Không login được account seed {email}. {await DescribeResponseAsync(response)}");
        }

        using var root = await GetRootAsync(response);
        return GetRequiredString(root.RootElement, "accessToken");
    }

    private async Task WithMutatedUserAndLoginAsync(string email, string role, Func<HttpClient, Task> action)
    {
        await using var db = CreateDbContext();
        var user = await db.NguoiDungs.FirstAsync(x => x.Email == email);
        var oldRole = user.VaiTroChinh;
        var oldStatus = user.TrangThai;
        user.VaiTroChinh = role;
        user.TrangThai = UserStatuses.DbActive;
        await db.SaveChangesAsync();

        try
        {
            using var outerClient = new HttpClient { BaseAddress = BaseUri };
            using var loginResponse = await outerClient.PostAsJsonAsync("api/auth/login", new
            {
                email,
                password = GetSharedTestPassword()
            });
            Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(loginResponse));
            using var root = await GetRootAsync(loginResponse);
            var token = GetRequiredString(root.RootElement, "accessToken");
            using var client = new HttpClient { BaseAddress = outerClient.BaseAddress };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await action(client);
        }
        finally
        {
            user.VaiTroChinh = oldRole;
            user.TrangThai = oldStatus;
            await db.SaveChangesAsync();
        }
    }

    private static async Task TouchApplicationAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.NgayCapNhat = DateTime.UtcNow.AddSeconds(1);
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountLogsAsync(int applicationId, string? action = null)
    {
        await using var db = CreateDbContext();
        var query = db.NhatKyDuyetDons.AsNoTracking().Where(x => x.MaDonTu == applicationId);
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.HanhDong == action);
        }

        return await query.CountAsync();
    }

    private static async Task<int> GetUserIdAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static async Task<ApplicationSnapshot> GetApplicationSnapshotFromDbAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == applicationId);
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var connectionString = GetSharedTestConnectionString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed record ApplicationSnapshot(int MaDonTu, int MaDonVi, string RowVersion);
}

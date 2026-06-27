using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT6_PostApprovalProcessingTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string CampusAdminEmail = "campusadmin.hcm@lms.local";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string TestPrefix = "NUnit P0-DT6";

    [OneTimeSetUp]
    public void ValidateP0Dt6Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
    }

    [Test]
    public async Task Anonymous_Process_ShouldReturn401()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);
        using var client = new HttpClient { BaseAddress = BaseUri };

        using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task PrincipalAndStudent_Process_ShouldReturn403()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.Principal), async principal =>
        {
            using var response = await principal.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });

        using var student = await CreateAuthenticatedClientAsync(StudentEmail);
        using var studentResponse = await student.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });
        Assert.That(studentResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(studentResponse));
    }

    [Test]
    public async Task CrossCampus_Process_ShouldReturn404()
    {
        using var campusAdmin = await CreateAuthenticatedClientAsync(CampusAdminEmail);
        var admin = await GetUserAsync(CampusAdminEmail);
        var otherCampus = await GetDifferentCampusIdAsync(admin.MaDonVi);
        var studentId = await CreateStudentInCampusAsync(otherCampus);
        var app = await CreateApplicationAsync(
            ApplicationTypes.Confirmation,
            ApplicationStatuses.Approved,
            ApplicationProcessingStatuses.Pending,
            studentId: studentId,
            campusId: otherCampus);

        using var response = await campusAdmin.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Confirmation_Process_ShouldRecordResultTimelineAuditAndAllowedActions()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var allowed = GetRequiredProperty(data, "allowedActions");
        var db = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(data, "trangThaiXuLyNghiepVu"), Is.EqualTo(ApplicationProcessingStatuses.Recorded));
            Assert.That(GetRequiredString(data, "tenTrangThaiXuLyNghiepVu"), Is.EqualTo("Đã ghi nhận"));
            Assert.That(GetBoolean(allowed, "canProcessAutomatically"), Is.False);
            Assert.That(GetBoolean(allowed, "canRecordProcessingResult"), Is.False);
            Assert.That(db.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.Recorded));
            Assert.That(db.KetQuaXuLyJson, Is.Not.Null.And.Not.Empty);
            Assert.That(db.NhatKyTuDong, Is.Not.Null.And.Not.Empty);
            Assert.That(db.NguoiXuLyCuoi, Is.EqualTo(GetUserIdAsync(SuperAdminEmail).GetAwaiter().GetResult()));
        });

        using var resultJson = JsonDocument.Parse(db.KetQuaXuLyJson!);
        using var autoLogJson = JsonDocument.Parse(db.NhatKyTuDong!);
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(resultJson.RootElement, "mode"), Is.EqualTo("automatic"));
            Assert.That(GetRequiredString(resultJson.RootElement, "handler"), Is.EqualTo("confirmation_request"));
            Assert.That(GetRequiredString(resultJson.RootElement, "outcome"), Is.EqualTo(ApplicationProcessingStatuses.Recorded));
            Assert.That(GetRequiredString(GetRequiredProperty(resultJson.RootElement, "data"), "confirmationType"), Is.EqualTo("dang_hoc"));
            Assert.That(GetRequiredString(autoLogJson.RootElement, "handler"), Is.EqualTo("confirmation_request"));
        });

        Assert.That(await CountLogsAsync(app.MaDonTu, ApplicationActions.BusinessProcess), Is.EqualTo(1));
        Assert.That(await CountAuditsAsync(app.MaDonTu, ApplicationActions.BusinessProcess), Is.EqualTo(1));
    }

    [Test]
    public async Task UnsupportedType_Process_ShouldBecomeManualRequired()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Other, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending, formJson: "{\"noi_dung\":\"Can xu ly thu cong\"}");

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var db = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.That(db.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.ManualRequired));
        using var resultJson = JsonDocument.Parse(db.KetQuaXuLyJson!);
        Assert.That(GetRequiredString(resultJson.RootElement, "handler"), Is.EqualTo("manual_required_fallback"));
    }

    [Test]
    public async Task Process_TerminalOrManualRequired_ShouldBeIdempotentNoOp()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Recorded);
        var before = await GetApplicationFromDbAsync(app.MaDonTu);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var after = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(after.RowVersion, Is.EqualTo(before.RowVersion));
            Assert.That(after.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.Recorded));
            Assert.That(CountLogsAsync(app.MaDonTu, ApplicationActions.BusinessProcess).GetAwaiter().GetResult(), Is.Zero);
            Assert.That(CountAuditsAsync(app.MaDonTu, ApplicationActions.BusinessProcess).GetAwaiter().GetResult(), Is.Zero);
        });
    }

    [Test]
    public async Task Process_NotApprovedOrNotProcessed_ShouldFail()
    {
        var draft = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.InReview, ApplicationProcessingStatuses.Pending);
        using var draftResponse = await Client.PostAsJsonAsync($"api/admin/applications/{draft.MaDonTu}/process", new { rowVersion = draft.RowVersion });
        Assert.That(draftResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(draftResponse));

        var notProcessed = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.NotProcessed);
        using var notProcessedResponse = await Client.PostAsJsonAsync($"api/admin/applications/{notProcessed.MaDonTu}/process", new { rowVersion = notProcessed.RowVersion });
        Assert.That(notProcessedResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(notProcessedResponse));
    }

    [TestCase("")]
    [TestCase("not-base64")]
    [TestCase("AQ==")]
    public async Task Process_InvalidRowVersion_ShouldReturn400(string rowVersion)
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Process_StaleRowVersion_ShouldReturn409()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);
        await TouchApplicationAsync(app.MaDonTu);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ManualRecord_FromManualRequired_ShouldSucceedWithoutOverwritingAutomaticLog()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Other, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending, formJson: "{\"noi_dung\":\"Can xu ly thu cong\"}");
        using var autoResponse = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });
        Assert.That(autoResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(autoResponse));
        var afterAuto = await GetApplicationFromDbAsync(app.MaDonTu);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/record-processing-result", new
        {
            outcome = ApplicationProcessingStatuses.Succeeded,
            publicNote = "Đã xử lý thủ công thành công cho đơn này.",
            internalNote = "Ghi chú nội bộ DT6.",
            result = new { referenceCode = "DT6-OK", issuedCopies = 1 },
            rowVersion = afterAuto.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var db = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(db.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.Succeeded));
            Assert.That(db.NhatKyTuDong, Is.EqualTo(afterAuto.NhatKyTuDong));
        });
        using var resultJson = JsonDocument.Parse(db.KetQuaXuLyJson!);
        Assert.That(GetRequiredString(resultJson.RootElement, "mode"), Is.EqualTo("manual"));
        Assert.That(await CountLogsAsync(app.MaDonTu, ApplicationActions.BusinessProcess), Is.EqualTo(2));
    }

    [Test]
    public async Task ManualRecord_Terminal_ShouldReturn409()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Confirmation, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Recorded);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/record-processing-result", new
        {
            outcome = ApplicationProcessingStatuses.Succeeded,
            publicNote = "Không được ghi đè kết quả đã kết thúc.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ManualRecord_InvalidResult_ShouldReturn400()
    {
        var sensitive = await CreateApplicationAsync(ApplicationTypes.Other, ApplicationStatuses.Approved, ApplicationProcessingStatuses.ManualRequired);
        using var sensitiveResponse = await Client.PostAsJsonAsync($"api/admin/applications/{sensitive.MaDonTu}/record-processing-result", new
        {
            outcome = ApplicationProcessingStatuses.Failed,
            publicNote = "Kết quả chứa dữ liệu nhạy cảm bị từ chối.",
            result = new { storageKey = "secret/path" },
            rowVersion = sensitive.RowVersion
        });
        Assert.That(sensitiveResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(sensitiveResponse));

        var oversized = await CreateApplicationAsync(ApplicationTypes.Other, ApplicationStatuses.Approved, ApplicationProcessingStatuses.ManualRequired);
        using var oversizedResponse = await Client.PostAsJsonAsync($"api/admin/applications/{oversized.MaDonTu}/record-processing-result", new
        {
            outcome = ApplicationProcessingStatuses.Failed,
            publicNote = "Kết quả có payload quá lớn bị từ chối.",
            result = new { note = new string('x', 17 * 1024) },
            rowVersion = oversized.RowVersion
        });
        Assert.That(oversizedResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(oversizedResponse));
    }

    [Test]
    public async Task QueueAndStudentDtos_ShouldExposeProcessingStatusWithoutInternalJson()
    {
        var searchToken = $"{TestPrefix} queue {Guid.NewGuid():N}";
        var app = await CreateApplicationAsync(
            ApplicationTypes.Confirmation,
            ApplicationStatuses.Approved,
            ApplicationProcessingStatuses.Pending,
            title: searchToken);

        using var adminResponse = await Client.GetAsync($"api/admin/applications?status={ApplicationStatuses.Approved}&processingStatus={ApplicationProcessingStatuses.Pending}&search={Uri.EscapeDataString(searchToken)}");
        Assert.That(adminResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(adminResponse));
        using var adminRoot = await GetRootAsync(adminResponse);
        var items = GetDataItems(adminRoot.RootElement).EnumerateArray().ToList();
        Assert.That(items.Any(x => GetInt32(x, "maDonTu") == app.MaDonTu && GetRequiredString(x, "trangThaiXuLyNghiepVu") == ApplicationProcessingStatuses.Pending), Is.True);

        using var student = await CreateAuthenticatedClientAsync(StudentEmail);
        using var studentResponse = await student.GetAsync($"api/student/applications/{app.MaDonTu}");
        Assert.That(studentResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(studentResponse));
        var body = await studentResponse.Content.ReadAsStringAsync();
        using var studentRoot = JsonDocument.Parse(body);
        var data = GetRequiredProperty(studentRoot.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(data, "trangThaiXuLyNghiepVu"), Is.EqualTo(ApplicationProcessingStatuses.Pending));
            Assert.That(body.ToLowerInvariant(), Does.Not.Contain("ketquaxulyjson"));
            Assert.That(body.ToLowerInvariant(), Does.Not.Contain("nhatkytudong"));
        });
    }

    [Test]
    public async Task ConcurrentAutoAndManual_ShouldHaveOne200One409()
    {
        var app = await CreateApplicationAsync(ApplicationTypes.Other, ApplicationStatuses.Approved, ApplicationProcessingStatuses.Pending);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var processTask = firstClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });
        var manualTask = secondClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/record-processing-result", new
        {
            outcome = ApplicationProcessingStatuses.Succeeded,
            publicNote = "Ghi nhận kết quả xử lý cạnh tranh.",
            rowVersion = app.RowVersion
        });
        var responses = await Task.WhenAll(processTask, manualTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(app.MaDonTu, ApplicationActions.BusinessProcess), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private static async Task<ApplicationSnapshot> CreateApplicationAsync(
        string type,
        string status,
        string processingStatus,
        int? studentId = null,
        int? campusId = null,
        string? formJson = null,
        string? title = null)
    {
        await using var db = CreateDbContext();
        var student = studentId.HasValue
            ? await db.NguoiDungs.AsNoTracking().Where(x => x.MaNguoiDung == studentId.Value).Select(x => new { x.MaNguoiDung, x.MaDonVi }).FirstAsync()
            : await db.NguoiDungs.AsNoTracking().Where(x => x.Email == StudentEmail).Select(x => new { x.MaNguoiDung, x.MaDonVi }).FirstAsync();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = campusId ?? student.MaDonVi,
            MaMauDon = templateId,
            LoaiDon = type,
            TieuDe = title ?? $"{TestPrefix} {type} {Guid.NewGuid():N}",
            DuLieuBieuMau = formJson ?? "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = processingStatus,
            NguoiDuyetHienTai = null,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = now.AddMinutes(-30),
            NgayDuyet = status == ApplicationStatuses.Approved ? now.AddMinutes(-5) : null,
            HanXuLyLuc = now.AddHours(48)
        };
        db.DonTus.Add(application);
        await db.SaveChangesAsync();
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion));
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

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
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
            using var client = await CreateAuthenticatedClientAsync(email);
            await action(client);
        }
        finally
        {
            user.VaiTroChinh = oldRole;
            user.TrangThai = oldStatus;
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationDbRecord> GetApplicationFromDbAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        return await db.DonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => new ApplicationDbRecord(
                x.MaDonTu,
                x.TrangThai,
                x.TrangThaiXuLyNghiepVu,
                x.KetQuaXuLyJson,
                x.NhatKyTuDong,
                x.NguoiXuLyCuoi,
                Convert.ToBase64String(x.RowVersion)))
            .SingleAsync();
    }

    private static async Task<UserRecord> GetUserAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new UserRecord(x.MaNguoiDung, x.MaDonVi))
            .SingleAsync();
    }

    private static async Task<int> GetUserIdAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static async Task TouchApplicationAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.NgayCapNhat = DateTime.UtcNow.AddSeconds(1);
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountLogsAsync(int applicationId, string action)
    {
        await using var db = CreateDbContext();
        return await db.NhatKyDuyetDons.AsNoTracking()
            .CountAsync(x => x.MaDonTu == applicationId && x.HanhDong == action);
    }

    private static async Task<int> CountAuditsAsync(int applicationId, string action)
    {
        await using var db = CreateDbContext();
        return await db.NhatKyKiemToans.AsNoTracking().CountAsync(x =>
            x.LoaiDoiTuong == nameof(DonTu) &&
            x.MaDoiTuong == applicationId.ToString() &&
            x.HanhDong == action);
    }

    private static async Task<int> CreateStudentInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt6-student-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT6 Student",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task<int> GetDifferentCampusIdAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var existing = await db.DonVis.AsNoTracking()
            .Where(x => x.MaDonVi != campusId && x.ConHoatDong)
            .Select(x => (int?)x.MaDonVi)
            .FirstOrDefaultAsync();
        if (existing.HasValue)
        {
            return existing.Value;
        }

        var campus = new DonVi
        {
            TenDonVi = $"NUnit P0-DT6 campus {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        return campus.MaDonVi;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed record ApplicationSnapshot(int MaDonTu, int MaDonVi, string RowVersion);

    private sealed record ApplicationDbRecord(
        int MaDonTu,
        string TrangThai,
        string TrangThaiXuLyNghiepVu,
        string? KetQuaXuLyJson,
        string? NhatKyTuDong,
        int? NguoiXuLyCuoi,
        string RowVersion);

    private sealed record UserRecord(int MaNguoiDung, int MaDonVi);
}

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
public class P0_9_AttendanceAutomationTests : ApiTestBase
{
    private const string DefaultPassword = "Admin@123";
    private const string RunOnceEndpoint = "api/admin/attendance-automation/run-once";

    [Test]
    public async Task RunOnce_AutoSubmitDueAttendance_ShouldSetDaGui()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "dang_diem_danh",
            sessionStatus: "du_kien",
            dueSendAt: now.AddMinutes(-5),
            dueEditAt: null,
            ensureAttendanceRows: true);

        try
        {
            var result = await RunOnceAsync();

            Assert.That(HasProcessedItem(result, context.SessionId, "auto_submit"), Is.True);
            Assert.That(result.AutoSubmitted, Is.GreaterThanOrEqualTo(1));

            await using var db = CreateDbContext();
            var session = await db.BuoiHocs.AsNoTracking().FirstAsync(x => x.MaBuoiHoc == context.SessionId);
            Assert.That(session.TrangThaiDiemDanh, Is.EqualTo("da_gui"));
            Assert.That(session.DiemDanhDaGuiLuc, Is.Not.Null);
            Assert.That(session.DiemDanhHanChinhSuaLuc, Is.Not.Null);
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    [Test]
    public async Task RunOnce_AutoLockDueSubmittedAttendance_ShouldSetDaKhoa()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "da_gui",
            sessionStatus: "du_kien",
            dueSendAt: now.AddMinutes(-20),
            dueEditAt: now.AddMinutes(-5),
            ensureAttendanceRows: true);

        try
        {
            var result = await RunOnceAsync();

            Assert.That(HasProcessedItem(result, context.SessionId, "auto_lock"), Is.True);
            Assert.That(result.AutoLocked, Is.GreaterThanOrEqualTo(1));

            await using var db = CreateDbContext();
            var session = await db.BuoiHocs.AsNoTracking().FirstAsync(x => x.MaBuoiHoc == context.SessionId);
            Assert.That(session.TrangThaiDiemDanh, Is.EqualTo("da_khoa"));
            Assert.That(session.DiemDanhKhoaLuc, Is.Not.Null);

            var attendanceRows = await db.DiemDanhs
                .AsNoTracking()
                .Where(x => x.MaBuoiHoc == context.SessionId)
                .ToListAsync();
            Assert.That(attendanceRows, Is.Not.Empty);
            Assert.That(attendanceRows.All(x => x.KhoaLuc is not null), Is.True);
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    [Test]
    public async Task RunOnce_ShouldIgnoreChuaMoAttendance()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "chua_mo",
            sessionStatus: "du_kien",
            dueSendAt: now.AddMinutes(-5),
            dueEditAt: null,
            ensureAttendanceRows: false);

        try
        {
            var result = await RunOnceAsync();

            Assert.That(HasProcessedItem(result, context.SessionId, "auto_submit"), Is.False);
            await using var db = CreateDbContext();
            var session = await db.BuoiHocs.AsNoTracking().FirstAsync(x => x.MaBuoiHoc == context.SessionId);
            Assert.That(session.TrangThaiDiemDanh, Is.EqualTo("chua_mo"));
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    [Test]
    public async Task RunOnce_ShouldIgnoreCanceledSession()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "dang_diem_danh",
            sessionStatus: "da_huy",
            dueSendAt: now.AddMinutes(-5),
            dueEditAt: null,
            ensureAttendanceRows: true);

        try
        {
            var result = await RunOnceAsync();

            Assert.That(HasProcessedItem(result, context.SessionId, "auto_submit"), Is.False);
            await using var db = CreateDbContext();
            var session = await db.BuoiHocs.AsNoTracking().FirstAsync(x => x.MaBuoiHoc == context.SessionId);
            Assert.That(session.TrangThaiBuoi, Is.EqualTo("da_huy"));
            Assert.That(session.TrangThaiDiemDanh, Is.EqualTo("dang_diem_danh"));
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    [Test]
    public async Task RunOnce_ShouldBeIdempotent()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "dang_diem_danh",
            sessionStatus: "du_kien",
            dueSendAt: now.AddMinutes(-5),
            dueEditAt: null,
            ensureAttendanceRows: true);

        try
        {
            var first = await RunOnceAsync();
            var second = await RunOnceAsync();

            Assert.That(HasProcessedItem(first, context.SessionId, "auto_submit"), Is.True);
            Assert.That(HasProcessedItem(second, context.SessionId, "auto_submit"), Is.False);
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient { BaseAddress = BaseUri };

        using var response = await anonymousClient.PostAsync(RunOnceEndpoint, null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Student_RunOnce_ShouldReturnForbidden()
    {
        using var studentClient = await CreateAuthenticatedClientAsync("student.cntt01@lms.local");

        using var response = await studentClient.PostAsync(RunOnceEndpoint, null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Teacher_RunOnce_ShouldReturnForbidden()
    {
        using var teacherClient = await CreateAuthenticatedClientAsync("teacher.csharp.a@lms.local");

        using var response = await teacherClient.PostAsync(RunOnceEndpoint, null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AutoSubmit_ShouldCreateTeacherNotification()
    {
        var now = DateTime.UtcNow;
        var context = await PrepareSessionAsync(
            attendanceStatus: "dang_diem_danh",
            sessionStatus: "du_kien",
            dueSendAt: now.AddMinutes(-5),
            dueEditAt: null,
            ensureAttendanceRows: true);

        try
        {
            await RunOnceAsync();

            await using var db = CreateDbContext();
            var teacher = await db.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == context.EffectiveTeacherId);
            if (teacher is null || string.IsNullOrWhiteSpace(teacher.Email))
            {
                Assert.Inconclusive("Không tìm thấy email giáo viên hiệu lực để kiểm tra notification.");
            }

            using var teacherClient = await CreateAuthenticatedClientAsync(teacher.Email);
            using var response = await teacherClient.GetAsync("api/notifications?loaiThongBao=system&pageIndex=1&pageSize=20");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

            using var root = await GetRootAsync(response);
            var hasNotification = GetDataItems(root.RootElement)
                .EnumerateArray()
                .Any(x =>
                    string.Equals(GetOptionalString(x, "doiTuongLienKet"), "buoi_hoc", StringComparison.OrdinalIgnoreCase) &&
                    GetInt32(x, "maDoiTuongLienKet") == context.SessionId);

            Assert.That(hasNotification, Is.True);
        }
        finally
        {
            await context.RestoreAsync();
        }
    }

    private async Task<AutomationRunResult> RunOnceAsync()
    {
        using var response = await Client.PostAsync(RunOnceEndpoint, null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");

        var items = GetRequiredProperty(data, "items")
            .EnumerateArray()
            .Select(x => new AutomationRunItem(
                GetInt32(x, "maBuoiHoc"),
                GetRequiredString(x, "action"),
                GetRequiredString(x, "status")))
            .ToList();

        return new AutomationRunResult(
            GetInt32(data, "autoSubmitted"),
            GetInt32(data, "autoLocked"),
            items);
    }

    private static bool HasProcessedItem(AutomationRunResult result, int sessionId, string action)
    {
        return result.Items.Any(x =>
            x.MaBuoiHoc == sessionId &&
            string.Equals(x.Action, action, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Status, "processed", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<AutomationSessionContext> PrepareSessionAsync(
        string attendanceStatus,
        string sessionStatus,
        DateTime? dueSendAt,
        DateTime? dueEditAt,
        bool ensureAttendanceRows)
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);

        using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={timetable.MaTkb}&pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var sessions = ReadBuoiHocItems(root.RootElement).ToList();

        foreach (var candidate in sessions)
        {
            await using var db = CreateDbContext();
            var session = await db.BuoiHocs.FirstOrDefaultAsync(x => x.MaBuoiHoc == candidate.MaBuoiHoc);
            if (session is null)
            {
                continue;
            }

            var course = await db.KhoaHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc);
            if (course is null)
            {
                continue;
            }

            var originalSession = OriginalSessionState.From(session);
            var originalAttendanceLocks = await db.DiemDanhs
                .AsNoTracking()
                .Where(x => x.MaBuoiHoc == session.MaBuoiHoc)
                .Select(x => new AttendanceLockState(x.MaDiemDanh, x.KhoaLuc))
                .ToListAsync();

            int? addedAttendanceId = null;
            if (ensureAttendanceRows && originalAttendanceLocks.Count == 0)
            {
                var studentId = await db.NguoiDungs
                    .AsNoTracking()
                    .Where(x =>
                        x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                        x.TrangThai == UserStatuses.DbActive &&
                        x.MaLop == course.MaLop &&
                        x.MaDonVi == course.MaDonVi)
                    .Select(x => x.MaNguoiDung)
                    .FirstOrDefaultAsync();

                if (studentId == 0)
                {
                    continue;
                }

                var attendance = new DiemDanh
                {
                    MaDonVi = course.MaDonVi,
                    MaBuoiHoc = session.MaBuoiHoc,
                    MaHocSinh = studentId,
                    TrangThai = "vang",
                    NguoiGhiNhan = session.MaGiaoVien,
                    GhiNhanLuc = DateTime.UtcNow,
                    KhoaLuc = null,
                    HeSoVang = 1,
                    MaYcMoKhoa = null
                };

                await db.DiemDanhs.AddAsync(attendance);
                await db.SaveChangesAsync();
                addedAttendanceId = attendance.MaDiemDanh;
            }

            var now = DateTime.UtcNow;
            session.TrangThaiBuoi = sessionStatus;
            session.TrangThaiDiemDanh = attendanceStatus;
            session.DiemDanhBatDauLuc = attendanceStatus == "chua_mo" ? null : now.AddMinutes(-30);
            session.DiemDanhHanGuiLuc = dueSendAt;
            session.DiemDanhDaGuiLuc = attendanceStatus is "da_gui" or "da_khoa" ? now.AddMinutes(-20) : null;
            session.DiemDanhHanChinhSuaLuc = dueEditAt;
            session.DiemDanhKhoaLuc = attendanceStatus == "da_khoa" ? now.AddMinutes(-1) : null;
            session.KhoaLuc = null;
            session.NgayCapNhat = now;

            await db.SaveChangesAsync();

            return new AutomationSessionContext(
                session.MaBuoiHoc,
                session.MaGiaoVienDayThay ?? session.MaGiaoVien,
                originalSession,
                originalAttendanceLocks,
                addedAttendanceId,
                GetConnectionString());
        }

        Assert.Inconclusive("Không tìm được BuoiHoc có dữ liệu khóa học/sinh viên phù hợp để test automation.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static string GetConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var root = FindRepositoryRoot();
        foreach (var relativePath in new[]
                 {
                     Path.Combine("Backend", "appsettings.Development.json"),
                     Path.Combine("Backend", "appsettings.json")
                 })
        {
            var path = Path.Combine(root, relativePath);
            if (!File.Exists(path))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection) &&
                defaultConnection.ValueKind == JsonValueKind.String)
            {
                var value = defaultConnection.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        Assert.Fail("Không tìm thấy ConnectionStrings:DefaultConnection để test EF helper.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Source Code.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Không tìm thấy repo root từ thư mục test output.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
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
            password = DefaultPassword
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetRequiredString(root.RootElement, "accessToken");
    }

    private sealed record AutomationRunResult(
        int AutoSubmitted,
        int AutoLocked,
        IReadOnlyList<AutomationRunItem> Items);

    private sealed record AutomationRunItem(
        int MaBuoiHoc,
        string Action,
        string Status);

    private sealed record AttendanceLockState(
        int MaDiemDanh,
        DateTime? KhoaLuc);

    private sealed record OriginalSessionState(
        string TrangThaiBuoi,
        string TrangThaiDiemDanh,
        DateTime? DiemDanhBatDauLuc,
        DateTime? DiemDanhHanGuiLuc,
        DateTime? DiemDanhDaGuiLuc,
        DateTime? DiemDanhHanChinhSuaLuc,
        DateTime? DiemDanhKhoaLuc,
        DateTime? KhoaLuc,
        DateTime? NgayCapNhat)
    {
        public static OriginalSessionState From(BuoiHoc session)
        {
            return new OriginalSessionState(
                session.TrangThaiBuoi,
                session.TrangThaiDiemDanh,
                session.DiemDanhBatDauLuc,
                session.DiemDanhHanGuiLuc,
                session.DiemDanhDaGuiLuc,
                session.DiemDanhHanChinhSuaLuc,
                session.DiemDanhKhoaLuc,
                session.KhoaLuc,
                session.NgayCapNhat);
        }
    }

    private sealed record AutomationSessionContext(
        int SessionId,
        int EffectiveTeacherId,
        OriginalSessionState OriginalSession,
        IReadOnlyList<AttendanceLockState> OriginalAttendanceLocks,
        int? AddedAttendanceId,
        string ConnectionString)
    {
        public async Task RestoreAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var db = new ApplicationDbContext(options);
            var session = await db.BuoiHocs.FirstOrDefaultAsync(x => x.MaBuoiHoc == SessionId);
            if (session is not null)
            {
                session.TrangThaiBuoi = OriginalSession.TrangThaiBuoi;
                session.TrangThaiDiemDanh = OriginalSession.TrangThaiDiemDanh;
                session.DiemDanhBatDauLuc = OriginalSession.DiemDanhBatDauLuc;
                session.DiemDanhHanGuiLuc = OriginalSession.DiemDanhHanGuiLuc;
                session.DiemDanhDaGuiLuc = OriginalSession.DiemDanhDaGuiLuc;
                session.DiemDanhHanChinhSuaLuc = OriginalSession.DiemDanhHanChinhSuaLuc;
                session.DiemDanhKhoaLuc = OriginalSession.DiemDanhKhoaLuc;
                session.KhoaLuc = OriginalSession.KhoaLuc;
                session.NgayCapNhat = OriginalSession.NgayCapNhat;
            }

            if (AddedAttendanceId.HasValue)
            {
                var added = await db.DiemDanhs.FirstOrDefaultAsync(x => x.MaDiemDanh == AddedAttendanceId.Value);
                if (added is not null)
                {
                    db.DiemDanhs.Remove(added);
                }
            }

            foreach (var state in OriginalAttendanceLocks)
            {
                var attendance = await db.DiemDanhs.FirstOrDefaultAsync(x => x.MaDiemDanh == state.MaDiemDanh);
                if (attendance is not null)
                {
                    attendance.KhoaLuc = state.KhoaLuc;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}

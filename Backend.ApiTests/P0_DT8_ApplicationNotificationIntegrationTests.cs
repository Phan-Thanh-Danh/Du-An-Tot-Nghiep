using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Audit;
using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Models;
using Backend.Services.Applications;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT8_ApplicationNotificationIntegrationTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string TestPrefix = "NUnit P0-DT8";

    [OneTimeSetUp]
    public void ValidateP0Dt8Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
    }

    [Test]
    public async Task SubmitApplication_ShouldCreateNotificationForStudent()
    {
        var student = await GetUserAsync(StudentEmail);
        var app = await CreateApplicationAsync(ApplicationStatuses.Draft, student.MaNguoiDung, student.MaDonVi);
        using var client = await CreateAuthenticatedClientAsync(StudentEmail);

        using var response = await client.PostAsJsonAsync($"api/student/applications/{app.MaDonTu}/submit", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var notification = await GetNotificationAsync(app.MaDonTu, student.MaNguoiDung, "Đơn từ đã được nộp");
        Assert.Multiple(() =>
        {
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification!.ThongBao!.LoaiDoiTuongLienKet, Is.EqualTo("DonTu"));
            Assert.That(notification.ThongBao.MaDoiTuongLienKet, Is.EqualTo(app.MaDonTu));
            Assert.That(notification.ThongBao.DuongDan, Is.EqualTo($"/student/applications/{app.MaDonTu}"));
        });
    }

    [Test]
    public async Task RequestSupplement_ShouldNotifyStudentWithPublicMessageOnly()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
        {
            request = "Vui lòng bổ sung thông tin công khai.",
            internalNote = "Ghi chú nội bộ DT8 tuyệt mật.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var notification = await GetNotificationAsync(app.MaDonTu, app.MaHocSinh, "Đơn từ cần bổ sung");
        Assert.Multiple(() =>
        {
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification!.ThongBao!.NoiDungText, Does.Contain("Vui lòng bổ sung thông tin công khai."));
            Assert.That(notification.ThongBao.NoiDungText, Does.Not.Contain("Ghi chú nội bộ DT8 tuyệt mật."));
            Assert.That(notification.ThongBao.NoiDungJson, Does.Not.Contain("Ghi chú nội bộ DT8 tuyệt mật."));
        });
    }

    [Test]
    public async Task Approve_ShouldCreateLinkedNotificationVisibleInStudentInbox()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var student = await CreateAuthenticatedClientAsync(StudentEmail);
        using var inbox = await student.GetAsync($"api/notifications/me?keyword={Uri.EscapeDataString("Đơn từ đã được duyệt")}&pageIndex=1&pageSize=20");
        Assert.That(inbox.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(inbox));
        using var root = await GetRootAsync(inbox);
        var item = GetDataItems(root.RootElement).EnumerateArray()
            .FirstOrDefault(x => GetInt32(x, "maDoiTuongLienKet") == app.MaDonTu);

        Assert.Multiple(() =>
        {
            Assert.That(item.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined));
            Assert.That(GetRequiredString(item, "loaiDoiTuongLienKet"), Is.EqualTo("DonTu"));
            Assert.That(GetRequiredString(item, "duongDan"), Is.EqualTo($"/student/applications/{app.MaDonTu}"));
        });
    }

    [Test]
    public async Task Reject_ShouldNotifyPublicReasonOnly()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new
        {
            reason = "Không đáp ứng điều kiện xử lý.",
            internalNote = "Không được lộ ghi chú nội bộ DT8.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var notification = await GetNotificationAsync(app.MaDonTu, app.MaHocSinh, "Đơn từ bị từ chối");
        Assert.Multiple(() =>
        {
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification!.ThongBao!.NoiDungText, Does.Contain("Không đáp ứng điều kiện xử lý."));
            Assert.That(notification.ThongBao.NoiDungText, Does.Not.Contain("Không được lộ"));
            Assert.That(notification.ThongBao.NoiDungJson, Does.Not.Contain("Không được lộ"));
        });
    }

    [Test]
    public async Task Cancel_ShouldCreateStudentNotification()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.Submitted);
        using var student = await CreateAuthenticatedClientAsync(StudentEmail);

        using var response = await student.PostAsJsonAsync($"api/student/applications/{app.MaDonTu}/cancel", new
        {
            lyDo = "Hủy đơn trong test DT8.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        Assert.That(await GetNotificationAsync(app.MaDonTu, app.MaHocSinh, "Đơn từ đã bị hủy"), Is.Not.Null);
    }

    [Test]
    public async Task ProcessRecorded_ShouldCreateProcessingNotification()
    {
        var app = await CreateApplicationForSeedStudentAsync(
            ApplicationStatuses.Approved,
            processingStatus: ApplicationProcessingStatuses.Pending,
            type: ApplicationTypes.Confirmation);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        Assert.That(await GetNotificationAsync(app.MaDonTu, app.MaHocSinh, "Đơn từ đã được ghi nhận"), Is.Not.Null);
    }

    [Test]
    public async Task ProcessManualRequired_ShouldCreateManualRequiredNotification()
    {
        var app = await CreateApplicationForSeedStudentAsync(
            ApplicationStatuses.Approved,
            processingStatus: ApplicationProcessingStatuses.Pending,
            type: ApplicationTypes.Other,
            formJson: "{\"noi_dung\":\"Can xu ly thu cong\"}");

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/process", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        Assert.That(await GetNotificationAsync(app.MaDonTu, app.MaHocSinh, "Đơn từ cần xử lý thủ công"), Is.Not.Null);
    }

    [Test]
    public async Task OtherUser_ShouldNotSeeStudentApplicationNotification()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var approve = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
        Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(approve));

        using var teacher = await CreateAuthenticatedClientAsync(TeacherEmail);
        using var inbox = await teacher.GetAsync($"api/notifications/me?keyword={Uri.EscapeDataString("Đơn từ đã được duyệt")}&pageIndex=1&pageSize=50");

        Assert.That(inbox.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(inbox));
        using var root = await GetRootAsync(inbox);
        var visible = GetDataItems(root.RootElement).EnumerateArray()
            .Any(x => GetInt32(x, "maDoiTuongLienKet") == app.MaDonTu);
        Assert.That(visible, Is.False);
    }

    [Test]
    public async Task ApplicationNotificationService_ShouldDeduplicateSameEvent()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.Approved);
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == app.MaDonTu);
        var service = CreateApplicationNotificationService(db);

        await service.NotifyApprovedAsync(application);
        await service.NotifyApprovedAsync(application);

        var count = await db.ThongBaoNguoiNhans.CountAsync(x =>
            x.MaNguoiNhan == app.MaHocSinh &&
            x.ThongBao != null &&
            x.ThongBao.MaDoiTuongLienKet == app.MaDonTu &&
            x.ThongBao.TieuDe == "Đơn từ đã được duyệt");
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task NotificationFailure_ShouldNotThrowToBusinessFlow()
    {
        var app = await CreateApplicationForSeedStudentAsync(ApplicationStatuses.Approved);
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == app.MaDonTu);
        var service = new ApplicationNotificationService(
            db,
            new ThrowingNotificationService(),
            NullLogger<ApplicationNotificationService>.Instance);

        Assert.DoesNotThrowAsync(async () => await service.NotifyApprovedAsync(application));
    }

    private static ApplicationNotificationService CreateApplicationNotificationService(ApplicationDbContext db)
    {
        var httpContextAccessor = new HttpContextAccessor();
        var notificationService = new NotificationService(db, httpContextAccessor, new NoOpAuditLogService());
        return new ApplicationNotificationService(db, notificationService, NullLogger<ApplicationNotificationService>.Instance);
    }

    private static async Task<ApplicationSnapshot> CreateApplicationForSeedStudentAsync(
        string status,
        int? assigneeId = null,
        string processingStatus = ApplicationProcessingStatuses.NotProcessed,
        string type = ApplicationTypes.Confirmation,
        string? formJson = null)
    {
        var student = await GetUserAsync(StudentEmail);
        return await CreateApplicationAsync(status, student.MaNguoiDung, student.MaDonVi, assigneeId, processingStatus, type, formJson);
    }

    private static async Task<ApplicationSnapshot> CreateApplicationAsync(
        string status,
        int studentId,
        int campusId,
        int? assigneeId = null,
        string processingStatus = ApplicationProcessingStatuses.NotProcessed,
        string type = ApplicationTypes.Confirmation,
        string? formJson = null)
    {
        await using var db = CreateDbContext();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaHocSinh = studentId,
            MaDonVi = campusId,
            MaMauDon = templateId,
            LoaiDon = type,
            TieuDe = $"{TestPrefix} {status} {Guid.NewGuid():N}",
            DuLieuBieuMau = formJson ?? "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = processingStatus,
            NguoiDuyetHienTai = assigneeId,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = status == ApplicationStatuses.Draft ? null : now.AddMinutes(-30),
            NgayDuyet = status == ApplicationStatuses.Approved ? now.AddMinutes(-5) : null,
            HanXuLyLuc = now.AddHours(48)
        };
        db.DonTus.Add(application);
        await db.SaveChangesAsync();
        return new ApplicationSnapshot(
            application.MaDonTu,
            application.MaHocSinh,
            application.MaDonVi,
            Convert.ToBase64String(application.RowVersion));
    }

    private static async Task<ThongBaoNguoiNhan?> GetNotificationAsync(int applicationId, int studentId, string title)
    {
        await using var db = CreateDbContext();
        return await db.ThongBaoNguoiNhans
            .AsNoTracking()
            .Include(x => x.ThongBao)
            .Where(x =>
                x.MaNguoiNhan == studentId &&
                x.ThongBao != null &&
                x.ThongBao.LoaiDoiTuongLienKet == "DonTu" &&
                x.ThongBao.MaDoiTuongLienKet == applicationId &&
                x.ThongBao.TieuDe == title)
            .OrderByDescending(x => x.MaThongBaoNguoiNhan)
            .FirstOrDefaultAsync();
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
            .SingleAsync();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed record ApplicationSnapshot(int MaDonTu, int MaHocSinh, int MaDonVi, string RowVersion);
    private sealed record UserRecord(int MaNguoiDung, int MaDonVi);

    private sealed class NoOpAuditLogService : IAuditLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, object? oldValue, object? newValue, int? changedBy, int? maDonVi, string? description, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(int campusId, string entityName, int entityId, string action, int actorUserId, object? oldValue, object? newValue, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PagedResultDto<AuditLogListItemDto>> GetAsync(AuditLogQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AuditLogDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ThrowingNotificationService : INotificationService
    {
        public Task<PagedResultDto<NotificationDto>> GetMyNotificationsAsync(NotificationQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<NotificationDetailDto> GetMyNotificationDetailAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<UnreadCountDto> GetMyUnreadCountAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<NotificationDto> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<UnreadCountDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task HideAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<NotificationRecipientPreviewResultDto> PreviewRecipientsAsync(PreviewNotificationRecipientsRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AdminNotificationDto> CreateManualNotificationAsync(CreateManualNotificationRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PagedResultDto<AdminNotificationDto>> GetAdminNotificationsAsync(AdminNotificationQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AdminNotificationDto> GetAdminNotificationDetailAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PagedResultDto<AdminNotificationRecipientDto>> GetAdminNotificationRecipientsAsync(int notificationId, AdminNotificationRecipientQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<NotificationStatisticsDto> GetAdminNotificationStatisticsAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<AdminNotificationDto> CancelNotificationAsync(int notificationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task CreateSystemNotificationAsync(SystemNotificationRequest request, IReadOnlyCollection<int> recipientIds, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Simulated notification failure.");
        public Task SendToUsersAsync(SystemNotificationRequest request, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Simulated notification failure.");
        public Task SendToClassAsync(SystemNotificationRequest request, int classId, IReadOnlyCollection<int>? additionalUserIds = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SendToCourseAsync(SystemNotificationRequest request, int courseId, IReadOnlyCollection<int>? additionalUserIds = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SendToCampusAsync(SystemNotificationRequest request, int organizationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

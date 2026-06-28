using System.Text.Encodings.Web;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Notifications;
using Backend.Models;
using Backend.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationNotificationService : IApplicationNotificationService
{
    private const string LinkedEntityType = "DonTu";
    private const string RoutePrefix = "/student/applications/";

    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ApplicationNotificationService> _logger;

    public ApplicationNotificationService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<ApplicationNotificationService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public Task NotifySubmittedAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_submitted",
            "Đơn từ đã được nộp",
            "Đơn của bạn đã được nộp thành công.",
            "Đơn của bạn đã được nộp thành công và đang chờ tiếp nhận xử lý.",
            NotificationConstants.Levels.Info,
            cancellationToken);
    }

    public Task NotifyAssignedAsync(DonTu application, int? assigneeId, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_assigned",
            "Đơn từ đang được xử lý",
            "Đơn của bạn đang được xử lý.",
            "Đơn của bạn đã được tiếp nhận và đang được xử lý.",
            NotificationConstants.Levels.Info,
            cancellationToken);
    }

    public Task NotifySupplementRequestedAsync(DonTu application, string? publicMessage, CancellationToken cancellationToken = default)
    {
        var text = string.IsNullOrWhiteSpace(publicMessage)
            ? "Đơn của bạn cần bổ sung thông tin."
            : $"Đơn của bạn cần bổ sung thông tin: {publicMessage.Trim()}";
        return NotifyStudentAsync(
            application,
            "application_supplement_requested",
            "Đơn từ cần bổ sung",
            "Đơn của bạn cần bổ sung thông tin.",
            text,
            NotificationConstants.Levels.Important,
            cancellationToken);
    }

    public Task NotifyApprovedAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_approved",
            "Đơn từ đã được duyệt",
            "Đơn của bạn đã được duyệt.",
            "Đơn của bạn đã được duyệt và sẽ tiếp tục được xử lý nghiệp vụ nếu cần.",
            NotificationConstants.Levels.Important,
            cancellationToken);
    }

    public Task NotifyRejectedAsync(DonTu application, string? publicReason, CancellationToken cancellationToken = default)
    {
        var text = string.IsNullOrWhiteSpace(publicReason)
            ? "Đơn của bạn đã bị từ chối."
            : $"Đơn của bạn đã bị từ chối. Lý do: {publicReason.Trim()}";
        return NotifyStudentAsync(
            application,
            "application_rejected",
            "Đơn từ bị từ chối",
            "Đơn của bạn đã bị từ chối.",
            text,
            NotificationConstants.Levels.Important,
            cancellationToken);
    }

    public Task NotifyCancelledAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_cancelled",
            "Đơn từ đã bị hủy",
            "Đơn của bạn đã bị hủy.",
            "Đơn của bạn đã bị hủy.",
            NotificationConstants.Levels.Important,
            cancellationToken);
    }

    public Task NotifyProcessingSucceededAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_processing_succeeded",
            "Đơn từ đã xử lý nghiệp vụ thành công",
            "Đơn của bạn đã xử lý nghiệp vụ thành công.",
            "Đơn của bạn đã được xử lý nghiệp vụ thành công.",
            NotificationConstants.Levels.Info,
            cancellationToken);
    }

    public Task NotifyProcessingFailedAsync(DonTu application, string? publicMessage, CancellationToken cancellationToken = default)
    {
        var text = string.IsNullOrWhiteSpace(publicMessage)
            ? "Đơn của bạn xử lý nghiệp vụ chưa thành công."
            : publicMessage.Trim();
        return NotifyStudentAsync(
            application,
            "application_processing_failed",
            "Đơn từ xử lý nghiệp vụ thất bại",
            "Đơn của bạn xử lý nghiệp vụ chưa thành công.",
            text,
            NotificationConstants.Levels.Warning,
            cancellationToken);
    }

    public Task NotifyManualProcessingRequiredAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_manual_processing_required",
            "Đơn từ cần xử lý thủ công",
            "Đơn của bạn cần xử lý thủ công.",
            "Đơn của bạn đã được chuyển sang bộ phận phụ trách để xử lý thủ công.",
            NotificationConstants.Levels.Important,
            cancellationToken);
    }

    public Task NotifyProcessingRecordedAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        return NotifyStudentAsync(
            application,
            "application_processing_recorded",
            "Đơn từ đã được ghi nhận",
            "Kết quả xử lý đơn của bạn đã được ghi nhận.",
            "Kết quả xử lý đơn của bạn đã được ghi nhận.",
            NotificationConstants.Levels.Info,
            cancellationToken);
    }

    private async Task NotifyStudentAsync(
        DonTu application,
        string eventType,
        string title,
        string summary,
        string body,
        string level,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await AlreadySentAsync(application.MaDonTu, application.MaHocSinh, title, cancellationToken))
            {
                return;
            }

            var request = new SystemNotificationRequest
            {
                TieuDe = title,
                TomTat = summary,
                NoiDungText = body,
                NoiDungJson = BuildEditorJson(eventType, application, body),
                LoaiThongBao = NotificationConstants.Types.System,
                MucDo = level,
                DoiTuongLienKet = LinkedEntityType,
                MaDoiTuongLienKet = application.MaDonTu,
                DuongDan = RoutePrefix + application.MaDonTu,
                MaDonVi = application.MaDonVi,
                NguoiTao = null
            };

            await _notificationService.SendToUsersAsync(request, [application.MaHocSinh], cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Không gửi được thông báo đơn từ. MaDonTu={MaDonTu}, EventType={EventType}",
                application.MaDonTu,
                eventType);
        }
    }

    private async Task<bool> AlreadySentAsync(
        int applicationId,
        int studentId,
        string title,
        CancellationToken cancellationToken)
    {
        return await _context.ThongBaoNguoiNhans.AsNoTracking()
            .AnyAsync(x =>
                x.MaNguoiNhan == studentId &&
                x.ThongBao != null &&
                x.ThongBao.LoaiThongBao == NotificationConstants.Types.System &&
                x.ThongBao.LoaiDoiTuongLienKet == LinkedEntityType &&
                x.ThongBao.MaDoiTuongLienKet == applicationId &&
                x.ThongBao.TieuDe == title,
                cancellationToken);
    }

    private static string BuildEditorJson(string eventType, DonTu application, string body)
    {
        var payload = new
        {
            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            version = "2.30.0",
            eventType,
            application = new
            {
                maDonTu = application.MaDonTu,
                loaiDon = application.LoaiDon,
                trangThaiMoi = application.TrangThai,
                tieuDe = application.TieuDe,
                duongDan = RoutePrefix + application.MaDonTu
            },
            blocks = new object[]
            {
                new
                {
                    id = "application-status",
                    type = "paragraph",
                    data = new
                    {
                        text = body
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}

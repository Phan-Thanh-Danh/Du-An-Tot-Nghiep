using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Constants;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<AdminNotificationDto>>> Create(
        [FromBody] CreateManualNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.CreateManualNotificationAsync(request, cancellationToken);
        return Ok(ApiResponseDto<AdminNotificationDto>.Ok(result, "Tạo thông báo thủ công thành công."));
    }

    [HttpPost("preview-recipients")]
    public async Task<ActionResult<ApiResponseDto<NotificationRecipientPreviewResultDto>>> PreviewRecipients(
        [FromBody] PreviewNotificationRecipientsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.PreviewRecipientsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<NotificationRecipientPreviewResultDto>.Ok(result, "Xem trước người nhận thông báo thành công."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AdminNotificationDto>>>> Get(
        [FromQuery] AdminNotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetAdminNotificationsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AdminNotificationDto>>.Ok(result, "Lấy danh sách thông báo quản trị thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AdminNotificationDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetAdminNotificationDetailAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AdminNotificationDto>.Ok(result, "Lấy chi tiết thông báo quản trị thành công."));
    }

    [HttpGet("{id:int}/recipients")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AdminNotificationRecipientDto>>>> GetRecipients(
        int id,
        [FromQuery] AdminNotificationRecipientQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetAdminNotificationRecipientsAsync(id, parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AdminNotificationRecipientDto>>.Ok(result, "Lấy danh sách người nhận thông báo thành công."));
    }

    [HttpGet("{id:int}/statistics")]
    public async Task<ActionResult<ApiResponseDto<NotificationStatisticsDto>>> GetStatistics(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetAdminNotificationStatisticsAsync(id, cancellationToken);
        return Ok(ApiResponseDto<NotificationStatisticsDto>.Ok(result, "Lấy thống kê thông báo thành công."));
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<AdminNotificationDto>>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.CancelNotificationAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AdminNotificationDto>.Ok(result, "Hủy thông báo thành công."));
    }

    [HttpPost("trigger-due-reminders")]
    public async Task<ActionResult<ApiResponseDto<int>>> TriggerDueReminders(
        [FromServices] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddHours(24);

        var dueAssignments = await context.BaiTaps
            .Where(b => b.TrangThai == "da_xuat_ban" && b.HanNop > now && b.HanNop <= threshold)
            .ToListAsync(cancellationToken);

        int sentCount = 0;

        foreach (var assignment in dueAssignments)
        {
            var classIds = await context.KhoaHocs
                .Where(k => k.MaMonHoc == assignment.MaMonHoc)
                .Select(k => k.MaLop)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (classIds.Count == 0) continue;

            var studentIds = await context.NguoiDungs
                .Where(n => n.VaiTroChinh == AuthRoles.Student && n.MaLop.HasValue && classIds.Contains(n.MaLop.Value))
                .Select(n => n.MaNguoiDung)
                .ToListAsync(cancellationToken);

            if (studentIds.Count == 0) continue;

            var submittedStudentIds = await context.BaiNops
                .Where(b => b.MaBaiTap == assignment.MaBaiTap)
                .Select(b => b.MaHocSinh)
                .ToListAsync(cancellationToken);

            var pendingStudentIds = studentIds.Except(submittedStudentIds).ToList();

            if (pendingStudentIds.Count != 0)
            {
                bool alreadySent = await context.ThongBaos
                    .AnyAsync(t => t.LoaiSuKien == "academic.assignment.due_reminder" && 
                                   t.MaDoiTuongLienKet == assignment.MaBaiTap &&
                                   t.NgayTao >= now.AddDays(-2), cancellationToken);

                if (!alreadySent)
                {
                    await _notificationService.SendToUsersAsync(new SystemNotificationRequest
                    {
                        LoaiSuKien = "academic.assignment.due_reminder",
                        LoaiThongBao = "system",
                        TieuDe = "Bài tập sắp hết hạn",
                        TomTat = $"Bài tập {assignment.TieuDe} sắp hết hạn",
                        NoiDungText = $"Bài tập {assignment.TieuDe} sẽ hết hạn vào {assignment.HanNop:dd/MM/yyyy HH:mm}. Vui lòng nộp bài đúng hạn.",
                        MucDo = "warning",
                        DoiTuongLienKet = "assignment",
                        MaDoiTuongLienKet = assignment.MaBaiTap,
                        DuongDan = $"/student/assignments/{assignment.MaBaiTap}"
                    }, pendingStudentIds, cancellationToken);

                    sentCount += pendingStudentIds.Count;
                }
            }
        }

        return Ok(ApiResponseDto<int>.Ok(sentCount, $"Đã gửi {sentCount} thông báo nhắc nhở bài tập."));
    }
}

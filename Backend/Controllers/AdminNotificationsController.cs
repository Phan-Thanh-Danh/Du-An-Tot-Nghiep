using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Constants;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff}")]
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
}

using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<NotificationDto>>>> Get(
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetMyNotificationsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<NotificationDto>>.Ok(result, "Lấy danh sách thông báo thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<NotificationDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetMyNotificationDetailAsync(id, cancellationToken);
        return Ok(ApiResponseDto<NotificationDetailDto>.Ok(result, "Lấy chi tiết thông báo thành công."));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponseDto<UnreadCountDto>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetMyUnreadCountAsync(cancellationToken);
        return Ok(ApiResponseDto<UnreadCountDto>.Ok(result, "Lấy số thông báo chưa đọc thành công."));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponseDto<NotificationDto>>> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return Ok(ApiResponseDto<NotificationDto>.Ok(result, "Đánh dấu thông báo đã đọc thành công."));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponseDto<UnreadCountDto>>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAllAsReadAsync(cancellationToken);
        return Ok(ApiResponseDto<UnreadCountDto>.Ok(result, "Đánh dấu tất cả thông báo đã đọc thành công."));
    }
}

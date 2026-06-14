using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize]
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
}

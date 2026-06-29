using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.Services.Notification;
using Backend.Services.Applications;
using Backend.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Constants;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/specialized-notifications")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminSpecializedNotificationsController : ControllerBase
{
    private readonly ISpecializedNotificationService _service;

    public AdminSpecializedNotificationsController(ISpecializedNotificationService service)
    {
        _service = service;
    }

    private ApplicationActorContext GetCurrentActor()
    {
        if (HttpContext.Items["CurrentUser"] is ApplicationActorContext actor)
        {
            return actor;
        }
        throw new ApiException(401, "Không tìm thấy thông tin xác thực.");
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponseDto<SpecializedNotificationCategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var data = await _service.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponseDto<SpecializedNotificationCategoryDto>.Ok(data));
    }

    [HttpPost("preview-recipients")]
    public async Task<ActionResult<ApiResponseDto<PreviewSpecializedRecipientsResultDto>>> PreviewRecipients(
        [FromBody] PreviewSpecializedRecipientsRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _service.PreviewRecipientsAsync(request, actor, cancellationToken);
        return Ok(new ApiResponseDto<PreviewSpecializedRecipientsResultDto>
        {
            Success = true,
            Data = data
        });
    }

    [HttpPost("tuition")]
    public async Task<ActionResult<ApiResponseDto<SpecializedNotificationSendResultDto>>> SendTuitionNotification(
        [FromBody] SendTuitionNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var result = await _service.SendTuitionNotificationAsync(request, actor, cancellationToken);
        return Ok(new ApiResponseDto<SpecializedNotificationSendResultDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = result
        });
    }

    [HttpPost("academic")]
    public async Task<ActionResult<ApiResponseDto<SpecializedNotificationSendResultDto>>> SendAcademicNotification(
        [FromBody] SendAcademicNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var result = await _service.SendAcademicNotificationAsync(request, actor, cancellationToken);
        return Ok(new ApiResponseDto<SpecializedNotificationSendResultDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = result
        });
    }

    [HttpPost("urgent")]
    public async Task<ActionResult<ApiResponseDto<SpecializedNotificationSendResultDto>>> SendUrgentNotification(
        [FromBody] SendUrgentNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var result = await _service.SendUrgentNotificationAsync(request, actor, cancellationToken);
        return Ok(new ApiResponseDto<SpecializedNotificationSendResultDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = result
        });
    }

    [HttpPost("maintenance")]
    public async Task<ActionResult<ApiResponseDto<SpecializedNotificationSendResultDto>>> SendMaintenanceNotification(
        [FromBody] SendMaintenanceNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var result = await _service.SendMaintenanceNotificationAsync(request, actor, cancellationToken);
        return Ok(new ApiResponseDto<SpecializedNotificationSendResultDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = result
        });
    }
}

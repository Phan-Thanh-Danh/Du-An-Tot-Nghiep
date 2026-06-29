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
[Route("api/admin/notification-templates")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminNotificationTemplatesController : ControllerBase
{
    private readonly INotificationTemplateService _templateService;

    public AdminNotificationTemplatesController(INotificationTemplateService templateService)
    {
        _templateService = templateService;
    }

    private ApplicationActorContext GetCurrentActor()
    {
        if (HttpContext.Items["CurrentUser"] is ApplicationActorContext actor)
        {
            return actor;
        }
        throw new ApiException(401, "Không tìm thấy thông tin xác thực.");
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<NotificationTemplateListItemDto>>>> GetTemplates(
        [FromQuery] NotificationTemplateQueryParameters query,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _templateService.GetTemplatesAsync(query, actor, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<NotificationTemplateListItemDto>>.Ok(data));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateDetailDto>>> GetTemplateDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _templateService.GetTemplateDetailAsync(id, actor, cancellationToken);
        return Ok(new ApiResponseDto<NotificationTemplateDetailDto>
        {
            Success = true,
            Data = data
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateDetailDto>>> CreateTemplate(
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _templateService.CreateTemplateAsync(request, actor, cancellationToken);
        return CreatedAtAction(nameof(GetTemplateDetail), new { id = data.MaMauThongBao }, new ApiResponseDto<NotificationTemplateDetailDto>
        {
            Success = true,
            Message = "Tạo mẫu thông báo thành công.",
            Data = data
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<NotificationTemplateDetailDto>>> UpdateTemplate(
        int id,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _templateService.UpdateTemplateAsync(id, request, actor, cancellationToken);
        return Ok(new ApiResponseDto<NotificationTemplateDetailDto>
        {
            Success = true,
            Message = "Cập nhật mẫu thông báo thành công.",
            Data = data
        });
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponseDto<object>>> ActivateTemplate(
        int id,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        await _templateService.ActivateTemplateAsync(id, actor, cancellationToken);
        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Đã bật mẫu thông báo."
        });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeactivateTemplate(
        int id,
        [FromBody] DeactivateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        await _templateService.DeactivateTemplateAsync(id, request, actor, cancellationToken);
        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Đã tắt mẫu thông báo."
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteTemplate(
        int id,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        await _templateService.DeleteTemplateAsync(id, actor, cancellationToken);
        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Đã xử lý mẫu thông báo."
        });
    }

    [HttpPost("{id}/preview")]
    public async Task<ActionResult<ApiResponseDto<PreviewNotificationTemplateResultDto>>> PreviewTemplate(
        int id,
        [FromBody] PreviewNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var actor = GetCurrentActor();
        var data = await _templateService.PreviewTemplateAsync(id, request, actor, cancellationToken);
        return Ok(new ApiResponseDto<PreviewNotificationTemplateResultDto>
        {
            Success = true,
            Data = data
        });
    }
}

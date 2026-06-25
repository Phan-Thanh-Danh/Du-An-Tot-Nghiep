using Backend.Constants;
using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/applications")]
[Authorize(Policy = AuthPolicies.ApplicationQueueRead)]
public class AdminApplicationsController : ControllerBase
{
    private readonly IApplicationAdminQueueService _queueService;
    private readonly IApplicationAssignmentService _assignmentService;
    private readonly IApplicationAdminEvidenceService _evidenceService;

    public AdminApplicationsController(
        IApplicationAdminQueueService queueService,
        IApplicationAssignmentService assignmentService,
        IApplicationAdminEvidenceService evidenceService)
    {
        _queueService = queueService;
        _assignmentService = assignmentService;
        _evidenceService = evidenceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationQueueResponseDto>>> GetQueue(
        [FromQuery] AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _queueService.GetQueueAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationQueueResponseDto>.Ok(result, "Lấy hàng đợi đơn từ thành công."));
    }

    [HttpGet("queue-summary")]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationQueueSummaryDto>>> GetQueueSummary(
        [FromQuery] AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _queueService.GetQueueSummaryAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationQueueSummaryDto>.Ok(result, "Lấy tổng quan hàng đợi đơn từ thành công."));
    }

    [HttpGet("assignees")]
    [Authorize(Policy = AuthPolicies.ApplicationAssignmentManage)]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationAssigneeResponseDto>>> GetAssignees(
        [FromQuery] AdminApplicationAssigneeQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _queueService.GetAssigneesAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationAssigneeResponseDto>.Ok(result, "Lấy danh sách người xử lý đơn từ thành công."));
    }

    [HttpGet("{applicationId:int}")]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationDetailDto>>> GetDetail(
        int applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _queueService.GetDetailAsync(applicationId, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationDetailDto>.Ok(result, "Lấy chi tiết đơn từ thành công."));
    }

    [HttpPost("{applicationId:int}/receive")]
    [Authorize(Policy = AuthPolicies.ApplicationReceive)]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationDetailDto>>> Receive(
        int applicationId,
        AdminApplicationReceiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assignmentService.ReceiveAsync(applicationId, request, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationDetailDto>.Ok(result, "Tiếp nhận đơn từ thành công."));
    }

    [HttpPost("{applicationId:int}/assign")]
    [Authorize(Policy = AuthPolicies.ApplicationAssignmentManage)]
    public async Task<ActionResult<ApiResponseDto<AdminApplicationDetailDto>>> Assign(
        int applicationId,
        AdminApplicationAssignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assignmentService.AssignAsync(applicationId, request, cancellationToken);
        return Ok(ApiResponseDto<AdminApplicationDetailDto>.Ok(result, "Phân công đơn từ thành công."));
    }

    [HttpGet("{applicationId:int}/attachments/{attachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _evidenceService.DownloadAsync(applicationId, attachmentId, cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(result.Content, result.ContentType, result.FileName, enableRangeProcessing: false);
    }
}

using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/discipline-records")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminDisciplineRecordsController : ControllerBase
{
    private readonly IRewardDisciplineNotificationService _notificationService;
    private readonly IDisciplineRecordService _service;

    public AdminDisciplineRecordsController(IDisciplineRecordService service)
    {
        _notificationService = HttpContext.RequestServices.GetRequiredService<IRewardDisciplineNotificationService>();
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DisciplineRecordListItemDto>>> GetRecords(
        [FromQuery] DisciplineRecordQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDisciplineRecordsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DisciplineRecordDetailDto>> GetRecordDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDisciplineRecordDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DisciplineRecordResultDto>> CreateRecord(
        [FromBody] CreateDisciplineRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateDisciplineRecordAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRecordDetail), new { id = result.MaHoSoKyLuat }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DisciplineRecordResultDto>> UpdateRecord(
        int id,
        [FromBody] UpdateDisciplineRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<DisciplineRecordResultDto>> SubmitRecord(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.SubmitDisciplineRecordAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<DisciplineRecordResultDto>> CancelRecord(
        int id,
        [FromBody] CancelDisciplineRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CancelDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending-approval")]
    public async Task<ActionResult<PagedResultDto<DisciplineRecordListItemDto>>> GetPendingRecords(
        [FromQuery] DisciplineRecordQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPendingDisciplineRecordsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<DisciplineRecordResultDto>> ApproveRecord(
        int id,
        [FromBody] DisciplineApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ApproveDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<DisciplineRecordResultDto>> RejectRecord(
        int id,
        [FromBody] DisciplineRejectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RejectDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<DisciplineRecordResultDto>> ActivateRecord(
        int id,
        [FromBody] DisciplineActivateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ActivateDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/expire")]
    public async Task<ActionResult<DisciplineRecordResultDto>> ExpireRecord(
        int id,
        [FromBody] DisciplineExpireRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ExpireDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/remove-effect")]
    public async Task<ActionResult<DisciplineRecordResultDto>> RemoveEffect(
        int id,
        [FromBody] RemoveDisciplineEffectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RemoveDisciplineEffectAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/void-approved")]
    public async Task<ActionResult<DisciplineRecordResultDto>> VoidApproved(
        int id,
        [FromBody] VoidApprovedDisciplineRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.VoidApprovedDisciplineRecordAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/notifications/resend")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<IActionResult> ResendNotificationAsync(int id, [FromBody] ResendNotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 10)
            return BadRequest(new { Success = false, Message = "Lý do phải có ít nhất 10 ký tự." });
            
        await _notificationService.ResendDisciplineRecordNotificationAsync(id, request.Reason, cancellationToken);
        return Ok(new { Success = true, Message = "Đã gửi lại thông báo thành công." });
    }
}

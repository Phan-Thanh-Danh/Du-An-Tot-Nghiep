using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/rewards")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminRewardsController : ControllerBase
{
    private readonly IRewardDisciplineNotificationService _notificationService;
    private readonly IRewardLifecycleService _service;

    public AdminRewardsController(
        IRewardLifecycleService service,
        IRewardDisciplineNotificationService notificationService)
    {
        _service = service;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AdminRewardListItemDto>>> GetRewards(
        [FromQuery] AdminRewardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetRewardsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminRewardDetailDto>> GetRewardDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetRewardDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<RewardLifecycleResultDto>> CancelReward(
        int id,
        [FromBody] CancelRewardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CancelRewardAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/restore")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<RewardLifecycleResultDto>> RestoreReward(
        int id,
        [FromBody] RestoreRewardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RestoreRewardAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/mark-issued")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<RewardLifecycleResultDto>> MarkRewardIssued(
        int id,
        [FromBody] MarkRewardIssuedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.MarkIssuedAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/regenerate-certificate")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<RewardLifecycleResultDto>> RegenerateSingleRewardCertificate(
        int id,
        [FromBody] RegenerateSingleRewardCertificateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RegenerateCertificateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpPost("{rewardId}/notifications/resend")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<IActionResult> ResendNotificationAsync(int rewardId, [FromBody] ResendNotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 10)
            return BadRequest(new { Success = false, Message = "Lý do phải có ít nhất 10 ký tự." });
            
        await _notificationService.ResendRewardNotificationAsync(rewardId, request.Reason, cancellationToken);
        return Ok(new { Success = true, Message = "Đã gửi lại thông báo thành công." });
    }
}

public class ResendNotificationRequest { public string Reason { get; set; } = string.Empty; }

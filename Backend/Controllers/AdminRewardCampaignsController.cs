using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/reward-campaigns")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminRewardCampaignsController : ControllerBase
{
    private readonly IRewardCampaignService _rewardCampaignService;

    public AdminRewardCampaignsController(IRewardCampaignService rewardCampaignService)
    {
        _rewardCampaignService = rewardCampaignService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<RewardCampaignListItemDto>>>> Get(
        [FromQuery] RewardCampaignQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _rewardCampaignService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<RewardCampaignListItemDto>>.Ok(
            result,
            "Lấy danh sách đợt khen thưởng thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RewardCampaignDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _rewardCampaignService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RewardCampaignDetailDto>.Ok(
            result,
            "Lấy chi tiết đợt khen thưởng thành công."));
    }

    [HttpPost("top100")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardCampaignDetailDto>>> CreateTop100(
        CreateTop100RewardCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rewardCampaignService.CreateTop100Async(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.MaDotKhenThuong },
            ApiResponseDto<RewardCampaignDetailDto>.Ok(
                result,
                "Tạo đợt khen thưởng Top 100 thành công."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardCampaignDetailDto>>> Update(
        int id,
        UpdateRewardCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rewardCampaignService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<RewardCampaignDetailDto>.Ok(
            result,
            "Cập nhật đợt khen thưởng thành công."));
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardCampaignDetailDto>>> Cancel(
        int id,
        CancelRewardCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rewardCampaignService.CancelAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<RewardCampaignDetailDto>.Ok(
            result,
            "Hủy đợt khen thưởng thành công."));
    }
}

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
    private readonly IRewardEvaluationService _rewardEvaluationService;

    public AdminRewardCampaignsController(IRewardCampaignService rewardCampaignService, IRewardEvaluationService rewardEvaluationService)
    {
        _rewardCampaignService = rewardCampaignService;
        _rewardEvaluationService = rewardEvaluationService;
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

    [HttpPost("{id:int}/evaluate")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardEvaluationResultDto>>> Evaluate(
        int id,
        EvaluateRewardCampaignRequest request)
    {
        var result = await _rewardEvaluationService.EvaluateCampaignAsync(id, request);
        return Ok(ApiResponseDto<RewardEvaluationResultDto>.Ok(
            result,
            result.IsDryRun ? "Tính toán thử thành công (Dry Run)." : "Xét duyệt đợt khen thưởng thành công."));
    }

    [HttpGet("{id:int}/candidates")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<RewardCandidateDto>>>> GetCandidates(
        int id,
        [FromQuery] RewardCandidateQueryParameters query)
    {
        // current user ID would typically come from context, for scope
        int currentUserId = 0; 
        var (candidates, totalCount) = await _rewardEvaluationService.GetCandidatesAsync(id, query, currentUserId);
        
        return Ok(ApiResponseDto<PagedResultDto<RewardCandidateDto>>.Ok(
            new PagedResultDto<RewardCandidateDto> { Items = candidates.ToList(), TotalItems = totalCount, PageIndex = query.PageIndex, PageSize = query.PageSize },
            "Lấy danh sách ứng viên thành công."));
    }

    [HttpGet("{id:int}/excluded-candidates")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ExcludedRewardCandidateDto>>>> GetExcludedCandidates(
        int id,
        [FromQuery] ExcludedRewardCandidateQueryParameters query)
    {
        int currentUserId = 0; 
        var (excluded, totalCount) = await _rewardEvaluationService.GetExcludedCandidatesAsync(id, query, currentUserId);
        
        return Ok(ApiResponseDto<PagedResultDto<ExcludedRewardCandidateDto>>.Ok(
            new PagedResultDto<ExcludedRewardCandidateDto> { Items = excluded.ToList(), TotalItems = totalCount, PageIndex = query.PageIndex, PageSize = query.PageSize },
            "Lấy danh sách ứng viên bị loại thành công."));
    }
}

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
    private readonly ICertificateGenerationService _certificateGenerationService;

    public AdminRewardCampaignsController(
        IRewardCampaignService rewardCampaignService,
        IRewardEvaluationService rewardEvaluationService,
        ICertificateGenerationService certificateGenerationService)
    {
        _rewardCampaignService = rewardCampaignService;
        _rewardEvaluationService = rewardEvaluationService;
        _certificateGenerationService = certificateGenerationService;
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

    [HttpGet("{id:int}/approval-summary")]
    public async Task<ActionResult<ApiResponseDto<RewardApprovalSummaryDto>>> GetApprovalSummary(int id)
    {
        var result = await _rewardEvaluationService.GetApprovalSummaryAsync(id);
        return Ok(ApiResponseDto<RewardApprovalSummaryDto>.Ok(
            result,
            "Lấy tổng quan duyệt khen thưởng thành công."));
    }

    [HttpPatch("{id:int}/candidates/{candidateId:int}")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardCandidateDto>>> AdjustCandidate(
        int id,
        int candidateId,
        AdjustCandidateRequest request)
    {
        var result = await _rewardEvaluationService.AdjustCandidateAsync(id, candidateId, request);
        return Ok(ApiResponseDto<RewardCandidateDto>.Ok(
            result,
            "Điều chỉnh ứng viên khen thưởng thành công."));
    }

    [HttpPost("{id:int}/candidates/manual-add")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardCandidateDto>>> ManualAddCandidate(
        int id,
        ManualAddCandidateRequest request)
    {
        var result = await _rewardEvaluationService.ManualAddCandidateAsync(id, request);
        return Ok(ApiResponseDto<RewardCandidateDto>.Ok(
            result,
            "Thêm ứng viên thủ công thành công."));
    }

    [HttpPost("{id:int}/candidates/reorder")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardApprovalSummaryDto>>> ReorderCandidates(
        int id,
        ReorderCandidatesRequest request)
    {
        var result = await _rewardEvaluationService.ReorderCandidatesAsync(id, request);
        return Ok(ApiResponseDto<RewardApprovalSummaryDto>.Ok(
            result,
            "Sắp xếp lại ứng viên khen thưởng thành công."));
    }

    [HttpPost("{id:int}/submit-for-approval")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<RewardApprovalSummaryDto>>> SubmitForApproval(int id)
    {
        var result = await _rewardEvaluationService.SubmitForApprovalAsync(id);
        return Ok(ApiResponseDto<RewardApprovalSummaryDto>.Ok(
            result,
            "Trình duyệt đợt khen thưởng thành công."));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<ApproveRewardCampaignResultDto>>> Approve(int id)
    {
        var result = await _rewardEvaluationService.ApproveCampaignAsync(id);
        return Ok(ApiResponseDto<ApproveRewardCampaignResultDto>.Ok(
            result,
            "Duyệt đợt khen thưởng và tạo quyết định khen thưởng thành công."));
    }

    [HttpPost("{id:int}/certificates/generate")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<GenerateRewardCertificatesResultDto>>> GenerateCertificates(
        int id,
        GenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificateGenerationService.GenerateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<GenerateRewardCertificatesResultDto>.Ok(
            result,
            "Sinh PDF bằng khen thành công."));
    }

    [HttpPost("{id:int}/certificates/regenerate")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponseDto<GenerateRewardCertificatesResultDto>>> RegenerateCertificates(
        int id,
        RegenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificateGenerationService.RegenerateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<GenerateRewardCertificatesResultDto>.Ok(
            result,
            "Sinh lại PDF bằng khen thành công."));
    }

    [HttpGet("{id:int}/certificates")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<RewardCertificateListItemDto>>>> GetCertificates(
        int id,
        [FromQuery] RewardCertificateQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _certificateGenerationService.GetCertificatesAsync(id, parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<RewardCertificateListItemDto>>.Ok(
            result,
            "Lấy danh sách PDF bằng khen thành công."));
    }
}

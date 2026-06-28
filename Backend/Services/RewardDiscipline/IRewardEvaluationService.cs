using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IRewardEvaluationService
{
    Task<RewardEvaluationResultDto> EvaluateCampaignAsync(int campaignId, EvaluateRewardCampaignRequest request);
    Task<(IEnumerable<RewardCandidateDto> Candidates, int TotalCount)> GetCandidatesAsync(int campaignId, RewardCandidateQueryParameters query, int currentUserId);
    Task<(IEnumerable<ExcludedRewardCandidateDto> ExcludedCandidates, int TotalCount)> GetExcludedCandidatesAsync(int campaignId, ExcludedRewardCandidateQueryParameters query, int currentUserId);
    Task<RewardApprovalSummaryDto> GetApprovalSummaryAsync(int campaignId);
    Task<RewardCandidateDto> AdjustCandidateAsync(int campaignId, int candidateId, AdjustCandidateRequest request);
    Task<RewardCandidateDto> ManualAddCandidateAsync(int campaignId, ManualAddCandidateRequest request);
    Task<RewardApprovalSummaryDto> ReorderCandidatesAsync(int campaignId, ReorderCandidatesRequest request);
    Task<RewardApprovalSummaryDto> SubmitForApprovalAsync(int campaignId);
    Task<ApproveRewardCampaignResultDto> ApproveCampaignAsync(int campaignId);
}

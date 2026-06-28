using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IRewardEvaluationService
{
    Task<RewardEvaluationResultDto> EvaluateCampaignAsync(int campaignId, EvaluateRewardCampaignRequest request);
    Task<(IEnumerable<RewardCandidateDto> Candidates, int TotalCount)> GetCandidatesAsync(int campaignId, RewardCandidateQueryParameters query, int currentUserId);
    Task<(IEnumerable<ExcludedRewardCandidateDto> ExcludedCandidates, int TotalCount)> GetExcludedCandidatesAsync(int campaignId, ExcludedRewardCandidateQueryParameters query, int currentUserId);
}

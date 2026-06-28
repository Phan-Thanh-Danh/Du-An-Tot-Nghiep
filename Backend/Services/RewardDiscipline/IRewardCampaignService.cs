using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IRewardCampaignService
{
    Task<PagedResultDto<RewardCampaignListItemDto>> GetAsync(
        RewardCampaignQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RewardCampaignDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<RewardCampaignDetailDto> CreateTop100Async(
        CreateTop100RewardCampaignRequest request,
        CancellationToken cancellationToken = default);

    Task<RewardCampaignDetailDto> UpdateAsync(
        int id,
        UpdateRewardCampaignRequest request,
        CancellationToken cancellationToken = default);

    Task<RewardCampaignDetailDto> CancelAsync(
        int id,
        CancelRewardCampaignRequest request,
        CancellationToken cancellationToken = default);
}

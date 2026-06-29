using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IRewardLifecycleService
{
    Task<PagedResultDto<AdminRewardListItemDto>> GetRewardsAsync(
        AdminRewardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AdminRewardDetailDto> GetRewardDetailAsync(
        int rewardId,
        CancellationToken cancellationToken = default);

    Task<RewardLifecycleResultDto> CancelRewardAsync(
        int rewardId,
        CancelRewardRequest request,
        CancellationToken cancellationToken = default);

    Task<RewardLifecycleResultDto> RestoreRewardAsync(
        int rewardId,
        RestoreRewardRequest request,
        CancellationToken cancellationToken = default);

    Task<RewardLifecycleResultDto> MarkIssuedAsync(
        int rewardId,
        MarkRewardIssuedRequest request,
        CancellationToken cancellationToken = default);

    Task<RewardLifecycleResultDto> RegenerateCertificateAsync(
        int rewardId,
        RegenerateSingleRewardCertificateRequest request,
        CancellationToken cancellationToken = default);
}

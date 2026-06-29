using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface ICertificateGenerationService
{
    Task<GenerateRewardCertificatesResultDto> GenerateAsync(
        int campaignId,
        GenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken = default);

    Task<GenerateRewardCertificatesResultDto> RegenerateAsync(
        int campaignId,
        RegenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<RewardCertificateListItemDto>> GetCertificatesAsync(
        int campaignId,
        RewardCertificateQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RewardCertificateDownloadDto> DownloadAsync(
        int rewardId,
        CancellationToken cancellationToken = default);
}

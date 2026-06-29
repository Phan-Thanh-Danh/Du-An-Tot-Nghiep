using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IStudentRewardService
{
    Task<PagedResultDto<StudentRewardListItemDto>> GetMyRewardsAsync(
        int studentUserId,
        StudentRewardQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<StudentRewardDetailDto> GetMyRewardByIdAsync(
        int studentUserId,
        int rewardId,
        CancellationToken cancellationToken = default);

    Task<RewardCertificateDownloadDto> DownloadMyCertificateAsync(
        int studentUserId,
        int rewardId,
        CancellationToken cancellationToken = default);
}

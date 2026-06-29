using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IRewardDisciplineReportService
{
    Task<RewardDisciplineOverviewReportDto> GetOverviewAsync(
        RewardDisciplineReportQuery query,
        CancellationToken cancellationToken = default);

    Task<RewardReportDto> GetRewardsAsync(
        RewardReportQuery query,
        CancellationToken cancellationToken = default);

    Task<DisciplineReportDto> GetDisciplineAsync(
        DisciplineReportQuery query,
        CancellationToken cancellationToken = default);

    Task<CertificateReportDto> GetCertificatesAsync(
        CertificateReportQuery query,
        CancellationToken cancellationToken = default);

    Task<DisciplineAppealReportDto> GetAppealsAsync(
        DisciplineAppealReportQuery query,
        CancellationToken cancellationToken = default);

    Task<RewardDisciplineTrendReportDto> GetTrendsAsync(
        RewardDisciplineTrendQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopStudentReportItemDto>> GetTopStudentsAsync(
        TopStudentReportQuery query,
        CancellationToken cancellationToken = default);
}

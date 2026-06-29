using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationReportService
{
    Task<ApplicationReportOverviewDto> GetOverviewAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<List<ApplicationByTypeReportDto>> GetByTypeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PendingApplicationReportDto> GetPendingAsync(
        PendingApplicationReportQuery parameters,
        CancellationToken cancellationToken = default);

    Task<OverdueApplicationReportDto> GetOverdueAsync(
        OverdueApplicationReportQuery parameters,
        CancellationToken cancellationToken = default);

    Task<ProcessingTimeReportDto> GetProcessingTimeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<List<ApplicationByAssigneeReportDto>> GetByAssigneeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ApplicationTrendReportDto> GetTrendsAsync(
        ApplicationTrendQuery parameters,
        CancellationToken cancellationToken = default);
}

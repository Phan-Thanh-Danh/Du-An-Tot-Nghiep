using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationReportService
{
    Task<ApplicationReportOverviewDto> GetOverviewAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default);
}

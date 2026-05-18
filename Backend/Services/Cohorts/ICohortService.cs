using Backend.DTOs.Cohorts;
using Backend.DTOs.Common;

namespace Backend.Services.Cohorts;

public interface ICohortService
{
    Task<PagedResultDto<CohortDto>> GetAsync(CohortQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<CohortDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CohortDto> CreateAsync(CreateCohortRequest request, CancellationToken cancellationToken = default);
    Task<CohortDto> UpdateAsync(int id, UpdateCohortRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<CohortDto> ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task<CohortDto> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

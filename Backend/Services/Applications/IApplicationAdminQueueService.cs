using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationAdminQueueService
{
    Task<AdminApplicationQueueResponseDto> GetQueueAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationQueueSummaryDto> GetQueueSummaryAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationDetailDto> GetDetailAsync(
        int applicationId,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationAssigneeResponseDto> GetAssigneesAsync(
        AdminApplicationAssigneeQueryParameters parameters,
        CancellationToken cancellationToken = default);
}

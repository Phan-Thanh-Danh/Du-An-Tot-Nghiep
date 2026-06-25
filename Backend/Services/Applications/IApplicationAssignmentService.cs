using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationAssignmentService
{
    Task<AdminApplicationDetailDto> ReceiveAsync(
        int applicationId,
        AdminApplicationReceiveRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationDetailDto> AssignAsync(
        int applicationId,
        AdminApplicationAssignRequest request,
        CancellationToken cancellationToken = default);
}

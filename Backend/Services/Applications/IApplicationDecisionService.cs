using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationDecisionService
{
    Task<AdminApplicationDetailDto> RequestSupplementAsync(
        int applicationId,
        AdminApplicationRequestSupplementRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationDetailDto> ApproveAsync(
        int applicationId,
        AdminApplicationApproveRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationDetailDto> RejectAsync(
        int applicationId,
        AdminApplicationRejectRequest request,
        CancellationToken cancellationToken = default);
}

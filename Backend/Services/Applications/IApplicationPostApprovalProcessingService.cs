using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationPostApprovalProcessingService
{
    Task<AdminApplicationDetailDto> ProcessAsync(
        int applicationId,
        AdminApplicationProcessRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminApplicationDetailDto> RecordProcessingResultAsync(
        int applicationId,
        AdminApplicationRecordProcessingResultRequest request,
        CancellationToken cancellationToken = default);
}

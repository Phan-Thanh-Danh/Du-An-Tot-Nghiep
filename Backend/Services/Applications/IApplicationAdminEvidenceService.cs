using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationAdminEvidenceService
{
    Task<ApplicationEvidenceDownloadDto> DownloadAsync(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken = default);
}

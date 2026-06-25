using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationEvidenceService
{
    Task<ApplicationEvidenceUploadResponseDto> UploadAsync(
        int applicationId,
        IReadOnlyList<IFormFile> files,
        string rowVersion,
        CancellationToken cancellationToken = default);

    Task<ApplicationEvidenceDownloadDto> DownloadAsync(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken = default);

    Task<ApplicationEvidenceDeleteResponseDto> DeleteAsync(
        int applicationId,
        int attachmentId,
        DeleteApplicationEvidenceRequest request,
        CancellationToken cancellationToken = default);
}

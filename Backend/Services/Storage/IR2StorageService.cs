using Backend.DTOs.Curriculum;

namespace Backend.Services.Storage;

public interface IR2StorageService
{
    Task<UploadResultDto> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default);
}

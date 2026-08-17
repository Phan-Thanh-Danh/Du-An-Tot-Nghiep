using Backend.DTOs.Curriculum;

namespace Backend.Services.Storage;

public interface IR2StorageService
{
    Task<UploadResultDto> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        bool keepOriginalFileName = false,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, long? ContentLength)> GetFileStreamAsync(string storageKey, CancellationToken cancellationToken = default);

    string? GetPresignedStreamUrl(string storageKey, TimeSpan? expiry = null);
}

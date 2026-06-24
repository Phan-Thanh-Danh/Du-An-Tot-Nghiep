namespace Backend.Services.Storage;

public interface IApplicationEvidenceObjectStore
{
    Task StoreAsync(
        string storageKey,
        Stream content,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default);

    Task<ApplicationEvidenceObjectReadResult> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

public sealed class ApplicationEvidenceObjectReadResult
{
    public Stream Content { get; init; } = Stream.Null;
    public long ContentLength { get; init; }
    public string? ContentType { get; init; }
}

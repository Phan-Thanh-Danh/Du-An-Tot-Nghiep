namespace Backend.Services.Applications;

public interface IApplicationEvidenceFileInspector
{
    Task<IReadOnlyList<InspectedApplicationEvidenceFile>> InspectAsync(
        IReadOnlyList<IFormFile> files,
        CancellationToken cancellationToken = default);
}

public sealed class InspectedApplicationEvidenceFile : IDisposable
{
    public string OriginalFileName { get; init; } = string.Empty;
    public string StorageFileName { get; init; } = string.Empty;
    public string CanonicalExtension { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Length { get; init; }
    public string Sha256Hex { get; init; } = string.Empty;
    public MemoryStream Content { get; init; } = new();
    public string StorageKey { get; set; } = string.Empty;

    public void Dispose()
    {
        Content.Dispose();
    }
}

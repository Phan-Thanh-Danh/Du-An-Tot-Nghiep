namespace Backend.Services.RewardDiscipline;

public interface ICertificatePdfStorageService
{
    Task<StoredCertificatePdf> SaveAsync(int rewardId, byte[] pdfBytes, CancellationToken cancellationToken = default);
    Task<CertificatePdfDownload?> TryReadAsync(string url, CancellationToken cancellationToken = default);
}

public sealed class StoredCertificatePdf
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}

public sealed class CertificatePdfDownload
{
    public byte[] Content { get; init; } = [];
    public string FileName { get; init; } = "certificate.pdf";
}

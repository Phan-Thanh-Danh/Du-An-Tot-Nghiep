using Backend.Exceptions;
using Microsoft.Extensions.Options;

namespace Backend.Services.RewardDiscipline;

public class LocalCertificatePdfStorageService : ICertificatePdfStorageService
{
    private readonly CertificateStorageOptions _options;
    private readonly IWebHostEnvironment _environment;

    public LocalCertificatePdfStorageService(
        IOptions<CertificateStorageOptions> options,
        IWebHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<StoredCertificatePdf> SaveAsync(int rewardId, byte[] pdfBytes, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(_options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "CertificateStorage provider chưa được hỗ trợ.");
        }

        if (pdfBytes.Length == 0)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Nội dung PDF bằng khen rỗng.");
        }

        var root = ResolveRoot();
        Directory.CreateDirectory(root);
        var fileName = $"reward-{rewardId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.pdf";
        var path = Path.Combine(root, fileName);
        await File.WriteAllBytesAsync(path, pdfBytes, cancellationToken);

        var basePath = string.IsNullOrWhiteSpace(_options.PublicBasePath)
            ? "/certificates"
            : _options.PublicBasePath.TrimEnd('/');

        return new StoredCertificatePdf
        {
            FileName = fileName,
            Url = $"{basePath}/{fileName}"
        };
    }

    public async Task<CertificatePdfDownload?> TryReadAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!string.Equals(_options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "CertificateStorage provider chưa được hỗ trợ.");
        }

        var fileName = Path.GetFileName(url.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var root = ResolveRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, fileName));
        if (!fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(fullPath))
        {
            return null;
        }

        return new CertificatePdfDownload
        {
            FileName = fileName,
            Content = await File.ReadAllBytesAsync(fullPath, cancellationToken)
        };
    }

    private string ResolveRoot()
    {
        if (!string.IsNullOrWhiteSpace(_options.LocalRoot))
        {
            return Path.GetFullPath(_options.LocalRoot);
        }

        return Path.Combine(_environment.ContentRootPath, "App_Data", "certificates");
    }
}

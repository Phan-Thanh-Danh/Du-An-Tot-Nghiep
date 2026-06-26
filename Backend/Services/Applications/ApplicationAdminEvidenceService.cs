using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationAdminEvidenceService : IApplicationAdminEvidenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly IApplicationEvidenceObjectStore _objectStore;

    public ApplicationAdminEvidenceService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IApplicationEvidenceObjectStore objectStore)
    {
        _context = context;
        _scopeService = scopeService;
        _objectStore = objectStore;
    }

    public async Task<ApplicationEvidenceDownloadDto> DownloadAsync(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var attachment = await _context.TepDinhKemDonTus.AsNoTracking()
            .Include(x => x.DonTu)
            .FirstOrDefaultAsync(x =>
                x.MaTep == attachmentId &&
                x.MaDonTu == applicationId &&
                !x.DaXoa &&
                x.DonTu != null,
                cancellationToken);
        if (attachment is null ||
            attachment.DonTu is null ||
            !await _scopeService.CanAccessCampusAsync(actor, attachment.DonTu.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tệp minh chứng.");
        }

        try
        {
            var read = await _objectStore.OpenReadAsync(attachment.StorageKey, cancellationToken);
            return new ApplicationEvidenceDownloadDto
            {
                Content = read.Content,
                ContentLength = read.ContentLength,
                ContentType = NormalizeStoredContentType(attachment.ContentType),
                FileName = SanitizeStoredFileName(attachment.TenFileGoc)
            };
        }
        catch (ApplicationEvidenceObjectNotFoundException)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tệp minh chứng.");
        }
        catch (ApplicationEvidenceStorageException)
        {
            throw new ApiException(StatusCodes.Status503ServiceUnavailable, "Không thể truy cập kho lưu trữ minh chứng.");
        }
    }

    private static string NormalizeStoredContentType(string? contentType)
    {
        return ApplicationEvidenceConstants.AllowedContentTypes.Contains(contentType ?? string.Empty)
            ? contentType!
            : "application/octet-stream";
    }

    private static string SanitizeStoredFileName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "minh-chung";
        }

        var fileName = raw.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "minh-chung";
        }

        var sanitized = new string(fileName.Where(character => !char.IsControl(character)).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized is "." or "..")
        {
            return "minh-chung";
        }

        sanitized = sanitized.Replace("/", string.Empty).Replace("\\", string.Empty);
        if (sanitized.Length <= 255)
        {
            return sanitized;
        }

        var extension = Path.GetExtension(sanitized);
        var baseName = Path.GetFileNameWithoutExtension(sanitized);
        var maxBaseLength = Math.Max(1, 255 - extension.Length);
        return baseName[..Math.Min(baseName.Length, maxBaseLength)] + extension;
    }
}

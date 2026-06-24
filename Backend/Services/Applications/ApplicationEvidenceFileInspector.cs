using System.Security.Cryptography;
using System.Text;
using Backend.Constants;
using Backend.Exceptions;

namespace Backend.Services.Applications;

public class ApplicationEvidenceFileInspector : IApplicationEvidenceFileInspector
{
    private const int MaxFileNameLength = 255;
    private static readonly IReadOnlyDictionary<string, EvidenceFileType> TypesByContentType =
        new Dictionary<string, EvidenceFileType>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new("application/pdf", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf" }, bytes =>
                bytes.Length >= 5 && Encoding.ASCII.GetString(bytes[..5]) == "%PDF-"),
            ["image/jpeg"] = new("image/jpeg", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg" }, bytes =>
                bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF),
            ["image/png"] = new("image/png", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png" }, bytes =>
                bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A),
            ["image/webp"] = new("image/webp", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".webp" }, bytes =>
                bytes.Length >= 12 &&
                Encoding.ASCII.GetString(bytes[..4]) == "RIFF" &&
                Encoding.ASCII.GetString(bytes[8..12]) == "WEBP")
        };

    public async Task<IReadOnlyList<InspectedApplicationEvidenceFile>> InspectAsync(
        IReadOnlyList<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không có file minh chứng nào được gửi lên.");
        }

        if (files.Count > ApplicationEvidenceConstants.MaxFiles)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số lượng tệp minh chứng vượt quá giới hạn.");
        }

        var inspected = new List<InspectedApplicationEvidenceFile>(files.Count);
        try
        {
            foreach (var file in files)
            {
                inspected.Add(await InspectOneAsync(file, cancellationToken));
            }

            var duplicateInRequest = inspected
                .GroupBy(x => x.Sha256Hex, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(x => x.Count() > 1);
            if (duplicateInRequest is not null)
            {
                throw new ApiException(StatusCodes.Status409Conflict, "Không được tải trùng nội dung file trong cùng một yêu cầu.");
            }

            return inspected;
        }
        catch
        {
            foreach (var item in inspected)
            {
                item.Dispose();
            }

            throw;
        }
    }

    private static async Task<InspectedApplicationEvidenceFile> InspectOneAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File minh chứng không hợp lệ.");
        }

        if (file.Length <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File minh chứng không được rỗng.");
        }

        if (file.Length > ApplicationEvidenceConstants.MaxFileSizeBytes)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dung lượng một tệp minh chứng vượt quá giới hạn.");
        }

        var fileName = NormalizeFileName(file.FileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "File minh chứng phải có phần mở rộng hợp lệ.");
        }

        if (!TypesByContentType.TryGetValue(file.ContentType, out var declaredType))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Định dạng file minh chứng không được hỗ trợ.");
        }

        if (!declaredType.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phần mở rộng file không khớp định dạng khai báo.");
        }

        var memory = new MemoryStream((int)file.Length);
        await using (var stream = file.OpenReadStream())
        {
            await stream.CopyToAsync(memory, cancellationToken);
        }

        if (memory.Length != file.Length)
        {
            memory.Dispose();
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể đọc đầy đủ file minh chứng.");
        }

        var bytes = memory.ToArray();
        if (!declaredType.MatchesSignature(bytes))
        {
            memory.Dispose();
            throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung file không khớp định dạng khai báo.");
        }

        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        memory.Position = 0;
        return new InspectedApplicationEvidenceFile
        {
            OriginalFileName = fileName,
            CanonicalExtension = extension,
            StorageFileName = Guid.NewGuid().ToString("N") + extension,
            ContentType = declaredType.ContentType,
            Length = memory.Length,
            Sha256Hex = hash,
            Content = memory
        };
    }

    private static string NormalizeFileName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên file minh chứng không hợp lệ.");
        }

        var normalizedSeparators = raw.Trim().Replace('\\', '/');
        var fileName = normalizedSeparators.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên file minh chứng không hợp lệ.");
        }

        var builder = new StringBuilder(fileName.Length);
        foreach (var character in fileName)
        {
            if (char.IsControl(character))
            {
                continue;
            }

            builder.Append(character);
        }

        var safe = builder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(safe))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên file minh chứng không hợp lệ.");
        }

        if (safe.Length > MaxFileNameLength)
        {
            var extension = Path.GetExtension(safe);
            var baseName = Path.GetFileNameWithoutExtension(safe);
            var maxBaseLength = Math.Max(1, MaxFileNameLength - extension.Length);
            safe = baseName[..Math.Min(baseName.Length, maxBaseLength)] + extension;
        }

        if (safe.Contains('/') || safe.Contains('\\') || safe is "." or "..")
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên file minh chứng không hợp lệ.");
        }

        return safe;
    }

    private sealed record EvidenceFileType(
        string ContentType,
        IReadOnlySet<string> Extensions,
        Func<byte[], bool> MatchesSignature);
}

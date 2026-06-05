using Backend.DTOs.Curriculum;
using Backend.Exceptions;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize(Policy = "AcademicOperations")]
public class StorageController : ControllerBase
{
    private readonly IR2StorageService _storage;
    private const long MaxVideoSize = 500L * 1024 * 1024; // 500MB
    private const long MaxDocumentSize = 50L * 1024 * 1024; // 50MB
    private const long MaxImageSize = 10L * 1024 * 1024; // 10MB

    private static readonly HashSet<string> AllowedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/x-matroska", "video/webm", "video/quicktime"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/zip",
        "application/x-zip-compressed"
    };

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml"
    };

    public StorageController(IR2StorageService storage)
    {
        _storage = storage;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500L * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        List<IFormFile> file,
        [FromQuery] string folder = "general",
        CancellationToken ct = default)
    {
        if (file is null || file.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không có file nào được gửi lên.");
        }

        // Validate all files first before uploading any
        foreach (var f in file)
        {
            if (f.Length == 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest,
                    $"File '{f.FileName}' rỗng.");
            }

            var contentType = f.ContentType;
            var maxSize = GetMaxSize(contentType);

            if (f.Length > maxSize)
            {
                var maxMb = maxSize / (1024 * 1024);
                throw new ApiException(StatusCodes.Status400BadRequest,
                    $"File '{f.FileName}' vượt quá dung lượng cho phép ({maxMb}MB).");
            }

            if (!IsAllowedContentType(contentType))
            {
                throw new ApiException(StatusCodes.Status400BadRequest,
                    $"Định dạng file không được hỗ trợ: {contentType} (file: {f.FileName})");
            }
        }

        // Upload all files
        var results = new List<UploadResultDto>();
        foreach (var f in file)
        {
            await using var stream = f.OpenReadStream();
            var result = await _storage.UploadFileAsync(stream, f.FileName, f.ContentType, folder, ct);
            results.Add(result);
        }

        // Return single object for backward compatibility if only 1 file
        if (results.Count == 1)
        {
            return Ok(new { success = true, data = results[0] });
        }

        return Ok(new { success = true, data = results });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string storageKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "storageKey là bắt buộc.");
        }

        await _storage.DeleteFileAsync(storageKey, ct);
        return Ok(new { success = true, message = "Xóa file thành công." });
    }

    private static long GetMaxSize(string contentType)
    {
        if (AllowedVideoTypes.Contains(contentType)) return MaxVideoSize;
        if (AllowedImageTypes.Contains(contentType)) return MaxImageSize;
        return MaxDocumentSize;
    }

    private static bool IsAllowedContentType(string contentType)
    {
        return AllowedVideoTypes.Contains(contentType) ||
               AllowedDocumentTypes.Contains(contentType) ||
               AllowedImageTypes.Contains(contentType);
    }
}

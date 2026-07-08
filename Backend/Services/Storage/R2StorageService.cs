using Amazon.S3;
using Amazon.S3.Model;
using Backend.DTOs.Curriculum;

namespace Backend.Services.Storage;

public class R2StorageSettings
{
    public string AccountId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string? PublicDomain { get; set; }
}

public class R2StorageService : IR2StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly R2StorageSettings _settings;
    private readonly ILogger<R2StorageService> _logger;

    /// <summary>Files larger than this threshold use multipart upload.</summary>
    private const long MultipartThreshold = 5L * 1024 * 1024; // 5 MB

    /// <summary>Each multipart part is 10 MB.</summary>
    private const int PartSize = 10 * 1024 * 1024; // 10 MB

    public R2StorageService(
        R2StorageSettings settings,
        ILogger<R2StorageService> logger)
    {
        _settings = settings;
        _logger = logger;

        var s3Config = new AmazonS3Config
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true,
            Timeout = TimeSpan.FromMinutes(30)
        };

        _s3Client = new AmazonS3Client(
            settings.AccessKeyId,
            settings.SecretAccessKey,
            s3Config);
    }

    public async Task<UploadResultDto> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var storageKey = $"{folder}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}_{safeFileName}";

        long fileSize;

        // Determine file size – if the stream supports seeking, use Length;
        // otherwise we'll find out during multipart upload.
        if (fileStream.CanSeek)
        {
            fileSize = fileStream.Length;
        }
        else
        {
            // Non-seekable streams always go through multipart to be safe
            fileSize = MultipartThreshold + 1;
        }

        if (fileSize <= MultipartThreshold)
        {
            await UploadSinglePartAsync(fileStream, storageKey, contentType, cancellationToken);
        }
        else
        {
            fileSize = await UploadMultipartAsync(fileStream, storageKey, contentType, cancellationToken);
        }

        // ── Verify the object actually exists in R2 ──
        var actualSize = await VerifyObjectExistsAsync(storageKey, cancellationToken);
        _logger.LogInformation(
            "Verified object {StorageKey} in R2: {ActualSize} bytes.",
            storageKey, actualSize);

        var publicUrl = string.IsNullOrWhiteSpace(_settings.PublicDomain)
            ? $"{_settings.Endpoint}/{_settings.BucketName}/{storageKey}"
            : $"{_settings.PublicDomain.TrimEnd('/')}/{storageKey}";

        _logger.LogInformation(
            "Uploaded file {StorageKey} ({ContentType}, {Size} bytes) to R2.",
            storageKey, contentType, actualSize);

        return new UploadResultDto
        {
            Url = publicUrl,
            StorageKey = storageKey,
            KichThuocByte = actualSize,
            ContentType = contentType
        };
    }

    // ── Single-part upload (small files ≤ 5 MB) ──────────────────────
    private async Task UploadSinglePartAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = storageKey,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        if ((int)response.HttpStatusCode < 200 || (int)response.HttpStatusCode >= 300)
        {
            throw new InvalidOperationException(
                $"R2 PutObject failed with HTTP {(int)response.HttpStatusCode} for key '{storageKey}'.");
        }
    }

    // ── Multipart upload (large files > 5 MB, e.g. videos) ──────────
    private async Task<long> UploadMultipartAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        var initiateRequest = new InitiateMultipartUploadRequest
        {
            BucketName = _settings.BucketName,
            Key = storageKey,
            ContentType = contentType
        };

        var initResponse = await _s3Client.InitiateMultipartUploadAsync(initiateRequest, cancellationToken);
        var uploadId = initResponse.UploadId;

        _logger.LogInformation(
            "Started multipart upload {UploadId} for {StorageKey}.",
            uploadId, storageKey);

        var partETags = new List<PartETag>();
        var buffer = new byte[PartSize];
        int partNumber = 1;
        long totalBytes = 0;

        try
        {
            int bytesRead;
            while ((bytesRead = await ReadFullBufferAsync(fileStream, buffer, cancellationToken)) > 0)
            {
                using var partStream = new MemoryStream(buffer, 0, bytesRead);
                totalBytes += bytesRead;

                var uploadPartRequest = new UploadPartRequest
                {
                    BucketName = _settings.BucketName,
                    Key = storageKey,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    InputStream = partStream,
                    DisablePayloadSigning = true
                };

                var partResponse = await _s3Client.UploadPartAsync(uploadPartRequest, cancellationToken);
                partETags.Add(new PartETag(partNumber, partResponse.ETag));

                _logger.LogDebug(
                    "Uploaded part {PartNumber} ({BytesRead} bytes) for {StorageKey}.",
                    partNumber, bytesRead, storageKey);

                partNumber++;
            }

            // Complete the multipart upload
            var completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = _settings.BucketName,
                Key = storageKey,
                UploadId = uploadId,
                PartETags = partETags
            };

            var completeResponse = await _s3Client.CompleteMultipartUploadAsync(completeRequest, cancellationToken);

            if ((int)completeResponse.HttpStatusCode < 200 || (int)completeResponse.HttpStatusCode >= 300)
            {
                throw new InvalidOperationException(
                    $"R2 CompleteMultipartUpload failed with HTTP {(int)completeResponse.HttpStatusCode} for key '{storageKey}'.");
            }

            _logger.LogInformation(
                "Completed multipart upload {UploadId} for {StorageKey}: {Parts} parts, {TotalBytes} bytes.",
                uploadId, storageKey, partETags.Count, totalBytes);

            return totalBytes;
        }
        catch
        {
            // Abort the multipart upload on failure so we don't leave orphaned parts
            _logger.LogWarning(
                "Aborting multipart upload {UploadId} for {StorageKey} due to error.",
                uploadId, storageKey);

            try
            {
                await _s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = _settings.BucketName,
                    Key = storageKey,
                    UploadId = uploadId
                }, CancellationToken.None);
            }
            catch (Exception abortEx)
            {
                _logger.LogWarning(abortEx, "Failed to abort multipart upload {UploadId}.", uploadId);
            }

            throw;
        }
    }

    /// <summary>
    /// Reads from the stream until the buffer is full or the stream ends.
    /// This is needed because Stream.ReadAsync may return fewer bytes than requested.
    /// </summary>
    private static async Task<int> ReadFullBufferAsync(
        Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int read = await stream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken);
            if (read == 0) break;
            totalRead += read;
        }
        return totalRead;
    }

    /// <summary>
    /// Issues a HeadObject request to verify the uploaded file exists in R2.
    /// Throws if the object is not found.
    /// </summary>
    private async Task<long> VerifyObjectExistsAsync(
        string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            var headResponse = await _s3Client.GetObjectMetadataAsync(
                _settings.BucketName, storageKey, cancellationToken);
            return headResponse.ContentLength;
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Upload verification failed: object '{storageKey}' not found in R2 after upload.", ex);
        }
    }

    public async Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = storageKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);

            _logger.LogInformation("Deleted file {StorageKey} from R2.", storageKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file {StorageKey} from R2.", storageKey);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Tách tên file và extension
        var extension = Path.GetExtension(fileName); // e.g. ".mp4"
        var nameOnly = Path.GetFileNameWithoutExtension(fileName);

        // Loại bỏ ký tự không hợp lệ cho hệ điều hành
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", nameOnly.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Loại bỏ thêm ký tự nguy hiểm cho URL: # [ ] % { } | \ ^ ~ ` và khoảng trắng
        // Ký tự # đặc biệt nguy hiểm vì trình duyệt coi nó là fragment identifier
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[#\[\]%\{\}|\\^~`\s]+", "_");

        // Loại bỏ dấu tiếng Việt (normalize → remove diacritics)
        var normalized = sanitized.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        sanitized = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

        // Thay thế đ/Đ (không bị xử lý bởi normalization)
        sanitized = sanitized.Replace("đ", "d").Replace("Đ", "D");

        // Gộp nhiều dấu gạch dưới liên tiếp thành 1, trim đầu cuối
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_+", "_").Trim('_');

        // Giới hạn độ dài
        if (sanitized.Length > 150) sanitized = sanitized[..150];

        // Ghép lại với extension
        return sanitized + extension;
    }
}

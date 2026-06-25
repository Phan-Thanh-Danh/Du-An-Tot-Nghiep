using System.Data;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationEvidenceService : IApplicationEvidenceService
{
    private static readonly string[] EditableStatuses =
    [
        ApplicationStatuses.Draft,
        ApplicationStatuses.NeedSupplement
    ];

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationTemplateValidator _templateValidator;
    private readonly IApplicationEvidenceFileInspector _fileInspector;
    private readonly IApplicationEvidenceObjectStore _objectStore;
    private readonly ILogger<ApplicationEvidenceService> _logger;

    public ApplicationEvidenceService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IApplicationTemplateValidator templateValidator,
        IApplicationEvidenceFileInspector fileInspector,
        IApplicationEvidenceObjectStore objectStore,
        ILogger<ApplicationEvidenceService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _templateValidator = templateValidator;
        _fileInspector = fileInspector;
        _objectStore = objectStore;
        _logger = logger;
    }

    public async Task<ApplicationEvidenceUploadResponseDto> UploadAsync(
        int applicationId,
        IReadOnlyList<IFormFile> files,
        string rowVersion,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var rowVersionBytes = DecodeRowVersion(rowVersion);
        var application = await GetOwnedApplicationNoTrackingAsync(applicationId, student.MaNguoiDung, cancellationToken);
        EnsureEditableState(application.TrangThai);
        EnsureRowVersion(application.RowVersion, rowVersionBytes);
        var template = await ResolveTemplateNoTrackingAsync(application, cancellationToken);
        var limits = EvidenceLimits.FromTemplate(template);

        using var inspectedFiles = new InspectedFileCollection(await _fileInspector.InspectAsync(files, cancellationToken));
        ValidateInspectedFiles(inspectedFiles.Items, limits);
        var activeAttachments = await GetActiveAttachmentsAsync(applicationId, cancellationToken);
        ValidateAggregateLimits(activeAttachments, inspectedFiles.Items, limits);
        ValidateDuplicateHashes(activeAttachments, inspectedFiles.Items);

        var attemptedKeys = new List<string>();
        try
        {
            foreach (var file in inspectedFiles.Items)
            {
                file.StorageKey = BuildStorageKey(application.MaDonVi, application.MaDonTu, file.StorageFileName);
                attemptedKeys.Add(file.StorageKey);
                file.Content.Position = 0;
                await _objectStore.StoreAsync(
                    file.StorageKey,
                    file.Content,
                    file.ContentType,
                    file.Length,
                    cancellationToken);
            }

            return await ExecuteConcurrencyAwareAsync(async () =>
            {
                return await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
                {
                    await AcquireEvidenceLockAsync(applicationId, cancellationToken);
                    var trackedApplication = await GetOwnedApplicationTrackedAsync(applicationId, student.MaNguoiDung, cancellationToken);
                    EnsureRowVersion(trackedApplication.RowVersion, rowVersionBytes);
                    EnsureEditableState(trackedApplication.TrangThai);
                    var trackedTemplate = await ResolveTemplateTrackedAsync(trackedApplication, cancellationToken);
                    var trackedLimits = EvidenceLimits.FromTemplate(trackedTemplate);
                    ValidateInspectedFiles(inspectedFiles.Items, trackedLimits);
                    var currentAttachments = await GetActiveAttachmentsAsync(applicationId, cancellationToken);
                    ValidateAggregateLimits(currentAttachments, inspectedFiles.Items, trackedLimits);
                    ValidateDuplicateHashes(currentAttachments, inspectedFiles.Items);

                    var now = DateTime.UtcNow;
                    var added = inspectedFiles.Items.Select(file => new TepDinhKemDonTu
                    {
                        MaDonTu = trackedApplication.MaDonTu,
                        StorageKey = file.StorageKey,
                        TenFileGoc = file.OriginalFileName,
                        TenFileLuu = file.StorageFileName,
                        ContentType = file.ContentType,
                        KichThuocByte = file.Length,
                        FileHash = file.Sha256Hex,
                        NguoiTaiLen = student.MaNguoiDung,
                        NgayTao = now,
                        DaXoa = false
                    }).ToList();

                    _context.TepDinhKemDonTus.AddRange(added);
                    trackedApplication.MaMauDon = trackedTemplate.MaMauDon;
                    trackedApplication.NgayCapNhat = now;
                    SetOriginalRowVersion(trackedApplication, rowVersionBytes);
                    _context.NhatKyDuyetDons.Add(CreateHiddenLog(
                        trackedApplication,
                        student.MaNguoiDung,
                        new
                        {
                            attachmentAction = "upload",
                            count = added.Count,
                            totalBytes = added.Sum(x => x.KichThuocByte)
                        }));

                    await _context.SaveChangesAsync(cancellationToken);
                    var uploadedDtos = added.Select(ToAttachmentDto).ToList();
                    var activeCount = currentAttachments.Count + added.Count;
                    var totalSize = currentAttachments.Sum(x => x.KichThuocByte) + added.Sum(x => x.KichThuocByte);
                    return new ApplicationEvidenceUploadResponseDto
                    {
                        UploadedFiles = uploadedDtos,
                        RowVersion = Convert.ToBase64String(trackedApplication.RowVersion),
                        ActiveFileCount = activeCount,
                        TotalSizeBytes = totalSize
                    };
                }, cancellationToken);
            });
        }
        catch (ApplicationEvidenceStorageException exception)
        {
            await CleanupUploadedObjectsAsync(attemptedKeys);
            _logger.LogWarning(exception, "Application evidence object store failed during upload.");
            throw new ApiException(StatusCodes.Status503ServiceUnavailable, "Không thể truy cập kho lưu trữ minh chứng.");
        }
        catch
        {
            await CleanupUploadedObjectsAsync(attemptedKeys);
            throw;
        }
    }

    public async Task<ApplicationEvidenceDownloadDto> DownloadAsync(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var attachment = await _context.TepDinhKemDonTus.AsNoTracking()
            .Include(x => x.DonTu)
            .FirstOrDefaultAsync(x =>
                x.MaTep == attachmentId &&
                x.MaDonTu == applicationId &&
                !x.DaXoa &&
                x.DonTu != null &&
                x.DonTu.MaHocSinh == student.MaNguoiDung,
                cancellationToken);
        if (attachment is null)
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

    public async Task<ApplicationEvidenceDeleteResponseDto> DeleteAsync(
        int applicationId,
        int attachmentId,
        DeleteApplicationEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var rowVersionBytes = DecodeRowVersion(request.RowVersion);
        string? storageKey = null;
        ApplicationEvidenceDeleteResponseDto result;

        result = await ExecuteConcurrencyAwareAsync(async () =>
        {
            return await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireEvidenceLockAsync(applicationId, cancellationToken);
                var application = await GetOwnedApplicationTrackedAsync(applicationId, student.MaNguoiDung, cancellationToken);
                EnsureRowVersion(application.RowVersion, rowVersionBytes);
                EnsureEditableState(application.TrangThai);

                var attachment = await _context.TepDinhKemDonTus
                    .FirstOrDefaultAsync(x =>
                        x.MaTep == attachmentId &&
                        x.MaDonTu == applicationId &&
                        !x.DaXoa &&
                        x.NguoiTaiLen == student.MaNguoiDung,
                        cancellationToken);
                if (attachment is null)
                {
                    throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tệp minh chứng.");
                }

                storageKey = attachment.StorageKey;
                var now = DateTime.UtcNow;
                attachment.DaXoa = true;
                attachment.NguoiXoa = student.MaNguoiDung;
                attachment.NgayXoa = now;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, rowVersionBytes);
                _context.NhatKyDuyetDons.Add(CreateHiddenLog(
                    application,
                    student.MaNguoiDung,
                    new { attachmentAction = "delete", maTep = attachment.MaTep }));

                await _context.SaveChangesAsync(cancellationToken);
                var active = await GetActiveAttachmentsAsync(applicationId, cancellationToken);
                return new ApplicationEvidenceDeleteResponseDto
                {
                    MaTep = attachment.MaTep,
                    RowVersion = Convert.ToBase64String(application.RowVersion),
                    ActiveFileCount = active.Count,
                    TotalSizeBytes = active.Sum(x => x.KichThuocByte)
                };
            }, cancellationToken);
        });

        if (!string.IsNullOrWhiteSpace(storageKey))
        {
            try
            {
                await _objectStore.DeleteAsync(storageKey, CancellationToken.None);
            }
            catch (ApplicationEvidenceObjectNotFoundException)
            {
                // API access is already revoked by soft delete. Missing physical object is not exposed.
            }
            catch (ApplicationEvidenceStorageException exception)
            {
                _logger.LogWarning(exception, "Failed to delete application evidence object after metadata soft delete.");
            }
        }

        return result;
    }

    private async Task<NguoiDung> GetCurrentStudentAsync(CancellationToken cancellationToken)
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        var student = await _context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == currentUser.UserId, cancellationToken);
        if (student is null ||
            student.VaiTroChinh != AuthRoles.ToDatabaseCode(AuthRoles.Student) ||
            student.TrangThai != UserStatuses.DbActive)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ sinh viên đang hoạt động được sử dụng minh chứng đơn từ.");
        }

        return student;
    }

    private async Task<DonTu> GetOwnedApplicationNoTrackingAsync(
        int applicationId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var application = await _context.DonTus.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonTu == applicationId && x.MaHocSinh == studentId, cancellationToken);
        if (application is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        return application;
    }

    private async Task<DonTu> GetOwnedApplicationTrackedAsync(
        int applicationId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var application = await _context.DonTus
            .FirstOrDefaultAsync(x => x.MaDonTu == applicationId && x.MaHocSinh == studentId, cancellationToken);
        if (application is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        return application;
    }

    private async Task<MauDonTu> ResolveTemplateNoTrackingAsync(DonTu application, CancellationToken cancellationToken)
    {
        MauDonTu? template = null;
        if (application.MaMauDon.HasValue)
        {
            template = await _context.MauDonTus.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaMauDon == application.MaMauDon.Value, cancellationToken);
        }

        if (template is null)
        {
            template = await _context.MauDonTus.AsNoTracking()
                .FirstOrDefaultAsync(x => x.LoaiDon == application.LoaiDon && x.DangHoatDong, cancellationToken);
        }

        if (template is null)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Không tìm thấy mẫu đơn phù hợp cho đơn legacy.");
        }

        _templateValidator.Validate(template.CauHinhJson);
        return template;
    }

    private async Task<MauDonTu> ResolveTemplateTrackedAsync(DonTu application, CancellationToken cancellationToken)
    {
        MauDonTu? template = null;
        if (application.MaMauDon.HasValue)
        {
            template = await _context.MauDonTus
                .FirstOrDefaultAsync(x => x.MaMauDon == application.MaMauDon.Value, cancellationToken);
        }

        if (template is null)
        {
            template = await _context.MauDonTus
                .FirstOrDefaultAsync(x => x.LoaiDon == application.LoaiDon && x.DangHoatDong, cancellationToken);
            if (template is null)
            {
                throw new ApiException(StatusCodes.Status409Conflict, "Không tìm thấy mẫu đơn phù hợp cho đơn legacy.");
            }

            application.MaMauDon = template.MaMauDon;
        }

        _templateValidator.Validate(template.CauHinhJson);
        return template;
    }

    private async Task<List<TepDinhKemDonTu>> GetActiveAttachmentsAsync(
        int applicationId,
        CancellationToken cancellationToken)
    {
        return await _context.TepDinhKemDonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId && !x.DaXoa)
            .ToListAsync(cancellationToken);
    }

    private async Task AcquireEvidenceLockAsync(int applicationId, CancellationToken cancellationToken)
    {
        var result = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        var resource = new SqlParameter("@resource", SqlDbType.NVarChar, 255)
        {
            Value = $"ApplicationEvidence:{applicationId}"
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000",
            [result, resource],
            cancellationToken);

        if (result.Value is not int code || code < 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Minh chứng đơn từ đang được xử lý đồng thời. Vui lòng thử lại.");
        }
    }

    private static void EnsureEditableState(string status)
    {
        if (!EditableStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái hiện tại không cho phép thay đổi minh chứng.");
        }
    }

    private static void ValidateInspectedFiles(
        IReadOnlyList<InspectedApplicationEvidenceFile> files,
        EvidenceLimits limits)
    {
        if (limits.MaxFiles <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mẫu đơn không cho phép tải tệp minh chứng.");
        }

        foreach (var file in files)
        {
            if (file.Length > limits.MaxFileSizeBytes)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Dung lượng một tệp minh chứng vượt quá giới hạn.");
            }
        }

        if (files.Sum(x => x.Length) > limits.MaxTotalSizeBytes)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tổng dung lượng minh chứng vượt quá giới hạn.");
        }
    }

    private static void ValidateAggregateLimits(
        IReadOnlyList<TepDinhKemDonTu> existing,
        IReadOnlyList<InspectedApplicationEvidenceFile> incoming,
        EvidenceLimits limits)
    {
        if (existing.Count + incoming.Count > limits.MaxFiles)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số lượng tệp minh chứng vượt quá giới hạn.");
        }

        if (existing.Sum(x => x.KichThuocByte) + incoming.Sum(x => x.Length) > limits.MaxTotalSizeBytes)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tổng dung lượng minh chứng vượt quá giới hạn.");
        }
    }

    private static void ValidateDuplicateHashes(
        IReadOnlyList<TepDinhKemDonTu> existing,
        IReadOnlyList<InspectedApplicationEvidenceFile> incoming)
    {
        var existingHashes = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.FileHash))
            .Select(x => x.FileHash!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (incoming.Any(x => existingHashes.Contains(x.Sha256Hex)))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "File minh chứng đã tồn tại trong đơn.");
        }
    }

    private static string BuildStorageKey(int campusId, int applicationId, string storageFileName)
    {
        return $"applications/evidence/{campusId}/{applicationId}/{storageFileName}";
    }

    private static void EnsureRowVersion(byte[] actual, byte[] expected)
    {
        if (!actual.SequenceEqual(expected))
        {
            throw ConcurrencyException();
        }
    }

    private static byte[] DecodeRowVersion(string rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }

        try
        {
            var decoded = Convert.FromBase64String(rowVersion);
            if (decoded.Length != 8)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
            }

            return decoded;
        }
        catch (FormatException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }
    }

    private void SetOriginalRowVersion(DonTu application, byte[] rowVersion)
    {
        _context.Entry(application).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    private static ApiException ConcurrencyException()
    {
        return new ApiException(
            StatusCodes.Status409Conflict,
            "Đơn đã được thay đổi bởi một thao tác khác. Vui lòng tải lại dữ liệu.");
    }

    private static async Task<TResult> ExecuteConcurrencyAwareAsync<TResult>(Func<Task<TResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConcurrencyException();
        }
        catch (SqlException exception) when (exception.Number is -2 or 1205 or 1222 or 2601 or 2627 or 3960 or 3961 or 3962 or 3963)
        {
            throw ConcurrencyException();
        }
    }

    private static StudentApplicationAttachmentDto ToAttachmentDto(TepDinhKemDonTu attachment)
    {
        return new StudentApplicationAttachmentDto
        {
            MaTep = attachment.MaTep,
            TenFileGoc = attachment.TenFileGoc,
            ContentType = attachment.ContentType,
            KichThuocByte = attachment.KichThuocByte,
            NgayTao = attachment.NgayTao
        };
    }

    private static NhatKyDuyetDon CreateHiddenLog(DonTu application, int userId, object snapshot)
    {
        return new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = userId,
            NguonThucHien = "user",
            HanhDong = ApplicationActions.Update,
            TrangThaiCu = application.TrangThai,
            TrangThaiMoi = application.TrangThai,
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            HienThiChoHocSinh = false,
            NgayTao = DateTime.UtcNow
        };
    }

    private async Task CleanupUploadedObjectsAsync(IEnumerable<string> storageKeys)
    {
        foreach (var storageKey in storageKeys)
        {
            try
            {
                await _objectStore.DeleteAsync(storageKey, CancellationToken.None);
            }
            catch (ApplicationEvidenceObjectNotFoundException)
            {
                // Object may not have been created before the storage client reported failure.
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to cleanup uploaded application evidence object after failed DB commit.");
            }
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

    private sealed class InspectedFileCollection : IDisposable
    {
        public InspectedFileCollection(IReadOnlyList<InspectedApplicationEvidenceFile> items)
        {
            Items = items;
        }

        public IReadOnlyList<InspectedApplicationEvidenceFile> Items { get; }

        public void Dispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }
    }

    private sealed record EvidenceLimits(int MaxFiles, long MaxFileSizeBytes, long MaxTotalSizeBytes)
    {
        public static EvidenceLimits FromTemplate(MauDonTu template)
        {
            return new EvidenceLimits(
                Math.Min(template.SoTepToiDa, ApplicationEvidenceConstants.MaxFiles),
                Math.Min(template.DungLuongTepToiDaByte, ApplicationEvidenceConstants.MaxFileSizeBytes),
                Math.Min(template.TongDungLuongToiDaByte, ApplicationEvidenceConstants.MaxTotalSizeBytes));
        }
    }
}

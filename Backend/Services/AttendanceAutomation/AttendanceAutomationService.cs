using System.Globalization;
using Backend.Data;
using Backend.DTOs.AttendanceAutomation;
using Backend.DTOs.Notifications;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.AttendanceAutomation;

public class AttendanceAutomationService : IAttendanceAutomationService
{
    private const string CanceledSessionStatus = "da_huy";
    private const string AttendanceInProgressStatus = "dang_diem_danh";
    private const string AttendanceSubmittedStatus = "da_gui";
    private const string AttendanceLockedStatus = "da_khoa";
    private const string EntityType = "BuoiHoc";

    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly AttendanceAutomationOptions _options;
    private readonly ILogger<AttendanceAutomationService> _logger;

    public AttendanceAutomationService(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IOptions<AttendanceAutomationOptions> options,
        ILogger<AttendanceAutomationService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AttendanceAutomationRunResultDto> ProcessDueAttendanceAsync(
        DateTime? now = null,
        CancellationToken cancellationToken = default)
    {
        var processedAt = now ?? DateTime.UtcNow;
        var items = new List<AttendanceAutomationItemDto>();
        var autoSubmitted = 0;
        var autoLocked = 0;

        if (_options.AutoSubmitEnabled)
        {
            var dueSubmitSessionIds = await _context.BuoiHocs
                .AsNoTracking()
                .Where(x =>
                    x.TrangThaiDiemDanh == AttendanceInProgressStatus &&
                    x.DiemDanhHanGuiLuc.HasValue &&
                    x.DiemDanhHanGuiLuc.Value <= processedAt &&
                    x.TrangThaiBuoi != CanceledSessionStatus)
                .OrderBy(x => x.DiemDanhHanGuiLuc)
                .ThenBy(x => x.MaBuoiHoc)
                .Select(x => x.MaBuoiHoc)
                .Take(GetBatchSize())
                .ToListAsync(cancellationToken);

            foreach (var sessionId in dueSubmitSessionIds)
            {
                var item = await TryAutoSubmitAsync(sessionId, processedAt, cancellationToken);
                items.Add(item);
                if (item.Status == "processed")
                {
                    autoSubmitted++;
                }
            }
        }

        if (_options.AutoLockEnabled)
        {
            var dueLockSessionIds = await _context.BuoiHocs
                .AsNoTracking()
                .Where(x =>
                    x.TrangThaiDiemDanh == AttendanceSubmittedStatus &&
                    x.DiemDanhHanChinhSuaLuc.HasValue &&
                    x.DiemDanhHanChinhSuaLuc.Value <= processedAt &&
                    x.TrangThaiBuoi != CanceledSessionStatus &&
                    x.DiemDanhKhoaLuc == null)
                .OrderBy(x => x.DiemDanhHanChinhSuaLuc)
                .ThenBy(x => x.MaBuoiHoc)
                .Select(x => x.MaBuoiHoc)
                .Take(GetBatchSize())
                .ToListAsync(cancellationToken);

            foreach (var sessionId in dueLockSessionIds)
            {
                var item = await TryAutoLockAsync(sessionId, processedAt, cancellationToken);
                items.Add(item);
                if (item.Status == "processed")
                {
                    autoLocked++;
                }
            }
        }

        return new AttendanceAutomationRunResultDto
        {
            AutoSubmitted = autoSubmitted,
            AutoLocked = autoLocked,
            ProcessedAt = processedAt,
            Items = items
        };
    }

    private async Task<AttendanceAutomationItemDto> TryAutoSubmitAsync(
        int sessionId,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        const string action = "auto_submit";
        try
        {
            var (session, course) = await GetSessionAndCourseAsync(sessionId, cancellationToken);
            if (session is null || course is null)
            {
                return BuildItem(sessionId, 0, 0, action, "skipped", "Không tìm thấy buổi học hoặc khóa học.", processedAt);
            }

            if (!CanAutoSubmit(session, processedAt))
            {
                return BuildItem(session, action, "skipped", "Buổi học không còn thỏa điều kiện auto submit.", processedAt);
            }

            object? oldSnapshot = null;

            await _context.ExecuteInTransactionAsync(async () =>
            {
                oldSnapshot = ToSessionSnapshot(session);

                session.TrangThaiDiemDanh = AttendanceSubmittedStatus;
                session.DiemDanhDaGuiLuc ??= processedAt;
                session.DiemDanhHanChinhSuaLuc = processedAt.AddMinutes(GetLockAfterSubmitMinutes());
                session.NgayCapNhat = processedAt;

                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await WriteAuditAsync(
                "AUTO_SUBMIT_ATTENDANCE",
                session,
                course.MaDonVi,
                oldSnapshot,
                ToSessionSnapshot(session),
                "Tự động gửi điểm danh quá hạn.",
                cancellationToken);

            await TrySendTeacherNotificationAsync(
                session,
                course,
                "Điểm danh đã được tự động gửi",
                $"Điểm danh buổi học #{session.MaBuoiHoc} đã được hệ thống tự động gửi do quá hạn.",
                processedAt,
                cancellationToken);

            return BuildItem(session, action, "processed", "Đã tự động gửi điểm danh.", processedAt);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _context.ChangeTracker.Clear();
            _logger.LogWarning(exception, "Không thể auto submit điểm danh cho buổi học {SessionId}.", sessionId);
            return BuildItem(sessionId, 0, 0, action, "error", exception.Message, processedAt);
        }
    }

    private async Task<AttendanceAutomationItemDto> TryAutoLockAsync(
        int sessionId,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        const string action = "auto_lock";
        try
        {
            var (session, course) = await GetSessionAndCourseAsync(sessionId, cancellationToken);
            if (session is null || course is null)
            {
                return BuildItem(sessionId, 0, 0, action, "skipped", "Không tìm thấy buổi học hoặc khóa học.", processedAt);
            }

            if (!CanAutoLock(session, processedAt))
            {
                return BuildItem(session, action, "skipped", "Buổi học không còn thỏa điều kiện auto lock.", processedAt);
            }

            object? oldSnapshot = null;
            var lockedAttendanceCount = 0;
            await _context.ExecuteInTransactionAsync(async () =>
            {
                oldSnapshot = ToSessionSnapshot(session);

                session.TrangThaiDiemDanh = AttendanceLockedStatus;
                session.DiemDanhKhoaLuc = processedAt;
                session.NgayCapNhat = processedAt;

                var attendances = await _context.DiemDanhs
                    .Where(x => x.MaBuoiHoc == session.MaBuoiHoc && x.KhoaLuc == null)
                    .ToListAsync(cancellationToken);

                foreach (var attendance in attendances)
                {
                    attendance.KhoaLuc = processedAt;
                }

                lockedAttendanceCount = attendances.Count;
                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await WriteAuditAsync(
                "AUTO_LOCK_ATTENDANCE",
                session,
                course.MaDonVi,
                oldSnapshot,
                ToSessionSnapshot(session, lockedAttendanceCount),
                "Tự động khóa điểm danh sau hạn chỉnh sửa.",
                cancellationToken);

            await TrySendTeacherNotificationAsync(
                session,
                course,
                "Điểm danh đã được tự động khóa",
                $"Điểm danh buổi học #{session.MaBuoiHoc} đã được hệ thống tự động khóa sau hạn chỉnh sửa.",
                processedAt,
                cancellationToken);

            return BuildItem(session, action, "processed", "Đã tự động khóa điểm danh.", processedAt);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _context.ChangeTracker.Clear();
            _logger.LogWarning(exception, "Không thể auto lock điểm danh cho buổi học {SessionId}.", sessionId);
            return BuildItem(sessionId, 0, 0, action, "error", exception.Message, processedAt);
        }
    }

    private async Task<(Backend.Models.BuoiHoc? Session, KhoaHoc? Course)> GetSessionAndCourseAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _context.BuoiHocs
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == sessionId, cancellationToken);
        if (session is null)
        {
            return (null, null);
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc, cancellationToken);

        return (session, course);
    }

    private static bool CanAutoSubmit(Backend.Models.BuoiHoc session, DateTime now)
    {
        return session.TrangThaiDiemDanh == AttendanceInProgressStatus &&
            session.DiemDanhHanGuiLuc.HasValue &&
            session.DiemDanhHanGuiLuc.Value <= now &&
            session.TrangThaiBuoi != CanceledSessionStatus;
    }

    private static bool CanAutoLock(Backend.Models.BuoiHoc session, DateTime now)
    {
        return session.TrangThaiDiemDanh == AttendanceSubmittedStatus &&
            session.DiemDanhHanChinhSuaLuc.HasValue &&
            session.DiemDanhHanChinhSuaLuc.Value <= now &&
            session.TrangThaiBuoi != CanceledSessionStatus &&
            session.DiemDanhKhoaLuc == null;
    }

    private async Task WriteAuditAsync(
        string action,
        Backend.Models.BuoiHoc session,
        int organizationId,
        object? oldValue,
        object? newValue,
        string description,
        CancellationToken cancellationToken)
    {
        await _auditLogService.LogAsync(
            EntityType,
            session.MaBuoiHoc.ToString(CultureInfo.InvariantCulture),
            action,
            oldValue,
            newValue,
            null,
            organizationId,
            description,
            cancellationToken);
    }

    private async Task TrySendTeacherNotificationAsync(
        Backend.Models.BuoiHoc session,
        KhoaHoc course,
        string title,
        string content,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var teacherId = session.MaGiaoVienDayThay ?? session.MaGiaoVien;
            await _notificationService.SendToUsersAsync(
                new SystemNotificationRequest
                {
                    TieuDe = title,
                    TomTat = content,
                    NoiDungText = content,
                    LoaiThongBao = "system",
                    MucDo = "info",
                    DoiTuongLienKet = "buoi_hoc",
                    MaDoiTuongLienKet = session.MaBuoiHoc,
                    MaDonVi = course.MaDonVi,
                    NguoiTao = null
                },
                [teacherId],
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Không thể gửi thông báo automation điểm danh cho buổi học {SessionId} lúc {ProcessedAt}.",
                session.MaBuoiHoc,
                processedAt);
        }
    }

    private int GetBatchSize()
    {
        return _options.BatchSize <= 0 ? 100 : _options.BatchSize;
    }

    private int GetLockAfterSubmitMinutes()
    {
        return _options.LockAfterSubmitMinutes <= 0 ? 10 : _options.LockAfterSubmitMinutes;
    }

    private static AttendanceAutomationItemDto BuildItem(
        Backend.Models.BuoiHoc session,
        string action,
        string status,
        string message,
        DateTime processedAt)
    {
        return BuildItem(
            session.MaBuoiHoc,
            session.MaKhoaHoc,
            session.MaGiaoVienDayThay ?? session.MaGiaoVien,
            action,
            status,
            message,
            processedAt);
    }

    private static AttendanceAutomationItemDto BuildItem(
        int sessionId,
        int courseId,
        int teacherId,
        string action,
        string status,
        string message,
        DateTime processedAt)
    {
        return new AttendanceAutomationItemDto
        {
            MaBuoiHoc = sessionId,
            MaKhoaHoc = courseId,
            MaGiaoVien = teacherId,
            Action = action,
            Status = status,
            Message = message,
            ProcessedAt = processedAt
        };
    }

    private static object ToSessionSnapshot(Backend.Models.BuoiHoc session, int? lockedAttendanceRows = null)
    {
        return new
        {
            session.MaBuoiHoc,
            session.MaKhoaHoc,
            session.MaGiaoVien,
            session.MaGiaoVienDayThay,
            session.TrangThaiBuoi,
            session.TrangThaiDiemDanh,
            session.DiemDanhBatDauLuc,
            session.DiemDanhHanGuiLuc,
            session.DiemDanhDaGuiLuc,
            session.DiemDanhHanChinhSuaLuc,
            session.DiemDanhKhoaLuc,
            LockedAttendanceRows = lockedAttendanceRows
        };
    }
}

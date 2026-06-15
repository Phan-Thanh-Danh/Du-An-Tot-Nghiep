using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AttendanceUnlock;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AttendanceUnlock;

public class AttendanceUnlockService : IAttendanceUnlockService
{
    private const string CanceledSessionStatus = "da_huy";
    private const string AttendanceInProgressStatus = "dang_diem_danh";
    private const string AttendanceSubmittedStatus = "da_gui";
    private const string AttendanceLockedStatus = "da_khoa";
    private const string PendingStatus = "cho_duyet";
    private const string ApprovedStatus = "da_duyet";
    private const string RejectedStatus = "tu_choi";
    private const string ExpiredStatus = "het_han";

    private static readonly HashSet<string> ValidRequestStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PendingStatus,
        ApprovedStatus,
        RejectedStatus,
        ExpiredStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AttendanceUnlockService> _logger;

    public AttendanceUnlockService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        ILogger<AttendanceUnlockService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AttendanceUnlockRequestDto> CreateAsync(
        int sessionId,
        CreateAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.Teacher)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ giáo viên phụ trách buổi học được tạo yêu cầu mở khóa điểm danh.");
        }

        var reason = RequireTrimmed(request.LyDo, "Lý do mở khóa không được để trống.");
        var session = await _context.BuoiHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == sessionId, cancellationToken);

        if (session is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy buổi học.");
        }

        if (session.MaGiaoVien != currentUser.UserId && session.MaGiaoVienDayThay != currentUser.UserId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không phải giáo viên phụ trách buổi học này.");
        }

        if (session.TrangThaiBuoi == CanceledSessionStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể yêu cầu mở khóa điểm danh cho buổi học đã hủy.");
        }

        if (session.TrangThaiDiemDanh is not (AttendanceSubmittedStatus or AttendanceLockedStatus))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được yêu cầu mở khóa khi điểm danh đã gửi hoặc đã khóa.");
        }

        var hasPendingRequest = await _context.YeuCauMoKhoaDiemDanhs
            .AnyAsync(x => x.MaBuoiHoc == session.MaBuoiHoc && x.TrangThai == PendingStatus, cancellationToken);
        if (hasPendingRequest)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học đã có yêu cầu mở khóa đang chờ duyệt.");
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc, cancellationToken);
        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu khóa học của buổi học không hợp lệ.");
        }

        var now = DateTime.UtcNow;
        var unlockRequest = new YeuCauMoKhoaDiemDanh
        {
            MaBuoiHoc = session.MaBuoiHoc,
            NguoiYeuCau = currentUser.UserId,
            LyDo = reason,
            TrangThai = PendingStatus,
            NgayTao = now
        };

        await _context.YeuCauMoKhoaDiemDanhs.AddAsync(unlockRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(
            "CREATE_ATTENDANCE_UNLOCK_REQUEST",
            unlockRequest.MaYcMoKhoa,
            course.MaDonVi,
            null,
            ToUnlockAuditSnapshot(unlockRequest),
            currentUser,
            "Tạo yêu cầu mở khóa điểm danh.",
            cancellationToken);

        return await GetRequestDtoAsync(unlockRequest.MaYcMoKhoa, cancellationToken);
    }

    public async Task<PagedResultDto<AttendanceUnlockRequestDto>> GetTeacherRequestsAsync(
        AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.Teacher)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ giáo viên được xem danh sách yêu cầu mở khóa của mình.");
        }

        var query = ApplyFilters(CreateRequestQuery(), parameters)
            .Where(x =>
                x.Request.NguoiYeuCau == currentUser.UserId ||
                x.Session.MaGiaoVien == currentUser.UserId ||
                x.Session.MaGiaoVienDayThay == currentUser.UserId);

        return await ToPagedResultAsync(query, parameters, cancellationToken);
    }

    public async Task<PagedResultDto<AttendanceUnlockRequestDto>> GetAdminRequestsAsync(
        AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureAdminCanProcess(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyFilters(CreateRequestQuery(), parameters)
            .Where(x => allowedOrganizationIds.Contains(x.Course.MaDonVi));

        return await ToPagedResultAsync(query, parameters, cancellationToken);
    }

    public async Task<AttendanceUnlockRequestDto> GetAdminRequestByIdAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureAdminCanProcess(currentUser);
        var result = await CreateRequestQuery()
            .FirstOrDefaultAsync(x => x.Request.MaYcMoKhoa == requestId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy yêu cầu mở khóa điểm danh.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, result.Course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập yêu cầu thuộc cơ sở này.");
        }

        return ToDto(result);
    }

    public async Task<AttendanceUnlockRequestDto> ApproveAsync(
        int requestId,
        ApproveAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureAdminCanProcess(currentUser);
        var now = DateTime.UtcNow;
        var unlockRequest = await _context.YeuCauMoKhoaDiemDanhs
            .FirstOrDefaultAsync(x => x.MaYcMoKhoa == requestId, cancellationToken);

        if (unlockRequest is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy yêu cầu mở khóa điểm danh.");
        }

        var session = await _context.BuoiHocs
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == unlockRequest.MaBuoiHoc, cancellationToken);
        if (session is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu buổi học của yêu cầu không hợp lệ.");
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc, cancellationToken);
        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu khóa học của buổi học không hợp lệ.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xử lý yêu cầu thuộc cơ sở này.");
        }

        if (unlockRequest.TrangThai != PendingStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được duyệt yêu cầu đang chờ duyệt.");
        }

        object? oldSnapshot = null;
        await _context.ExecuteInTransactionAsync(async () =>
        {
            oldSnapshot = ToUnlockAuditSnapshot(unlockRequest, session);

            unlockRequest.TrangThai = ApprovedStatus;
            unlockRequest.NguoiDuyet = currentUser.UserId;
            unlockRequest.MoKhoaDenLuc = now.AddMinutes(10);
            unlockRequest.GhiChu = NormalizeOptional(request.GhiChu);
            unlockRequest.ThoiGianXuLy = now;

            session.TrangThaiDiemDanh = AttendanceInProgressStatus;
            session.DiemDanhHanGuiLuc = now.AddMinutes(10);
            session.DiemDanhHanChinhSuaLuc = now.AddMinutes(10);
            session.DiemDanhKhoaLuc = null;
            session.KhoaLuc = null;
            session.NgayCapNhat = now;

            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        await WriteAuditAsync(
            "APPROVE_ATTENDANCE_UNLOCK_REQUEST",
            unlockRequest.MaYcMoKhoa,
            course.MaDonVi,
            oldSnapshot,
            ToUnlockAuditSnapshot(unlockRequest, session),
            currentUser,
            "Duyệt yêu cầu mở khóa điểm danh.",
            cancellationToken);

        await TrySendUnlockNotificationAsync(
            unlockRequest,
            course,
            currentUser.UserId,
            "Yêu cầu mở khóa điểm danh đã được duyệt",
            $"Yêu cầu mở khóa điểm danh cho buổi học #{session.MaBuoiHoc} đã được duyệt. Bạn có 10 phút để chỉnh sửa và gửi lại.",
            "attendance_unlock_approved",
            "info",
            cancellationToken);

        return await GetRequestDtoAsync(unlockRequest.MaYcMoKhoa, cancellationToken);
    }

    public async Task<AttendanceUnlockRequestDto> RejectAsync(
        int requestId,
        RejectAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureAdminCanProcess(currentUser);
        var reason = RequireTrimmed(request.LyDoTuChoi, "Lý do từ chối không được để trống.");
        var now = DateTime.UtcNow;
        var unlockRequest = await _context.YeuCauMoKhoaDiemDanhs
            .FirstOrDefaultAsync(x => x.MaYcMoKhoa == requestId, cancellationToken);

        if (unlockRequest is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy yêu cầu mở khóa điểm danh.");
        }

        var session = await _context.BuoiHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == unlockRequest.MaBuoiHoc, cancellationToken);
        if (session is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu buổi học của yêu cầu không hợp lệ.");
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc, cancellationToken);
        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu khóa học của buổi học không hợp lệ.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xử lý yêu cầu thuộc cơ sở này.");
        }

        if (unlockRequest.TrangThai != PendingStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được từ chối yêu cầu đang chờ duyệt.");
        }

        var oldSnapshot = ToUnlockAuditSnapshot(unlockRequest, session);
        unlockRequest.TrangThai = RejectedStatus;
        unlockRequest.NguoiDuyet = currentUser.UserId;
        unlockRequest.LyDoTuChoi = reason;
        unlockRequest.ThoiGianXuLy = now;

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(
            "REJECT_ATTENDANCE_UNLOCK_REQUEST",
            unlockRequest.MaYcMoKhoa,
            course.MaDonVi,
            oldSnapshot,
            ToUnlockAuditSnapshot(unlockRequest, session),
            currentUser,
            "Từ chối yêu cầu mở khóa điểm danh.",
            cancellationToken);

        await TrySendUnlockNotificationAsync(
            unlockRequest,
            course,
            currentUser.UserId,
            "Yêu cầu mở khóa điểm danh bị từ chối",
            $"Yêu cầu mở khóa điểm danh cho buổi học #{session.MaBuoiHoc} đã bị từ chối.",
            "attendance_unlock_rejected",
            "warning",
            cancellationToken);

        return await GetRequestDtoAsync(unlockRequest.MaYcMoKhoa, cancellationToken);
    }

    private async Task TrySendUnlockNotificationAsync(
        YeuCauMoKhoaDiemDanh request,
        KhoaHoc course,
        int actorUserId,
        string title,
        string content,
        string notificationType,
        string level,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.SendToUsersAsync(
                new SystemNotificationRequest
                {
                    TieuDe = title,
                    TomTat = content,
                    NoiDungText = content,
                    LoaiThongBao = notificationType,
                    MucDo = level,
                    DoiTuongLienKet = "yeu_cau_mo_khoa_diem_danh",
                    MaDoiTuongLienKet = request.MaYcMoKhoa,
                    MaDonVi = course.MaDonVi,
                    NguoiTao = actorUserId
                },
                [request.NguoiYeuCau],
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Không thể gửi thông báo tự động cho yêu cầu mở khóa điểm danh {RequestId}.",
                request.MaYcMoKhoa);
        }
    }

    private IQueryable<UnlockRequestQueryResult> CreateRequestQuery()
    {
        return
            from request in _context.YeuCauMoKhoaDiemDanhs.AsNoTracking()
            join session in _context.BuoiHocs.AsNoTracking()
                on request.MaBuoiHoc equals session.MaBuoiHoc
            join course in _context.KhoaHocs.AsNoTracking()
                on session.MaKhoaHoc equals course.MaKhoaHoc
            join subject in _context.DanhMucMonHocs.AsNoTracking()
                on course.MaMonHoc equals subject.MaMonHoc
            join classEntity in _context.LopHanhChinhs.AsNoTracking()
                on course.MaLop equals classEntity.MaLop
            join term in _context.HocKys.AsNoTracking()
                on course.MaHocKy equals term.MaHocKy into termJoin
            from term in termJoin.DefaultIfEmpty()
            join shift in _context.CaHocs.AsNoTracking()
                on session.MaCaHoc equals shift.MaCaHoc
            join room in _context.PhongHocs.AsNoTracking()
                on session.MaPhong equals room.MaPhong
            join requester in _context.NguoiDungs.AsNoTracking()
                on request.NguoiYeuCau equals requester.MaNguoiDung
            join approver in _context.NguoiDungs.AsNoTracking()
                on request.NguoiDuyet equals approver.MaNguoiDung into approverJoin
            from approver in approverJoin.DefaultIfEmpty()
            select new UnlockRequestQueryResult
            {
                Request = request,
                Session = session,
                Course = course,
                Subject = subject,
                Class = classEntity,
                Term = term,
                Shift = shift,
                Room = room,
                Requester = requester,
                Approver = approver
            };
    }

    private static IQueryable<UnlockRequestQueryResult> ApplyFilters(
        IQueryable<UnlockRequestQueryResult> query,
        AttendanceUnlockQueryParameters parameters)
    {
        if (parameters.MaBuoiHoc.HasValue)
        {
            query = query.Where(x => x.Request.MaBuoiHoc == parameters.MaBuoiHoc.Value);
        }

        if (parameters.MaKhoaHoc.HasValue)
        {
            query = query.Where(x => x.Session.MaKhoaHoc == parameters.MaKhoaHoc.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeRequestStatus(parameters.TrangThai);
            query = query.Where(x => x.Request.TrangThai == status);
        }

        return query;
    }

    private static async Task<PagedResultDto<AttendanceUnlockRequestDto>> ToPagedResultAsync(
        IQueryable<UnlockRequestQueryResult> query,
        AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Request.NgayTao)
            .ThenByDescending(x => x.Request.MaYcMoKhoa)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AttendanceUnlockRequestDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    private async Task<AttendanceUnlockRequestDto> GetRequestDtoAsync(
        int requestId,
        CancellationToken cancellationToken)
    {
        var result = await CreateRequestQuery()
            .FirstOrDefaultAsync(x => x.Request.MaYcMoKhoa == requestId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy yêu cầu mở khóa điểm danh.");
        }

        return ToDto(result);
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }

    private static void EnsureAdminCanProcess(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xử lý yêu cầu mở khóa điểm danh.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _context.DonVis
                .AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            var organizations = await _context.DonVis
                .AsNoTracking()
                .Select(x => new { x.MaDonVi, x.MaDonViCha })
                .ToListAsync(cancellationToken);

            var allowedIds = new HashSet<int> { currentUser.CampusId };
            var queue = new Queue<int>();
            queue.Enqueue(currentUser.CampusId);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                foreach (var child in organizations.Where(x => x.MaDonViCha == parentId))
                {
                    if (allowedIds.Add(child.MaDonVi))
                    {
                        queue.Enqueue(child.MaDonVi);
                    }
                }
            }

            return allowedIds;
        }

        return new HashSet<int> { currentUser.CampusId };
    }

    private async Task<bool> CanAccessOrganizationAsync(
        CurrentUserContext currentUser,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        return allowedOrganizationIds.Contains(organizationId);
    }

    private Task WriteAuditAsync(
        string action,
        int entityId,
        int organizationId,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken)
    {
        return _auditLogService.LogAsync(
            "YeuCauMoKhoaDiemDanh",
            entityId.ToString(CultureInfo.InvariantCulture),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            organizationId,
            description,
            cancellationToken);
    }

    private static AttendanceUnlockRequestDto ToDto(UnlockRequestQueryResult result)
    {
        return new AttendanceUnlockRequestDto
        {
            MaYcMoKhoa = result.Request.MaYcMoKhoa,
            MaBuoiHoc = result.Request.MaBuoiHoc,
            MaKhoaHoc = result.Session.MaKhoaHoc,
            TieuDeKhoaHoc = result.Course.TieuDe,
            MaHocKy = result.Course.MaHocKy,
            TenHocKy = result.Term?.TenHocKy,
            MaLop = result.Course.MaLop,
            TenLop = result.Class.TenLop,
            MaMonHoc = result.Course.MaMonHoc,
            TenMonHoc = result.Subject.TenMonHoc,
            NgayHoc = result.Session.NgayHoc,
            MaCaHoc = result.Session.MaCaHoc,
            TenCa = result.Shift.TenCa,
            MaPhong = result.Session.MaPhong,
            TenPhong = result.Room.TenPhong,
            NguoiYeuCau = result.Request.NguoiYeuCau,
            TenNguoiYeuCau = result.Requester.HoTen,
            LyDo = result.Request.LyDo,
            TrangThai = result.Request.TrangThai,
            NguoiDuyet = result.Request.NguoiDuyet,
            TenNguoiDuyet = result.Approver?.HoTen,
            MoKhoaDenLuc = result.Request.MoKhoaDenLuc,
            NgayTao = result.Request.NgayTao,
            GhiChu = result.Request.GhiChu,
            LyDoTuChoi = result.Request.LyDoTuChoi,
            ThoiGianXuLy = result.Request.ThoiGianXuLy
        };
    }

    private static object ToUnlockAuditSnapshot(
        YeuCauMoKhoaDiemDanh request,
        Models.BuoiHoc? session = null)
    {
        return new
        {
            request.MaYcMoKhoa,
            request.MaBuoiHoc,
            request.NguoiYeuCau,
            request.LyDo,
            request.TrangThai,
            request.NguoiDuyet,
            request.MoKhoaDenLuc,
            request.GhiChu,
            request.LyDoTuChoi,
            request.ThoiGianXuLy,
            request.NgayTao,
            Session = session is null
                ? null
                : new
                {
                    session.TrangThaiDiemDanh,
                    session.DiemDanhHanGuiLuc,
                    session.DiemDanhHanChinhSuaLuc,
                    session.DiemDanhKhoaLuc,
                    session.KhoaLuc
                }
        };
    }

    private static string RequireTrimmed(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRequestStatus(string value)
    {
        var status = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái yêu cầu không được để trống.");
        }

        if (!ValidRequestStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái yêu cầu mở khóa không hợp lệ.");
        }

        return status;
    }

    private sealed class UnlockRequestQueryResult
    {
        public YeuCauMoKhoaDiemDanh Request { get; init; } = null!;
        public Models.BuoiHoc Session { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public HocKy? Term { get; init; }
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
        public NguoiDung Requester { get; init; } = null!;
        public NguoiDung? Approver { get; init; }
    }
}

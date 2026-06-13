using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.BuoiHoc;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.BuoiHoc;

public class BuoiHocService : IBuoiHocService
{
    private const string PublishedScheduleStatus = "da_xuat_ban";
    private const string DraftScheduleStatus = "nhap";
    private const string CanceledScheduleStatus = "da_huy";
    private const string ArchivedCourseStatus = "luu_tru";
    private const string ActiveRoomStatus = "hoat_dong";
    private const string PlannedSessionStatus = "du_kien";
    private const string CompletedSessionStatus = "da_dien_ra";
    private const string CanceledSessionStatus = "da_huy";
    private const string ChangedScheduleStatus = "doi_lich";
    private const string SubstituteTeacherStatus = "day_thay";
    private const string AttendanceNotOpenedStatus = "chua_mo";
    private const string AttendanceLockedStatus = "da_khoa";
    private const string ChangeTeacherType = "doi_giang_vien";
    private const string ChangeRoomType = "doi_phong";
    private const string ChangeShiftType = "doi_ca";
    private const string CancelSessionType = "huy_buoi";

    private static readonly HashSet<string> ValidSessionStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "du_kien",
        "da_dien_ra",
        "da_huy",
        "doi_lich",
        "day_thay"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public BuoiHocService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<BuoiHocDto>> GetAsync(
        BuoiHocQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(parameters.NgayTu, parameters.NgayDen);

        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyScope(CreateSessionQuery(), allowedOrganizationIds);

        if (parameters.MaTkb.HasValue)
        {
            query = query.Where(x => x.Session.MaTkb == parameters.MaTkb.Value);
        }

        if (parameters.MaKhoaHoc.HasValue)
        {
            query = query.Where(x => x.Session.MaKhoaHoc == parameters.MaKhoaHoc.Value);
        }

        if (parameters.MaGiaoVien.HasValue)
        {
            query = query.Where(x =>
                x.Session.MaGiaoVien == parameters.MaGiaoVien.Value ||
                x.Session.MaGiaoVienDayThay == parameters.MaGiaoVien.Value);
        }

        if (parameters.MaPhong.HasValue)
        {
            query = query.Where(x => x.Session.MaPhong == parameters.MaPhong.Value);
        }

        if (parameters.MaCaHoc.HasValue)
        {
            query = query.Where(x => x.Session.MaCaHoc == parameters.MaCaHoc.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThaiBuoi))
        {
            var status = NormalizeSessionStatus(parameters.TrangThaiBuoi);
            query = query.Where(x => x.Session.TrangThaiBuoi == status);
        }

        if (parameters.NgayTu.HasValue)
        {
            query = query.Where(x => x.Session.NgayHoc >= parameters.NgayTu.Value);
        }

        if (parameters.NgayDen.HasValue)
        {
            query = query.Where(x => x.Session.NgayHoc <= parameters.NgayDen.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var results = await query
            .OrderBy(x => x.Session.NgayHoc)
            .ThenBy(x => x.Shift.ThuTu)
            .ThenBy(x => x.Room.MaCodePhong)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BuoiHocDto>
        {
            Items = results.Select(ToDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<BuoiHocDetailDto> GetByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await ApplyScope(CreateSessionQuery(), allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.Session.MaBuoiHoc == sessionId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy buổi học.");
        }

        return ToDetailDto(result);
    }

    public async Task<GenerateSessionsResultDto> GenerateSessionsAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanGenerateSessions(currentUser);

        var schedule = await _context.ThoiKhoaBieus
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTkb == scheduleId, cancellationToken);

        if (schedule is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thời khóa biểu.");
        }

        var course = await ValidateCourseAsync(schedule.MaKhoaHoc, currentUser, cancellationToken);
        var shift = await ValidateShiftAsync(schedule.MaCaHoc, cancellationToken);
        var room = await ValidateRoomAsync(schedule.MaPhong, course.MaDonVi, currentUser, cancellationToken);
        var teacher = await ValidateTeacherAsync(course.MaGiaoVien, course.MaDonVi, cancellationToken);
        ValidateScheduleForGeneration(schedule);

        var targetDates = GetSessionDates(
            schedule.NgayBatDau!.Value,
            schedule.NgayKetThuc!.Value,
            schedule.ThuTrongTuan);

        var existingDates = await _context.BuoiHocs
            .AsNoTracking()
            .Where(x => x.MaTkb == schedule.MaTkb && targetDates.Contains(x.NgayHoc))
            .Select(x => x.NgayHoc)
            .ToHashSetAsync(cancellationToken);

        var sessionsToCreate = targetDates
            .Where(date => !existingDates.Contains(date))
            .Select(date => new Models.BuoiHoc
            {
                MaTkb = schedule.MaTkb,
                MaKhoaHoc = schedule.MaKhoaHoc,
                NgayHoc = date,
                MaCaHoc = shift.MaCaHoc,
                MaPhong = room.MaPhong,
                MaGiaoVien = teacher.MaNguoiDung,
                MaGiaoVienDayThay = null,
                TrangThaiBuoi = PlannedSessionStatus,
                TrangThaiDiemDanh = AttendanceNotOpenedStatus,
                LoaiThayDoi = null,
                LyDoThayDoi = null,
                GhiChu = null,
                KhoaLuc = null,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            })
            .ToList();

        if (sessionsToCreate.Count > 0)
        {
            await _context.BuoiHocs.AddRangeAsync(sessionsToCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var createdIds = sessionsToCreate.Select(x => x.MaBuoiHoc).ToList();
        var createdSessions = createdIds.Count == 0
            ? new List<SessionQueryResult>()
            : await CreateSessionQuery()
                .Where(x => createdIds.Contains(x.Session.MaBuoiHoc))
                .OrderBy(x => x.Session.NgayHoc)
                .ThenBy(x => x.Shift.ThuTu)
                .ToListAsync(cancellationToken);

        var result = new GenerateSessionsResultDto
        {
            MaTkb = schedule.MaTkb,
            TotalDates = targetDates.Count,
            Created = sessionsToCreate.Count,
            SkippedExisting = existingDates.Count,
            Sessions = createdSessions.Select(ToDto).ToList()
        };

        await _auditLogService.LogAsync(
            "BuoiHoc",
            schedule.MaTkb.ToString(CultureInfo.InvariantCulture),
            "GENERATE_BUOI_HOC",
            null,
            new
            {
                schedule.MaTkb,
                schedule.MaKhoaHoc,
                schedule.ThuTrongTuan,
                schedule.MaCaHoc,
                schedule.MaPhong,
                schedule.NgayBatDau,
                schedule.NgayKetThuc,
                result.TotalDates,
                result.Created,
                result.SkippedExisting
            },
            currentUser.UserId,
            course.MaDonVi,
            "Sinh buổi học từ thời khóa biểu.",
            cancellationToken);

        return result;
    }

    public async Task<BuoiHocDetailDto> ChangeTeacherAsync(
        int sessionId,
        ChangeSessionTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSessions(currentUser);

        var reason = NormalizeReason(request.LyDoThayDoi);
        var (session, course) = await GetManagedSessionAsync(sessionId, currentUser, cancellationToken);
        ValidateSessionCanBeChanged(session);

        if (request.MaGiaoVienDayThay == session.MaGiaoVien)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên dạy thay không được trùng với giáo viên chính.");
        }

        var substituteTeacher = await ValidateTeacherAsync(
            request.MaGiaoVienDayThay,
            course.MaDonVi,
            cancellationToken);

        await EnsureNoSessionConflictAsync(
            session,
            course,
            session.MaCaHoc,
            substituteTeacher.MaNguoiDung,
            session.MaPhong,
            checkTeacher: true,
            checkClass: false,
            checkRoom: false,
            cancellationToken);

        var oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);

        session.MaGiaoVienDayThay = substituteTeacher.MaNguoiDung;
        session.TrangThaiBuoi = SubstituteTeacherStatus;
        session.LoaiThayDoi = ChangeTeacherType;
        session.LyDoThayDoi = reason;
        session.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "CHANGE_BUOI_HOC_TEACHER",
            session.MaBuoiHoc,
            course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Đổi giáo viên dạy thay cho buổi học.",
            cancellationToken);

        return await GetByIdAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<BuoiHocDetailDto> ChangeRoomAsync(
        int sessionId,
        ChangeSessionRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSessions(currentUser);

        var reason = NormalizeReason(request.LyDoThayDoi);
        var (session, course) = await GetManagedSessionAsync(sessionId, currentUser, cancellationToken);
        ValidateSessionCanBeChanged(session);
        var room = await ValidateRoomAsync(request.MaPhong, course.MaDonVi, currentUser, cancellationToken);

        await EnsureNoSessionConflictAsync(
            session,
            course,
            session.MaCaHoc,
            session.MaGiaoVienDayThay ?? session.MaGiaoVien,
            room.MaPhong,
            checkTeacher: false,
            checkClass: false,
            checkRoom: true,
            cancellationToken);

        var oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);

        session.MaPhong = room.MaPhong;
        session.TrangThaiBuoi = ChangedScheduleStatus;
        session.LoaiThayDoi = ChangeRoomType;
        session.LyDoThayDoi = reason;
        session.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "CHANGE_BUOI_HOC_ROOM",
            session.MaBuoiHoc,
            course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Đổi phòng cho buổi học.",
            cancellationToken);

        return await GetByIdAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<BuoiHocDetailDto> ChangeShiftAsync(
        int sessionId,
        ChangeSessionShiftRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSessions(currentUser);

        var reason = NormalizeReason(request.LyDoThayDoi);
        var (session, course) = await GetManagedSessionAsync(sessionId, currentUser, cancellationToken);
        ValidateSessionCanBeChanged(session);
        var shift = await ValidateShiftAsync(request.MaCaHoc, cancellationToken);

        await EnsureNoSessionConflictAsync(
            session,
            course,
            shift.MaCaHoc,
            session.MaGiaoVienDayThay ?? session.MaGiaoVien,
            session.MaPhong,
            checkTeacher: true,
            checkClass: true,
            checkRoom: true,
            cancellationToken);

        var oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);

        session.MaCaHoc = shift.MaCaHoc;
        session.TrangThaiBuoi = ChangedScheduleStatus;
        session.LoaiThayDoi = ChangeShiftType;
        session.LyDoThayDoi = reason;
        session.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "CHANGE_BUOI_HOC_SHIFT",
            session.MaBuoiHoc,
            course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Đổi ca cho buổi học.",
            cancellationToken);

        return await GetByIdAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<BuoiHocDetailDto> CancelAsync(
        int sessionId,
        CancelSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSessions(currentUser);

        var reason = NormalizeReason(request.LyDoThayDoi);
        var (session, course) = await GetManagedSessionAsync(sessionId, currentUser, cancellationToken);
        ValidateSessionCanBeChanged(session);
        var oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);

        session.TrangThaiBuoi = CanceledSessionStatus;
        session.LoaiThayDoi = CancelSessionType;
        session.LyDoThayDoi = reason;
        session.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "CANCEL_BUOI_HOC",
            session.MaBuoiHoc,
            course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Hủy buổi học.",
            cancellationToken);

        return await GetByIdAsync(session.MaBuoiHoc, cancellationToken);
    }

    private IQueryable<SessionQueryResult> CreateSessionQuery()
    {
        return
            from session in _context.BuoiHocs.AsNoTracking()
            join schedule in _context.ThoiKhoaBieus.AsNoTracking()
                on session.MaTkb equals schedule.MaTkb
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
            join teacher in _context.NguoiDungs.AsNoTracking()
                on session.MaGiaoVien equals teacher.MaNguoiDung
            join substituteTeacher in _context.NguoiDungs.AsNoTracking()
                on session.MaGiaoVienDayThay equals substituteTeacher.MaNguoiDung into substituteTeacherJoin
            from substituteTeacher in substituteTeacherJoin.DefaultIfEmpty()
            select new SessionQueryResult
            {
                Session = session,
                Schedule = schedule,
                Course = course,
                Subject = subject,
                Class = classEntity,
                Term = term,
                Shift = shift,
                Room = room,
                Teacher = teacher,
                SubstituteTeacher = substituteTeacher
            };
    }

    private static IQueryable<SessionQueryResult> ApplyScope(
        IQueryable<SessionQueryResult> query,
        HashSet<int> allowedOrganizationIds)
    {
        return query.Where(x => allowedOrganizationIds.Contains(x.Course.MaDonVi));
    }

    private async Task<KhoaHoc> ValidateCourseAsync(
        int courseId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == courseId, cancellationToken);

        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học của thời khóa biểu không tồn tại.");
        }

        if (course.TrangThai == ArchivedCourseStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học đã lưu trữ, không thể sinh buổi học.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền sinh buổi học cho cơ sở này.");
        }

        return course;
    }

    private async Task<Models.CaHoc> ValidateShiftAsync(int shiftId, CancellationToken cancellationToken)
    {
        var shift = await _context.CaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaCaHoc == shiftId, cancellationToken);

        if (shift is null || !shift.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ca học của thời khóa biểu không tồn tại hoặc không hoạt động.");
        }

        return shift;
    }

    private async Task<PhongHoc> ValidateRoomAsync(
        int roomId,
        int courseOrganizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var room = await _context.PhongHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaPhong == roomId, cancellationToken);

        if (room is null || room.TrangThaiPhong != ActiveRoomStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học của thời khóa biểu không tồn tại hoặc không hoạt động.");
        }

        if (room.MaDonVi != courseOrganizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học không thuộc cùng cơ sở với khóa học.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, room.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền sinh buổi học cho phòng học này.");
        }

        return room;
    }

    private async Task<NguoiDung> ValidateTeacherAsync(
        int teacherId,
        int courseOrganizationId,
        CancellationToken cancellationToken)
    {
        var teacher = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == teacherId, cancellationToken);

        if (teacher is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên của khóa học không tồn tại.");
        }

        if (teacher.VaiTroChinh != AuthRoles.ToDatabaseCode(AuthRoles.Teacher))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Người dùng được gán cho khóa học không phải giáo viên.");
        }

        if (teacher.TrangThai == UserStatuses.DbLocked)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên của khóa học đang bị khóa.");
        }

        if (teacher.MaDonVi != courseOrganizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên không thuộc cùng cơ sở với khóa học.");
        }

        return teacher;
    }

    private static void ValidateScheduleForGeneration(Models.ThoiKhoaBieu schedule)
    {
        if (schedule.TrangThai == DraftScheduleStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được sinh buổi học khi thời khóa biểu đã xuất bản.");
        }

        if (schedule.TrangThai == CanceledScheduleStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể sinh buổi học từ thời khóa biểu đã hủy.");
        }

        if (schedule.TrangThai != PublishedScheduleStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được sinh buổi học khi thời khóa biểu đã xuất bản.");
        }

        if (!schedule.NgayBatDau.HasValue || !schedule.NgayKetThuc.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thời khóa biểu phải có ngày bắt đầu và ngày kết thúc trước khi sinh buổi học.");
        }

        if (schedule.NgayKetThuc.Value < schedule.NgayBatDau.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày kết thúc thời khóa biểu không được trước ngày bắt đầu.");
        }

        if (schedule.ThuTrongTuan is < 1 or > 7)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thứ trong tuần của thời khóa biểu phải từ 1 đến 7.");
        }
    }

    private static List<DateOnly> GetSessionDates(
        DateOnly startDate,
        DateOnly endDate,
        int vietnameseDayOfWeek)
    {
        var dates = new List<DateOnly>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (ToVietnameseDayOfWeek(date.DayOfWeek) == vietnameseDayOfWeek)
            {
                dates.Add(date);
            }
        }

        return dates;
    }

    private static int ToVietnameseDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 1 : (int)dayOfWeek + 1;
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

    private static void EnsureCanGenerateSessions(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền sinh buổi học.");
        }
    }

    private static void EnsureCanManageSessions(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền điều chỉnh buổi học.");
        }
    }

    private static string NormalizeReason(string reason)
    {
        var normalizedReason = reason.Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lý do thay đổi không được để trống.");
        }

        return normalizedReason;
    }

    private async Task<(Models.BuoiHoc Session, KhoaHoc Course)> GetManagedSessionAsync(
        int sessionId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var session = await _context.BuoiHocs
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == sessionId, cancellationToken);

        if (session is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy buổi học.");
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == session.MaKhoaHoc, cancellationToken);

        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học của buổi học không tồn tại.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền điều chỉnh buổi học thuộc cơ sở này.");
        }

        return (session, course);
    }

    private static void ValidateSessionCanBeChanged(Models.BuoiHoc session)
    {
        if (session.TrangThaiBuoi == CanceledSessionStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học đã hủy, không thể điều chỉnh.");
        }

        if (session.TrangThaiBuoi == CompletedSessionStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học đã diễn ra, không thể điều chỉnh.");
        }

        if (session.TrangThaiDiemDanh == AttendanceLockedStatus || session.DiemDanhKhoaLuc.HasValue || session.KhoaLuc.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học đã khóa điểm danh hoặc đã khóa lịch, không thể điều chỉnh.");
        }
    }

    private async Task EnsureNoSessionConflictAsync(
        Models.BuoiHoc session,
        KhoaHoc course,
        int targetShiftId,
        int targetTeacherId,
        int targetRoomId,
        bool checkTeacher,
        bool checkClass,
        bool checkRoom,
        CancellationToken cancellationToken)
    {
        var existingSessions = await CreateSessionQuery()
            .Where(x =>
                x.Session.MaBuoiHoc != session.MaBuoiHoc &&
                x.Session.TrangThaiBuoi != CanceledSessionStatus &&
                x.Session.NgayHoc == session.NgayHoc &&
                x.Session.MaCaHoc == targetShiftId)
            .ToListAsync(cancellationToken);

        var conflicts = new List<SessionConflictDto>();

        foreach (var existing in existingSessions)
        {
            if (checkTeacher)
            {
                var effectiveTeacherId = existing.Session.MaGiaoVienDayThay ?? existing.Session.MaGiaoVien;
                if (effectiveTeacherId == targetTeacherId)
                {
                    conflicts.Add(ToConflictDto(
                        "teacher",
                        "Giáo viên đã có buổi học cùng ngày và ca.",
                        existing));
                }
            }

            if (checkClass && existing.Course.MaLop == course.MaLop)
            {
                conflicts.Add(ToConflictDto(
                    "class",
                    "Lớp hành chính đã có buổi học cùng ngày và ca.",
                    existing));
            }

            if (checkRoom && existing.Session.MaPhong == targetRoomId)
            {
                conflicts.Add(ToConflictDto(
                    "room",
                    "Phòng học đã được sử dụng cùng ngày và ca.",
                    existing));
            }
        }

        if (conflicts.Count > 0)
        {
            throw new SessionConflictException(new SessionConflictResultDto
            {
                Conflicts = conflicts
            });
        }
    }

    private static SessionConflictDto ToConflictDto(
        string type,
        string message,
        SessionQueryResult result)
    {
        var effectiveTeacherId = result.Session.MaGiaoVienDayThay ?? result.Session.MaGiaoVien;
        var effectiveTeacherName = result.SubstituteTeacher?.HoTen ?? result.Teacher.HoTen;

        return new SessionConflictDto
        {
            Type = type,
            Message = message,
            MaBuoiHoc = result.Session.MaBuoiHoc,
            MaKhoaHoc = result.Session.MaKhoaHoc,
            TenKhoaHoc = result.Course.TieuDe,
            NgayHoc = result.Session.NgayHoc,
            MaCaHoc = result.Session.MaCaHoc,
            TenCa = result.Shift.TenCa,
            MaPhong = result.Session.MaPhong,
            TenPhong = result.Room.TenPhong,
            MaGiaoVien = result.Session.MaGiaoVien,
            TenGiaoVien = result.Teacher.HoTen,
            MaGiaoVienDayThay = result.Session.MaGiaoVienDayThay,
            TenGiaoVienDayThay = result.SubstituteTeacher?.HoTen,
            EffectiveTeacherId = effectiveTeacherId,
            EffectiveTeacherName = effectiveTeacherName,
            MaLop = result.Course.MaLop,
            TenLop = result.Class.TenLop
        };
    }

    private async Task<object?> GetAuditSnapshotAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var result = await CreateSessionQuery()
            .FirstOrDefaultAsync(x => x.Session.MaBuoiHoc == sessionId, cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new
        {
            result.Session.MaBuoiHoc,
            result.Session.MaTkb,
            result.Session.MaKhoaHoc,
            result.Course.TieuDe,
            result.Session.NgayHoc,
            result.Session.MaCaHoc,
            result.Shift.TenCa,
            result.Session.MaPhong,
            result.Room.TenPhong,
            result.Session.MaGiaoVien,
            TenGiaoVien = result.Teacher.HoTen,
            result.Session.MaGiaoVienDayThay,
            TenGiaoVienDayThay = result.SubstituteTeacher?.HoTen,
            result.Session.TrangThaiBuoi,
            result.Session.TrangThaiDiemDanh,
            result.Session.LoaiThayDoi,
            result.Session.LyDoThayDoi,
            result.Session.KhoaLuc,
            result.Session.DiemDanhKhoaLuc,
            result.Session.NgayCapNhat
        };
    }

    private Task WriteAuditAsync(
        string action,
        int sessionId,
        int organizationId,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken)
    {
        return _auditLogService.LogAsync(
            "BuoiHoc",
            sessionId.ToString(CultureInfo.InvariantCulture),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            organizationId,
            description,
            cancellationToken);
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

        if (currentUser.Role is AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin)
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

    private static string NormalizeSessionStatus(string value)
    {
        var status = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái buổi học không được để trống.");
        }

        if (!ValidSessionStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái buổi học không hợp lệ.");
        }

        return status;
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    }

    private static BuoiHocDto ToDto(SessionQueryResult result)
    {
        return new BuoiHocDto
        {
            MaBuoiHoc = result.Session.MaBuoiHoc,
            MaTkb = result.Session.MaTkb,
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
            GioBatDau = FormatTime(result.Shift.GioBatDau),
            GioKetThuc = FormatTime(result.Shift.GioKetThuc),
            MaPhong = result.Session.MaPhong,
            TenPhong = result.Room.TenPhong,
            MaCodePhong = result.Room.MaCodePhong,
            MaGiaoVien = result.Session.MaGiaoVien,
            TenGiaoVien = result.Teacher.HoTen,
            MaGiaoVienDayThay = result.Session.MaGiaoVienDayThay,
            TenGiaoVienDayThay = result.SubstituteTeacher?.HoTen,
            TrangThaiBuoi = result.Session.TrangThaiBuoi,
            TrangThaiDiemDanh = result.Session.TrangThaiDiemDanh,
            LoaiThayDoi = result.Session.LoaiThayDoi,
            LyDoThayDoi = result.Session.LyDoThayDoi,
            GhiChu = result.Session.GhiChu,
            KhoaLuc = result.Session.KhoaLuc,
            NgayTao = result.Session.NgayTao,
            NgayCapNhat = result.Session.NgayCapNhat
        };
    }

    private static BuoiHocDetailDto ToDetailDto(SessionQueryResult result)
    {
        var dto = ToDto(result);
        return new BuoiHocDetailDto
        {
            MaBuoiHoc = dto.MaBuoiHoc,
            MaTkb = dto.MaTkb,
            MaKhoaHoc = dto.MaKhoaHoc,
            TieuDeKhoaHoc = dto.TieuDeKhoaHoc,
            MaHocKy = dto.MaHocKy,
            TenHocKy = dto.TenHocKy,
            MaLop = dto.MaLop,
            TenLop = dto.TenLop,
            MaMonHoc = dto.MaMonHoc,
            TenMonHoc = dto.TenMonHoc,
            NgayHoc = dto.NgayHoc,
            MaCaHoc = dto.MaCaHoc,
            TenCa = dto.TenCa,
            GioBatDau = dto.GioBatDau,
            GioKetThuc = dto.GioKetThuc,
            MaPhong = dto.MaPhong,
            TenPhong = dto.TenPhong,
            MaCodePhong = dto.MaCodePhong,
            MaGiaoVien = dto.MaGiaoVien,
            TenGiaoVien = dto.TenGiaoVien,
            MaGiaoVienDayThay = dto.MaGiaoVienDayThay,
            TenGiaoVienDayThay = dto.TenGiaoVienDayThay,
            TrangThaiBuoi = dto.TrangThaiBuoi,
            TrangThaiDiemDanh = dto.TrangThaiDiemDanh,
            LoaiThayDoi = dto.LoaiThayDoi,
            LyDoThayDoi = dto.LyDoThayDoi,
            GhiChu = dto.GhiChu,
            KhoaLuc = dto.KhoaLuc,
            NgayTao = dto.NgayTao,
            NgayCapNhat = dto.NgayCapNhat
        };
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private sealed class SessionQueryResult
    {
        public Models.BuoiHoc Session { get; init; } = null!;
        public Models.ThoiKhoaBieu Schedule { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public HocKy? Term { get; init; }
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public NguoiDung? SubstituteTeacher { get; init; }
    }
}

using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Attendance;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Attendance;

public class AttendanceService : IAttendanceService
{
    private const string CanceledSessionStatus = "da_huy";
    private const string AttendanceNotOpenedStatus = "chua_mo";
    private const string AttendanceInProgressStatus = "dang_diem_danh";
    private const string AttendanceSubmittedStatus = "da_gui";
    private const string AttendanceLockedStatus = "da_khoa";
    private const string DefaultAttendanceStatus = "vang";

    private static readonly HashSet<string> ValidAttendanceStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "co_mat",
        "vang",
        "di_muon",
        "co_phep"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public AttendanceService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<AttendanceSessionDto>> GetTeacherTodayAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.Teacher)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ giáo viên được xem danh sách buổi học cần điểm danh hôm nay.");
        }

        var today = DateOnly.FromDateTime(GetVietnamNow());
        var sessions = await CreateSessionQuery()
            .Where(x =>
                x.Session.NgayHoc == today &&
                (x.Session.MaGiaoVien == currentUser.UserId || x.Session.MaGiaoVienDayThay == currentUser.UserId))
            .OrderBy(x => x.Shift.ThuTu)
            .ThenBy(x => x.Room.MaCodePhong)
            .ToListAsync(cancellationToken);

        return sessions.Select(ToSessionDto).ToList();
    }

    public async Task<AttendanceDetailDto> StartAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var (session, queryResult) = await GetSessionForMutationAsync(sessionId, currentUser, cancellationToken);
        EnsureTeacherCanManageAttendance(currentUser, session);
        EnsureSessionCanStartAttendance(session);

        object? oldSnapshot = null;
        await _context.ExecuteInTransactionAsync(async () =>
        {
            oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);
            var studentIds = await GetActiveStudentIdsAsync(queryResult.Course, cancellationToken);
            if (studentIds.Count == 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Lớp chưa có sinh viên hoạt động để điểm danh.");
            }

            var existingStudentIds = await _context.DiemDanhs
                .Where(x => x.MaBuoiHoc == session.MaBuoiHoc)
                .Select(x => x.MaHocSinh)
                .ToHashSetAsync(cancellationToken);

            var missingAttendances = studentIds
                .Where(studentId => !existingStudentIds.Contains(studentId))
                .Select(studentId => new DiemDanh
                {
                    MaDonVi = queryResult.Course.MaDonVi,
                    MaBuoiHoc = session.MaBuoiHoc,
                    MaHocSinh = studentId,
                    TrangThai = DefaultAttendanceStatus,
                    NguoiGhiNhan = currentUser.UserId,
                    GhiNhanLuc = now,
                    HeSoVang = 1,
                    KhoaLuc = null,
                    MaYcMoKhoa = null
                })
                .ToList();

            if (missingAttendances.Count > 0)
            {
                await _context.DiemDanhs.AddRangeAsync(missingAttendances, cancellationToken);
            }

            if (session.TrangThaiDiemDanh == AttendanceNotOpenedStatus)
            {
                session.TrangThaiDiemDanh = AttendanceInProgressStatus;
                session.DiemDanhBatDauLuc = now;
                session.DiemDanhHanGuiLuc = now.AddMinutes(15);
                session.NgayCapNhat = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        await WriteAuditAsync(
            "START_ATTENDANCE",
            session.MaBuoiHoc,
            queryResult.Course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Mở điểm danh buổi học.",
            cancellationToken);

        return await GetAttendanceDetailForCurrentUserAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<AttendanceDetailDto> GetSessionAttendanceAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await GetAttendanceDetailForCurrentUserAsync(sessionId, cancellationToken);
    }

    public async Task<AttendanceStudentDto> UpdateStudentAsync(
        int sessionId,
        int studentId,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var status = NormalizeAttendanceStatus(request.TrangThai);
        var (session, queryResult) = await GetSessionForMutationAsync(sessionId, currentUser, cancellationToken);
        EnsureTeacherCanManageAttendance(currentUser, session);
        EnsureSessionCanUpdateAttendance(session, now);

        var attendance = await _context.DiemDanhs
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == session.MaBuoiHoc && x.MaHocSinh == studentId, cancellationToken);

        if (attendance is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Sinh viên không thuộc danh sách điểm danh của buổi học.");
        }

        var oldSnapshot = ToAttendanceAuditSnapshot(attendance);
        attendance.TrangThai = status;
        attendance.HeSoVang = GetAbsenceWeight(status);
        attendance.NguoiGhiNhan = currentUser.UserId;
        attendance.GhiNhanLuc = now;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "UPDATE_ATTENDANCE",
            attendance.MaDiemDanh,
            queryResult.Course.MaDonVi,
            oldSnapshot,
            ToAttendanceAuditSnapshot(attendance),
            currentUser,
            "Cập nhật điểm danh một sinh viên.",
            cancellationToken,
            "DiemDanh");

        return await GetAttendanceStudentDtoAsync(attendance.MaDiemDanh, cancellationToken);
    }

    public async Task<AttendanceDetailDto> BulkUpdateAsync(
        int sessionId,
        BulkUpdateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var items = ValidateBulkRequest(request);
        var (session, queryResult) = await GetSessionForMutationAsync(sessionId, currentUser, cancellationToken);
        EnsureTeacherCanManageAttendance(currentUser, session);
        EnsureSessionCanUpdateAttendance(session, now);

        var requestedStudentIds = items.Select(x => x.MaSinhVien).ToHashSet();
        var attendances = await _context.DiemDanhs
            .Where(x => x.MaBuoiHoc == session.MaBuoiHoc && requestedStudentIds.Contains(x.MaHocSinh))
            .ToListAsync(cancellationToken);

        if (attendances.Count != requestedStudentIds.Count)
        {
            var foundStudentIds = attendances.Select(x => x.MaHocSinh).ToHashSet();
            var missingStudentIds = requestedStudentIds.Where(x => !foundStudentIds.Contains(x)).OrderBy(x => x).ToList();
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Sinh viên không thuộc danh sách điểm danh của buổi học: {string.Join(", ", missingStudentIds)}.");
        }

        List<object> oldSnapshot = [];
        await _context.ExecuteInTransactionAsync(async () =>
        {
            oldSnapshot = attendances.Select(ToAttendanceAuditSnapshot).ToList();
            var itemByStudent = items.ToDictionary(x => x.MaSinhVien);

            foreach (var attendance in attendances)
            {
                var status = itemByStudent[attendance.MaHocSinh].TrangThai;
                attendance.TrangThai = status;
                attendance.HeSoVang = GetAbsenceWeight(status);
                attendance.NguoiGhiNhan = currentUser.UserId;
                attendance.GhiNhanLuc = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        await WriteAuditAsync(
            "BULK_UPDATE_ATTENDANCE",
            session.MaBuoiHoc,
            queryResult.Course.MaDonVi,
            oldSnapshot,
            attendances.Select(ToAttendanceAuditSnapshot).ToList(),
            currentUser,
            $"Cập nhật điểm danh hàng loạt {attendances.Count} sinh viên.",
            cancellationToken);

        return await GetAttendanceDetailForCurrentUserAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<AttendanceDetailDto> SubmitAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var (session, queryResult) = await GetSessionForMutationAsync(sessionId, currentUser, cancellationToken);
        EnsureTeacherCanManageAttendance(currentUser, session);
        EnsureSessionCanSubmitAttendance(session, now);

        var oldSnapshot = await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken);
        session.TrangThaiDiemDanh = AttendanceSubmittedStatus;
        session.DiemDanhDaGuiLuc = now;
        session.DiemDanhHanChinhSuaLuc = now.AddMinutes(10);
        session.NgayCapNhat = now;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            "SUBMIT_ATTENDANCE",
            session.MaBuoiHoc,
            queryResult.Course.MaDonVi,
            oldSnapshot,
            await GetAuditSnapshotAsync(session.MaBuoiHoc, cancellationToken),
            currentUser,
            "Gửi điểm danh buổi học.",
            cancellationToken);

        return await GetAttendanceDetailForCurrentUserAsync(session.MaBuoiHoc, cancellationToken);
    }

    public async Task<PagedResultDto<StudentAttendanceDto>> GetStudentAttendanceAsync(
        StudentAttendanceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(parameters.NgayTu, parameters.NgayDen);
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.Student)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ sinh viên được xem điểm danh của chính mình.");
        }

        var query = CreateAttendanceQuery()
            .Where(x => x.Attendance.MaHocSinh == currentUser.UserId);

        if (parameters.NgayTu.HasValue)
        {
            query = query.Where(x => x.Session.NgayHoc >= parameters.NgayTu.Value);
        }

        if (parameters.NgayDen.HasValue)
        {
            query = query.Where(x => x.Session.NgayHoc <= parameters.NgayDen.Value);
        }

        if (parameters.MaKhoaHoc.HasValue)
        {
            query = query.Where(x => x.Session.MaKhoaHoc == parameters.MaKhoaHoc.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeAttendanceStatus(parameters.TrangThai);
            query = query.Where(x => x.Attendance.TrangThai == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Session.NgayHoc)
            .ThenBy(x => x.Shift.ThuTu)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<StudentAttendanceDto>
        {
            Items = items.Select(ToStudentAttendanceDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<SessionQueryResult> CreateSessionQuery()
    {
        return
            from session in _context.BuoiHocs.AsNoTracking()
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

    private IQueryable<AttendanceQueryResult> CreateAttendanceQuery()
    {
        return
            from attendance in _context.DiemDanhs.AsNoTracking()
            join session in _context.BuoiHocs.AsNoTracking()
                on attendance.MaBuoiHoc equals session.MaBuoiHoc
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
            join student in _context.NguoiDungs.AsNoTracking()
                on attendance.MaHocSinh equals student.MaNguoiDung
            join recorder in _context.NguoiDungs.AsNoTracking()
                on attendance.NguoiGhiNhan equals recorder.MaNguoiDung
            select new AttendanceQueryResult
            {
                Attendance = attendance,
                Session = session,
                Course = course,
                Subject = subject,
                Class = classEntity,
                Term = term,
                Shift = shift,
                Room = room,
                Student = student,
                Recorder = recorder
            };
    }

    private async Task<AttendanceDetailDto> GetAttendanceDetailForCurrentUserAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        var sessionResult = await CreateSessionQuery()
            .FirstOrDefaultAsync(x => x.Session.MaBuoiHoc == sessionId, cancellationToken);

        if (sessionResult is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy buổi học.");
        }

        await EnsureCanViewSessionAttendanceAsync(currentUser, sessionResult, cancellationToken);

        var students = await CreateAttendanceQuery()
            .Where(x => x.Attendance.MaBuoiHoc == sessionId)
            .OrderBy(x => x.Student.HoTen)
            .ToListAsync(cancellationToken);

        return ToDetailDto(sessionResult, students.Select(ToAttendanceStudentDto).ToList());
    }

    private async Task<(Models.BuoiHoc Session, SessionQueryResult QueryResult)> GetSessionForMutationAsync(
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

        var queryResult = await CreateSessionQuery()
            .FirstOrDefaultAsync(x => x.Session.MaBuoiHoc == sessionId, cancellationToken);

        if (queryResult is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu buổi học không hợp lệ.");
        }

        if (currentUser.Role is AuthRoles.Admin or AuthRoles.SuperAdmin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff &&
            !await CanAccessOrganizationAsync(currentUser, queryResult.Course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập buổi học thuộc cơ sở này.");
        }

        return (session, queryResult);
    }

    private async Task EnsureCanViewSessionAttendanceAsync(
        CurrentUserContext currentUser,
        SessionQueryResult result,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.Teacher &&
            (result.Session.MaGiaoVien == currentUser.UserId || result.Session.MaGiaoVienDayThay == currentUser.UserId))
        {
            return;
        }

        if (currentUser.Role is AuthRoles.Admin or AuthRoles.SuperAdmin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff &&
            await CanAccessOrganizationAsync(currentUser, result.Course.MaDonVi, cancellationToken))
        {
            return;
        }

        throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem điểm danh buổi học này.");
    }

    private static void EnsureTeacherCanManageAttendance(CurrentUserContext currentUser, Models.BuoiHoc session)
    {
        if (currentUser.Role != AuthRoles.Teacher)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ giáo viên phụ trách buổi học được thao tác điểm danh.");
        }

        if (session.MaGiaoVien != currentUser.UserId && session.MaGiaoVienDayThay != currentUser.UserId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không phải giáo viên phụ trách buổi học này.");
        }
    }

    private static void EnsureSessionCanStartAttendance(Models.BuoiHoc session)
    {
        if (session.TrangThaiBuoi == CanceledSessionStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể mở điểm danh cho buổi học đã hủy.");
        }

        if (session.TrangThaiDiemDanh is AttendanceSubmittedStatus or AttendanceLockedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học đã gửi hoặc đã khóa điểm danh.");
        }
    }

    private static void EnsureSessionCanUpdateAttendance(Models.BuoiHoc session, DateTime now)
    {
        if (session.TrangThaiDiemDanh != AttendanceInProgressStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được cập nhật khi điểm danh đang mở.");
        }

        EnsureAttendanceDeadlineNotPassed(session, now);
    }

    private static void EnsureSessionCanSubmitAttendance(Models.BuoiHoc session, DateTime now)
    {
        if (session.TrangThaiDiemDanh != AttendanceInProgressStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được gửi khi điểm danh đang mở.");
        }

        EnsureAttendanceDeadlineNotPassed(session, now);
    }

    private static void EnsureAttendanceDeadlineNotPassed(Models.BuoiHoc session, DateTime now)
    {
        if (session.DiemDanhHanGuiLuc.HasValue && now > session.DiemDanhHanGuiLuc.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đã quá hạn điểm danh.");
        }
    }

    private async Task<HashSet<int>> GetActiveStudentIdsAsync(
        KhoaHoc course,
        CancellationToken cancellationToken)
    {
        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                x.TrangThai == UserStatuses.DbActive &&
                x.MaLop == course.MaLop &&
                x.MaDonVi == course.MaDonVi)
            .Select(x => x.MaNguoiDung)
            .ToHashSetAsync(cancellationToken);
    }

    private static List<BulkUpdateAttendanceItemRequest> ValidateBulkRequest(BulkUpdateAttendanceRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách cập nhật điểm danh không được để trống.");
        }

        var normalizedItems = request.Items
            .Select((item, index) => new
            {
                Index = index + 1,
                item.MaSinhVien,
                TrangThai = NormalizeAttendanceStatus(item.TrangThai)
            })
            .ToList();

        var duplicateStudentIds = normalizedItems
            .GroupBy(x => x.MaSinhVien)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateStudentIds.Count > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Request có sinh viên bị trùng: {string.Join(", ", duplicateStudentIds)}.");
        }

        return normalizedItems
            .Select(x => new BulkUpdateAttendanceItemRequest
            {
                MaSinhVien = x.MaSinhVien,
                TrangThai = x.TrangThai
            })
            .ToList();
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

    private async Task<AttendanceStudentDto> GetAttendanceStudentDtoAsync(
        int attendanceId,
        CancellationToken cancellationToken)
    {
        var attendance = await CreateAttendanceQuery()
            .FirstOrDefaultAsync(x => x.Attendance.MaDiemDanh == attendanceId, cancellationToken);

        if (attendance is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy dòng điểm danh.");
        }

        return ToAttendanceStudentDto(attendance);
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

        var total = await _context.DiemDanhs.CountAsync(x => x.MaBuoiHoc == sessionId, cancellationToken);
        return new
        {
            result.Session.MaBuoiHoc,
            result.Session.MaKhoaHoc,
            result.Course.TieuDe,
            result.Session.NgayHoc,
            result.Session.TrangThaiDiemDanh,
            result.Session.DiemDanhBatDauLuc,
            result.Session.DiemDanhHanGuiLuc,
            result.Session.DiemDanhDaGuiLuc,
            result.Session.DiemDanhHanChinhSuaLuc,
            result.Session.DiemDanhKhoaLuc,
            TotalAttendanceRows = total
        };
    }

    private Task WriteAuditAsync(
        string action,
        int entityId,
        int organizationId,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken,
        string entityType = "BuoiHoc")
    {
        return _auditLogService.LogAsync(
            entityType,
            entityId.ToString(CultureInfo.InvariantCulture),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            organizationId,
            description,
            cancellationToken);
    }

    private static object ToAttendanceAuditSnapshot(DiemDanh attendance)
    {
        return new
        {
            attendance.MaDiemDanh,
            attendance.MaBuoiHoc,
            attendance.MaHocSinh,
            attendance.TrangThai,
            attendance.HeSoVang,
            attendance.NguoiGhiNhan,
            attendance.GhiNhanLuc,
            attendance.KhoaLuc
        };
    }

    private static string NormalizeAttendanceStatus(string value)
    {
        var status = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái điểm danh không được để trống.");
        }

        if (!ValidAttendanceStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái điểm danh không hợp lệ.");
        }

        return status;
    }

    private static int GetAbsenceWeight(string status)
    {
        return status == DefaultAttendanceStatus ? 1 : 0;
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    }

    private static DateTime GetVietnamNow()
    {
        return DateTime.UtcNow.AddHours(7);
    }

    private static AttendanceDetailDto ToDetailDto(
        SessionQueryResult result,
        IReadOnlyList<AttendanceStudentDto> students)
    {
        var session = ToSessionDto(result);
        return new AttendanceDetailDto
        {
            MaBuoiHoc = session.MaBuoiHoc,
            MaTkb = session.MaTkb,
            MaKhoaHoc = session.MaKhoaHoc,
            TieuDeKhoaHoc = session.TieuDeKhoaHoc,
            MaHocKy = session.MaHocKy,
            TenHocKy = session.TenHocKy,
            MaLop = session.MaLop,
            TenLop = session.TenLop,
            MaMonHoc = session.MaMonHoc,
            TenMonHoc = session.TenMonHoc,
            NgayHoc = session.NgayHoc,
            MaCaHoc = session.MaCaHoc,
            TenCa = session.TenCa,
            GioBatDau = session.GioBatDau,
            GioKetThuc = session.GioKetThuc,
            MaPhong = session.MaPhong,
            TenPhong = session.TenPhong,
            MaCodePhong = session.MaCodePhong,
            MaGiaoVien = session.MaGiaoVien,
            TenGiaoVien = session.TenGiaoVien,
            MaGiaoVienDayThay = session.MaGiaoVienDayThay,
            TenGiaoVienDayThay = session.TenGiaoVienDayThay,
            TrangThaiBuoi = session.TrangThaiBuoi,
            TrangThaiDiemDanh = session.TrangThaiDiemDanh,
            DiemDanhBatDauLuc = session.DiemDanhBatDauLuc,
            DiemDanhHanGuiLuc = session.DiemDanhHanGuiLuc,
            DiemDanhDaGuiLuc = session.DiemDanhDaGuiLuc,
            DiemDanhHanChinhSuaLuc = session.DiemDanhHanChinhSuaLuc,
            DiemDanhKhoaLuc = session.DiemDanhKhoaLuc,
            Students = students
        };
    }

    private static AttendanceSessionDto ToSessionDto(SessionQueryResult result)
    {
        return new AttendanceSessionDto
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
            DiemDanhBatDauLuc = result.Session.DiemDanhBatDauLuc,
            DiemDanhHanGuiLuc = result.Session.DiemDanhHanGuiLuc,
            DiemDanhDaGuiLuc = result.Session.DiemDanhDaGuiLuc,
            DiemDanhHanChinhSuaLuc = result.Session.DiemDanhHanChinhSuaLuc,
            DiemDanhKhoaLuc = result.Session.DiemDanhKhoaLuc
        };
    }

    private static AttendanceStudentDto ToAttendanceStudentDto(AttendanceQueryResult result)
    {
        return new AttendanceStudentDto
        {
            MaDiemDanh = result.Attendance.MaDiemDanh,
            MaBuoiHoc = result.Attendance.MaBuoiHoc,
            MaHocSinh = result.Attendance.MaHocSinh,
            HoTenHocSinh = result.Student.HoTen,
            Email = result.Student.Email,
            MaLop = result.Student.MaLop,
            TenLop = result.Class.TenLop,
            TrangThai = result.Attendance.TrangThai,
            HeSoVang = result.Attendance.HeSoVang,
            NguoiGhiNhan = result.Attendance.NguoiGhiNhan,
            TenNguoiGhiNhan = result.Recorder.HoTen,
            GhiNhanLuc = result.Attendance.GhiNhanLuc,
            KhoaLuc = result.Attendance.KhoaLuc
        };
    }

    private static StudentAttendanceDto ToStudentAttendanceDto(AttendanceQueryResult result)
    {
        return new StudentAttendanceDto
        {
            MaDiemDanh = result.Attendance.MaDiemDanh,
            MaBuoiHoc = result.Attendance.MaBuoiHoc,
            MaKhoaHoc = result.Session.MaKhoaHoc,
            TieuDeKhoaHoc = result.Course.TieuDe,
            MaHocKy = result.Course.MaHocKy,
            TenHocKy = result.Term?.TenHocKy,
            MaMonHoc = result.Course.MaMonHoc,
            TenMonHoc = result.Subject.TenMonHoc,
            NgayHoc = result.Session.NgayHoc,
            MaCaHoc = result.Session.MaCaHoc,
            TenCa = result.Shift.TenCa,
            GioBatDau = FormatTime(result.Shift.GioBatDau),
            GioKetThuc = FormatTime(result.Shift.GioKetThuc),
            MaPhong = result.Session.MaPhong,
            TenPhong = result.Room.TenPhong,
            TrangThai = result.Attendance.TrangThai,
            HeSoVang = result.Attendance.HeSoVang,
            GhiNhanLuc = result.Attendance.GhiNhanLuc
        };
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private sealed class SessionQueryResult
    {
        public Models.BuoiHoc Session { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public HocKy? Term { get; init; }
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public NguoiDung? SubstituteTeacher { get; init; }
    }

    private sealed class AttendanceQueryResult
    {
        public DiemDanh Attendance { get; init; } = null!;
        public Models.BuoiHoc Session { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public HocKy? Term { get; init; }
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
        public NguoiDung Student { get; init; } = null!;
        public NguoiDung Recorder { get; init; } = null!;
    }
}

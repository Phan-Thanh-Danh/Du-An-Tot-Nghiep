using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.ThoiKhoaBieu;

public class ThoiKhoaBieuService : IThoiKhoaBieuService
{
    private const string DraftStatus = "nhap";
    private const string PublishedStatus = "da_xuat_ban";
    private const string CanceledStatus = "da_huy";
    private const string ArchivedCourseStatus = "luu_tru";
    private const string ActiveRoomStatus = "hoat_dong";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        DraftStatus,
        PublishedStatus,
        CanceledStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly IScheduleConflictService _scheduleConflictService;

    public ThoiKhoaBieuService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IScheduleConflictService scheduleConflictService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _scheduleConflictService = scheduleConflictService;
    }

    public async Task<PagedResultDto<ThoiKhoaBieuDto>> GetAsync(
        ThoiKhoaBieuQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyReadScope(CreateScheduleQuery(), currentUser, allowedOrganizationIds);

        if (parameters.MaKhoaHoc.HasValue)
        {
            query = query.Where(x => x.Schedule.MaKhoaHoc == parameters.MaKhoaHoc.Value);
        }

        if (parameters.MaHocKy.HasValue)
        {
            query = query.Where(x => x.Course.MaHocKy == parameters.MaHocKy.Value);
        }

        if (parameters.MaLop.HasValue)
        {
            query = query.Where(x => x.Course.MaLop == parameters.MaLop.Value);
        }

        if (parameters.MaGiaoVien.HasValue)
        {
            query = query.Where(x => x.Course.MaGiaoVien == parameters.MaGiaoVien.Value);
        }

        if (parameters.MaPhong.HasValue)
        {
            query = query.Where(x => x.Schedule.MaPhong == parameters.MaPhong.Value);
        }

        if (parameters.MaCaHoc.HasValue)
        {
            query = query.Where(x => x.Schedule.MaCaHoc == parameters.MaCaHoc.Value);
        }

        if (parameters.ThuTrongTuan.HasValue)
        {
            query = query.Where(x => x.Schedule.ThuTrongTuan == parameters.ThuTrongTuan.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.Schedule.TrangThai == status);
        }

        if (parameters.NgayBatDau.HasValue)
        {
            query = query.Where(x =>
                x.Schedule.NgayBatDau.HasValue &&
                x.Schedule.NgayBatDau.Value >= parameters.NgayBatDau.Value);
        }

        if (parameters.NgayKetThuc.HasValue)
        {
            query = query.Where(x =>
                x.Schedule.NgayKetThuc.HasValue &&
                x.Schedule.NgayKetThuc.Value <= parameters.NgayKetThuc.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Term == null ? string.Empty : x.Term.TenHocKy)
            .ThenBy(x => x.Class.MaCodeLop)
            .ThenBy(x => x.Schedule.ThuTrongTuan)
            .ThenBy(x => x.Shift.ThuTu)
            .ThenBy(x => x.Room.MaCodePhong)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ThoiKhoaBieuDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ThoiKhoaBieuDetailDto> GetByIdAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await ApplyReadScope(CreateScheduleQuery(), currentUser, allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.Schedule.MaTkb == scheduleId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thời khóa biểu.");
        }

        return ToDetailDto(result);
    }

    public async Task<ThoiKhoaBieuDetailDto> CreateAsync(
        CreateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedules(currentUser);

        var status = NormalizeStatus(string.IsNullOrWhiteSpace(request.TrangThai) ? DraftStatus : request.TrangThai);
        var course = await ValidateCourseAsync(request.MaKhoaHoc, currentUser, cancellationToken);
        var shift = await ValidateShiftAsync(request.MaCaHoc, cancellationToken);
        var room = await ValidateRoomAsync(request.MaPhong, course.MaDonVi, currentUser, cancellationToken);
        ValidateDayOfWeek(request.ThuTrongTuan);
        ValidateDateRange(request.NgayBatDau, request.NgayKetThuc);
        await ValidateScheduleDatesInTermAsync(course, request.NgayBatDau, request.NgayKetThuc, cancellationToken);
        await _scheduleConflictService.EnsureNoConflictAsync(
            new CheckScheduleConflictRequest
            {
                MaKhoaHoc = course.MaKhoaHoc,
                ThuTrongTuan = request.ThuTrongTuan,
                MaCaHoc = shift.MaCaHoc,
                MaPhong = room.MaPhong
            },
            cancellationToken);
        await ValidateDuplicateAsync(
            course.MaKhoaHoc,
            request.ThuTrongTuan,
            shift.MaCaHoc,
            null,
            cancellationToken);

        var schedule = new Models.ThoiKhoaBieu
        {
            MaKhoaHoc = course.MaKhoaHoc,
            ThuTrongTuan = request.ThuTrongTuan,
            MaCaHoc = shift.MaCaHoc,
            MaPhong = room.MaPhong,
            NgayBatDau = request.NgayBatDau,
            NgayKetThuc = request.NgayKetThuc,
            TrangThai = status,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        _context.ThoiKhoaBieus.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(schedule.MaTkb, cancellationToken);
        await WriteAuditAsync(
            "CREATE_THOI_KHOA_BIEU",
            schedule,
            null,
            newSnapshot,
            currentUser,
            "Tạo thời khóa biểu.",
            cancellationToken);

        return await GetByIdAsync(schedule.MaTkb, cancellationToken);
    }

    public async Task<ThoiKhoaBieuDetailDto> UpdateAsync(
        int scheduleId,
        UpdateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedules(currentUser);

        var schedule = await GetManagedScheduleAsync(scheduleId, currentUser, cancellationToken);
        if (schedule.TrangThai == CanceledStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể cập nhật thời khóa biểu đã hủy.");
        }

        var oldSnapshot = await GetAuditSnapshotAsync(scheduleId, cancellationToken);
        var status = NormalizeStatus(request.TrangThai);
        var course = await ValidateCourseAsync(request.MaKhoaHoc, currentUser, cancellationToken);
        var shift = await ValidateShiftAsync(request.MaCaHoc, cancellationToken);
        var room = await ValidateRoomAsync(request.MaPhong, course.MaDonVi, currentUser, cancellationToken);
        ValidateDayOfWeek(request.ThuTrongTuan);
        ValidateDateRange(request.NgayBatDau, request.NgayKetThuc);
        await ValidateScheduleDatesInTermAsync(course, request.NgayBatDau, request.NgayKetThuc, cancellationToken);
        await _scheduleConflictService.EnsureNoConflictAsync(
            new CheckScheduleConflictRequest
            {
                MaKhoaHoc = course.MaKhoaHoc,
                ThuTrongTuan = request.ThuTrongTuan,
                MaCaHoc = shift.MaCaHoc,
                MaPhong = room.MaPhong,
                ExcludeMaTkb = scheduleId
            },
            cancellationToken);
        await ValidateDuplicateAsync(
            course.MaKhoaHoc,
            request.ThuTrongTuan,
            shift.MaCaHoc,
            scheduleId,
            cancellationToken);

        if (status == PublishedStatus && schedule.TrangThai != PublishedStatus)
        {
            var courseToValidate = await _context.KhoaHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKhoaHoc == request.MaKhoaHoc, cancellationToken);

            if (courseToValidate?.TrangThai == "luu_tru")
            {
                throw new ApiException(StatusCodes.Status400BadRequest,
                    "Không thể xuất bản thời khóa biểu khi khóa học đang ở trạng thái lưu trữ.");
            }

            var roomForPublish = await _context.PhongHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaPhong == request.MaPhong, cancellationToken);

            if (roomForPublish != null && roomForPublish.TrangThaiPhong != "hoat_dong")
            {
                throw new ApiException(StatusCodes.Status400BadRequest,
                    "Không thể xuất bản thời khóa biểu vì phòng học không hoạt động.");
            }
        }

        schedule.MaKhoaHoc = course.MaKhoaHoc;
        schedule.ThuTrongTuan = request.ThuTrongTuan;
        schedule.MaCaHoc = shift.MaCaHoc;
        schedule.MaPhong = room.MaPhong;
        schedule.NgayBatDau = request.NgayBatDau;
        schedule.NgayKetThuc = request.NgayKetThuc;
        schedule.TrangThai = status;
        schedule.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(scheduleId, cancellationToken);
        await WriteAuditAsync(
            "UPDATE_THOI_KHOA_BIEU",
            schedule,
            oldSnapshot,
            newSnapshot,
            currentUser,
            "Cập nhật thời khóa biểu.",
            cancellationToken);

        return await GetByIdAsync(scheduleId, cancellationToken);
    }

    public async Task<ThoiKhoaBieuDetailDto> CancelAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedules(currentUser);

        var schedule = await GetManagedScheduleAsync(scheduleId, currentUser, cancellationToken);
        var oldSnapshot = await GetAuditSnapshotAsync(scheduleId, cancellationToken);

        schedule.TrangThai = CanceledStatus;
        schedule.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(scheduleId, cancellationToken);
        await WriteAuditAsync(
            "CANCEL_THOI_KHOA_BIEU",
            schedule,
            oldSnapshot,
            newSnapshot,
            currentUser,
            "Hủy thời khóa biểu.",
            cancellationToken);

        return await GetByIdAsync(scheduleId, cancellationToken);
    }

    private IQueryable<ScheduleQueryResult> CreateScheduleQuery()
    {
        return
            from schedule in _context.ThoiKhoaBieus.AsNoTracking()
            join course in _context.KhoaHocs.AsNoTracking()
                on schedule.MaKhoaHoc equals course.MaKhoaHoc
            join organization in _context.DonVis.AsNoTracking()
                on course.MaDonVi equals organization.MaDonVi
            join subject in _context.DanhMucMonHocs.AsNoTracking()
                on course.MaMonHoc equals subject.MaMonHoc
            join teacher in _context.NguoiDungs.AsNoTracking()
                on course.MaGiaoVien equals teacher.MaNguoiDung
            join classEntity in _context.LopHanhChinhs.AsNoTracking()
                on course.MaLop equals classEntity.MaLop
            join term in _context.HocKys.AsNoTracking()
                on course.MaHocKy equals term.MaHocKy into termJoin
            from term in termJoin.DefaultIfEmpty()
            join shift in _context.CaHocs.AsNoTracking()
                on schedule.MaCaHoc equals shift.MaCaHoc
            join room in _context.PhongHocs.AsNoTracking()
                on schedule.MaPhong equals room.MaPhong
            select new ScheduleQueryResult
            {
                Schedule = schedule,
                Course = course,
                Organization = organization,
                Subject = subject,
                Teacher = teacher,
                Class = classEntity,
                Term = term,
                Shift = shift,
                Room = room
            };
    }

    private IQueryable<ScheduleQueryResult> ApplyReadScope(
        IQueryable<ScheduleQueryResult> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        return query.Where(x => allowedOrganizationIds.Contains(x.Course.MaDonVi));
    }

    private async Task<Models.ThoiKhoaBieu> GetManagedScheduleAsync(
        int scheduleId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var schedule = await _context.ThoiKhoaBieus
            .FirstOrDefaultAsync(x => x.MaTkb == scheduleId, cancellationToken);

        if (schedule is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thời khóa biểu.");
        }

        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == schedule.MaKhoaHoc, cancellationToken);

        if (course is null ||
            !await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thời khóa biểu.");
        }

        return schedule;
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
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học không tồn tại.");
        }

        if (course.TrangThai == ArchivedCourseStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học đã lưu trữ, không thể xếp thời khóa biểu.");
        }

        if (!course.MaHocKy.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học phải có học kỳ trước khi xếp thời khóa biểu.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thời khóa biểu của cơ sở này.");
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
            throw new ApiException(StatusCodes.Status400BadRequest, "Ca học không tồn tại hoặc không hoạt động.");
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
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học không tồn tại hoặc không hoạt động.");
        }

        if (room.MaDonVi != courseOrganizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học không thuộc cùng cơ sở với khóa học.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, room.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý phòng học của cơ sở này.");
        }

        return room;
    }

    private async Task ValidateScheduleDatesInTermAsync(
        KhoaHoc course,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            return;
        }

        var term = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == course.MaHocKy, cancellationToken);

        if (term is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ của khóa học không tồn tại.");
        }

        if (startDate.HasValue && startDate.Value < term.NgayBatDau)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu thời khóa biểu không được trước ngày bắt đầu học kỳ.");
        }

        if (endDate.HasValue && endDate.Value > term.NgayKetThuc)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày kết thúc thời khóa biểu không được sau ngày kết thúc học kỳ.");
        }
    }

    private async Task ValidateDuplicateAsync(
        int courseId,
        int dayOfWeek,
        int shiftId,
        int? excludedScheduleId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ThoiKhoaBieus
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaKhoaHoc == courseId &&
                x.ThuTrongTuan == dayOfWeek &&
                x.MaCaHoc == shiftId &&
                x.TrangThai != CanceledStatus &&
                (!excludedScheduleId.HasValue || x.MaTkb != excludedScheduleId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Khóa học đã có thời khóa biểu cho thứ và ca học này.");
        }
    }

    private async Task<object?> GetAuditSnapshotAsync(int scheduleId, CancellationToken cancellationToken)
    {
        return await CreateScheduleQuery()
            .Where(x => x.Schedule.MaTkb == scheduleId)
            .Select(x => new
            {
                x.Schedule.MaTkb,
                x.Schedule.MaKhoaHoc,
                x.Course.TieuDe,
                x.Course.MaDonVi,
                TenDonVi = x.Organization.TenDonVi,
                x.Course.MaHocKy,
                TenHocKy = x.Term == null ? null : x.Term.TenHocKy,
                x.Course.MaLop,
                x.Class.MaCodeLop,
                x.Class.TenLop,
                x.Course.MaMonHoc,
                x.Subject.MaCodeMonHoc,
                x.Subject.TenMonHoc,
                x.Course.MaGiaoVien,
                TenGiaoVien = x.Teacher.HoTen,
                x.Schedule.ThuTrongTuan,
                x.Schedule.MaCaHoc,
                x.Shift.TenCa,
                x.Schedule.MaPhong,
                x.Room.MaCodePhong,
                x.Room.TenPhong,
                x.Schedule.NgayBatDau,
                x.Schedule.NgayKetThuc,
                x.Schedule.TrangThai,
                x.Schedule.NgayTao,
                x.Schedule.NgayCapNhat
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(
        string action,
        Models.ThoiKhoaBieu schedule,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken)
    {
        var organizationId = await _context.KhoaHocs
            .AsNoTracking()
            .Where(x => x.MaKhoaHoc == schedule.MaKhoaHoc)
            .Select(x => (int?)x.MaDonVi)
            .FirstOrDefaultAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "ThoiKhoaBieu",
            schedule.MaTkb.ToString(),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            organizationId,
            description,
            cancellationToken);
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

    private static void EnsureCanManageSchedules(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thời khóa biểu.");
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

    private static string NormalizeStatus(string value)
    {
        var status = NormalizeRequiredText(value, "Trạng thái").ToLowerInvariant();
        if (!ValidStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái thời khóa biểu không hợp lệ.");
        }

        return status;
    }

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalizedValue;
    }

    private static void ValidateDayOfWeek(int dayOfWeek)
    {
        if (dayOfWeek is < 1 or > 7)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thứ trong tuần phải từ 1 đến 7.");
        }
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    }

    private static ThoiKhoaBieuDto ToDto(ScheduleQueryResult result)
    {
        return new ThoiKhoaBieuDto
        {
            MaTkb = result.Schedule.MaTkb,
            MaKhoaHoc = result.Schedule.MaKhoaHoc,
            TieuDeKhoaHoc = result.Course.TieuDe,
            MaDonVi = result.Course.MaDonVi,
            TenDonVi = result.Organization.TenDonVi,
            MaHocKy = result.Course.MaHocKy,
            TenHocKy = result.Term?.TenHocKy,
            MaLop = result.Course.MaLop,
            TenLop = result.Class.TenLop,
            MaCodeLop = result.Class.MaCodeLop,
            MaMonHoc = result.Course.MaMonHoc,
            MaCodeMonHoc = result.Subject.MaCodeMonHoc,
            TenMonHoc = result.Subject.TenMonHoc,
            MaGiaoVien = result.Course.MaGiaoVien,
            TenGiaoVien = result.Teacher.HoTen,
            ThuTrongTuan = result.Schedule.ThuTrongTuan,
            MaCaHoc = result.Schedule.MaCaHoc,
            TenCa = result.Shift.TenCa,
            Buoi = result.Shift.Buoi,
            GioBatDau = FormatTime(result.Shift.GioBatDau),
            GioKetThuc = FormatTime(result.Shift.GioKetThuc),
            MaPhong = result.Schedule.MaPhong,
            MaCodePhong = result.Room.MaCodePhong,
            TenPhong = result.Room.TenPhong,
            NgayBatDau = result.Schedule.NgayBatDau,
            NgayKetThuc = result.Schedule.NgayKetThuc,
            TrangThai = result.Schedule.TrangThai,
            NgayTao = result.Schedule.NgayTao,
            NgayCapNhat = result.Schedule.NgayCapNhat
        };
    }

    private static ThoiKhoaBieuDetailDto ToDetailDto(ScheduleQueryResult result)
    {
        var dto = ToDto(result);
        return new ThoiKhoaBieuDetailDto
        {
            MaTkb = dto.MaTkb,
            MaKhoaHoc = dto.MaKhoaHoc,
            TieuDeKhoaHoc = dto.TieuDeKhoaHoc,
            MaDonVi = dto.MaDonVi,
            TenDonVi = dto.TenDonVi,
            MaHocKy = dto.MaHocKy,
            TenHocKy = dto.TenHocKy,
            MaLop = dto.MaLop,
            TenLop = dto.TenLop,
            MaCodeLop = dto.MaCodeLop,
            MaMonHoc = dto.MaMonHoc,
            MaCodeMonHoc = dto.MaCodeMonHoc,
            TenMonHoc = dto.TenMonHoc,
            MaGiaoVien = dto.MaGiaoVien,
            TenGiaoVien = dto.TenGiaoVien,
            ThuTrongTuan = dto.ThuTrongTuan,
            MaCaHoc = dto.MaCaHoc,
            TenCa = dto.TenCa,
            Buoi = dto.Buoi,
            GioBatDau = dto.GioBatDau,
            GioKetThuc = dto.GioKetThuc,
            MaPhong = dto.MaPhong,
            MaCodePhong = dto.MaCodePhong,
            TenPhong = dto.TenPhong,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            TrangThai = dto.TrangThai,
            NgayTao = dto.NgayTao,
            NgayCapNhat = dto.NgayCapNhat
        };
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private sealed class ScheduleQueryResult
    {
        public Models.ThoiKhoaBieu Schedule { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public DonVi Organization { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public HocKy? Term { get; init; }
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
    }
}

using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.ThoiKhoaBieu;

public class ScheduleConflictService : IScheduleConflictService
{
    private const string CanceledStatus = "da_huy";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ScheduleConflictService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ScheduleConflictResultDto> CheckConflictsAsync(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDayOfWeek(request.ThuTrongTuan);

        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var requestCourse = await ValidateRequestCourseAsync(
            request.MaKhoaHoc,
            allowedOrganizationIds,
            cancellationToken);

        await ValidateShiftAsync(request.MaCaHoc, cancellationToken);
        await ValidateRoomAsync(
            request.MaPhong,
            requestCourse.MaDonVi,
            allowedOrganizationIds,
            cancellationToken);

        var existingSchedules = await CreateConflictQuery()
            .Where(x =>
                x.Schedule.TrangThai != CanceledStatus &&
                x.Course.MaHocKy == requestCourse.MaHocKy &&
                x.Schedule.ThuTrongTuan == request.ThuTrongTuan &&
                x.Schedule.MaCaHoc == request.MaCaHoc &&
                allowedOrganizationIds.Contains(x.Course.MaDonVi) &&
                (!request.ExcludeMaTkb.HasValue || x.Schedule.MaTkb != request.ExcludeMaTkb.Value))
            .OrderBy(x => x.Schedule.MaTkb)
            .ToListAsync(cancellationToken);

        var conflicts = new List<ScheduleConflictDto>();
        foreach (var existing in existingSchedules)
        {
            if (existing.Course.MaGiaoVien == requestCourse.MaGiaoVien)
            {
                conflicts.Add(ToConflictDto(
                    "teacher",
                    "Giáo viên đã có lịch dạy cùng học kỳ, thứ và ca.",
                    existing));
            }

            if (existing.Course.MaLop == requestCourse.MaLop)
            {
                conflicts.Add(ToConflictDto(
                    "class",
                    "Lớp hành chính đã có lịch học cùng học kỳ, thứ và ca.",
                    existing));
            }

            if (existing.Schedule.MaPhong == request.MaPhong)
            {
                conflicts.Add(ToConflictDto(
                    "room",
                    "Phòng học đã được sử dụng cùng học kỳ, thứ và ca.",
                    existing));
            }
        }

        return new ScheduleConflictResultDto
        {
            Conflicts = conflicts
        };
    }

    public async Task EnsureNoConflictAsync(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await CheckConflictsAsync(request, cancellationToken);
        if (result.HasConflict)
        {
            throw new ScheduleConflictException(result);
        }
    }

    private IQueryable<ConflictQueryResult> CreateConflictQuery()
    {
        return
            from schedule in _context.ThoiKhoaBieus.AsNoTracking()
            join course in _context.KhoaHocs.AsNoTracking()
                on schedule.MaKhoaHoc equals course.MaKhoaHoc
            join teacher in _context.NguoiDungs.AsNoTracking()
                on course.MaGiaoVien equals teacher.MaNguoiDung
            join classEntity in _context.LopHanhChinhs.AsNoTracking()
                on course.MaLop equals classEntity.MaLop
            join shift in _context.CaHocs.AsNoTracking()
                on schedule.MaCaHoc equals shift.MaCaHoc
            join room in _context.PhongHocs.AsNoTracking()
                on schedule.MaPhong equals room.MaPhong
            select new ConflictQueryResult
            {
                Schedule = schedule,
                Course = course,
                Teacher = teacher,
                Class = classEntity,
                Shift = shift,
                Room = room
            };
    }

    private async Task<KhoaHoc> ValidateRequestCourseAsync(
        int courseId,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == courseId, cancellationToken);

        if (course is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học không tồn tại.");
        }

        if (!course.MaHocKy.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học phải có học kỳ trước khi kiểm tra xung đột.");
        }

        if (!allowedOrganizationIds.Contains(course.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền kiểm tra thời khóa biểu của cơ sở này.");
        }

        return course;
    }

    private async Task ValidateShiftAsync(int shiftId, CancellationToken cancellationToken)
    {
        var exists = await _context.CaHocs
            .AsNoTracking()
            .AnyAsync(x => x.MaCaHoc == shiftId, cancellationToken);

        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ca học không tồn tại.");
        }
    }

    private async Task ValidateRoomAsync(
        int roomId,
        int courseOrganizationId,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var room = await _context.PhongHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaPhong == roomId, cancellationToken);

        if (room is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học không tồn tại.");
        }

        if (room.MaDonVi != courseOrganizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phòng học không thuộc cùng cơ sở với khóa học.");
        }

        if (!allowedOrganizationIds.Contains(room.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền kiểm tra phòng học của cơ sở này.");
        }
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

    private static void ValidateDayOfWeek(int dayOfWeek)
    {
        if (dayOfWeek is < 1 or > 7)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thứ trong tuần phải từ 1 đến 7.");
        }
    }

    private static ScheduleConflictDto ToConflictDto(
        string type,
        string message,
        ConflictQueryResult result)
    {
        return new ScheduleConflictDto
        {
            Type = type,
            Message = message,
            MaTkb = result.Schedule.MaTkb,
            MaKhoaHoc = result.Schedule.MaKhoaHoc,
            TenKhoaHoc = result.Course.TieuDe,
            MaGiaoVien = result.Course.MaGiaoVien,
            TenGiaoVien = result.Teacher.HoTen,
            MaLop = result.Course.MaLop,
            TenLop = result.Class.TenLop,
            MaCaHoc = result.Schedule.MaCaHoc,
            TenCa = result.Shift.TenCa,
            ThuTrongTuan = result.Schedule.ThuTrongTuan,
            MaPhong = result.Schedule.MaPhong,
            TenPhong = result.Room.TenPhong
        };
    }

    private sealed class ConflictQueryResult
    {
        public Models.ThoiKhoaBieu Schedule { get; init; } = null!;
        public KhoaHoc Course { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public LopHanhChinh Class { get; init; } = null!;
        public Models.CaHoc Shift { get; init; } = null!;
        public PhongHoc Room { get; init; } = null!;
    }
}

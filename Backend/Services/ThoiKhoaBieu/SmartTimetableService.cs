using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.ThoiKhoaBieu;

public class SmartTimetableService : ISmartTimetableService
{
    private const string PublishedScheduleStatus = "da_xuat_ban";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SmartTimetableService> _logger;

    public SmartTimetableService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        ILogger<SmartTimetableService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<ScheduleDraftDto> GenerateAsync(
        GenerateTimetableRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        var courses = await LoadCoursesAsync(request.MaHocKy, request.MaDonVi, request.MaKhoaHocFilter, cancellationToken);
        if (courses.Count == 0)
            throw new ApiException(StatusCodes.Status400BadRequest, "Không có khóa học nào để xếp lịch.");

        var shifts = await _context.CaHocs.AsNoTracking()
            .Where(x => x.ConHoatDong)
            .OrderBy(x => x.ThuTu)
            .ToListAsync(cancellationToken);

        var rooms = await _context.PhongHocs.AsNoTracking()
            .Where(x => x.TrangThaiPhong == "hoat_dong" && x.MaDonVi == request.MaDonVi)
            .ToListAsync(cancellationToken);

        var map = await BuildOccupationMapAsync(request.MaHocKy, request.MaDonVi, cancellationToken);
        var draftId = Guid.NewGuid();

        var job = new ScheduleGenerationJob
        {
            DraftId = draftId,
            MaDonVi = request.MaDonVi,
            MaHocKy = request.MaHocKy,
            NguoiYeuCau = currentUser.UserId,
            TrangThai = "draft",
            TongCourse = courses.Count,
            NgayTao = DateTime.UtcNow
        };

        _context.ScheduleGenerationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        var items = new List<ScheduleDraftItem>();
        var xepDuoc = 0;
        var khongXepDuoc = 0;

        foreach (var course in courses)
        {
            var assigned = TryAssignSlot(course, map, shifts, rooms, request, out var thu, out var ca, out var phong, out var lois);

            var item = new ScheduleDraftItem
            {
                MaJob = job.MaJob,
                MaKhoaHoc = course.MaKhoaHoc,
                ThuTrongTuan = thu,
                MaCaHoc = ca,
                MaPhong = phong,
                TrangThai = assigned ? "xep_duoc" : "khong_xep_duoc",
                LoiJson = lois.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(lois) : null
            };

            items.Add(item);
            if (assigned) xepDuoc++; else khongXepDuoc++;
        }

        _context.ScheduleDraftItems.AddRange(items);
        job.SoXepDuoc = xepDuoc;
        job.SoKhongXepDuoc = khongXepDuoc;
        job.Score = items.Count > 0 ? (double)xepDuoc / items.Count * 100 : 0;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "SmartTimetable", draftId.ToString(), "GENERATE",
            null, new { request.MaHocKy, request.MaDonVi, xepDuoc, khongXepDuoc },
            currentUser.UserId, request.MaDonVi,
            "Sinh thời khóa biểu thông minh.", cancellationToken);

        job.TomTatJson = System.Text.Json.JsonSerializer.Serialize(
            new { xepDuoc, khongXepDuoc, Score = job.Score });

        return await ToDraftDtoAsync(job.MaJob, cancellationToken);
    }

    public async Task<ScheduleDraftDto> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DraftId == draftId, cancellationToken);

        if (job is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bản nháp.");

        return await ToDraftDtoAsync(job.MaJob, cancellationToken);
    }

    public async Task<List<ScheduleDraftDto>> ListDraftsAsync(
        int maDonVi,
        int maHocKy,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .Where(x => x.MaDonVi == maDonVi && x.MaHocKy == maHocKy)
            .OrderByDescending(x => x.NgayTao)
            .ToListAsync(cancellationToken);

        var result = new List<ScheduleDraftDto>();
        foreach (var job in jobs)
            result.Add(await ToDraftDtoAsync(job.MaJob, cancellationToken));

        return result;
    }

    public async Task<TimetablePublishResultDto> PublishAsync(
        PublishTimetableRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        var job = await _context.ScheduleGenerationJobs
            .Include(x => x.HocKy)
            .FirstOrDefaultAsync(x => x.DraftId == request.DraftId, cancellationToken);

        if (job is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bản nháp.");

        if (job.TrangThai == "da_xuat_ban")
            throw new ApiException(StatusCodes.Status400BadRequest, "Bản nháp này đã được xuất bản.");

        var items = await _context.ScheduleDraftItems
            .AsNoTracking()
            .Where(x => x.MaJob == job.MaJob && x.TrangThai == "xep_duoc")
            .ToListAsync(cancellationToken);

        var result = new TimetablePublishResultDto();
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var map = await BuildOccupationMapAsync(job.MaHocKy, job.MaDonVi, cancellationToken);
                var courses = await _context.KhoaHocs.AsNoTracking()
                    .Where(x => x.MaHocKy == job.MaHocKy && x.MaDonVi == job.MaDonVi)
                    .ToDictionaryAsync(x => x.MaKhoaHoc, cancellationToken);

                foreach (var item in items)
                {
                    if (!item.ThuTrongTuan.HasValue || !item.MaCaHoc.HasValue || !item.MaPhong.HasValue)
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: thiếu thông tin thứ/ca/phòng.");
                        continue;
                    }

                    if (!courses.TryGetValue(item.MaKhoaHoc, out var course))
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: khóa học không tồn tại.");
                        continue;
                    }

                    if (map.IsTeacherOccupied(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaGiaoVien))
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: xung đột giáo viên.");
                        continue;
                    }

                    if (map.IsClassOccupied(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaLop))
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: xung đột lớp.");
                        continue;
                    }

                    if (map.IsRoomOccupied(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, item.MaPhong.Value))
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: xung đột phòng.");
                        continue;
                    }

                    var schedule = new Models.ThoiKhoaBieu
                    {
                        MaKhoaHoc = item.MaKhoaHoc,
                        ThuTrongTuan = item.ThuTrongTuan.Value,
                        MaCaHoc = item.MaCaHoc.Value,
                        MaPhong = item.MaPhong.Value,
                        TrangThai = PublishedScheduleStatus,
                        NgayTao = DateTime.UtcNow,
                        NgayCapNhat = DateTime.UtcNow
                    };

                    if (job.HocKy != null)
                    {
                        schedule.NgayBatDau = job.HocKy.NgayBatDau;
                        schedule.NgayKetThuc = job.HocKy.NgayKetThuc;
                    }

                    _context.ThoiKhoaBieus.Add(schedule);
                    map.OccupyTeacher(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaGiaoVien);
                    map.OccupyClass(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaLop);
                    map.OccupyRoom(job.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, item.MaPhong.Value);
                    result.BuoiHocDaTao++;
                }

                job.TrangThai = "da_xuat_ban";
                job.NgayXuatBan = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });

        result.Success = result.BuoiHocLoi == 0;

        await _auditLogService.LogAsync(
            "SmartTimetable", request.DraftId.ToString(), "PUBLISH",
            null, result, currentUser.UserId, job.MaDonVi,
            "Xuất bản thời khóa biểu thông minh.", cancellationToken);

        return result;
    }

    public async Task<ConflictCheckBatchResultDto> CheckConflictsAsync(
        ConflictCheckBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var map = await BuildOccupationMapAsync(request.MaHocKy, request.MaDonVi, cancellationToken);
        var courses = await _context.KhoaHocs.AsNoTracking()
            .Where(x => x.MaHocKy == request.MaHocKy && x.MaDonVi == request.MaDonVi)
            .ToDictionaryAsync(x => x.MaKhoaHoc, cancellationToken);

        var result = new ConflictCheckBatchResultDto();

        foreach (var item in request.Items)
        {
            if (!courses.TryGetValue(item.MaKhoaHoc, out var course))
            {
                result.Results.Add(new ConflictCheckResultItem
                {
                    MaKhoaHoc = item.MaKhoaHoc,
                    HasConflict = true,
                    Conflicts = new List<string> { "Khóa học không tồn tại." }
                });
                continue;
            }

            var conflicts = new List<string>();

            if (item.ThuTrongTuan.HasValue && item.MaCaHoc.HasValue)
            {
                if (map.IsTeacherOccupied(request.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaGiaoVien))
                    conflicts.Add("Giáo viên đã có lịch.");

                if (map.IsClassOccupied(request.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, course.MaLop))
                    conflicts.Add("Lớp đã có lịch.");

                if (item.MaPhong.HasValue && map.IsRoomOccupied(request.MaHocKy, item.ThuTrongTuan.Value, item.MaCaHoc.Value, item.MaPhong.Value))
                    conflicts.Add("Phòng đã được sử dụng.");
            }

            result.Results.Add(new ConflictCheckResultItem
            {
                MaKhoaHoc = item.MaKhoaHoc,
                HasConflict = conflicts.Count > 0,
                Conflicts = conflicts,
                ThuTrongTuan = item.ThuTrongTuan,
                MaCaHoc = item.MaCaHoc,
                MaPhong = item.MaPhong
            });
        }

        return result;
    }

    public async Task<bool> DeleteDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        var job = await _context.ScheduleGenerationJobs
            .FirstOrDefaultAsync(x => x.DraftId == draftId, cancellationToken);

        if (job is null) return false;
        if (job.TrangThai == "da_xuat_ban")
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể xóa bản nháp đã xuất bản.");

        var items = await _context.ScheduleDraftItems
            .Where(x => x.MaJob == job.MaJob)
            .ToListAsync(cancellationToken);

        _context.ScheduleDraftItems.RemoveRange(items);
        _context.ScheduleGenerationJobs.Remove(job);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<OccupationMap> BuildOccupationMapAsync(int maHocKy, int maDonVi, CancellationToken cancellationToken)
    {
        var map = new OccupationMap();

        var schedules = await (
            from s in _context.ThoiKhoaBieus.AsNoTracking()
            join c in _context.KhoaHocs.AsNoTracking() on s.MaKhoaHoc equals c.MaKhoaHoc
            where c.MaHocKy == maHocKy && c.MaDonVi == maDonVi && s.TrangThai != "da_huy"
            select new { s, c }
        ).ToListAsync(cancellationToken);

        foreach (var item in schedules)
        {
            map.OccupyTeacher(maHocKy, item.s.ThuTrongTuan, item.s.MaCaHoc, item.c.MaGiaoVien);
            map.OccupyClass(maHocKy, item.s.ThuTrongTuan, item.s.MaCaHoc, item.c.MaLop);
            map.OccupyRoom(maHocKy, item.s.ThuTrongTuan, item.s.MaCaHoc, item.s.MaPhong);
        }

        return map;
    }

    private static bool TryAssignSlot(
        KhoaHoc course,
        OccupationMap map,
        List<Models.CaHoc> shifts,
        List<Models.PhongHoc> rooms,
        GenerateTimetableRequest request,
        out int? thu,
        out int? maCa,
        out int? maPhong,
        out List<string> errors)
    {
        thu = null;
        maCa = null;
        maPhong = null;
        errors = new List<string>();

        var maHocKy = course.MaHocKy ?? 0;
        if (maHocKy == 0)
        {
            errors.Add("Khóa học không có học kỳ.");
            return false;
        }

        var bestScore = -1d;
        var assigned = false;

        foreach (var day in Enumerable.Range(2, 6))
        {
            foreach (var shift in shifts)
            {
                if (map.IsTeacherOccupied(maHocKy, day, shift.MaCaHoc, course.MaGiaoVien))
                    continue;
                if (map.IsClassOccupied(maHocKy, day, shift.MaCaHoc, course.MaLop))
                    continue;

                foreach (var room in rooms)
                {
                    if (map.IsRoomOccupied(maHocKy, day, shift.MaCaHoc, room.MaPhong))
                        continue;

                    var score = ScoreAssignment(course, shift, room, day, request);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        thu = day;
                        maCa = shift.MaCaHoc;
                        maPhong = room.MaPhong;
                        assigned = true;
                    }

                    if (assigned) break;
                }

                if (assigned) break;
            }

            if (assigned) break;
        }

        if (!assigned)
        {
            errors.Add("Không tìm được slot trống cho khóa học này.");
            return false;
        }

        map.OccupyTeacher(maHocKy, thu!.Value, maCa!.Value, course.MaGiaoVien);
        map.OccupyClass(maHocKy, thu.Value, maCa.Value, course.MaLop);
        map.OccupyRoom(maHocKy, thu.Value, maCa.Value, maPhong!.Value);

        return true;
    }

    private static double ScoreAssignment(KhoaHoc course, Models.CaHoc shift, Models.PhongHoc room, int dayOfWeek, GenerateTimetableRequest request)
    {
        var score = 0d;

        if (dayOfWeek is >= 2 and <= 4)
            score += 10;
        else
            score += 5;

        score += (6 - shift.ThuTu) * 2;

        if (room.SucChua > 0)
        {
            var ratio = (double)room.SucChua / 30;
            if (ratio >= 1 && ratio <= 2)
                score += 5;
            else if (ratio < 1)
                score += 2;
        }

        return score;
    }

    private async Task<List<KhoaHoc>> LoadCoursesAsync(
        int maHocKy,
        int maDonVi,
        List<int>? filter,
        CancellationToken cancellationToken)
    {
        var query = _context.KhoaHocs
            .AsNoTracking()
            .Where(x => x.MaHocKy == maHocKy && x.MaDonVi == maDonVi && x.TrangThai != "luu_tru");

        if (filter is { Count: > 0 })
            query = query.Where(x => filter.Contains(x.MaKhoaHoc));

        return await query.OrderBy(x => x.TieuDe).ToListAsync(cancellationToken);
    }

    private async Task<ScheduleDraftDto> ToDraftDtoAsync(int maJob, CancellationToken cancellationToken)
    {
        var job = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaJob == maJob, cancellationToken);
        if (job is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bản nháp.");

        var draftItems = await _context.ScheduleDraftItems
            .AsNoTracking()
            .Where(x => x.MaJob == maJob)
            .OrderBy(x => x.MaDraftItem)
            .ToListAsync(cancellationToken);

        var courseIds = draftItems.Select(x => x.MaKhoaHoc).Distinct().ToList();
        var roomIds = draftItems.Select(x => x.MaPhong).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var shiftIds = draftItems.Select(x => x.MaCaHoc).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();

        var courses = courseIds.Count > 0
            ? await _context.KhoaHocs.AsNoTracking().Where(x => courseIds.Contains(x.MaKhoaHoc)).ToListAsync(cancellationToken)
            : new List<Backend.Models.KhoaHoc>();

        var rooms = roomIds.Count > 0
            ? await _context.PhongHocs.AsNoTracking().Where(x => roomIds.Contains(x.MaPhong)).ToListAsync(cancellationToken)
            : new List<Backend.Models.PhongHoc>();

        var shifts = shiftIds.Count > 0
            ? await _context.CaHocs.AsNoTracking().Where(x => shiftIds.Contains(x.MaCaHoc)).ToListAsync(cancellationToken)
            : new List<Backend.Models.CaHoc>();

        var courseMap = courses.ToDictionary(x => x.MaKhoaHoc);
        var roomMap = rooms.ToDictionary(x => x.MaPhong);
        var shiftMap = shifts.ToDictionary(x => x.MaCaHoc);

        return new ScheduleDraftDto
        {
            MaJob = job.MaJob,
            DraftId = job.DraftId,
            MaDonVi = job.MaDonVi,
            MaHocKy = job.MaHocKy,
            TrangThai = job.TrangThai,
            TongCourse = job.TongCourse,
            SoXepDuoc = job.SoXepDuoc,
            SoKhongXepDuoc = job.SoKhongXepDuoc,
            Score = job.Score,
            NgayTao = job.NgayTao,
            NgayXuatBan = job.NgayXuatBan,
            Items = draftItems.Select(x =>
            {
                courseMap.TryGetValue(x.MaKhoaHoc, out var course);
                roomMap.TryGetValue(x.MaPhong ?? 0, out var room);
                shiftMap.TryGetValue(x.MaCaHoc ?? 0, out var shift);

                return new ScheduleDraftItemDto
                {
                    MaDraftItem = x.MaDraftItem,
                    MaKhoaHoc = x.MaKhoaHoc,
                    MaKhoaHocCode = null,
                    ThuTrongTuan = x.ThuTrongTuan,
                    MaCaHoc = x.MaCaHoc,
                    TenCa = shift?.TenCa,
                    MaPhong = x.MaPhong,
                    TenPhong = room?.TenPhong,
                    TrangThai = x.TrangThai,
                    Score = x.Score,
                    CanhBao = x.CanhBaoJson != null
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.CanhBaoJson) ?? new()
                        : new(),
                    Loi = x.LoiJson != null
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.LoiJson) ?? new()
                        : new()
                };
            }).ToList()
        };
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        return currentUser;
    }

    private static void EnsureCanManageSchedule(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thời khóa biểu thông minh.");
    }
}

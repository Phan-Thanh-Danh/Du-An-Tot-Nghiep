using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

using Backend.Services.ThoiKhoaBieu.Scoring;
using Backend.DTOs.SmartTimetable.Suggestions;
using System.Text.Json;

namespace Backend.Services.ThoiKhoaBieu;

public class SmartTimetableService : ISmartTimetableService
{
    private const string PublishedScheduleStatus = "da_xuat_ban";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SmartTimetableService> _logger;
    private readonly IAcademicSchedulingContextService _schedulingContextService;
    private readonly IScheduleCandidateScoringService _scoringService;

    public SmartTimetableService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        ILogger<SmartTimetableService> logger,
        IAcademicSchedulingContextService schedulingContextService,
        IScheduleCandidateScoringService scoringService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _logger = logger;
        _schedulingContextService = schedulingContextService;
        _scoringService = scoringService;
    }

    public async Task<ScheduleDraftDto> GenerateAsync(
        GenerateTimetableRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        await _schedulingContextService.ValidateSchedulableTermAsync(request.MaDonVi, request.MaHocKy, cancellationToken);

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

        var preferences = await LoadPreferencesAsync(request.MaHocKy, request.MaDonVi, courses.Select(x => x.MaGiaoVien), cancellationToken);
        var studentCounts = await GetClassStudentCountsAsync(courses.Select(x => x.MaLop), cancellationToken);

        var sortedCourses = courses
            .OrderBy(c => c.MaGiaoVien)
            .ThenBy(c => c.MaKhoaHoc)
            .ToList();

        foreach (var course in sortedCourses)
        {
            var suggestions = GetCourseSlotSuggestions(course, map, shifts, rooms, null, preferences, studentCounts, 1);
            var best = suggestions.Candidates.FirstOrDefault();

            var item = new ScheduleDraftItem
            {
                MaJob = job.MaJob,
                MaKhoaHoc = course.MaKhoaHoc,
                TrangThai = best != null ? "xep_duoc" : "khong_xep_duoc"
            };

            if (best != null)
            {
                item.ThuTrongTuan = best.ThuTrongTuan;
                item.MaCaHoc = best.MaCaHoc;
                item.MaPhong = best.MaPhong;
                item.Score = best.Score;
                item.ScoreBreakdownJson = JsonSerializer.Serialize(best.Components);
                item.LyDoGoiYJson = JsonSerializer.Serialize(best.Reasons);
                item.CanhBaoJson = best.Warnings.Count > 0 ? JsonSerializer.Serialize(best.Warnings) : null;
                
                map.OccupyTeacher(request.MaHocKy, best.ThuTrongTuan, best.MaCaHoc, course.MaGiaoVien);
                map.OccupyClass(request.MaHocKy, best.ThuTrongTuan, best.MaCaHoc, course.MaLop);
                map.OccupyRoom(request.MaHocKy, best.ThuTrongTuan, best.MaCaHoc, best.MaPhong);
                
                xepDuoc++;
            }
            else
            {
                khongXepDuoc++;
                item.LoiJson = JsonSerializer.Serialize(new List<string> { "Không tìm được slot trống hoặc vi phạm nguyện vọng." });
            }

            items.Add(item);
        }

        _context.ScheduleDraftItems.AddRange(items);
        job.SoXepDuoc = xepDuoc;
        job.SoKhongXepDuoc = khongXepDuoc;
        
        // Calculate average score of assigned items
        var assignedItems = items.Where(x => x.TrangThai == "xep_duoc").ToList();
        job.Score = assignedItems.Count > 0 ? assignedItems.Average(x => x.Score ?? 0) : 0;

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
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        var job = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DraftId == draftId, cancellationToken);

        if (job is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bản nháp.");

        if (job.MaDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");

        return await ToDraftDtoAsync(job.MaJob, cancellationToken);
    }

    public async Task<List<ScheduleDraftDto>> ListDraftsAsync(
        int maDonVi,
        int maHocKy,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        if (maDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");

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

        if (job.MaDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");

        await _schedulingContextService.ValidateSchedulableTermAsync(job.MaDonVi, job.MaHocKy, cancellationToken);

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
                var courses = await _context.KhoaHocs.Include(x => x.MonHoc).AsNoTracking()
                    .Where(x => x.MaHocKy == job.MaHocKy && x.MaDonVi == job.MaDonVi)
                    .ToDictionaryAsync(x => x.MaKhoaHoc, cancellationToken);
                
                var quyDoiDict = await _context.QuyDoiTinChis.AsNoTracking()
                    .ToDictionaryAsync(x => x.SoTinChi, x => x.SoBuoiMoiTuan, cancellationToken);

                var rooms = await _context.PhongHocs.AsNoTracking()
                    .Where(x => x.MaDonVi == job.MaDonVi)
                    .ToDictionaryAsync(x => x.MaPhong, cancellationToken);

                var existingSchedulesCount = await _context.ThoiKhoaBieus.AsNoTracking()
                    .Where(x => x.TrangThai != "da_huy")
                    .GroupBy(x => x.MaKhoaHoc)
                    .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

                var groupedItems = items.GroupBy(x => x.MaKhoaHoc).ToList();
                var validItems = new List<Models.ScheduleDraftItem>();

                foreach (var group in groupedItems)
                {
                    if (courses.TryGetValue(group.Key, out var c) && c.MonHoc != null)
                    {
                        int soBuoiYeuCau = quyDoiDict.GetValueOrDefault(c.MonHoc.SoTinChi, 0);
                        int soBuoiHienTai = existingSchedulesCount.GetValueOrDefault(group.Key, 0);
                        int soBuoiThem = group.Count();

                        if (soBuoiYeuCau > 0 && (soBuoiHienTai + soBuoiThem) < soBuoiYeuCau)
                        {
                            result.BuoiHocLoi += soBuoiThem;
                            result.ChiTietLoi.Add($"MaKhoaHoc {group.Key}: chưa xếp đủ số buổi/tuần yêu cầu ({soBuoiHienTai + soBuoiThem}/{soBuoiYeuCau}).");
                            continue;
                        }
                    }
                    validItems.AddRange(group);
                }

                foreach (var item in validItems)
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

                    if (!rooms.TryGetValue(item.MaPhong.Value, out var room) || room.TrangThaiPhong != "hoat_dong")
                    {
                        result.BuoiHocLoi++;
                        result.ChiTietLoi.Add($"MaKhoaHoc {item.MaKhoaHoc}: phòng học không khả dụng.");
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

                if (result.BuoiHocLoi > 0)
                {
                    job.TrangThai = "xuat_ban_mot_phan";
                }
                else
                {
                    job.TrangThai = "da_xuat_ban";
                }
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
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        if (request.MaDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");
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
        
        if (job.MaDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");
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

    public async Task<CourseSlotSuggestionResultDto> SuggestSlotsAsync(
        SuggestScheduleSlotsRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);

        var course = await _context.KhoaHocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == request.MaKhoaHoc, cancellationToken);
            
        if (course is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa học.");
            
        await _schedulingContextService.ValidateSchedulableTermAsync(course.MaDonVi, course.MaHocKy ?? 0, cancellationToken);
        
        // Optionally validate against currentUser campus if needed
        if (course.MaDonVi != currentUser.CampusId && currentUser.Role != AuthRoles.SuperAdmin)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền trên cơ sở này.");

        var shifts = await LoadShiftsAsync(request.CandidateShiftIds, cancellationToken);
        var rooms = await LoadRoomsAsync(course.MaDonVi, request.CandidateRoomIds, cancellationToken);
        var map = await BuildOccupationMapAsync(course.MaHocKy ?? 0, course.MaDonVi, cancellationToken);
        var preferences = await LoadPreferencesAsync(course.MaHocKy ?? 0, course.MaDonVi, new[] { course.MaGiaoVien }, cancellationToken);
        var studentCounts = await GetClassStudentCountsAsync(new[] { course.MaLop }, cancellationToken);

        var result = GetCourseSlotSuggestions(course, map, shifts, rooms, request.CandidateDays, preferences, studentCounts, request.TopN);
        return result;
    }

    public async Task<BatchSlotSuggestionResultDto> SuggestSlotsBatchAsync(
        SuggestScheduleSlotsBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageSchedule(currentUser);
        
        if (request.MaKhoaHocIds.Count == 0 || request.MaKhoaHocIds.Count > 100)
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách khóa học không hợp lệ (1-100).");

        var courses = await _context.KhoaHocs.AsNoTracking()
            .Where(x => request.MaKhoaHocIds.Contains(x.MaKhoaHoc))
            .ToListAsync(cancellationToken);

        if (courses.Count == 0 || courses.Count != request.MaKhoaHocIds.Distinct().Count())
            throw new ApiException(StatusCodes.Status400BadRequest, "Có khóa học không tồn tại hoặc không thuộc phạm vi được phép.");

        var maDonVi = courses[0].MaDonVi;
        var maHocKy = courses[0].MaHocKy ?? 0;

        if (courses.Any(x => x.MaDonVi != maDonVi || x.MaHocKy != maHocKy))
            throw new ApiException(StatusCodes.Status400BadRequest, "Tất cả khóa học phải thuộc cùng cơ sở và học kỳ.");

        await _schedulingContextService.ValidateSchedulableTermAsync(maDonVi, maHocKy, cancellationToken);

        var shifts = await LoadShiftsAsync(null, cancellationToken);
        var rooms = await LoadRoomsAsync(maDonVi, null, cancellationToken);
        var map = await BuildOccupationMapAsync(maHocKy, maDonVi, cancellationToken);
        var teacherIds = courses.Select(x => x.MaGiaoVien).Distinct();
        var preferences = await LoadPreferencesAsync(maHocKy, maDonVi, teacherIds, cancellationToken);
        var studentCounts = await GetClassStudentCountsAsync(courses.Select(x => x.MaLop), cancellationToken);

        var result = new BatchSlotSuggestionResultDto();

        // Sort deterministic to ensure batch suggestions are stable
        var sortedCourses = courses
            .OrderBy(c => c.MaGiaoVien)
            .ThenBy(c => c.MaKhoaHoc)
            .ToList();

        foreach (var course in sortedCourses)
        {
            var suggestions = GetCourseSlotSuggestions(course, map, shifts, rooms, null, preferences, studentCounts, request.TopNPerCourse);
            var best = suggestions.Candidates.FirstOrDefault();

            if (best != null)
            {
                result.Assigned.Add(new AssignedCourseSuggestionDto
                {
                    MaKhoaHoc = course.MaKhoaHoc,
                    SelectedCandidate = best,
                    Alternatives = suggestions.Candidates.Skip(1).ToList()
                });
                
                map.OccupyTeacher(maHocKy, best.ThuTrongTuan, best.MaCaHoc, course.MaGiaoVien);
                map.OccupyClass(maHocKy, best.ThuTrongTuan, best.MaCaHoc, course.MaLop);
                map.OccupyRoom(maHocKy, best.ThuTrongTuan, best.MaCaHoc, best.MaPhong);
            }
            else
            {
                result.Unassigned.Add(new UnassignedCourseSuggestionDto
                {
                    MaKhoaHoc = course.MaKhoaHoc,
                    ReasonCode = "NO_VALID_SLOT",
                    Reasons = new List<string> { "Không tìm được slot phù hợp (hoặc bị giới hạn bới các constraint cứng)." }
                });
            }
        }

        result.Summary.Total = courses.Count;
        result.Summary.Assigned = result.Assigned.Count;
        result.Summary.Unassigned = result.Unassigned.Count;

        return result;
    }

    private CourseSlotSuggestionResultDto GetCourseSlotSuggestions(
        KhoaHoc course,
        OccupationMap map,
        List<Models.CaHoc> shifts,
        List<Models.PhongHoc> rooms,
        List<int>? candidateDays,
        Dictionary<int, Dictionary<(int Day, int Shift), (string Level, bool IsDraft)>> preferencesMap,
        Dictionary<int, int> studentCounts,
        int topN)
    {
        var result = new CourseSlotSuggestionResultDto
        {
            MaKhoaHoc = course.MaKhoaHoc,
            MaHocKy = course.MaHocKy ?? 0,
            MaDonVi = course.MaDonVi,
            ExpectedStudentCount = studentCounts.GetValueOrDefault(course.MaLop, 0)
        };

        var teacherPrefs = preferencesMap.GetValueOrDefault(course.MaGiaoVien);
        result.TeacherPreferenceStatus = teacherPrefs != null 
            ? (teacherPrefs.Values.Any(x => x.IsDraft) ? "draft" : "submitted") 
            : "unknown";

        var days = candidateDays ?? new List<int> { 2, 3, 4, 5, 6, 7 };
        var candidates = new List<ScheduleSlotSuggestionDto>();

        foreach (var day in days)
        {
            foreach (var shift in shifts)
            {
                if (map.IsTeacherOccupied(result.MaHocKy, day, shift.MaCaHoc, course.MaGiaoVien))
                {
                    result.RejectedSummary.TeacherConflicts++;
                    continue;
                }
                
                if (map.IsClassOccupied(result.MaHocKy, day, shift.MaCaHoc, course.MaLop))
                {
                    result.RejectedSummary.ClassConflicts++;
                    continue;
                }

                foreach (var room in rooms)
                {
                    if (map.IsRoomOccupied(result.MaHocKy, day, shift.MaCaHoc, room.MaPhong))
                    {
                        result.RejectedSummary.RoomConflicts++;
                        continue;
                    }

                    var pref = teacherPrefs?.GetValueOrDefault((day, shift.MaCaHoc));
                    
                    var context = new ScheduleCandidateContext
                    {
                        MaHocKy = result.MaHocKy,
                        MaDonVi = result.MaDonVi,
                        Course = course,
                        Room = room,
                        Shift = shift,
                        DayOfWeek = day,
                        ExpectedStudentCount = result.ExpectedStudentCount,
                        PreferenceLevel = pref?.IsDraft == false ? pref?.Level : null,
                        HasDraftPreference = pref?.IsDraft == true,
                        TeacherDailyLoad = map.GetTeacherDailyLoad(result.MaHocKy, day, course.MaGiaoVien),
                        ClassDailyLoad = map.GetClassDailyLoad(result.MaHocKy, day, course.MaLop)
                    };

                    var suggestion = _scoringService.ScoreCandidate(context);

                    if (suggestion.HardConstraintPassed)
                    {
                        candidates.Add(suggestion);
                    }
                    else
                    {
                        if (suggestion.Warnings.Any(w => w.Contains("báo bận"))) result.RejectedSummary.UnavailablePreferences++;
                        else if (suggestion.Warnings.Any(w => w.Contains("Sức chứa"))) result.RejectedSummary.CapacityRejected++;
                        else if (suggestion.Warnings.Any(w => w.Contains("không hoạt động"))) result.RejectedSummary.InactiveRooms++;
                    }
                }
            }
        }

        result.Candidates = _scoringService.SortCandidates(candidates).Take(topN).ToList();
        return result;
    }

    private async Task<List<Models.CaHoc>> LoadShiftsAsync(List<int>? shiftIds, CancellationToken cancellationToken)
    {
        var query = _context.CaHocs.AsNoTracking().Where(x => x.ConHoatDong);
        if (shiftIds != null && shiftIds.Count > 0)
            query = query.Where(x => shiftIds.Contains(x.MaCaHoc));
            
        return await query.OrderBy(x => x.ThuTu).ToListAsync(cancellationToken);
    }

    private async Task<List<PhongHoc>> LoadRoomsAsync(int maDonVi, List<int>? roomIds, CancellationToken cancellationToken)
    {
        var query = _context.PhongHocs.AsNoTracking()
            .Where(x => x.TrangThaiPhong == "hoat_dong" && x.MaDonVi == maDonVi);
            
        if (roomIds != null && roomIds.Count > 0)
            query = query.Where(x => roomIds.Contains(x.MaPhong));
            
        return await query.ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<int, Dictionary<(int Day, int Shift), (string Level, bool IsDraft)>>> LoadPreferencesAsync(
        int maHocKy, int maDonVi, IEnumerable<int> teacherIds, CancellationToken cancellationToken)
    {
        var teacherIdList = teacherIds.Distinct().ToList();
        
        var rawPrefs = await _context.GiaoVienNguyenVongHocKys.AsNoTracking()
            .Include(x => x.ChiTietNguyenVong)
            .Where(x => x.MaHocKy == maHocKy && x.MaDonVi == maDonVi && teacherIdList.Contains(x.MaGiaoVien))
            .ToListAsync(cancellationToken);

        var map = new Dictionary<int, Dictionary<(int Day, int Shift), (string Level, bool IsDraft)>>();
        foreach (var pref in rawPrefs)
        {
            var isDraft = pref.TrangThai != "submitted";
            var details = new Dictionary<(int, int), (string, bool)>();
            
            foreach (var detail in pref.ChiTietNguyenVong)
            {
                details[(detail.ThuTrongTuan, detail.MaCaHoc)] = (detail.MucDo, isDraft);
            }
            map[pref.MaGiaoVien] = details;
        }

        return map;
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
                    ScoreBreakdown = x.ScoreBreakdownJson != null 
                        ? System.Text.Json.JsonSerializer.Deserialize<Backend.DTOs.SmartTimetable.Suggestions.ScheduleSlotScoreComponentsDto>(x.ScoreBreakdownJson) 
                        : null,
                    LyDoGoiY = x.LyDoGoiYJson != null
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.LyDoGoiYJson) ?? new()
                        : new(),
                    PreferenceLevel = null, // Backend does not store this separately yet
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

    private async Task<Dictionary<int, int>> GetClassStudentCountsAsync(IEnumerable<int> classIds, CancellationToken cancellationToken)
    {
        var ids = classIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, int>();

        var counts = await _context.NguoiDungs
            .Where(x => x.MaLop != null && ids.Contains(x.MaLop.Value) && x.TrangThai == "hoat_dong" && x.VaiTroChinh == "HocSinh")
            .GroupBy(x => x.MaLop!.Value)
            .Select(g => new { MaLop = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MaLop, x => x.Count, cancellationToken);
            
        return counts;
    }
}

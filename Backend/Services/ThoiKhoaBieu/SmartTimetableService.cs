using Backend.Constants;
using Backend.Configuration;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.Services.BuoiHoc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
    private readonly IGeneticTimetableSolver _geneticSolver;
    private readonly SmartTimetableScoringOptions _scoringOptions;

    public SmartTimetableService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        ILogger<SmartTimetableService> logger,
        IAcademicSchedulingContextService schedulingContextService,
        IScheduleCandidateScoringService scoringService,
        IGeneticTimetableSolver geneticSolver,
        IOptions<SmartTimetableScoringOptions> scoringOptions)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _logger = logger;
        _schedulingContextService = schedulingContextService;
        _scoringService = scoringService;
        _geneticSolver = geneticSolver;
        _scoringOptions = scoringOptions.Value;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, GenerationProgress> _progressStore = new();

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

        var draftId = request.ClientDraftId ?? Guid.NewGuid();

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

        var skillsByMonHoc = await LoadSkillMatrixAsync(request.MaDonVi, courses.Select(x => x.MaMonHoc).Distinct(), cancellationToken);
        var studentCounts = await GetClassStudentCountsAsync(courses.Select(x => x.MaLop), cancellationToken);
        var confirmedAvailability = await LoadConfirmedTeachingAvailabilityAsync(
            request.MaHocKy,
            request.MaDonVi,
            skillsByMonHoc.Values.SelectMany(x => x).Select(x => x.MaGiaoVien),
            cancellationToken);

        var quyDoi = await _context.QuyDoiTinChis.AsNoTracking()
            .ToDictionaryAsync(x => x.SoTinChi, x => x.SoBuoiMoiTuan, cancellationToken);

        var requiredSlots = new Dictionary<int, int>();
        foreach (var course in courses)
        {
            var soTinChi = course.MonHoc?.SoTinChi ?? 0;
            requiredSlots[course.MaKhoaHoc] = quyDoi.GetValueOrDefault(soTinChi, 1);
        }

        var progress = new GenerationProgress
        {
            TheHeHienTai = 0,
            TongTheHe = request.TongTheHe ?? 100,
            BestFitness = 0,
            XepDuoc = 0,
            KhongXepDuoc = courses.Count
        };
        _progressStore[draftId] = progress;

        var result = _geneticSolver.Solve(
            courses,
            shifts,
            rooms,
            requiredSlots,
            skillsByMonHoc,
            studentCounts,
            confirmedAvailability,
            request.TongTheHe ?? 100,
            request.KichThuocQuanThe ?? 50,
            request.TyLeCheo ?? 0.5,
            request.DoTuoiThoToiDa ?? 10,
            p => _progressStore[draftId] = p);

        _progressStore.TryRemove(draftId, out _);

        var items = new List<ScheduleDraftItem>();
        var assignedCourseIds = result.Assignments.Select(x => x.MaKhoaHoc).Distinct().ToHashSet();

        foreach (var assignment in result.Assignments)
        {
            items.Add(new ScheduleDraftItem
            {
                MaJob = job.MaJob,
                MaKhoaHoc = assignment.MaKhoaHoc,
                MaGiaoVien = assignment.MaGiaoVien,
                MucDoPhuHop = assignment.MucDoPhuHop,
                ThuTrongTuan = assignment.ThuTrongTuan,
                MaCaHoc = assignment.MaCaHoc,
                MaPhong = assignment.MaPhong,
                TrangThai = "xep_duoc",
                Score = assignment.Score,
                ScoreBreakdownJson = JsonSerializer.Serialize(assignment.Components),
                LyDoGoiYJson = JsonSerializer.Serialize(assignment.Reasons),
                CanhBaoJson = assignment.Warnings.Count > 0 ? JsonSerializer.Serialize(assignment.Warnings) : null
            });
        }

        foreach (var course in courses.Where(c => !assignedCourseIds.Contains(c.MaKhoaHoc)))
        {
            items.Add(new ScheduleDraftItem
            {
                MaJob = job.MaJob,
                MaKhoaHoc = course.MaKhoaHoc,
                TrangThai = "khong_xep_duoc",
                LoiJson = JsonSerializer.Serialize(new List<string> { "Thuật toán di truyền không tìm đủ slot hợp lệ cho khóa học này." })
            });
        }

        _context.ScheduleDraftItems.AddRange(items);
        job.TrangThai = "draft";
        job.SoXepDuoc = result.XepDuoc;
        job.SoKhongXepDuoc = result.KhongXepDuoc;

        var assignedItems = items.Where(x => x.TrangThai == "xep_duoc").ToList();
        job.Score = assignedItems.Count > 0 ? assignedItems.Average(x => x.Score ?? 0) : 0;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "SmartTimetable", draftId.ToString(), "GENERATE",
            null, new { request.MaHocKy, request.MaDonVi, xepDuoc = result.XepDuoc, khongXepDuoc = result.KhongXepDuoc },
            currentUser.UserId, request.MaDonVi,
            "Sinh thời khóa biểu thông minh bằng thuật toán di truyền.", cancellationToken);

        job.TomTatJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            xepDuoc = result.XepDuoc,
            khongXepDuoc = result.KhongXepDuoc,
            Score = job.Score,
            tongTheHe = request.TongTheHe ?? 100,
            kichThuocQuanThe = request.KichThuocQuanThe ?? 50,
            tyLeCheo = request.TyLeCheo ?? 0.5,
            doTuoiThoToiDa = request.DoTuoiThoToiDa ?? 10,
            theHeDaChay = result.TheHeDaChay,
            bestFitness = result.BestFitness,
            thoiGianChayMs = result.ThoiGianChayMs
        });

        await _context.SaveChangesAsync(cancellationToken);

        return await ToDraftDtoAsync(job.MaJob, cancellationToken);
    }

    public async Task<GenerationProgress> GetGenerationProgressAsync(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        if (_progressStore.TryGetValue(draftId, out var live))
            return live;

        var job = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DraftId == draftId, cancellationToken);

        if (job is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy công việc xếp lịch.");

        return new GenerationProgress
        {
            TheHeHienTai = job.TongCourse ?? 0,
            TongTheHe = job.TongCourse ?? 0,
            BestFitness = job.Score ?? 0,
            XepDuoc = job.SoXepDuoc ?? 0,
            KhongXepDuoc = job.SoKhongXepDuoc ?? 0,
            ThoiGianChayMs = null
        };
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

        if (job.TrangThai != "draft")
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ bản nháp ở trạng thái draft mới được xuất bản.");

        if ((job.SoKhongXepDuoc ?? 0) > 0)
            throw new ApiException(StatusCodes.Status400BadRequest,
                "Không thể xuất bản bản nháp chưa xếp đủ tất cả khóa học.");

        var items = await _context.ScheduleDraftItems
            .AsNoTracking()
            .Where(x => x.MaJob == job.MaJob && x.TrangThai == "xep_duoc")
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            throw new ApiException(StatusCodes.Status400BadRequest, "Bản nháp không có ca học hợp lệ để xuất bản.");

        var result = new TimetablePublishResultDto();
        var giaoVienChanges = new List<object>();
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var hasPublishedSchedule = await _context.ThoiKhoaBieus
                    .AnyAsync(x =>
                        x.KhoaHoc != null &&
                        x.KhoaHoc.MaHocKy == job.MaHocKy &&
                        x.KhoaHoc.MaDonVi == job.MaDonVi &&
                        x.TrangThai == PublishedScheduleStatus,
                        cancellationToken);

                if (hasPublishedSchedule)
                    throw new ApiException(StatusCodes.Status409Conflict,
                        "Học kỳ này đã có thời khóa biểu được xuất bản. Hãy dùng luồng điều chỉnh thời khóa biểu, không thể ghi đè bằng bản nháp mới.");

                // Lần xuất bản đầu chỉ thay thế lịch nháp cũ, không bao giờ thay lịch đã công bố.
                await _context.ThoiKhoaBieus
                    .Where(x =>
                        x.KhoaHoc != null &&
                        x.KhoaHoc.MaHocKy == job.MaHocKy &&
                        x.KhoaHoc.MaDonVi == job.MaDonVi &&
                        x.TrangThai == "nhap")
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, "da_huy"), cancellationToken);

                var map = await BuildOccupationMapAsync(job.MaHocKy, job.MaDonVi, cancellationToken);
                var courses = await _context.KhoaHocs.AsNoTracking()
                    .Where(x => x.MaHocKy == job.MaHocKy && x.MaDonVi == job.MaDonVi)
                    .ToDictionaryAsync(x => x.MaKhoaHoc, cancellationToken);

                var soTinChiByMonHoc = await _context.DanhMucMonHocs.AsNoTracking()
                    .ToDictionaryAsync(x => x.MaMonHoc, x => x.SoTinChi, cancellationToken);
                
                var quyDoiDict = await _context.QuyDoiTinChis.AsNoTracking()
                    .ToDictionaryAsync(x => x.SoTinChi, x => x.SoBuoiMoiTuan, cancellationToken);

                var rooms = await _context.PhongHocs.AsNoTracking()
                    .Where(x => x.MaDonVi == job.MaDonVi)
                    .ToDictionaryAsync(x => x.MaPhong, cancellationToken);

                var groupedItems = items.GroupBy(x => x.MaKhoaHoc).ToList();
                var activeCourseCount = courses.Count;
                if (job.TongCourse != activeCourseCount)
                    throw new ApiException(StatusCodes.Status400BadRequest,
                        "Không thể xuất bản bản nháp chỉ xếp một phần khóa học của học kỳ.");

                if (groupedItems.Count != activeCourseCount)
                    throw new ApiException(StatusCodes.Status400BadRequest,
                            "Bản nháp chưa có đủ ca học cho tất cả khóa học đang hoạt động.");

                var weeklyLoadByTeacher = items
                    .GroupBy(x => x.MaGiaoVien ?? (courses.TryGetValue(x.MaKhoaHoc, out var course) ? course.MaGiaoVien : 0))
                    .ToDictionary(x => x.Key, x => x.Count());
                if (weeklyLoadByTeacher.Any(x => x.Key <= 0 || x.Value > _scoringOptions.WeeklyCapCa))
                    throw new ApiException(StatusCodes.Status400BadRequest,
                        $"Bản nháp vượt giới hạn cứng {_scoringOptions.WeeklyCapCa} ca/tuần cho một giảng viên hoặc thiếu giảng viên hợp lệ.");

                foreach (var group in groupedItems)
                {
                    if (!courses.TryGetValue(group.Key, out var c))
                        throw new ApiException(StatusCodes.Status400BadRequest,
                            $"MaKhoaHoc {group.Key}: khóa học không tồn tại trong phạm vi xuất bản.");

                    var soBuoiYeuCau = quyDoiDict.GetValueOrDefault(
                        soTinChiByMonHoc.GetValueOrDefault(c.MaMonHoc, 0), 1);
                    if (group.Count() != soBuoiYeuCau)
                        throw new ApiException(StatusCodes.Status400BadRequest,
                            $"MaKhoaHoc {group.Key}: bản nháp phải có đúng {soBuoiYeuCau} ca/tuần (hiện có {group.Count()}).");
                }

                // GA chọn giảng viên cho từng khóa: ghi đè MaGiaoVien của khóa trước khi check conflict
                var attachedCourses = new HashSet<int>();
                foreach (var item in items)
                {
                    if (!item.MaGiaoVien.HasValue) continue;
                    if (courses.TryGetValue(item.MaKhoaHoc, out var c) && c.MaGiaoVien != item.MaGiaoVien.Value)
                    {
                        giaoVienChanges.Add(new { maKhoaHoc = item.MaKhoaHoc, tuGiaoVien = c.MaGiaoVien, denGiaoVien = item.MaGiaoVien.Value });
                        c.MaGiaoVien = item.MaGiaoVien.Value;
                        if (attachedCourses.Add(c.MaKhoaHoc))
                        {
                            _context.KhoaHocs.Attach(c);
                            _context.Entry(c).Property(x => x.MaGiaoVien).IsModified = true;
                        }
                    }
                }

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

                    if (schedule.NgayBatDau.HasValue && schedule.NgayKetThuc.HasValue)
                    {
                        var sessionDates = SessionDateHelper.ExpandSessionDates(
                            schedule.NgayBatDau.Value,
                            schedule.NgayKetThuc.Value,
                            schedule.ThuTrongTuan);

                        foreach (var sessionDate in sessionDates)
                        {
                            _context.BuoiHocs.Add(new Models.BuoiHoc
                            {
                                Tkb = schedule,
                                MaKhoaHoc = item.MaKhoaHoc,
                                NgayHoc = sessionDate,
                                MaCaHoc = item.MaCaHoc.Value,
                                MaPhong = item.MaPhong.Value,
                                MaGiaoVien = course.MaGiaoVien,
                                MaGiaoVienDayThay = null,
                                TrangThaiBuoi = "du_kien",
                                TrangThaiDiemDanh = "chua_mo",
                                LoaiThayDoi = null,
                                LyDoThayDoi = null,
                                GhiChu = null,
                                KhoaLuc = null,
                                NgayTao = DateTime.UtcNow,
                                NgayCapNhat = DateTime.UtcNow
                            });
                        }
                    }
                }

                if (result.BuoiHocLoi > 0)
                    throw new ApiException(StatusCodes.Status400BadRequest,
                        "Bản nháp có dữ liệu không hợp lệ nên không thể xuất bản.");

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
            null, new { publishResult = result, giaoVienChanges }, currentUser.UserId, job.MaDonVi,
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
        var studentCounts = await GetClassStudentCountsAsync(new[] { course.MaLop }, cancellationToken);
        var confirmedAvailability = await LoadConfirmedTeachingAvailabilityAsync(
            course.MaHocKy ?? 0, course.MaDonVi, new[] { course.MaGiaoVien }, cancellationToken);

        var result = GetCourseSlotSuggestions(course, map, shifts, rooms, request.CandidateDays, studentCounts, confirmedAvailability, request.TopN);
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
        var studentCounts = await GetClassStudentCountsAsync(courses.Select(x => x.MaLop), cancellationToken);
        var confirmedAvailability = await LoadConfirmedTeachingAvailabilityAsync(
            maHocKy, maDonVi, courses.Select(x => x.MaGiaoVien), cancellationToken);

        var result = new BatchSlotSuggestionResultDto();

        // Sort deterministic to ensure batch suggestions are stable
        var sortedCourses = courses
            .OrderBy(c => c.MaGiaoVien)
            .ThenBy(c => c.MaKhoaHoc)
            .ToList();

        foreach (var course in sortedCourses)
        {
            var suggestions = GetCourseSlotSuggestions(course, map, shifts, rooms, null, studentCounts, confirmedAvailability, request.TopNPerCourse);
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
        Dictionary<int, int> studentCounts,
        IReadOnlyDictionary<int, IReadOnlySet<(int Day, int Shift)>> confirmedAvailabilityByTeacher,
        int topN)
    {
        var result = new CourseSlotSuggestionResultDto
        {
            MaKhoaHoc = course.MaKhoaHoc,
            MaHocKy = course.MaHocKy ?? 0,
            MaDonVi = course.MaDonVi,
            ExpectedStudentCount = studentCounts.GetValueOrDefault(course.MaLop, 0)
        };

        var days = candidateDays ?? new List<int> { 2, 3, 4, 5, 6, 7 };
        var candidates = new List<ScheduleSlotSuggestionDto>();

        foreach (var day in days)
        {
            foreach (var shift in shifts)
            {
                if (map.GetTeacherWeeklyLoad(result.MaHocKy, course.MaGiaoVien) >= _scoringOptions.WeeklyCapCa)
                    continue;

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
                    if (confirmedAvailabilityByTeacher.TryGetValue(course.MaGiaoVien, out var availableSlots) &&
                        !availableSlots.Contains((day, shift.MaCaHoc)))
                        continue;

                    if (map.IsRoomOccupied(result.MaHocKy, day, shift.MaCaHoc, room.MaPhong))
                    {
                        result.RejectedSummary.RoomConflicts++;
                        continue;
                    }

                    var context = new ScheduleCandidateContext
                    {
                        MaHocKy = result.MaHocKy,
                        MaDonVi = result.MaDonVi,
                        Course = course,
                        Room = room,
                        Shift = shift,
                        DayOfWeek = day,
                        ExpectedStudentCount = result.ExpectedStudentCount,
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
                        if (suggestion.Warnings.Any(w => w.Contains("Sức chứa"))) result.RejectedSummary.CapacityRejected++;
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

    private async Task<Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>> LoadSkillMatrixAsync(
        int maDonVi,
        IEnumerable<int> monHocIds,
        CancellationToken cancellationToken)
    {
        var ids = monHocIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>();

        var skillRows = await _context.GiaoVienMonHocs.AsNoTracking()
            .Where(x => ids.Contains(x.MaMonHoc) && x.ConHoatDong)
            .ToListAsync(cancellationToken);

        var teacherIds = skillRows.Select(x => x.MaGiaoVien).Distinct().ToList();
        var teachers = teacherIds.Count > 0
            ? await _context.NguoiDungs.AsNoTracking()
                .Where(x => teacherIds.Contains(x.MaNguoiDung) && x.MaDonVi == maDonVi && x.VaiTroChinh == "giao_vien" && x.TrangThai == "hoat_dong")
                .ToDictionaryAsync(x => x.MaNguoiDung, cancellationToken)
            : new Dictionary<int, NguoiDung>();

        return skillRows
            .Where(x => teachers.ContainsKey(x.MaGiaoVien))
            .GroupBy(x => x.MaMonHoc)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<TeacherSkillCandidate>)g
                    .OrderByDescending(x => x.MucDoPhuHop)
                    .Select(x => new TeacherSkillCandidate
                    {
                        MaGiaoVien = x.MaGiaoVien,
                        TenGiaoVien = teachers[x.MaGiaoVien].HoTen,
                        MucDoPhuHop = x.MucDoPhuHop,
                        LaMonChinh = x.LaMonChinh
                    })
                    .ToList());
    }

    private async Task<Dictionary<int, IReadOnlySet<(int Day, int Shift)>>> LoadConfirmedTeachingAvailabilityAsync(
        int maHocKy,
        int maDonVi,
        IEnumerable<int> teacherIds,
        CancellationToken cancellationToken)
    {
        var ids = teacherIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>();

        var forms = await _context.GiaoVienNguyenVongHocKys.AsNoTracking()
            .Include(x => x.ChiTietNguyenVong)
            .Where(x => x.MaHocKy == maHocKy && x.MaDonVi == maDonVi &&
                ids.Contains(x.MaGiaoVien) && x.TrangThai == "submitted")
            .ToListAsync(cancellationToken);

        return forms.ToDictionary(
            x => x.MaGiaoVien,
            x => (IReadOnlySet<(int Day, int Shift)>)x.ChiTietNguyenVong
                .Where(slot => slot.MucDo is "available" or "preferred")
                .Select(slot => (slot.ThuTrongTuan, slot.MaCaHoc))
                .ToHashSet());
    }

    private async Task<List<KhoaHoc>> LoadCoursesAsync(
        int maHocKy,
        int maDonVi,
        List<int>? filter,
        CancellationToken cancellationToken)
    {
        var query = _context.KhoaHocs
            .AsNoTracking()
            .Include(x => x.MonHoc)
            .Where(x => x.MaHocKy == maHocKy && x.MaDonVi == maDonVi && x.TrangThai != "luu_tru");

        if (filter is { Count: > 0 })
            query = query.Where(x => filter.Contains(x.MaKhoaHoc));

        return await query.OrderBy(x => x.TieuDe).ToListAsync(cancellationToken);
    }

    private async Task<ScheduleDraftDto> ToDraftDtoAsync(int maJob, CancellationToken cancellationToken)
    {
        var job = await _context.ScheduleGenerationJobs
            .AsNoTracking()
            .Include(x => x.NguoiYeuCauNavigation)
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

        var monHocIds = courses.Select(x => x.MaMonHoc).Distinct().ToList();
        var lopIds = courses.Select(x => x.MaLop).Distinct().ToList();
        var monHocs = monHocIds.Count > 0
            ? await _context.DanhMucMonHocs.AsNoTracking().Where(x => monHocIds.Contains(x.MaMonHoc)).ToListAsync(cancellationToken)
            : new List<Backend.Models.DanhMucMonHoc>();
        var lopHanhChinhs = lopIds.Count > 0
            ? await _context.LopHanhChinhs.AsNoTracking().Where(x => lopIds.Contains(x.MaLop)).ToListAsync(cancellationToken)
            : new List<Backend.Models.LopHanhChinh>();

        var rooms = roomIds.Count > 0
            ? await _context.PhongHocs.AsNoTracking().Where(x => roomIds.Contains(x.MaPhong)).ToListAsync(cancellationToken)
            : new List<Backend.Models.PhongHoc>();

        var shifts = shiftIds.Count > 0
            ? await _context.CaHocs.AsNoTracking().Where(x => shiftIds.Contains(x.MaCaHoc)).ToListAsync(cancellationToken)
            : new List<Backend.Models.CaHoc>();

        var teacherIds = draftItems.Where(x => x.MaGiaoVien.HasValue).Select(x => x.MaGiaoVien!.Value).Distinct().ToList();
        var teachers = teacherIds.Count > 0
            ? await _context.NguoiDungs.AsNoTracking().Where(x => teacherIds.Contains(x.MaNguoiDung)).ToListAsync(cancellationToken)
            : new List<Backend.Models.NguoiDung>();

        var teacherSkills = teacherIds.Count > 0
            ? await _context.GiaoVienMonHocs.AsNoTracking()
                .Where(x => teacherIds.Contains(x.MaGiaoVien) && x.ConHoatDong)
                .ToListAsync(cancellationToken)
            : new List<Backend.Models.GiaoVienMonHoc>();
        var skillMonHocIds = teacherSkills.Select(x => x.MaMonHoc).Distinct().ToList();
        var skillMonHocs = skillMonHocIds.Count > 0
            ? await _context.DanhMucMonHocs.AsNoTracking().Where(x => skillMonHocIds.Contains(x.MaMonHoc)).ToListAsync(cancellationToken)
            : new List<Backend.Models.DanhMucMonHoc>();

        var courseMap = courses.ToDictionary(x => x.MaKhoaHoc);
        var roomMap = rooms.ToDictionary(x => x.MaPhong);
        var shiftMap = shifts.ToDictionary(x => x.MaCaHoc);
        var teacherMap = teachers.ToDictionary(x => x.MaNguoiDung);
        var monHocMap = monHocs.ToDictionary(x => x.MaMonHoc);
        var lopMap = lopHanhChinhs.ToDictionary(x => x.MaLop);
        var skillMonHocMap = skillMonHocs.ToDictionary(x => x.MaMonHoc);
        var teacherSkillGroup = teacherSkills
            .GroupBy(x => x.MaGiaoVien)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(x => x.MucDoPhuHop)
                    .Select(x => new TeacherSubjectSkillDto
                    {
                        MaMonHoc = x.MaMonHoc,
                        MaCodeMonHoc = skillMonHocMap.TryGetValue(x.MaMonHoc, out var mh) ? mh.MaCodeMonHoc : null,
                        TenMonHoc = skillMonHocMap.TryGetValue(x.MaMonHoc, out var mh2) ? mh2.TenMonHoc : null,
                        MucDoPhuHop = x.MucDoPhuHop,
                        LaMonChinh = x.LaMonChinh,
                    })
                    .ToList());

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
            NguoiYeuCau = job.NguoiYeuCau,
            TenNguoiYeuCau = job.NguoiYeuCauNavigation?.HoTen,
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
                    MaMonHoc = course?.MaMonHoc,
                    MaCodeMonHoc = course is not null && monHocMap.TryGetValue(course.MaMonHoc, out var mh) ? mh.MaCodeMonHoc : null,
                    TenMonHoc = course is not null && monHocMap.TryGetValue(course.MaMonHoc, out var mh2) ? mh2.TenMonHoc : null,
                    MaLop = course?.MaLop,
                    MaCodeLop = course is not null && lopMap.TryGetValue(course.MaLop, out var lop) ? lop.MaCodeLop : null,
                    TenLop = course is not null && lopMap.TryGetValue(course.MaLop, out var lop2) ? lop2.TenLop : null,
                    MaGiaoVien = x.MaGiaoVien,
                    TenGiaoVien = teacherMap.TryGetValue(x.MaGiaoVien ?? 0, out var gv) ? gv.HoTen : null,
                    MucDoPhuHop = x.MucDoPhuHop,
                    MonHocGiangDay = x.MaGiaoVien.HasValue && teacherSkillGroup.TryGetValue(x.MaGiaoVien.Value, out var skills)
                        ? skills
                        : new List<TeacherSubjectSkillDto>(),
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
            .Where(x => x.MaLop != null && ids.Contains(x.MaLop.Value) && x.TrangThai == "hoat_dong" && x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student))
            .GroupBy(x => x.MaLop!.Value)
            .Select(g => new { MaLop = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MaLop, x => x.Count, cancellationToken);
            
        return counts;
    }
}

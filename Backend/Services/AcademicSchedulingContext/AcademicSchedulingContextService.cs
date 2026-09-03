using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Exceptions;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AcademicSchedulingContext;

public class AcademicSchedulingContextService : IAcademicSchedulingContextService
{
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ICourseCapacityService _capacityService;

    public AcademicSchedulingContextService(
        ApplicationDbContext db,
        TimeProvider? timeProvider = null,
        ICourseCapacityService? capacityService = null)
    {
        _db = db;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _capacityService = capacityService ?? new CourseCapacityService(db);
    }

    private DateOnly GetVietnamToday()
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to local if both fail (unlikely in proper environment)
                tz = TimeZoneInfo.Local;
            }
        }
        var now = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
        return DateOnly.FromDateTime(now);
    }

    private string GetTimeZoneName()
    {
        return "Asia/Ho_Chi_Minh"; // standardized representation for output
    }

    public async Task<AcademicSchedulingContextDto> GetContextAsync(
        int campusId,
        CancellationToken cancellationToken = default
    )
    {
        var today = GetVietnamToday();

        var allTerms = await _db
            .HocKys.AsNoTracking()
            .Where(x => x.MaDonVi == campusId)
            .OrderBy(x => x.NgayBatDau)
            .ThenBy(x => x.ThuTuTrongNam)
            .ThenBy(x => x.MaHocKy)
            .Select(x => new SchedulingTermDto
            {
                MaHocKy = x.MaHocKy,
                MaCodeHocKy = x.MaCodeHocKy,
                TenHocKy = x.TenHocKy,
                NgayBatDau = x.NgayBatDau,
                NgayKetThuc = x.NgayKetThuc,
                DaKhoa = x.DaKhoa,
                DaysUntilStart =
                    x.NgayBatDau > today ? x.NgayBatDau.DayNumber - today.DayNumber : null,
            })
            .ToListAsync(cancellationToken);

        var currentTerms = allTerms
            .Where(x => x.NgayBatDau <= today && x.NgayKetThuc >= today && !x.DaKhoa)
            .ToList();
        var futureTerms = allTerms.Where(x => x.NgayBatDau > today && !x.DaKhoa).ToList();

        var result = new AcademicSchedulingContextDto
        {
            Today = today,
            TimeZone = GetTimeZoneName(),
        };

        if (currentTerms.Count > 1)
        {
            result.CanPrepareSchedule = false;
            result.ReasonCode = SchedulingContextReasonCodes.InvalidMultipleCurrentTerms;
            result.ReasonMessage =
                "Dữ liệu có nhiều học kỳ hiện tại đang diễn ra cùng lúc. Vui lòng kiểm tra lại.";
            return result;
        }

        result.CurrentTerm = currentTerms.FirstOrDefault();
        result.NextTerm = futureTerms.FirstOrDefault();

        // A future term with a permanently locked published timetable is not a
        // valid preparation target.  When a later future term exists, advance
        // to the first term that can actually accept a new draft.  Keep the
        // nearest locked term as the fallback so existing callers still receive
        // the precise SCHEDULE_ALREADY_PUBLISHED reason when no alternative
        // future term exists.
        var permanentlyLockedTermIds = await GetPermanentlyLockedTermIdsAsync(
            campusId,
            futureTerms.Select(x => x.MaHocKy).ToList(),
            cancellationToken);
        result.SchedulableTerm = futureTerms.FirstOrDefault(x => !permanentlyLockedTermIds.Contains(x.MaHocKy))
            ?? result.NextTerm;

        if (result.SchedulableTerm == null)
        {
            result.CanPrepareSchedule = false;
            result.ReasonCode = SchedulingContextReasonCodes.NoFutureTerm;
            result.ReasonMessage =
                "Chưa có học kỳ tương lai để chuẩn bị lịch. Vui lòng tạo học kỳ mới.";
            return result;
        }

        result.CanPrepareSchedule = true;
        result.ReasonCode = SchedulingContextReasonCodes.NextTermAvailable;
        result.ReasonMessage = "Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất.";

        // Calculate Structured Readiness (11 items)
        var schedulableTermId = result.SchedulableTerm.MaHocKy;
        var readinessItems = new List<SchedulingReadinessItemDto>();

        // 1. COURSES_READY
        var termCourses = await _db.KhoaHocs.AsNoTracking()
            .Where(x => x.MaHocKy == schedulableTermId && x.TrangThai != "luu_tru")
            .ToListAsync(cancellationToken);
        var subjectIds = termCourses.Select(c => c.MaMonHoc).Where(id => id > 0).Distinct().ToList();
        var subjects = await _db.DanhMucMonHocs.AsNoTracking()
            .Where(x => subjectIds.Contains(x.MaMonHoc))
            .ToDictionaryAsync(x => x.MaMonHoc, cancellationToken);
        var hasCourses = termCourses.Count > 0;
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "COURSES_READY",
            Status = hasCourses ? "ready" : "blocked",
            Message = hasCourses ? $"Có {termCourses.Count} khóa học cần xếp lịch." : "Học kỳ chưa có khóa học để xếp lịch.",
            ActionRoute = "/academic/courses",
            AffectedCount = termCourses.Count
        });

        // 2. BLOCKS_READY
        var blockCount = await _db.Blocks.AsNoTracking()
            .CountAsync(x => x.MaHocKy == schedulableTermId, cancellationToken);
        var hasBlocks = blockCount > 0;
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "BLOCKS_READY",
            Status = hasBlocks ? "ready" : "blocked",
            Message = hasBlocks ? $"Đã cấu hình {blockCount} Block cho học kỳ." : "Học kỳ chưa được cấu hình Block.",
            ActionRoute = "/academic/blocks",
            AffectedCount = blockCount
        });

        // 3. CREDIT_MAPPING_READY
        var creditMappings = await _db.QuyDoiTinChis.AsNoTracking()
            .ToDictionaryAsync(x => x.SoTinChi, cancellationToken);
        var courseCredits = termCourses
            .Select(c => subjects.TryGetValue(c.MaMonHoc, out var sub) ? sub.SoTinChi : (c.MonHoc?.SoTinChi ?? 0))
            .Where(cr => cr > 0)
            .Distinct()
            .ToList();
        var missingCreditMappings = courseCredits.Where(cr => !creditMappings.ContainsKey(cr)).ToList();
        var creditMappingReady = missingCreditMappings.Count == 0 && (courseCredits.Count > 0 || !hasCourses);
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "CREDIT_MAPPING_READY",
            Status = creditMappingReady ? "ready" : "blocked",
            Message = creditMappingReady ? "Đã có đầy đủ quy đổi tín chỉ cho các môn học." : $"Thiếu quy đổi cho các số tín chỉ: {string.Join(", ", missingCreditMappings)}",
            ActionRoute = "/academic/credit-conversion",
            AffectedCount = missingCreditMappings.Count,
            AffectedItems = missingCreditMappings.Select(c => $"{c} tín chỉ").ToList()
        });

        // 4. TEACHER_SKILL_READY
        var teacherRoleCode = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        var qualifiedCapabilities = await (
            from gvmh in _db.GiaoVienMonHocs.AsNoTracking()
            join gv in _db.NguoiDungs.AsNoTracking() on gvmh.MaGiaoVien equals gv.MaNguoiDung
            where gv.MaDonVi == campusId
                && gv.VaiTroChinh == teacherRoleCode
                && gv.TrangThai == UserStatuses.DbActive
                && gvmh.ConHoatDong
                && gvmh.MucDoPhuHop >= 70
                && gvmh.PhuHopChuyenMon != false
                && subjectIds.Contains(gvmh.MaMonHoc)
            select new { gvmh.MaMonHoc, gvmh.MaGiaoVien }
        ).ToListAsync(cancellationToken);
        var qualifiedSubjects = qualifiedCapabilities.Select(x => x.MaMonHoc).ToHashSet();
        var subjectsWithoutTeachers = subjectIds.Where(s => !qualifiedSubjects.Contains(s)).ToList();
        var unassignedTeacherCourses = termCourses.Where(c => c.MaGiaoVien <= 0).Select(c => c.MaKhoaHoc).ToList();
        var teacherSkillBlocked = subjectsWithoutTeachers.Count > 0 || (hasCourses && unassignedTeacherCourses.Count > 0);
        var teacherSkillAffectedItems = subjectsWithoutTeachers
            .Select(id => subjects.TryGetValue(id, out var subject) ? subject.TenMonHoc : $"Môn #{id}")
            .Concat(unassignedTeacherCourses.Select(id => $"Khóa học #{id} chưa phân công giảng viên"))
            .Take(20)
            .ToList();
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "TEACHER_SKILL_READY",
            Status = !hasCourses ? "ready" : (teacherSkillBlocked ? "blocked" : "ready"),
            Message = !hasCourses ? "Chưa có khóa học." : (teacherSkillBlocked
                ? $"Có {subjectsWithoutTeachers.Count} môn chưa có giảng viên đạt năng lực (>=70%) hoặc {unassignedTeacherCourses.Count} khóa chưa phân công giảng viên."
                : "Tất cả môn học đều có giảng viên đủ năng lực chuyên môn."),
            ActionRoute = "/academic/teacher-capabilities",
            AffectedCount = subjectsWithoutTeachers.Count + unassignedTeacherCourses.Count,
            AffectedItems = teacherSkillAffectedItems
        });

        // 5. TEACHER_AVAILABILITY_READY
        var courseTeacherIds = termCourses.Where(c => c.MaGiaoVien > 0).Select(c => c.MaGiaoVien).Distinct().ToList();
        var teacherPreferences = await _db.GiaoVienNguyenVongHocKys.AsNoTracking()
            .Include(x => x.ChiTietNguyenVong)
            .Where(x => x.MaHocKy == schedulableTermId && courseTeacherIds.Contains(x.MaGiaoVien))
            .ToListAsync(cancellationToken);
        var teacherPrefMap = teacherPreferences.ToDictionary(x => x.MaGiaoVien);
        var unavailableTeacherIds = new List<int>();
        foreach (var tId in courseTeacherIds)
        {
            if (teacherPrefMap.TryGetValue(tId, out var pref) && pref.SoCaToiDaMoiTuan <= 0)
            {
                unavailableTeacherIds.Add(tId);
            }
        }
        var teacherAvailabilityBlocked = unavailableTeacherIds.Count > 0;
        var unavailableTeacherNames = await _db.NguoiDungs.AsNoTracking()
            .Where(x => unavailableTeacherIds.Contains(x.MaNguoiDung))
            .OrderBy(x => x.HoTen)
            .Select(x => string.IsNullOrWhiteSpace(x.HoTen) ? $"Giảng viên #{x.MaNguoiDung}" : x.HoTen)
            .Take(20)
            .ToListAsync(cancellationToken);
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "TEACHER_AVAILABILITY_READY",
            Status = teacherAvailabilityBlocked ? "blocked" : "ready",
            Message = teacherAvailabilityBlocked
                ? $"Có {unavailableTeacherIds.Count} giảng viên bị cấu hình 0 ca khả dụng."
                : "Thời gian khả dụng của giảng viên hợp lệ.",
            ActionRoute = "/staff/teaching-preferences",
            AffectedCount = unavailableTeacherIds.Count,
            AffectedItems = unavailableTeacherNames
        });

        // 6. TEACHER_CAPACITY_READY
        var totalRequiredSlots = termCourses.Sum(c =>
        {
            var credits = subjects.TryGetValue(c.MaMonHoc, out var sub) ? sub.SoTinChi : (c.MonHoc?.SoTinChi ?? 0);
            return creditMappings.TryGetValue(credits, out var qd) ? qd.SoBuoiMoiTuan : 1;
        });
        const int weeklyCap = 6;
        var totalFacultyCapacity = courseTeacherIds.Count * weeklyCap;
        var teacherCapacityBlocked = hasCourses && (totalRequiredSlots > totalFacultyCapacity);
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "TEACHER_CAPACITY_READY",
            Status = teacherCapacityBlocked ? "blocked" : "ready",
            Message = teacherCapacityBlocked
                ? $"Tổng ca cần dạy ({totalRequiredSlots} ca) vượt quá tổng trần tải giảng viên khả dụng ({totalFacultyCapacity} ca, trần {weeklyCap} ca/GV)."
                : $"Tổng trần tải giảng viên ({totalFacultyCapacity} ca) đủ cho {totalRequiredSlots} ca học yêu cầu.",
            ActionRoute = "/academic/teachers",
            AffectedCount = teacherCapacityBlocked ? (totalRequiredSlots - totalFacultyCapacity) : 0,
            AffectedItems = teacherCapacityBlocked
                ? new List<string> { $"Thiếu {totalRequiredSlots - totalFacultyCapacity} ca/tuần so với trần tải giảng viên." }
                : new List<string>()
        });

        // 7. ACTIVE_ROOMS_READY
        var activeRooms = await _db.PhongHocs.AsNoTracking()
            .Where(x => x.MaDonVi == campusId && x.TrangThaiPhong == "hoat_dong")
            .ToListAsync(cancellationToken);
        var hasRooms = activeRooms.Count > 0;
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "ACTIVE_ROOMS_READY",
            Status = hasRooms ? "ready" : "blocked",
            Message = hasRooms ? $"Có {activeRooms.Count} phòng học đang hoạt động tại cơ sở." : "Không có phòng học nào đang hoạt động tại cơ sở này.",
            ActionRoute = "/facilities/rooms",
            AffectedCount = activeRooms.Count
        });

        // 8. ROOM_CAPACITY_READY
        var capacityByCourse = hasCourses
            ? await _capacityService.GetRequiredCapacitiesAsync(termCourses, cancellationToken)
            : new Dictionary<int, RequiredCapacity>();
        var missingCapCourses = termCourses.Where(c => capacityByCourse.TryGetValue(c.MaKhoaHoc, out var cap) && !cap.IsKnown).Select(c => c.MaKhoaHoc).ToList();
        var coursesWithoutFittingRoom = termCourses.Where(c =>
        {
            var cap = capacityByCourse.GetValueOrDefault(c.MaKhoaHoc);
            return cap == null || !activeRooms.Any(r => _capacityService.IsRoomEligible(r, cap, campusId));
        }).Select(c => c.MaKhoaHoc).ToList();
        var roomCapacityBlocked = hasCourses && (missingCapCourses.Count > 0 || coursesWithoutFittingRoom.Count > 0);
        var roomCapacityAffectedItems = termCourses
            .Where(c => missingCapCourses.Contains(c.MaKhoaHoc) || coursesWithoutFittingRoom.Contains(c.MaKhoaHoc))
            .Select(c => string.IsNullOrWhiteSpace(c.TieuDe) ? $"Khóa học #{c.MaKhoaHoc}" : $"{c.TieuDe} (#{c.MaKhoaHoc})")
            .Take(20)
            .ToList();
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "ROOM_CAPACITY_READY",
            Status = roomCapacityBlocked ? "blocked" : "ready",
            Message = !hasCourses ? "Chưa có khóa học." : (missingCapCourses.Count > 0
                ? $"Có {missingCapCourses.Count} khóa học chưa có dữ liệu sĩ số (STUDENT_CAPACITY_DATA_MISSING)."
                : (coursesWithoutFittingRoom.Count > 0
                    ? $"Có {coursesWithoutFittingRoom.Count} khóa học không có phòng nào đủ sức chứa tại cơ sở."
                    : "Sức chứa phòng học đáp ứng toàn bộ các khóa học.")),
            ActionRoute = "/facilities/rooms",
            AffectedCount = missingCapCourses.Count + coursesWithoutFittingRoom.Count,
            AffectedItems = roomCapacityAffectedItems
        });

        // 9. ACTIVE_SHIFTS_READY
        var activeShifts = await _db.CaHocs.AsNoTracking()
            .Where(x => x.ConHoatDong)
            .OrderBy(x => x.ThuTu)
            .ToListAsync(cancellationToken);
        var hasShifts = activeShifts.Count > 0;
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "ACTIVE_SHIFTS_READY",
            Status = hasShifts ? "ready" : "blocked",
            Message = hasShifts ? $"Có {activeShifts.Count} ca học đang hoạt động." : "Không có ca học nào đang hoạt động.",
            ActionRoute = "/facilities/shifts",
            AffectedCount = activeShifts.Count
        });


        // 10. TOTAL_ROOM_SLOTS_READY
        var totalRoomSlots = activeRooms.Count * activeShifts.Count * 6; // 6 days / week
        var totalRoomSlotsBlocked = hasCourses && (totalRequiredSlots > totalRoomSlots);
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "TOTAL_ROOM_SLOTS_READY",
            Status = totalRoomSlotsBlocked ? "blocked" : "ready",
            Message = totalRoomSlotsBlocked
                ? $"Tổng số slot phòng/tuần ({totalRoomSlots}) không đủ cho nhu cầu ({totalRequiredSlots} ca). Thiếu {totalRequiredSlots - totalRoomSlots} slot."
                : $"Tổng số slot phòng khả dụng ({totalRoomSlots} slot/tuần) đủ đáp ứng {totalRequiredSlots} ca học yêu cầu.",
            ActionRoute = "/facilities/rooms",
            AffectedCount = totalRoomSlotsBlocked ? (totalRequiredSlots - totalRoomSlots) : 0,
            AffectedItems = totalRoomSlotsBlocked
                ? new List<string> { $"Thiếu {totalRequiredSlots - totalRoomSlots} slot phòng/tuần." }
                : new List<string>()
        });

        // 11. EXISTING_SCHEDULE_LOCK_READY
        var publishedSchedules = await _db.ThoiKhoaBieus
            .Include(x => x.JobNguon)
            .Where(x =>
                x.KhoaHoc != null
                && x.KhoaHoc.MaHocKy == schedulableTermId
                && x.KhoaHoc.MaDonVi == campusId
                && x.TrangThai == "da_xuat_ban")
            .ToListAsync(cancellationToken);
        var hasPublishedSchedule = publishedSchedules.Count > 0;
        var isLockedPermanently = permanentlyLockedTermIds.Contains(schedulableTermId);
        readinessItems.Add(new SchedulingReadinessItemDto
        {
            Code = "EXISTING_SCHEDULE_LOCK_READY",
            Status = isLockedPermanently ? "blocked" : "ready",
            Message = isLockedPermanently
                ? "Thời khóa biểu đã được công bố chính thức và đã bị khóa vĩnh viễn (quá 30 phút hoặc đã điểm danh)."
                : "Học kỳ cho phép chuẩn bị hoặc xuất bản lịch mới.",
            ActionRoute = "/staff/schedule/published",
            AffectedCount = isLockedPermanently ? 1 : 0,
            AffectedItems = isLockedPermanently
                ? publishedSchedules.Select(x => $"Khóa học #{x.MaKhoaHoc}").Distinct().Take(20).ToList()
                : new List<string>()
        });

        var hasDraftSchedule = await _db.ScheduleGenerationJobs.AnyAsync(
            x => x.MaHocKy == schedulableTermId && x.MaDonVi == campusId,
            cancellationToken
        );

        result.Readiness = new SchedulingReadinessDto
        {
            HasCourses = hasCourses,
            HasClasses = await _db.LopHanhChinhs.AnyAsync(x => x.MaDonVi == campusId && x.ConHoatDong, cancellationToken),
            HasSubjects = subjectIds.Count > 0,
            HasTeachers = courseTeacherIds.Count > 0,
            HasRooms = hasRooms,
            HasShifts = hasShifts,
            HasPublishedSchedule = hasPublishedSchedule,
            HasDraftSchedule = hasDraftSchedule,
            Items = readinessItems,
            BlockingIssues = new List<SchedulingBlockingIssueDto>()
        };

        var blockedItems = readinessItems.Where(x => x.Status == "blocked").ToList();
        if (blockedItems.Count > 0)
        {
            result.CanPrepareSchedule = false;
            foreach (var b in blockedItems)
            {
                result.Readiness.BlockingIssues.Add(new SchedulingBlockingIssueDto
                {
                    Code = b.Code,
                    Message = b.Message,
                    ActionRoute = b.ActionRoute
                });
            }

            if (isLockedPermanently)
            {
                result.ReasonCode = "SCHEDULE_ALREADY_PUBLISHED";
                result.ReasonMessage = "Thời khóa biểu cho học kỳ này đã được công bố chính thức và đã bị khóa (quá 30 phút hoặc đã điểm danh).";
            }
            else if (!hasCourses)
            {
                result.ReasonCode = "NO_COURSES";
                result.ReasonMessage = "Học kỳ chưa có lớp học phần hoặc khóa học để xếp lịch.";
            }
            else if (!hasRooms)
            {
                result.ReasonCode = "NO_ACTIVE_ROOMS";
                result.ReasonMessage = "Không có phòng học nào đang hoạt động tại cơ sở này.";
            }
        }

        return result;
    }

    private async Task<HashSet<int>> GetPermanentlyLockedTermIdsAsync(
        int campusId,
        IReadOnlyCollection<int> futureTermIds,
        CancellationToken cancellationToken)
    {
        if (futureTermIds.Count == 0)
        {
            return new HashSet<int>();
        }

        var publishedSchedules = await _db.ThoiKhoaBieus
            .AsNoTracking()
            .Where(x => x.KhoaHoc != null
                && x.KhoaHoc.MaDonVi == campusId
                && x.KhoaHoc.MaHocKy.HasValue
                && futureTermIds.Contains(x.KhoaHoc.MaHocKy.Value)
                && x.TrangThai == "da_xuat_ban")
            .Select(x => new
            {
                TermId = x.KhoaHoc!.MaHocKy!.Value,
                x.MaKhoaHoc,
                PublishedAt = x.JobNguon != null && x.JobNguon.NgayXuatBan.HasValue
                    ? x.JobNguon.NgayXuatBan.Value
                    : x.NgayCapNhat ?? x.NgayTao,
            })
            .ToListAsync(cancellationToken);

        if (publishedSchedules.Count == 0)
        {
            return new HashSet<int>();
        }

        var publishedCourseIds = publishedSchedules.Select(x => x.MaKhoaHoc).Distinct().ToList();
        var attendedCourseIds = await (
            from dd in _db.DiemDanhs.AsNoTracking()
            join bh in _db.BuoiHocs.AsNoTracking() on dd.MaBuoiHoc equals bh.MaBuoiHoc
            where publishedCourseIds.Contains(bh.MaKhoaHoc)
            select bh.MaKhoaHoc
        ).Distinct().ToListAsync(cancellationToken);
        var attendedSet = attendedCourseIds.ToHashSet();
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        return publishedSchedules
            .GroupBy(x => x.TermId)
            .Where(group => group.Any(x => attendedSet.Contains(x.MaKhoaHoc))
                || now - group.Min(x => x.PublishedAt) > TimeSpan.FromMinutes(30))
            .Select(group => group.Key)
            .ToHashSet();
    }

    public async Task ValidateSchedulableTermAsync(
        int campusId,
        int requestedTermId,
        CancellationToken cancellationToken = default
    )
    {
        var context = await GetContextAsync(campusId, cancellationToken);

        if (!context.CanPrepareSchedule && context.SchedulableTerm?.MaHocKy == requestedTermId)
        {
            throw new ApiException(StatusCodes.Status409Conflict, context.ReasonMessage);
        }

        if (context.SchedulableTerm == null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Không thể chuẩn bị lịch do không có học kỳ hợp lệ."
            );
        }

        if (context.SchedulableTerm.MaHocKy != requestedTermId)
        {
            // Specifically checking if it's a cross-campus request or just wrong term
            var termExists = await _db.HocKys.AnyAsync(
                x => x.MaHocKy == requestedTermId,
                cancellationToken
            );
            if (!termExists)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Học kỳ không tồn tại.");
            }

            var termInCampus = await _db.HocKys.AnyAsync(
                x => x.MaHocKy == requestedTermId && x.MaDonVi == campusId,
                cancellationToken
            );
            if (!termInCampus)
            {
                throw new ApiException(
                    StatusCodes.Status403Forbidden,
                    "Học kỳ thuộc về cơ sở khác."
                );
            }

            var termName = !string.IsNullOrWhiteSpace(context.SchedulableTerm.MaCodeHocKy)
                ? context.SchedulableTerm.MaCodeHocKy
                : context.SchedulableTerm.TenHocKy;
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất: {termName}."
            );
        }
    }
}

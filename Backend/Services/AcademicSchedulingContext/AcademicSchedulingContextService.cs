using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AcademicSchedulingContext;

public class AcademicSchedulingContextService : IAcademicSchedulingContextService
{
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _timeProvider;

    public AcademicSchedulingContextService(ApplicationDbContext db, TimeProvider? timeProvider = null)
    {
        _db = db;
        _timeProvider = timeProvider ?? TimeProvider.System;
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

        // Calculate Readiness
        var schedulableTermId = result.SchedulableTerm.MaHocKy;

        var hasCourses = await _db.KhoaHocs.AnyAsync(
            x => x.MaHocKy == schedulableTermId && x.TrangThai != "luu_tru",
            cancellationToken
        );
        var hasClasses = await _db.LopHanhChinhs.AnyAsync(
            x => x.MaDonVi == campusId && x.ConHoatDong,
            cancellationToken
        );
        var hasSubjects = await _db.DanhMucMonHocs.AnyAsync(
            x => x.ConHoatDong,
            cancellationToken
        );
        var hasTeachers = await _db.NguoiDungs.AnyAsync(
            x =>
                x.MaDonVi == campusId
                && (x.VaiTroChinh == "giao_vien"
                    || x.VaiTroChinh == "Teacher"
                    || x.VaiTroChinh == "Lecturer")
                && x.TrangThai == "hoat_dong",
            cancellationToken
        );
        var hasRooms = await _db.PhongHocs.AnyAsync(
            x => x.MaDonVi == campusId && x.TrangThaiPhong == "hoat_dong",
            cancellationToken
        );
        var hasShifts = await _db.CaHocs.AnyAsync(
            x => x.ConHoatDong,
            cancellationToken
        );

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

        if (hasPublishedSchedule)
        {
            // The selected term's permanent-lock state was calculated for all
            // candidates above, before selecting the schedulable term.
        }

        var hasDraftSchedule = await _db.ScheduleGenerationJobs.AnyAsync(
            x => x.MaHocKy == schedulableTermId && x.MaDonVi == campusId,
            cancellationToken
        );

        result.Readiness = new SchedulingReadinessDto
        {
            HasCourses = hasCourses,
            HasClasses = hasClasses,
            HasSubjects = hasSubjects,
            HasTeachers = hasTeachers,
            HasRooms = hasRooms,
            HasShifts = hasShifts,
            HasPublishedSchedule = hasPublishedSchedule,
            HasDraftSchedule = hasDraftSchedule,
        };

        if (!hasCourses)
        {
            result.CanPrepareSchedule = false;
            result.ReasonCode = "NO_COURSES";
            result.ReasonMessage = "Học kỳ chưa có lớp học phần hoặc khóa học để xếp lịch.";
            result.Readiness.BlockingIssues.Add(
                new SchedulingBlockingIssueDto
                {
                    Code = "NO_COURSES",
                    Message = "Học kỳ chưa có lớp học phần hoặc khóa học để xếp lịch.",
                    ActionRoute = "/academic/courses",
                }
            );
        }

        if (!hasRooms)
        {
            result.CanPrepareSchedule = false;
            result.ReasonCode = "NO_ACTIVE_ROOMS";
            result.ReasonMessage = "Không có phòng học nào đang hoạt động tại cơ sở này.";
            result.Readiness.BlockingIssues.Add(
                new SchedulingBlockingIssueDto
                {
                    Code = "NO_ACTIVE_ROOMS",
                    Message = "Không có phòng học đang hoạt động.",
                    ActionRoute = "/facilities/rooms",
                }
            );
        }

        if (isLockedPermanently)
        {
            // Business rule: Do not allow generating new draft if schedule is locked permanently (>30m or attended).
            result.CanPrepareSchedule = false;
            result.ReasonCode = "SCHEDULE_ALREADY_PUBLISHED";
            result.ReasonMessage = "Thời khóa biểu cho học kỳ này đã được công bố chính thức và đã bị khóa (quá 30 phút hoặc đã điểm danh).";
            result.Readiness.BlockingIssues.Add(
                new SchedulingBlockingIssueDto
                {
                    Code = "SCHEDULE_ALREADY_PUBLISHED",
                    Message = "Không thể chuẩn bị lịch mới vì lịch công bố đã bị khóa.",
                    ActionRoute = "/staff/schedule/published",
                }
            );
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
        var attendedCourseIds = await _db.DiemDanhs
            .AsNoTracking()
            .Where(x => x.BuoiHoc != null && publishedCourseIds.Contains(x.BuoiHoc.MaKhoaHoc))
            .Select(x => x.BuoiHoc!.MaKhoaHoc)
            .Distinct()
            .ToListAsync(cancellationToken);
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

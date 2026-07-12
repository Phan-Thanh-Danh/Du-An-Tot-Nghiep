using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AcademicSchedulingContext;

public class AcademicSchedulingContextService : IAcademicSchedulingContextService
{
    private readonly ApplicationDbContext _db;

    public AcademicSchedulingContextService(ApplicationDbContext db)
    {
        _db = db;
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
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
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
            .Where(x => x.NgayBatDau <= today && x.NgayKetThuc >= today)
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
        result.SchedulableTerm = result.NextTerm;

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
                && (x.VaiTroChinh == "Teacher" || x.VaiTroChinh == "Lecturer")
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

        var hasPublishedSchedule = await _db.ThoiKhoaBieus.AnyAsync(
            x =>
                x.KhoaHoc != null
                && x.KhoaHoc.MaHocKy == schedulableTermId
                && x.KhoaHoc.MaDonVi == campusId
                && x.TrangThai == "da_xuat_ban",
            cancellationToken
        );
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
            result.Readiness.BlockingIssues.Add(
                new SchedulingBlockingIssueDto
                {
                    Code = "NO_ACTIVE_ROOMS",
                    Message = "Không có phòng học đang hoạt động.",
                    ActionRoute = "/facilities/rooms",
                }
            );
        }

        if (hasPublishedSchedule)
        {
            // Business rule: Do not allow generating new draft if there is already a published schedule.
            result.CanPrepareSchedule = false;
            result.ReasonCode = "SCHEDULE_ALREADY_PUBLISHED";
            result.ReasonMessage = "Thời khóa biểu cho học kỳ này đã được công bố chính thức.";
            result.Readiness.BlockingIssues.Add(
                new SchedulingBlockingIssueDto
                {
                    Code = "SCHEDULE_ALREADY_PUBLISHED",
                    Message = "Không thể chuẩn bị lịch mới vì đã có lịch công bố.",
                    ActionRoute = "/staff/schedule/published",
                }
            );
        }

        return result;
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

            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất: {context.SchedulableTerm.MaCodeHocKy}."
            );
        }
    }
}

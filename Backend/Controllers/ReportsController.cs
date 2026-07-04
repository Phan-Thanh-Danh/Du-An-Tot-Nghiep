using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = AuthRoles.AcademicStaff)]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponseDto<CourseReportDto>>> GetCourseReport()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var donViId = currentUser!.CampusId;

        var courses = await _db.KhoaHocs
            .Include(k => k.HocKy)
            .Where(k => k.MaDonVi == donViId)
            .ToListAsync();

        var statusDistribution = courses.GroupBy(k => k.TrangThai ?? "khong_ro")
            .Select(g => new ChartDataPoint { Label = MapCourseStatus(g.Key), Value = g.Count() })
            .ToList();

        var semesterDistribution = courses.Where(k => k.HocKy != null)
            .GroupBy(k => k.HocKy!.TenHocKy)
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
            .ToList();

        var data = new CourseReportDto
        {
            TotalCourses = courses.Count,
            StatusDistribution = statusDistribution,
            SemesterDistribution = semesterDistribution
        };

        return Ok(ApiResponseDto<CourseReportDto>.Ok(data));
    }

    [HttpGet("teacher-load")]
    public async Task<ActionResult<ApiResponseDto<TeacherLoadReportDto>>> GetTeacherLoadReport()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var donViId = currentUser!.CampusId;

        var teachers = await _db.NguoiDungs
            .Where(u => u.VaiTroChinh == "giao_vien" && u.MaDonVi == donViId)
            .ToListAsync();

        var courses = await _db.KhoaHocs
            .Where(k => k.MaDonVi == donViId && k.MaGiaoVien != null)
            .ToListAsync();

        var sessions = await _db.BuoiHocs
            .Include(b => b.KhoaHoc)
            .Where(b => b.KhoaHoc != null && b.KhoaHoc.MaDonVi == donViId && b.TrangThaiBuoi != "da_huy")
            .ToListAsync();

        var loadData = teachers.Select(t =>
        {
            var teacherCourses = courses.Where(c => c.MaGiaoVien == t.MaNguoiDung).ToList();
            var teacherCourseIds = teacherCourses.Select(c => c.MaKhoaHoc).ToList();
            var teacherSessions = sessions.Where(s => teacherCourseIds.Contains(s.MaKhoaHoc)).ToList();
            
            // Assume each session is 2 hours for now
            var totalHours = teacherSessions.Count * 2;

            return new TeacherLoadItemDto
            {
                TeacherId = t.MaNguoiDung,
                TeacherName = t.HoTen,
                TotalCourses = teacherCourses.Count,
                TotalSessions = teacherSessions.Count,
                TotalHours = totalHours
            };
        }).OrderByDescending(x => x.TotalHours).ToList();

        var data = new TeacherLoadReportDto
        {
            TotalTeachers = teachers.Count,
            TeacherLoads = loadData
        };

        return Ok(ApiResponseDto<TeacherLoadReportDto>.Ok(data));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<ApiResponseDto<AttendanceReportDto>>> GetAttendanceReport()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var donViId = currentUser!.CampusId;

        // Simplify for performance on large DB: Sample courses instead of full table scan
        var recentCourses = await _db.KhoaHocs
            .Include(k => k.MonHoc)
            .Where(k => k.MaDonVi == donViId && k.TrangThai == "mo")
            .Take(50)
            .ToListAsync();
            
        var courseIds = recentCourses.Select(c => c.MaKhoaHoc).ToList();
        
        var attendanceRecords = await _db.DiemDanhs
            .Include(d => d.BuoiHoc)
            .Where(d => d.BuoiHoc != null && courseIds.Contains(d.BuoiHoc.MaKhoaHoc))
            .ToListAsync();

        var classAttendance = recentCourses.Select(c =>
        {
            var classRecords = attendanceRecords.Where(a => a.BuoiHoc?.MaKhoaHoc == c.MaKhoaHoc).ToList();
            var present = classRecords.Count(a => a.TrangThai == "co_mat" || a.TrangThai == "di_tre");
            var rate = classRecords.Count > 0 ? (double)present / classRecords.Count * 100 : 0;

            return new ClassAttendanceItemDto
            {
                CourseName = c.MonHoc?.TenMonHoc ?? "Không rõ",
                AttendanceRate = Math.Round(rate, 2),
                WarningCount = classRecords.Count(a => a.TrangThai == "vang_mat")
            };
        }).OrderBy(x => x.AttendanceRate).ToList();

        var data = new AttendanceReportDto
        {
            AverageAttendanceRate = classAttendance.Any() ? Math.Round(classAttendance.Average(x => x.AttendanceRate), 2) : 0,
            ClassAttendance = classAttendance
        };

        return Ok(ApiResponseDto<AttendanceReportDto>.Ok(data));
    }

    private static string MapCourseStatus(string status)
    {
        return status switch
        {
            "mo" => "Đang diễn ra",
            "dong" => "Đã đóng",
            "hoan_thanh" => "Đã hoàn thành",
            "huy" => "Bị hủy",
            _ => "Khác"
        };
    }
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class CourseReportDto
{
    public int TotalCourses { get; set; }
    public List<ChartDataPoint> StatusDistribution { get; set; } = [];
    public List<ChartDataPoint> SemesterDistribution { get; set; } = [];
}

public class TeacherLoadItemDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int TotalCourses { get; set; }
    public int TotalSessions { get; set; }
    public int TotalHours { get; set; }
}

public class TeacherLoadReportDto
{
    public int TotalTeachers { get; set; }
    public List<TeacherLoadItemDto> TeacherLoads { get; set; } = [];
}

public class ClassAttendanceItemDto
{
    public string CourseName { get; set; } = string.Empty;
    public double AttendanceRate { get; set; }
    public int WarningCount { get; set; }
}

public class AttendanceReportDto
{
    public double AverageAttendanceRate { get; set; }
    public List<ClassAttendanceItemDto> ClassAttendance { get; set; } = [];
}

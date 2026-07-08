using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
public class BghAcademicController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghAcademicController(ApplicationDbContext db)
    {
        _db = db;
    }

    private (int CampusId, bool IsGlobal) GetUserScope()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var campusId = user?.CampusId ?? 0;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin || user?.Role == AuthRoles.Admin;
        return (campusId, isGlobal);
    }

    [HttpGet("academic/overview")]
    public async Task<ActionResult<ApiResponseDto<AcademicOverviewDto>>> GetAcademicOverview()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId));
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => isGlobal || l.MaDonVi == campusId);
        var activeCourses = await _db.KhoaHocs.CountAsync(k => k.TrangThai == "dang_mo" && (isGlobal || k.MaDonVi == campusId));

        var avgGpa = await _db.DiemSos.Where(d => isGlobal || d.MaDonVi == campusId).AverageAsync(d => (decimal?)d.GpaMonHoc) ?? 0;
        var passCount = await _db.DiemSos.CountAsync(d => d.GpaMonHoc >= 4 && (isGlobal || d.MaDonVi == campusId));
        var totalGrades = await _db.DiemSos.CountAsync(d => isGlobal || d.MaDonVi == campusId);
        var passRate = totalGrades > 0 ? (double)passCount / totalGrades * 100 : 0;

        var atRiskCount = await _db.DiemSos
            .Where(d => d.GpaMonHoc < 4 && (isGlobal || d.MaDonVi == campusId))
            .Select(d => d.MaHocSinh)
            .Distinct()
            .CountAsync();

        var distribution = await _db.DiemSos
            .Where(d => isGlobal || d.MaDonVi == campusId)
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A" :
                          d.GpaMonHoc >= 7 ? "B" :
                          d.GpaMonHoc >= 5.5m ? "C" :
                          d.GpaMonHoc >= 4 ? "D" : "F")
            .Select(g => new GradeDistributionDto
            {
                Grade = g.Key,
                Count = g.Count(),
                Percent = totalGrades > 0 ? Math.Round((double)g.Count() / totalGrades * 100, 1) : 0
            })
            .OrderByDescending(g => g.Grade == "A" ? 5 : g.Grade == "B" ? 4 : g.Grade == "C" ? 3 : g.Grade == "D" ? 2 : 1)
            .ToListAsync();

        var topSubjects = await _db.DiemSos
            .Where(d => d.MonHoc != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaMonHoc, TenMon = d.MonHoc!.TenMonHoc })
            .Select(g => new SubjectPassFailDto
            {
                SubjectName = g.Key.TenMon,
                Total = g.Count(),
                Pass = g.Count(d => d.GpaMonHoc >= 4),
                FailRate = Math.Round((double)g.Count(d => d.GpaMonHoc < 4) / g.Count() * 100, 1)
            })
            .OrderByDescending(s => s.FailRate)
            .Take(10)
            .ToListAsync();

        var totalMonHoc = await _db.DanhMucMonHocs.CountAsync();

        var semesterTrend = await _db.DiemSos
            .Where(d => d.HocKy != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaHocKy, TenHocKy = d.HocKy!.TenHocKy ?? "" })
            .Select(g => new GpaTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.Semester)
            .Take(5)
            .ToListAsync();

        var data = new AcademicOverviewDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            ActiveCourses = activeCourses,
            AvgGpa = Math.Round(avgGpa, 2),
            PassRate = Math.Round(passRate, 1),
            AtRiskCount = atRiskCount,
            TotalSubjects = totalMonHoc,
            GradeDistribution = distribution,
            TopSubjects = topSubjects,
            SemesterTrend = semesterTrend
        };

        return Ok(ApiResponseDto<AcademicOverviewDto>.Ok(data));
    }

    [HttpGet("academic/gpa")]
    public async Task<ActionResult<ApiResponseDto<GpaReportDto>>> GetGpaReports()
    {
        var (campusId, isGlobal) = GetUserScope();

        var semesterGroups = await _db.DiemSos
            .Where(d => d.HocKy != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaHocKy, TenHocKy = d.HocKy!.TenHocKy ?? "" })
            .Select(g => new GpaTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.Semester)
            .ToListAsync();

        var distribution = await _db.DiemSos
            .Where(d => isGlobal || d.MaDonVi == campusId)
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A (8.5-10)" :
                          d.GpaMonHoc >= 7 ? "B (7.0-8.4)" :
                          d.GpaMonHoc >= 5.5m ? "C (5.5-6.9)" :
                          d.GpaMonHoc >= 4 ? "D (4.0-5.4)" : "F (< 4.0)")
            .Select(g => new GradeDistributionDto
            {
                Grade = g.Key,
                Count = g.Count(),
                Percent = 0
            })
            .ToListAsync();

        var total = distribution.Sum(d => d.Count);
        foreach (var d in distribution)
            d.Percent = total > 0 ? Math.Round((double)d.Count / total * 100, 1) : 0;

        var data = new GpaReportDto
        {
            Trends = semesterGroups,
            Distribution = distribution.OrderByDescending(d => d.Grade).ToList()
        };

        return Ok(ApiResponseDto<GpaReportDto>.Ok(data));
    }

    [HttpGet("academic/at-risk")]
    public async Task<ActionResult<ApiResponseDto<AtRiskReportDto>>> GetAtRiskStudents()
    {
        var (campusId, isGlobal) = GetUserScope();

        var atRiskStudentIds = await _db.DiemSos
            .Where(d => d.GpaMonHoc < 4 && (isGlobal || d.MaDonVi == campusId))
            .Select(d => d.MaHocSinh)
            .Distinct()
            .ToListAsync();

        var students = await _db.NguoiDungs
            .Where(u => atRiskStudentIds.Contains(u.MaNguoiDung))
            .Select(u => new AtRiskStudentDto
            {
                Id = u.MaNguoiDung,
                Name = u.HoTen,
                Email = u.Email,
                ClassCode = u.Lop != null ? u.Lop.MaCodeLop : "",
                AvgGpa = _db.DiemSos.Where(d => d.MaHocSinh == u.MaNguoiDung).Average(d => (decimal?)d.GpaMonHoc) ?? 0,
                FailCount = _db.DiemSos.Count(d => d.MaHocSinh == u.MaNguoiDung && d.GpaMonHoc < 4)
            })
            .OrderBy(u => u.AvgGpa)
            .ToListAsync();

        var data = new AtRiskReportDto
        {
            TotalAtRisk = students.Count,
            Students = students,
            Summary = new AtRiskSummaryDto
            {
                TotalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId)),
                AvgGpaAtRisk = students.Count > 0 ? Math.Round((decimal)students.Average(s => (double)s.AvgGpa), 2) : 0,
                CriticalCount = students.Count(s => s.FailCount >= 3)
            }
        };

        return Ok(ApiResponseDto<AtRiskReportDto>.Ok(data));
    }

    [HttpGet("academic/reports")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAcademicReports()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId));
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => isGlobal || l.MaDonVi == campusId);
        var activeCourses = await _db.KhoaHocs.CountAsync(k => k.TrangThai == "dang_mo" && (isGlobal || k.MaDonVi == campusId));
        var avgGpa = await _db.DiemSos.Where(d => isGlobal || d.MaDonVi == campusId).AverageAsync(d => (decimal?)d.GpaMonHoc) ?? 0;

        var data = new
        {
            Summary = new
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                ActiveCourses = activeCourses,
                AvgGpa = Math.Round(avgGpa, 2)
            },
            MonthlyStats = new object[] { },
            DepartmentStats = new object[] { }
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("academic/pass-fail")]
    public async Task<ActionResult<ApiResponseDto<PassFailReportDto>>> GetPassFailRates()
    {
        var (campusId, isGlobal) = GetUserScope();

        var courseStats = await _db.DiemSos
            .Where(d => d.MonHoc != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaMonHoc, TenMon = d.MonHoc!.TenMonHoc })
            .Select(g => new CoursePassFailDto
            {
                SubjectName = g.Key.TenMon,
                Total = g.Count(),
                Pass = g.Count(d => d.GpaMonHoc >= 4),
                Fail = g.Count(d => d.GpaMonHoc < 4),
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2)
            })
            .OrderByDescending(s => s.Fail)
            .Take(20)
            .ToListAsync();

        foreach (var c in courseStats)
            c.FailRate = c.Total > 0 ? Math.Round((double)c.Fail / c.Total * 100, 1) : 0;

        var data = new PassFailReportDto
        {
            CourseStats = courseStats,
            OverallPassRate = courseStats.Sum(c => c.Pass) > 0
                ? Math.Round((double)courseStats.Sum(c => c.Pass) / courseStats.Sum(c => c.Total) * 100, 1)
                : 0
        };

        return Ok(ApiResponseDto<PassFailReportDto>.Ok(data));
    }

    [HttpGet("schedule/changes")]
    public async Task<ActionResult<ApiResponseDto<List<ScheduleChangeDto>>>> GetScheduleChanges()
    {
        var (campusId, isGlobal) = GetUserScope();

        var changes = await _db.BuoiHocs
            .Where(b => (b.LoaiThayDoi != null || b.TrangThaiBuoi == "da_huy") && (isGlobal || (b.KhoaHoc != null && b.KhoaHoc.MaDonVi == campusId)))
            .OrderByDescending(b => b.NgayCapNhat)
            .Take(50)
            .Select(b => new ScheduleChangeDto
            {
                Id = b.MaBuoiHoc,
                ChangeType = b.LoaiThayDoi ?? (b.TrangThaiBuoi == "da_huy" ? "da_huy" : ""),
                Reason = b.LyDoThayDoi ?? "",
                Date = b.NgayHoc,
                SubjectName = b.KhoaHoc != null && b.KhoaHoc.MonHoc != null ? b.KhoaHoc.MonHoc.TenMonHoc : "",
                ClassCode = b.KhoaHoc != null && b.KhoaHoc.Lop != null ? b.KhoaHoc.Lop.MaCodeLop : "",
                TeacherName = b.KhoaHoc != null && b.KhoaHoc.GiaoVien != null ? b.KhoaHoc.GiaoVien.HoTen : "",
                SubstituteTeacherName = b.MaGiaoVienDayThay != null
                    ? _db.NguoiDungs.Where(n => n.MaNguoiDung == b.MaGiaoVienDayThay).Select(n => n.HoTen).FirstOrDefault() ?? ""
                    : "",
                UpdatedAt = b.NgayCapNhat ?? b.NgayTao
            })
            .ToListAsync();

        return Ok(ApiResponseDto<List<ScheduleChangeDto>>.Ok(changes));
    }
}

// DTOs
public class AcademicOverviewDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int ActiveCourses { get; set; }
    public decimal AvgGpa { get; set; }
    public double PassRate { get; set; }
    public int AtRiskCount { get; set; }
    public int TotalSubjects { get; set; }
    public List<GradeDistributionDto> GradeDistribution { get; set; } = [];
    public List<SubjectPassFailDto> TopSubjects { get; set; } = [];
    public List<GpaTrendDto> SemesterTrend { get; set; } = [];
}

public class GradeDistributionDto
{
    public string Grade { get; set; } = "";
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class SubjectPassFailDto
{
    public string SubjectName { get; set; } = "";
    public int Total { get; set; }
    public int Pass { get; set; }
    public double FailRate { get; set; }
}

public class GpaReportDto
{
    public List<GpaTrendDto> Trends { get; set; } = [];
    public List<GradeDistributionDto> Distribution { get; set; } = [];
}

public class GpaTrendDto
{
    public string Semester { get; set; } = "";
    public decimal AvgGpa { get; set; }
    public int StudentCount { get; set; }
}

public class AtRiskReportDto
{
    public int TotalAtRisk { get; set; }
    public AtRiskSummaryDto Summary { get; set; } = new();
    public List<AtRiskStudentDto> Students { get; set; } = [];
}

public class AtRiskSummaryDto
{
    public int TotalStudents { get; set; }
    public decimal AvgGpaAtRisk { get; set; }
    public int CriticalCount { get; set; }
}

public class AtRiskStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public decimal AvgGpa { get; set; }
    public int FailCount { get; set; }
}

public class PassFailReportDto
{
    public List<CoursePassFailDto> CourseStats { get; set; } = [];
    public double OverallPassRate { get; set; }
}

public class CoursePassFailDto
{
    public string SubjectName { get; set; } = "";
    public int Total { get; set; }
    public int Pass { get; set; }
    public int Fail { get; set; }
    public double FailRate { get; set; }
    public decimal AvgGpa { get; set; }
}

public class ScheduleChangeDto
{
    public int Id { get; set; }
    public string ChangeType { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateOnly Date { get; set; }
    public string SubjectName { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string SubstituteTeacherName { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}

using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponseDto<TeacherDashboardDto>>> GetDashboard()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = currentUser!.UserId;

        var teacherCourseMonHocIds = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var totalClasses = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .CountAsync();

        var classIds = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .Select(k => k.MaLop)
            .Distinct()
            .ToListAsync();

        var studentIds = await _context.NguoiDungs
            .Where(n => n.MaLop != null && classIds.Contains(n.MaLop.Value))
            .Select(n => n.MaNguoiDung)
            .Distinct()
            .ToListAsync();

        var totalStudents = studentIds.Count;

        var pendingGrading = await _context.BaiNops
            .Where(b => b.DiemSo == null && b.BaiTap != null && teacherCourseMonHocIds.Contains(b.BaiTap.MaMonHoc) && studentIds.Contains(b.MaHocSinh))
            .CountAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaySessions = await _context.BuoiHocs
            .Where(b => b.MaGiaoVien == userId && b.NgayHoc == today)
            .CountAsync();

        var recentSubmissions = await _context.BaiNops
            .Where(b => b.BaiTap != null && teacherCourseMonHocIds.Contains(b.BaiTap.MaMonHoc) && studentIds.Contains(b.MaHocSinh))
            .OrderByDescending(b => b.ThoiDiemNop)
            .Take(5)
            .Select(b => new RecentSubmissionDto
            {
                SubmissionId = b.MaBaiNop,
                StudentName = b.HocSinh != null ? b.HocSinh.HoTen : "",
                AssignmentTitle = b.BaiTap != null ? b.BaiTap.TieuDe : "",
                SubmittedAt = b.ThoiDiemNop,
                Score = b.DiemSo,
                Status = b.DiemSo != null ? "da_cham" : "cho_cham"
            })
            .ToListAsync();

        var data = new TeacherDashboardDto
        {
            TotalClasses = totalClasses,
            TotalStudents = totalStudents,
            PendingGrading = pendingGrading,
            TodaySessions = todaySessions,
            RecentSubmissions = recentSubmissions
        };

        return Ok(ApiResponseDto<TeacherDashboardDto>.Ok(data));
    }
}

public class TeacherDashboardDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int PendingGrading { get; set; }
    public int TodaySessions { get; set; }
    public List<RecentSubmissionDto> RecentSubmissions { get; set; } = [];
}

public class RecentSubmissionDto
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty;
}

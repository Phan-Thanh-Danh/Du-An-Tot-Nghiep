using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/grades")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentGradesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StudentGradesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<StudentGradesResponseDto>>> GetGrades(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var scores = await _db.DiemSos
            .Include(d => d.MonHoc)
            .Include(d => d.HocKy)
            .Where(d => d.MaHocSinh == userId)
            .ToListAsync(ct);

        var subjects = scores.Select(d => new SubjectGradeDto
        {
            Code = d.MonHoc?.MaCodeMonHoc ?? "",
            Name = d.MonHoc?.TenMonHoc ?? "",
            Credits = d.MonHoc?.SoTinChi ?? 0,
            Semester = d.HocKy != null ? $"{d.HocKy.TenHocKy} {d.HocKy.NamHoc}" : "",
            Gpa = (double)d.GpaMonHoc,
            Status = d.TrangThai == "dat" ? "pass" : d.TrangThai == "khong_dat" ? "fail" : "studying",
            StatusLabel = d.TrangThai == "dat" ? "Đạt" : d.TrangThai == "khong_dat" ? "Chưa đạt" : "Đang học",
            LetterGrade = d.TrangThai == "dat" ? "Đạt" : d.TrangThai == "khong_dat" ? "Rớt" : "",
            ProcessScore = (double?)d.DiemQuaTrinh,
            MidtermScore = (double?)d.DiemGiuaKy,
            FinalScore = (double?)d.DiemCuoiKy,
            Note = d.LyDoRot
        }).ToList();

        var gpaValues = scores.Where(d => d.GpaMonHoc > 0).Select(d => (double)d.GpaMonHoc).ToList();
        var cumulative = gpaValues.Any() ? Math.Round(gpaValues.Average(), 2) : 0;
        var passed = subjects.Count(s => s.Status == "pass");
        var failed = subjects.Count(s => s.Status == "fail");
        var earnedCredits = subjects.Where(s => s.Status == "pass").Sum(s => s.Credits);
        var totalRequired = 120;

        var summary = new GradeSummaryDto
        {
            CumulativeGpa = cumulative,
            TotalCreditsEarned = earnedCredits,
            TotalCreditsRequired = totalRequired,
            Classification = cumulative >= 3.6 ? "Xuất sắc" : cumulative >= 3.2 ? "Giỏi" : cumulative >= 2.5 ? "Khá" : cumulative >= 2.0 ? "Trung bình" : "Yếu",
            TotalSubjectsPassed = passed,
            TotalSubjectsFailed = failed,
            RiskAlertCount = subjects.Count(s => s.Gpa < 2.0)
        };

        return Ok(ApiResponseDto<StudentGradesResponseDto>.Ok(new StudentGradesResponseDto
        {
            Summary = summary,
            Subjects = subjects
        }));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

public class StudentGradesResponseDto
{
    public GradeSummaryDto Summary { get; set; } = new();
    public List<SubjectGradeDto> Subjects { get; set; } = [];
}

public class GradeSummaryDto
{
    public double CumulativeGpa { get; set; }
    public int TotalCreditsEarned { get; set; }
    public int TotalCreditsRequired { get; set; }
    public string Classification { get; set; } = string.Empty;
    public int TotalSubjectsPassed { get; set; }
    public int TotalSubjectsFailed { get; set; }
    public int RiskAlertCount { get; set; }
}

public class SubjectGradeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Semester { get; set; } = string.Empty;
    public double Gpa { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string LetterGrade { get; set; } = string.Empty;
    public double? ProcessScore { get; set; }
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public string? Note { get; set; }
}

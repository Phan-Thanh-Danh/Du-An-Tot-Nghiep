using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Services.Grading;
using Backend.DTOs.Grading;
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
    private readonly IGradeAggregationService _gradeService;

    public StudentGradesController(ApplicationDbContext db, IGradeAggregationService gradeService)
    {
        _db = db;
        _gradeService = gradeService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<StudentGradesResponseDto>>> GetGrades(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var userId = GetCurrentUserId();
        var scores = await _db.DiemSos
            .Include(d => d.MonHoc)
            .Include(d => d.HocKy)
            .Where(d => d.MaHocSinh == userId && (d.HocKy == null || d.HocKy.NgayBatDau <= today.AddMonths(4)))
            .ToListAsync(ct);

        var subjects = scores.Select(d => new SubjectGradeDto
        {
            Code = d.MonHoc?.MaCodeMonHoc ?? "",
            Name = d.MonHoc?.TenMonHoc ?? "",
            Credits = d.MonHoc?.SoTinChi ?? 0,
            CourseId = d.MaMonHoc,
            SemesterId = d.MaHocKy,
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

    [HttpGet("{monHocId}/{hocKyId}/detail")]
    public async Task<ActionResult<ApiResponseDto<StudentGradeDetailDto>>> GetGradeDetail(int monHocId, int hocKyId)
    {
        try
        {
            var studentId = GetCurrentUserId();

            var diemRecord = await _db.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId);

            var configs = await _db.CauHinhDauDiemQuaTrinhs
                .Include(x => x.LoaiDauDiem)
                .Where(x => x.MaMonHoc == monHocId && x.MaHocKy == hocKyId)
                .OrderBy(x => x.LoaiDauDiem != null ? x.LoaiDauDiem.ThuTuHienThi : 0)
                .ToListAsync();

            var gradeTypes = new List<GradeTypeDetailDto>();

            foreach (var config in configs)
            {
                var loaiCode = config.LoaiDauDiem?.MaCode ?? "";
                var detail = new GradeTypeDetailDto
                {
                    Code = loaiCode,
                    Name = config.LoaiDauDiem?.TenLoai ?? "",
                    Weight = config.TrongSoNoiBo
                };

                if (loaiCode == "chuyen_can")
                {
                    detail.AverageGrade = await _gradeService.CalculateAttendanceGradeAsync(studentId, monHocId, hocKyId);
                    if (detail.AverageGrade.HasValue)
                        detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);
                }
                else if (loaiCode == "lab" || loaiCode == "assignment" || loaiCode == "bao_ve_mon")
                {
                    detail.AverageGrade = await _gradeService.CalculateAssignmentGradeAsync(studentId, monHocId, config);
                    if (detail.AverageGrade.HasValue)
                        detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);

                    var baiTaps = await _db.BaiTaps
                        .Where(b => b.MaMonHoc == monHocId && b.MaCauHinhDauDiem == config.MaCauHinhDauDiem)
                        .OrderBy(b => b.MaBaiTap)
                        .Select(b => new { b.MaBaiTap, b.TieuDe })
                        .ToListAsync();

                    foreach (var bt in baiTaps)
                    {
                        var latestSub = await _db.BaiNops
                            .Where(b => b.MaBaiTap == bt.MaBaiTap && b.MaHocSinh == studentId)
                            .OrderByDescending(b => b.ThoiDiemNop)
                            .Select(b => new { b.DiemSo })
                            .FirstOrDefaultAsync();

                        detail.Items.Add(new GradeItemDto
                        {
                            ItemId = bt.MaBaiTap,
                            ItemName = bt.TieuDe,
                            Grade = latestSub?.DiemSo
                        });
                    }
                }
                else if (loaiCode == "quiz" || loaiCode == "progress_test")
                {
                    detail.AverageGrade = await _gradeService.CalculateQuizGradeAsync(studentId, monHocId, hocKyId, loaiCode, config);
                    if (detail.AverageGrade.HasValue)
                        detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);

                    string expectedLoaiDeThi = loaiCode == "quiz" ? "quiz_bai_hoc" : "progress_test";
                    var deKiemTras = await _db.DeKiemTras
                        .Where(d => d.MaMonHoc == monHocId && d.MaHocKy == hocKyId && d.LoaiDeThi == expectedLoaiDeThi)
                        .OrderBy(d => d.MaDeKiemTra)
                        .Select(d => new { d.MaDeKiemTra, d.TieuDe, d.CauHinhDeThi })
                        .ToListAsync();

                    foreach (var dk in deKiemTras)
                    {
                        var attempts = await _db.PhienThiHocSinhs
                            .Where(x => x.MaDeKiemTra == dk.MaDeKiemTra && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "da_dung")
                            .ToListAsync();

                        var scoredAttempts = attempts.Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue).ToList();

                        decimal? attemptScore = null;
                        if (scoredAttempts.Any())
                        {
                            if (dk.CauHinhDeThi != null && dk.CauHinhDeThi.Contains("\"gradeMethod\":\"last\""))
                                attemptScore = scoredAttempts.OrderByDescending(x => x.BatDauLuc).First().DiemCuoiCung ?? scoredAttempts.OrderByDescending(x => x.BatDauLuc).First().DiemTuDong;
                            else if (dk.CauHinhDeThi != null && dk.CauHinhDeThi.Contains("\"gradeMethod\":\"highest\""))
                                attemptScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong);
                            else
                                attemptScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong);
                        }

                        detail.Items.Add(new GradeItemDto
                        {
                            ItemId = dk.MaDeKiemTra,
                            ItemName = dk.TieuDe,
                            Grade = attemptScore
                        });
                    }
                }

                gradeTypes.Add(detail);
            }

            var student = await _db.NguoiDungs.FindAsync(studentId);

            var result = new StudentGradeDetailDto
            {
                StudentId = studentId,
                StudentName = student?.HoTen ?? "",
                GradeTypes = gradeTypes,
                DiemQuaTrinh = diemRecord?.DiemQuaTrinh,
                DiemGiuaKy = diemRecord?.DiemGiuaKy,
                DiemCuoiKy = diemRecord?.DiemCuoiKy,
                GpaMonHoc = diemRecord?.GpaMonHoc,
                TrangThai = diemRecord?.TrangThai == "dat" ? "Đạt" : (diemRecord?.TrangThai == "rot" ? "Rớt" : (diemRecord?.TrangThai != "draft" ? diemRecord?.TrangThai : null))
            };

            return Ok(ApiResponseDto<StudentGradeDetailDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chi tiết điểm: " + ex.Message));
        }
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
    public int CourseId { get; set; }
    public int SemesterId { get; set; }
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

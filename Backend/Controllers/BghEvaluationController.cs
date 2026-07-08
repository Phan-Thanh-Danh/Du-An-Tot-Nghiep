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
public class BghEvaluationController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghEvaluationController(ApplicationDbContext db)
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

    [HttpGet("evaluations")]
    public async Task<ActionResult<ApiResponseDto<BghEvaluationListDto>>> GetEvaluations()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var avgRating = await _db.DanhGiaGiaoViens.Where(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId)).AverageAsync(g => (double?)g.DiemSo) ?? 0;
        var totalReviews = await _db.DanhGiaGiaoViens.CountAsync(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId));

        var data = new BghEvaluationListDto
        {
            TotalTeachers = totalTeachers,
            AvgRating = Math.Round(avgRating, 1),
            TotalReviews = totalReviews
        };

        return Ok(ApiResponseDto<BghEvaluationListDto>.Ok(data));
    }

    [HttpGet("evaluations/ranking")]
    public async Task<ActionResult<ApiResponseDto<List<TeacherRankingDto>>>> GetEvaluationRanking()
    {
        var (campusId, isGlobal) = GetUserScope();

        var rankings = await _db.DanhGiaGiaoViens
            .Where(g => g.GiaoVien != null && (isGlobal || g.GiaoVien.MaDonVi == campusId))
            .GroupBy(g => new { g.MaGiaoVien, HoTen = g.GiaoVien!.HoTen })
            .Select(g => new TeacherRankingDto
            {
                TeacherId = g.Key.MaGiaoVien,
                TeacherName = g.Key.HoTen,
                AvgRating = Math.Round(g.Average(x => (double)x.DiemSo), 1),
                ReviewCount = g.Count()
            })
            .OrderByDescending(r => r.AvgRating)
            .Take(20)
            .ToListAsync();

        return Ok(ApiResponseDto<List<TeacherRankingDto>>.Ok(rankings));
    }

    [HttpGet("evaluations/{teacherId:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherEvalDetailDto>>> GetEvaluationDetail(int teacherId)
    {
        var (campusId, isGlobal) = GetUserScope();

        var teacher = await _db.NguoiDungs
            .Where(u => u.MaNguoiDung == teacherId && u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId))
            .Select(u => new { u.HoTen, u.Email })
            .FirstOrDefaultAsync();

        if (teacher == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy giảng viên."));

        var avgRating = await _db.DanhGiaGiaoViens
            .Where(g => g.MaGiaoVien == teacherId)
            .AverageAsync(g => (double?)g.DiemSo) ?? 0;

        var totalReviews = await _db.DanhGiaGiaoViens
            .CountAsync(g => g.MaGiaoVien == teacherId);

        var criteria = await _db.DanhGiaGiaoViens
            .Where(g => g.MaGiaoVien == teacherId && g.CauHoiDg != null)
            .GroupBy(g => new { g.MaCauHoiDg, NoiDung = g.CauHoiDg!.NoiDungCauHoi })
            .Select(g => new EvalCriterionDto
            {
                CriterionName = g.Key.NoiDung,
                AvgScore = Math.Round(g.Average(x => (double)x.DiemSo), 1),
                MaxScore = 5
            })
            .ToListAsync();

        var recentFeedback = await _db.DanhGiaGiaoViens
            .Where(g => g.MaGiaoVien == teacherId && g.NhanXetTuDo != null && g.NhanXetTuDo != "")
            .OrderByDescending(g => g.NgayTao)
            .Take(10)
            .Select(g => new FeedbackEntryDto
            {
                Comment = g.NhanXetTuDo ?? "",
                Rating = g.DiemSo,
                Date = g.NgayTao
            })
            .ToListAsync();

        var semesterHistory = await _db.DanhGiaGiaoViens
            .Where(g => g.MaGiaoVien == teacherId && g.HocKy != null)
            .GroupBy(g => new { g.MaHocKy, TenHocKy = g.HocKy!.TenHocKy ?? "" })
            .Select(g => new EvalTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgRating = Math.Round(g.Average(x => (double)x.DiemSo), 1),
                ReviewCount = g.Count()
            })
            .OrderBy(t => t.Semester)
            .Take(4)
            .ToListAsync();

        var data = new TeacherEvalDetailDto
        {
            TeacherId = teacherId,
            TeacherName = teacher.HoTen,
            Email = teacher.Email,
            AvgRating = Math.Round(avgRating, 1),
            TotalReviews = totalReviews,
            Criteria = criteria,
            RecentFeedback = recentFeedback,
            SemesterHistory = semesterHistory
        };

        return Ok(ApiResponseDto<TeacherEvalDetailDto>.Ok(data));
    }

    [HttpGet("evaluations/overview")]
    public async Task<ActionResult<ApiResponseDto<EvalOverviewDto>>> GetEvaluationOverview()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalReviews = await _db.DanhGiaGiaoViens.CountAsync(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId));
        var avgRating = await _db.DanhGiaGiaoViens.Where(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId)).AverageAsync(g => (double?)g.DiemSo) ?? 0;

        var ratingDistribution = await _db.DanhGiaGiaoViens
            .Where(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId))
            .GroupBy(g => g.DiemSo)
            .Select(g => new RatingBucketDto
            {
                Rating = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(r => r.Rating)
            .ToListAsync();

        var semesterTrend = await _db.DanhGiaGiaoViens
            .Where(g => g.HocKy != null && (isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId)))
            .GroupBy(g => new { g.MaHocKy, TenHocKy = g.HocKy!.TenHocKy ?? "" })
            .Select(g => new EvalTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgRating = Math.Round(g.Average(x => (double)x.DiemSo), 1),
                ReviewCount = g.Count()
            })
            .OrderBy(t => t.Semester)
            .ToListAsync();

        var data = new EvalOverviewDto
        {
            TotalTeachers = totalTeachers,
            TotalReviews = totalReviews,
            AvgRating = Math.Round(avgRating, 1),
            RatingDistribution = ratingDistribution,
            SemesterTrend = semesterTrend
        };

        return Ok(ApiResponseDto<EvalOverviewDto>.Ok(data));
    }

    [HttpGet("evaluations/ai-analysis")]
    public async Task<ActionResult<ApiResponseDto<EvalAiAnalysisDto>>> GetEvaluationAiAnalysis()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalReviews = await _db.DanhGiaGiaoViens.CountAsync(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId));

        var topTopics = await _db.DanhGiaGiaoViens
            .Where(g => g.AiChuDe != null && g.AiChuDe != "" && (isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId)))
            .GroupBy(g => g.AiChuDe)
            .Select(g => new AiTopicDto
            {
                Topic = g.Key ?? "",
                Count = g.Count(),
                Sentiment = Math.Round(g.Average(x => (double)x.DiemSo) / 5 * 100, 0)
            })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync();

        var data = new EvalAiAnalysisDto
        {
            AnalysisMode = "rule_based",
            TotalReviews = totalReviews,
            TopTopics = topTopics,
            Status = totalReviews > 0 ? "enough_data" : "not_enough_data"
        };

        return Ok(ApiResponseDto<EvalAiAnalysisDto>.Ok(data));
    }
}

// DTOs
public class BghEvaluationListDto
{
    public int TotalTeachers { get; set; }
    public double AvgRating { get; set; }
    public int TotalReviews { get; set; }
}

public class TeacherRankingDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = "";
    public double AvgRating { get; set; }
    public int ReviewCount { get; set; }
}

public class TeacherEvalDetailDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = "";
    public string Email { get; set; } = "";
    public double AvgRating { get; set; }
    public int TotalReviews { get; set; }
    public List<EvalCriterionDto> Criteria { get; set; } = [];
    public List<FeedbackEntryDto> RecentFeedback { get; set; } = [];
    public List<EvalTrendDto> SemesterHistory { get; set; } = [];
}

public class EvalCriterionDto
{
    public string CriterionName { get; set; } = "";
    public double AvgScore { get; set; }
    public int MaxScore { get; set; }
}

public class FeedbackEntryDto
{
    public string Comment { get; set; } = "";
    public int Rating { get; set; }
    public DateTime Date { get; set; }
}

public class EvalOverviewDto
{
    public int TotalTeachers { get; set; }
    public int TotalReviews { get; set; }
    public double AvgRating { get; set; }
    public List<RatingBucketDto> RatingDistribution { get; set; } = [];
    public List<EvalTrendDto> SemesterTrend { get; set; } = [];
}

public class RatingBucketDto
{
    public int Rating { get; set; }
    public int Count { get; set; }
}

public class EvalTrendDto
{
    public string Semester { get; set; } = "";
    public double AvgRating { get; set; }
    public int ReviewCount { get; set; }
}

public class EvalAiAnalysisDto
{
    public string AnalysisMode { get; set; } = "rule_based";
    public int TotalReviews { get; set; }
    public string Status { get; set; } = "";
    public List<AiTopicDto> TopTopics { get; set; } = [];
}

public class AiTopicDto
{
    public string Topic { get; set; } = "";
    public int Count { get; set; }
    public double Sentiment { get; set; }
}

using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.Services.Bgh;
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
        var isCampusScoped = (user?.Role == AuthRoles.Principal || user?.Role == "hieu_truong") && campusId > 0 &&
                             !(user?.Email?.Contains("bgh_all", StringComparison.OrdinalIgnoreCase) ?? false) &&
                             !(user?.Email?.Contains("p15", StringComparison.OrdinalIgnoreCase) ?? false);

        var isGlobal = !isCampusScoped && (
            user?.Role == AuthRoles.SuperAdmin ||
            user?.Role == AuthRoles.Admin ||
            user?.Role == AuthRoles.Chairman ||
            campusId == 0 ||
            (user?.Email != null && (user.Email.Contains("bgh_all", StringComparison.OrdinalIgnoreCase) ||
                                     user.Email.Contains("p15", StringComparison.OrdinalIgnoreCase)))
        );
        return (campusId, isGlobal);
    }

    [HttpGet("evaluations")]
    [BghResponseCache(60)]
    public async Task<ActionResult<ApiResponseDto<BghEvaluationListDto>>> GetEvaluations()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalTeachers = await _db.NguoiDungs.AsNoTracking().CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var avgRating = await _db.DanhGiaGiaoViens.AsNoTracking().Where(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId)).AverageAsync(g => (double?)g.DiemSo) ?? 0;
        var totalReviews = await _db.DanhGiaGiaoViens.AsNoTracking().CountAsync(g => isGlobal || (g.GiaoVien != null && g.GiaoVien.MaDonVi == campusId));

        var data = new BghEvaluationListDto
        {
            TotalTeachers = totalTeachers,
            AvgRating = Math.Round(avgRating, 1),
            TotalReviews = totalReviews
        };

        return Ok(ApiResponseDto<BghEvaluationListDto>.Ok(data));
    }

    [HttpGet("evaluations/ranking")]
    [BghResponseCache(120)]
    public async Task<ActionResult<ApiResponseDto<List<TeacherRankingDto>>>> GetEvaluationRanking()
    {
        var (campusId, isGlobal) = GetUserScope();

        var aggregates = await _db.DanhGiaGiaoViens
            .AsNoTracking()
            .Where(g => g.GiaoVien != null && (isGlobal || g.GiaoVien.MaDonVi == campusId))
            .GroupBy(g => new { g.MaGiaoVien, HoTen = g.GiaoVien!.HoTen })
            .Where(g => g.Count() >= 5)
            .Select(g => new
            {
                TeacherId = g.Key.MaGiaoVien,
                TeacherName = g.Key.HoTen,
                AvgRating = g.Average(x => (double)x.DiemSo),
                ReviewCount = g.Count(),
                PositiveCount = g.Count(x => x.DiemSo >= 4),
                NegativeCount = g.Count(x => x.DiemSo <= 3)
            })
            .OrderByDescending(r => r.AvgRating)
            .ThenByDescending(r => r.ReviewCount)
            .Take(20)
            .ToListAsync();

        var teacherIds = aggregates.Select(item => item.TeacherId).ToList();

        var departments = await _db.GiaoVienChuyenNganhs
            .AsNoTracking()
            .Where(link => teacherIds.Contains(link.MaGiaoVien) && link.ChuyenNganh != null)
            .OrderByDescending(link => link.LaChuyenMonChinh)
            .ThenByDescending(link => link.MucDoPhuHop)
            .Select(link => new
            {
                link.MaGiaoVien,
                DepartmentId = link.MaChuyenNganh,
                DepartmentName = link.ChuyenNganh!.TenChuyenNganh
            })
            .ToListAsync();
        var departmentByTeacher = departments
            .GroupBy(item => item.MaGiaoVien)
            .ToDictionary(group => group.Key, group => group.First());

        var semesterAverages = await _db.DanhGiaGiaoViens
            .AsNoTracking()
            .Where(evaluation => teacherIds.Contains(evaluation.MaGiaoVien) && evaluation.HocKy != null)
            .GroupBy(evaluation => new
            {
                evaluation.MaGiaoVien,
                evaluation.MaHocKy,
                evaluation.HocKy!.NgayBatDau
            })
            .Select(group => new
            {
                group.Key.MaGiaoVien,
                group.Key.MaHocKy,
                group.Key.NgayBatDau,
                AvgRating = group.Average(evaluation => (double)evaluation.DiemSo)
            })
            .ToListAsync();
        var historyByTeacher = semesterAverages
            .GroupBy(item => item.MaGiaoVien)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(item => item.NgayBatDau).ThenBy(item => item.MaHocKy).ToList());

        var rankings = aggregates.Select(item =>
        {
            departmentByTeacher.TryGetValue(item.TeacherId, out var department);
            historyByTeacher.TryGetValue(item.TeacherId, out var history);
            var latest = history?.LastOrDefault();
            var previous = history is { Count: >= 2 } ? history[^2] : null;
            var trendDelta = latest != null && previous != null
                ? Math.Round(latest.AvgRating - previous.AvgRating, 2)
                : 0;

            return new TeacherRankingDto
            {
                TeacherId = item.TeacherId,
                TeacherName = item.TeacherName,
                DepartmentId = department?.DepartmentId,
                DepartmentName = department?.DepartmentName ?? "Chưa phân khoa",
                AvgRating = Math.Round(item.AvgRating, 2),
                ReviewCount = item.ReviewCount,
                Positive = Math.Round(item.PositiveCount * 100.0 / item.ReviewCount, 1),
                Negative = Math.Round(item.NegativeCount * 100.0 / item.ReviewCount, 1),
                Trend = previous == null
                    ? "new"
                    : trendDelta > 0 ? "up" : trendDelta < 0 ? "down" : "stable",
                TrendDelta = trendDelta,
                LatestSemesterRating = latest == null ? null : Math.Round(latest.AvgRating, 2),
                PreviousSemesterRating = previous == null ? null : Math.Round(previous.AvgRating, 2)
            };
        }).ToList();

        return Ok(ApiResponseDto<List<TeacherRankingDto>>.Ok(rankings));
    }

    [HttpGet("evaluations/{teacherId:int}")]
    [BghResponseCache(120)]
    public async Task<ActionResult<ApiResponseDto<TeacherEvalDetailDto>>> GetEvaluationDetail(int teacherId)
    {
        var (campusId, isGlobal) = GetUserScope();

        var teacher = await _db.NguoiDungs
            .Where(u => u.MaNguoiDung == teacherId && u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId))
            .Select(u => new
            {
                u.HoTen,
                u.Email,
                Department = _db.GiaoVienChuyenNganhs
                    .Where(link => link.MaGiaoVien == u.MaNguoiDung && link.ChuyenNganh != null)
                    .OrderByDescending(link => link.LaChuyenMonChinh)
                    .Select(link => link.ChuyenNganh!.TenChuyenNganh)
                    .FirstOrDefault() ?? ""
            })
            .FirstOrDefaultAsync();

        if (teacher == null)
        {
            return NotFound(ApiResponseDto.Fail("Không tìm thấy giảng viên trong phạm vi quản lý."));
        }

        var reviewSummary = await _db.DanhGiaGiaoViens
            .AsNoTracking()
            .Where(g => g.MaGiaoVien == teacherId)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AvgRating = group.Average(item => (double)item.DiemSo),
                TotalReviews = group.Count(),
                PositiveReviews = group.Count(item => item.DiemSo >= 4),
                NegativeReviews = group.Count(item => item.DiemSo <= 3)
            })
            .FirstOrDefaultAsync();

        var avgRating = reviewSummary?.AvgRating ?? 0;
        var totalReviews = reviewSummary?.TotalReviews ?? 0;
        var positiveReviews = reviewSummary?.PositiveReviews ?? 0;
        var negativeReviews = reviewSummary?.NegativeReviews ?? 0;

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
            .GroupBy(g => new
            {
                g.MaHocKy,
                TenHocKy = g.HocKy!.TenHocKy ?? "",
                g.HocKy.NgayBatDau
            })
            .Select(g => new EvalTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgRating = Math.Round(g.Average(x => (double)x.DiemSo), 1),
                ReviewCount = g.Count(),
                StartDate = g.Key.NgayBatDau
            })
            .OrderBy(t => t.StartDate)
            .Take(8)
            .ToListAsync();

        var data = new TeacherEvalDetailDto
        {
            TeacherId = teacherId,
            TeacherName = teacher.HoTen,
            Email = teacher.Email,
            Department = teacher.Department,
            AvgRating = Math.Round(avgRating, 1),
            TotalReviews = totalReviews,
            PositivePercentage = totalReviews == 0
                ? 0
                : Math.Round(positiveReviews * 100.0 / totalReviews, 1),
            NegativePercentage = totalReviews == 0
                ? 0
                : Math.Round(negativeReviews * 100.0 / totalReviews, 1),
            NeutralPercentage = totalReviews == 0
                ? 0
                : Math.Round((totalReviews - positiveReviews - negativeReviews) * 100.0 / totalReviews, 1),
            Criteria = criteria,
            RecentFeedback = recentFeedback,
            SemesterHistory = semesterHistory
        };

        return Ok(ApiResponseDto<TeacherEvalDetailDto>.Ok(data));
    }

    [HttpGet("evaluations/overview")]
    [BghResponseCache(120)]
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

        var lowRatingTeacherCount = await _db.DanhGiaGiaoViens
            .Where(g => g.GiaoVien != null && (isGlobal || g.GiaoVien.MaDonVi == campusId))
            .GroupBy(g => g.MaGiaoVien)
            .Where(g => g.Average(x => (double)x.DiemSo) < 3.5)
            .CountAsync();

        var data = new EvalOverviewDto
        {
            TotalTeachers = totalTeachers,
            TotalReviews = totalReviews,
            AvgRating = Math.Round(avgRating, 1),
            PositivePercentage = totalReviews == 0
                ? 0
                : Math.Round(ratingDistribution.Where(bucket => bucket.Rating >= 4).Sum(bucket => bucket.Count) * 100.0 / totalReviews, 1),
            NegativePercentage = totalReviews == 0
                ? 0
                : Math.Round(ratingDistribution.Where(bucket => bucket.Rating <= 3).Sum(bucket => bucket.Count) * 100.0 / totalReviews, 1),
            LowRatingTeacherCount = lowRatingTeacherCount,
            RatingDistribution = ratingDistribution,
            SemesterTrend = semesterTrend
        };

        return Ok(ApiResponseDto<EvalOverviewDto>.Ok(data));
    }

    [HttpGet("evaluations/ai-analysis")]
    [BghResponseCache(120)]
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
            Status = totalReviews > 0 ? "enough_data" : "not_enough_data",
            AnalysisNote = topTopics.Count == 0 && totalReviews > 0
                ? "Chủ đề AI chưa được phân tích tự động. Tính năng phân loại nhận xét sẽ khả dụng sau khi tích hợp AI Pipeline Giai đoạn 2."
                : null
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
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public double AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public double Positive { get; set; }
    public double Negative { get; set; }
    public string Trend { get; set; } = "new";
    public double TrendDelta { get; set; }
    public double? LatestSemesterRating { get; set; }
    public double? PreviousSemesterRating { get; set; }
}

public class TeacherEvalDetailDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public double AvgRating { get; set; }
    public int TotalReviews { get; set; }
    public double PositivePercentage { get; set; }
    public double NegativePercentage { get; set; }
    public double NeutralPercentage { get; set; }
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
    public double PositivePercentage { get; set; }
    public double NegativePercentage { get; set; }
    public int LowRatingTeacherCount { get; set; }
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
    public DateOnly StartDate { get; set; }
}

public class EvalAiAnalysisDto
{
    public string AnalysisMode { get; set; } = "rule_based";
    public int TotalReviews { get; set; }
    public string Status { get; set; } = "";
    public string? AnalysisNote { get; set; }
    public List<AiTopicDto> TopTopics { get; set; } = [];
}

public class AiTopicDto
{
    public string Topic { get; set; } = "";
    public int Count { get; set; }
    public double Sentiment { get; set; }
}

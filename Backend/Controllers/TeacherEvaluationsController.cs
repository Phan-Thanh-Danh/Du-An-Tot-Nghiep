using System.Text.RegularExpressions;
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
[Route("api/teacher/evaluations")]
[Authorize(Roles = AuthRoles.Teacher)]
public class TeacherEvaluationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherEvaluationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<TeacherEvaluationReportDto>>> GetMyEvaluations(CancellationToken ct)
    {
        var teacherId = GetCurrentUserId();

        // 1. Lấy tất cả khóa học / lớp giảng viên đang phụ trách
        var teacherCourses = await _context.KhoaHocs
            .AsNoTracking()
            .Include(k => k.MonHoc)
            .Include(k => k.Lop)
            .Where(k => k.MaGiaoVien == teacherId)
            .ToListAsync(ct);

        // 2. Lấy tất cả đánh giá của giảng viên này
        var evaluations = await _context.DanhGiaGiaoViens
            .AsNoTracking()
            .Include(d => d.CauHoiDg)
            .Include(d => d.HocKy)
            .Where(d => d.MaGiaoVien == teacherId)
            .OrderByDescending(d => d.NgayTao)
            .ToListAsync(ct);

        // 3. Lấy tất cả câu hỏi đánh giá
        var questions = await _context.CauHoiDanhGias
            .AsNoTracking()
            .Where(q => q.ConHoatDong)
            .OrderBy(q => q.MaCauHoiDg)
            .ToListAsync(ct);

        var totalEvaluations = evaluations.Count;
        var averageScore = totalEvaluations > 0 ? Math.Round(evaluations.Average(d => (double)d.DiemSo), 1) : 5.0;

        // Bóc tách courseId từ CohortHash: e.g. "student-24-course-25-term-1"
        var courseRegex = new Regex(@"course-(\d+)", RegexOptions.Compiled);

        var evalItemsWithCourse = evaluations.Select(e =>
        {
            int? courseId = null;
            if (!string.IsNullOrEmpty(e.CohortHash))
            {
                var match = courseRegex.Match(e.CohortHash);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var cid))
                {
                    courseId = cid;
                }
            }
            return new { Eval = e, CourseId = courseId };
        }).ToList();

        // 4. Nhóm đánh giá theo từng môn học / khóa học
        var subjectReports = new List<TeacherSubjectEvaluationDto>();

        // Nếu giảng viên có khóa học cụ thể
        if (teacherCourses.Count > 0)
        {
            // Nhóm theo Môn học (hoặc Khóa học)
            var courseGroupMap = teacherCourses.GroupBy(k => k.MaMonHoc).ToList();

            foreach (var group in courseGroupMap)
            {
                var firstCourse = group.First();
                var courseIdsInGroup = group.Select(k => k.MaKhoaHoc).ToHashSet();

                // Lấy các đánh giá thuộc môn học này
                var groupEvals = evalItemsWithCourse
                    .Where(x => (x.CourseId.HasValue && courseIdsInGroup.Contains(x.CourseId.Value)) ||
                                (!x.CourseId.HasValue && group == courseGroupMap.First())) // Gán unassigned evals vào nhóm đầu tiên
                    .Select(x => x.Eval)
                    .ToList();

                var subjectTotal = groupEvals.Count;
                var subjectAvg = subjectTotal > 0 ? Math.Round(groupEvals.Average(e => (double)e.DiemSo), 1) : 5.0;
                var subjectReviews = groupEvals.Where(e => !string.IsNullOrWhiteSpace(e.NhanXetTuDo)).ToList();

                var criteriaList = questions.Select((q, idx) =>
                {
                    var qEvals = groupEvals.Where(e => e.MaCauHoiDg == q.MaCauHoiDg).ToList();
                    var qAvg = qEvals.Count > 0 ? Math.Round(qEvals.Average(e => (double)e.DiemSo), 1) : 5.0;

                    var details = qEvals.Select(e => new TeacherAnonymousReviewDto
                    {
                        Id = e.MaDanhGia,
                        Score = e.DiemSo,
                        Feedback = string.IsNullOrWhiteSpace(e.NhanXetTuDo) ? "Đã chấm điểm đạt chất lượng." : e.NhanXetTuDo,
                        CriteriaName = q.NoiDungCauHoi,
                        SemesterName = e.HocKy?.TenHocKy ?? "Học kỳ hiện tại",
                        CreatedAt = e.NgayTao.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();

                    return new TeacherEvaluationCriteriaDto
                    {
                        QuestionId = q.MaCauHoiDg,
                        QuestionText = $"Mục {idx + 1}. {q.NoiDungCauHoi}",
                        AverageScore = qAvg,
                        ResponseCount = qEvals.Count,
                        Details = details
                    };
                }).ToList();

                subjectReports.Add(new TeacherSubjectEvaluationDto
                {
                    CourseId = firstCourse.MaKhoaHoc,
                    CourseTitle = firstCourse.TieuDe,
                    SubjectCode = firstCourse.MonHoc?.MaCodeMonHoc ?? "MON",
                    SubjectName = firstCourse.MonHoc?.TenMonHoc ?? firstCourse.TieuDe,
                    ClassName = string.Join(", ", group.Select(k => k.Lop?.TenLop).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                    AverageScore = subjectAvg,
                    TotalEvaluations = subjectTotal,
                    TotalReviews = subjectReviews.Count,
                    Criteria = criteriaList
                });
            }
        }
        else
        {
            // Trường hợp không có khóa học cụ thể trong bảng KhoaHoc, hiển thị môn mặc định
            var criteriaList = questions.Select((q, idx) =>
            {
                var qEvals = evaluations.Where(e => e.MaCauHoiDg == q.MaCauHoiDg).ToList();
                var qAvg = qEvals.Count > 0 ? Math.Round(qEvals.Average(e => (double)e.DiemSo), 1) : 5.0;

                var details = qEvals.Select(e => new TeacherAnonymousReviewDto
                {
                    Id = e.MaDanhGia,
                    Score = e.DiemSo,
                    Feedback = string.IsNullOrWhiteSpace(e.NhanXetTuDo) ? "Đã chấm điểm đạt chất lượng." : e.NhanXetTuDo,
                    CriteriaName = q.NoiDungCauHoi,
                    SemesterName = e.HocKy?.TenHocKy ?? "Học kỳ hiện tại",
                    CreatedAt = e.NgayTao.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                return new TeacherEvaluationCriteriaDto
                {
                    QuestionId = q.MaCauHoiDg,
                    QuestionText = $"Mục {idx + 1}. {q.NoiDungCauHoi}",
                    AverageScore = qAvg,
                    ResponseCount = qEvals.Count,
                    Details = details
                };
            }).ToList();

            subjectReports.Add(new TeacherSubjectEvaluationDto
            {
                CourseId = 0,
                CourseTitle = "Chất lượng giảng dạy chung",
                SubjectCode = "GV",
                SubjectName = "Các môn học đang giảng dạy",
                ClassName = "Toàn bộ sinh viên",
                AverageScore = averageScore,
                TotalEvaluations = totalEvaluations,
                TotalReviews = evaluations.Count(e => !string.IsNullOrWhiteSpace(e.NhanXetTuDo)),
                Criteria = criteriaList
            });
        }

        // Global Reviews list
        var reviews = evaluations
            .Where(e => !string.IsNullOrWhiteSpace(e.NhanXetTuDo))
            .Select(e => new TeacherAnonymousReviewDto
            {
                Id = e.MaDanhGia,
                Score = e.DiemSo,
                Feedback = e.NhanXetTuDo ?? "",
                CriteriaName = e.CauHoiDg?.NoiDungCauHoi ?? "Chất lượng giảng dạy",
                SemesterName = e.HocKy?.TenHocKy ?? "Học kỳ gần nhất",
                CreatedAt = e.NgayTao.ToString("dd/MM/yyyy HH:mm")
            })
            .ToList();

        var result = new TeacherEvaluationReportDto
        {
            AverageScore = averageScore,
            TotalEvaluations = totalEvaluations,
            TotalReviews = reviews.Count,
            Subjects = subjectReports,
            Reviews = reviews
        };

        return Ok(ApiResponseDto<TeacherEvaluationReportDto>.Ok(result));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

public class TeacherEvaluationReportDto
{
    public double AverageScore { get; set; }
    public int TotalEvaluations { get; set; }
    public int TotalReviews { get; set; }
    public List<TeacherSubjectEvaluationDto> Subjects { get; set; } = new();
    public List<TeacherAnonymousReviewDto> Reviews { get; set; } = new();
}

public class TeacherSubjectEvaluationDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int TotalEvaluations { get; set; }
    public int TotalReviews { get; set; }
    public List<TeacherEvaluationCriteriaDto> Criteria { get; set; } = new();
}

public class TeacherEvaluationCriteriaDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int ResponseCount { get; set; }
    public List<TeacherAnonymousReviewDto> Details { get; set; } = new();
}

public class TeacherAnonymousReviewDto
{
    public int Id { get; set; }
    public int Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string CriteriaName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

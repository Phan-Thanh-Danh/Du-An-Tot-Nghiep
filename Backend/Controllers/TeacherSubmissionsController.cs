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
public class TeacherSubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherSubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>>> GetSubmissions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = currentUser!.UserId;

        var teacherMonHocIds = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        if (teacherMonHocIds.Count == 0)
        {
            var emptyResult = new PagedResultDto<TeacherSubmissionDto>
            {
                Items = [],
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = 0
            };
            return Ok(ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>.Ok(emptyResult));
        }

        var query = _context.BaiNops
            .Include(b => b.BaiTap).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc));

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.ThoiDiemNop)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TeacherSubmissionDto
            {
                SubmissionId = b.MaBaiNop,
                StudentName = b.HocSinh != null ? b.HocSinh.HoTen : "",
                AssignmentTitle = b.BaiTap != null ? b.BaiTap.TieuDe : "",
                CourseName = b.BaiTap != null && b.BaiTap.MonHoc != null ? b.BaiTap.MonHoc.TenMonHoc : "",
                SubmittedAt = b.ThoiDiemNop,
                Score = b.DiemSo,
                Status = b.DiemSo != null ? "da_cham" : "cho_cham"
            })
            .ToListAsync();

        var result = new PagedResultDto<TeacherSubmissionDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return Ok(ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>.Ok(result));
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherSubmissionDetailDto>>> GetSubmissionDetail(int id)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = currentUser!.UserId;

        var teacherMonHocIds = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var submission = await _context.BaiNops
            .Include(b => b.BaiTap).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc))
            .FirstOrDefaultAsync(b => b.MaBaiNop == id);

        if (submission is null)
            return NotFound(ApiResponseDto.Ok("Không tìm thấy bài nộp"));

        var detail = new TeacherSubmissionDetailDto
        {
            SubmissionId = submission.MaBaiNop,
            StudentName = submission.HocSinh?.HoTen ?? "",
            AssignmentTitle = submission.BaiTap?.TieuDe ?? "",
            CourseName = submission.BaiTap?.MonHoc?.TenMonHoc ?? "",
            Description = submission.BaiTap?.MoTa ?? "",
            FileUrl = submission.UrlTapTin,
            SubmittedAt = submission.ThoiDiemNop,
            AttemptNumber = submission.SoLanNop,
            IsLate = submission.NopTre,
            Score = submission.DiemSo,
            PlagiarismScore = submission.DiemDaoVan,
            AiScore = submission.DiemAiDeXuat,
            Feedback = submission.NhanXet,
            IsPublished = submission.DaCongBo,
            Status = submission.DiemSo != null ? "da_cham" : "cho_cham"
        };

        return Ok(ApiResponseDto<TeacherSubmissionDetailDto>.Ok(detail));
    }

    [HttpPut("submissions/{id:int}/grade")]
    public async Task<ActionResult<ApiResponseDto<TeacherSubmissionDto>>> GradeSubmission(
        int id,
        GradeSubmissionRequest request)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = currentUser!.UserId;

        var teacherMonHocIds = await _context.KhoaHocs
            .Where(k => k.MaGiaoVien == userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var submission = await _context.BaiNops
            .Include(b => b.BaiTap).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc))
            .FirstOrDefaultAsync(b => b.MaBaiNop == id);

        if (submission is null)
            return NotFound(ApiResponseDto.Ok("Không tìm thấy bài nộp"));

        submission.DiemSo = request.Score;
        submission.NhanXet = request.Feedback;
        submission.DaCongBo = request.Publish ?? false;

        await _context.SaveChangesAsync();

        var dto = new TeacherSubmissionDto
        {
            SubmissionId = submission.MaBaiNop,
            StudentName = submission.HocSinh?.HoTen ?? "",
            AssignmentTitle = submission.BaiTap?.TieuDe ?? "",
            CourseName = submission.BaiTap?.MonHoc?.TenMonHoc ?? "",
            SubmittedAt = submission.ThoiDiemNop,
            Score = submission.DiemSo,
            Status = "da_cham"
        };

        return Ok(ApiResponseDto<TeacherSubmissionDto>.Ok(dto, "Chấm điểm thành công"));
    }
}

public class TeacherSubmissionDto
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TeacherSubmissionDetailDto
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsLate { get; set; }
    public decimal? Score { get; set; }
    public decimal? PlagiarismScore { get; set; }
    public decimal? AiScore { get; set; }
    public string? Feedback { get; set; }
    public bool IsPublished { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GradeSubmissionRequest
{
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public bool? Publish { get; set; }
}

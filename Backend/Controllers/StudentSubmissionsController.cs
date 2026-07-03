using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/submissions")]
[Authorize(Roles = "Student")]
public class StudentSubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentSubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<StudentSubmissionDto>>>> GetSubmissions()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var submissions = await _context.BaiNops
            .Include(s => s.BaiTap!).ThenInclude(a => a.MonHoc)
            .Where(s => s.MaHocSinh == currentUser.UserId)
            .OrderByDescending(s => s.ThoiDiemNop)
            .Select(s => new StudentSubmissionDto
            {
                Id = s.MaBaiNop.ToString(),
                AssignmentId = s.MaBaiTap.ToString(),
                AssignmentTitle = s.BaiTap != null ? s.BaiTap.TieuDe : "",
                Course = s.BaiTap != null && s.BaiTap.MonHoc != null ? s.BaiTap.MonHoc.TenMonHoc : "",
                SubmittedAt = s.ThoiDiemNop.ToString("dd/MM/yyyy HH:mm"),
                Attempt = s.SoLanNop,
                IsLate = s.NopTre,
                FileUrl = s.UrlTapTin,
                Score = s.DaCongBo ? s.DiemSo : null,
                Feedback = s.DaCongBo ? s.NhanXet : null,
                Status = s.DiemSo == null ? "checking" : s.DaCongBo ? "graded" : "graded_unpublished",
                StatusLabel = s.DiemSo == null ? "Đang chấm" : s.DaCongBo ? "Đã chấm" : "Chờ công bố"
            })
            .ToListAsync();

        return Ok(ApiResponseDto<List<StudentSubmissionDto>>.Ok(submissions));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentSubmissionDto>>> GetSubmissionDetail(int id)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var submission = await _context.BaiNops
            .Include(s => s.BaiTap!).ThenInclude(a => a.MonHoc)
            .Where(s => s.MaHocSinh == currentUser.UserId && s.MaBaiNop == id)
            .Select(s => new StudentSubmissionDto
            {
                Id = s.MaBaiNop.ToString(),
                AssignmentId = s.MaBaiTap.ToString(),
                AssignmentTitle = s.BaiTap != null ? s.BaiTap.TieuDe : "",
                Course = s.BaiTap != null && s.BaiTap.MonHoc != null ? s.BaiTap.MonHoc.TenMonHoc : "",
                SubmittedAt = s.ThoiDiemNop.ToString("dd/MM/yyyy HH:mm"),
                Attempt = s.SoLanNop,
                IsLate = s.NopTre,
                FileUrl = s.UrlTapTin,
                Score = s.DaCongBo ? s.DiemSo : null,
                Feedback = s.DaCongBo ? s.NhanXet : null,
                Status = s.DiemSo == null ? "checking" : s.DaCongBo ? "graded" : "graded_unpublished",
                StatusLabel = s.DiemSo == null ? "Đang chấm" : s.DaCongBo ? "Đã chấm" : "Chờ công bố"
            })
            .FirstOrDefaultAsync();

        if (submission == null)
        {
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài nộp."));
        }

        return Ok(ApiResponseDto<StudentSubmissionDto>.Ok(submission));
    }
}

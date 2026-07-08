using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherCommunicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherCommunicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("student-questions")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetStudentQuestions()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var monHocIds = await _context.KhoaHocs
                .Where(k => k.MaGiaoVien == userId)
                .Select(k => k.MaMonHoc)
                .Distinct()
                .ToListAsync();

            if (monHocIds.Count == 0)
                return Ok(ApiResponseDto<object>.Ok(new List<object>()));

            var questions = await _context.BinhLuans
                .Where(c => c.MaBinhLuanCha == null
                    && c.MaNguoiDung != userId
                    && c.BaiHoc != null && c.BaiHoc.Chuong != null
                    && monHocIds.Contains(c.BaiHoc.Chuong.MaMonHoc))
                .OrderByDescending(c => c.NgayTao)
                .Select(c => new
                {
                    QuestionId = c.MaBinhLuan,
                    StudentName = c.NguoiDung != null ? c.NguoiDung.HoTen : "",
                    LessonTitle = c.BaiHoc != null ? c.BaiHoc.TieuDe : "",
                    Content = c.NoiDung,
                    CreatedAt = c.NgayTao,
                    ReplyCount = _context.BinhLuans.Count(r => r.MaBinhLuanCha == c.MaBinhLuan)
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(questions));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải câu hỏi: " + ex.Message));
        }
    }

    [HttpPost("student-questions/{questionId}/reply")]
    public async Task<ActionResult<ApiResponseDto<object>>> ReplyToQuestion(int questionId, [FromBody] ReplyRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var question = await _context.BinhLuans
                .Include(c => c.BaiHoc)
                    .ThenInclude(b => b!.Chuong)
                .FirstOrDefaultAsync(c => c.MaBinhLuan == questionId);

            if (question == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy câu hỏi."));

            var ownsCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaGiaoVien == userId
                    && k.MaMonHoc == question.BaiHoc!.Chuong!.MaMonHoc);

            if (!ownsCourse)
                return Forbid();

            var reply = new BinhLuan
            {
                MaBaiHoc = question.MaBaiHoc,
                MaNguoiDung = userId,
                NoiDung = request.Content,
                MaBinhLuanCha = questionId,
                NgayTao = DateTime.UtcNow
            };

            _context.BinhLuans.Add(reply);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi trả lời: " + ex.Message));
        }
    }

    [HttpGet("lesson-comments")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonComments()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var monHocIds = await _context.KhoaHocs
                .Where(k => k.MaGiaoVien == userId)
                .Select(k => k.MaMonHoc)
                .Distinct()
                .ToListAsync();

            if (monHocIds.Count == 0)
                return Ok(ApiResponseDto<object>.Ok(new List<object>()));

            var comments = await _context.BinhLuans
                .Where(c => c.BaiHoc != null && c.BaiHoc.Chuong != null
                    && monHocIds.Contains(c.BaiHoc.Chuong.MaMonHoc))
                .OrderByDescending(c => c.NgayTao)
                .Select(c => new
                {
                    CommentId = c.MaBinhLuan,
                    StudentName = c.NguoiDung != null ? c.NguoiDung.HoTen : "",
                    LessonTitle = c.BaiHoc != null ? c.BaiHoc.TieuDe : "",
                    Content = c.NoiDung,
                    CreatedAt = c.NgayTao,
                    Replied = _context.BinhLuans.Any(r => r.MaBinhLuanCha == c.MaBinhLuan && r.MaNguoiDung == userId),
                    Pinned = c.DaGhim
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(comments));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải bình luận: " + ex.Message));
        }
    }

    [HttpPost("lesson-comments/{commentId}/reply")]
    public async Task<ActionResult<ApiResponseDto<object>>> ReplyToComment(int commentId, [FromBody] ReplyRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var comment = await _context.BinhLuans
                .Include(c => c.BaiHoc)
                    .ThenInclude(b => b!.Chuong)
                .FirstOrDefaultAsync(c => c.MaBinhLuan == commentId);

            if (comment == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy bình luận."));

            var ownsCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaGiaoVien == userId
                    && k.MaMonHoc == comment.BaiHoc!.Chuong!.MaMonHoc);

            if (!ownsCourse)
                return Forbid();

            var reply = new BinhLuan
            {
                MaBaiHoc = comment.MaBaiHoc,
                MaNguoiDung = userId,
                NoiDung = request.Content,
                MaBinhLuanCha = commentId,
                NgayTao = DateTime.UtcNow
            };

            _context.BinhLuans.Add(reply);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi trả lời: " + ex.Message));
        }
    }

    [HttpPatch("lesson-comments/{commentId}/hide")]
    public async Task<ActionResult<ApiResponseDto<object>>> HideComment(int commentId, [FromBody] HideCommentRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var comment = await _context.BinhLuans
                .Include(c => c.BaiHoc)
                    .ThenInclude(b => b!.Chuong)
                .FirstOrDefaultAsync(c => c.MaBinhLuan == commentId);

            if (comment == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy bình luận."));

            var ownsCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaGiaoVien == userId
                    && k.MaMonHoc == comment.BaiHoc!.Chuong!.MaMonHoc);

            if (!ownsCourse)
                return Forbid();

            comment.DaGhim = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { Success = true, Message = "Bình luận đã được ẩn." }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi ẩn bình luận: " + ex.Message));
        }
    }
}

public class ReplyRequest
{
    public string Content { get; set; } = string.Empty;
}

public class HideCommentRequest
{
    public string Reason { get; set; } = string.Empty;
}

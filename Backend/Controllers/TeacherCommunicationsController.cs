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
[Authorize(Roles = "Teacher,giao_vien")]
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
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonComments(
        [FromQuery] int? subjectId = null,
        [FromQuery] string? lesson = null,
        [FromQuery] string? keyword = null,
        [FromServices] Backend.Services.Comments.ICommentLikeService likeService = null!)
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

            var query = _context.BinhLuans
                .Include(c => c.NguoiDung)
                .Include(c => c.BaiHoc)
                    .ThenInclude(b => b!.Chuong)
                        .ThenInclude(ch => ch!.MonHoc)
                .Where(c => c.MaBinhLuanCha == null
                    && c.BaiHoc != null && c.BaiHoc.Chuong != null
                    && monHocIds.Contains(c.BaiHoc.Chuong.MaMonHoc));

            if (subjectId.HasValue && subjectId.Value > 0)
            {
                query = query.Where(c => c.BaiHoc!.Chuong!.MaMonHoc == subjectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(lesson))
            {
                query = query.Where(c => c.BaiHoc!.TieuDe.ToLower() == lesson.Trim().ToLower());
            }

            var rootComments = await query
                .OrderByDescending(c => c.NgayTao)
                .ToListAsync();

            var rootIds = rootComments.Select(c => c.MaBinhLuan).ToList();
            var allReplies = await _context.BinhLuans
                .Include(r => r.NguoiDung)
                .Where(r => r.MaBinhLuanCha != null && rootIds.Contains(r.MaBinhLuanCha.Value))
                .OrderBy(r => r.NgayTao)
                .ToListAsync();

            var repliesGrouped = allReplies
                .GroupBy(r => r.MaBinhLuanCha!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var comments = rootComments.Select(c =>
            {
                var repliesList = repliesGrouped.GetValueOrDefault(c.MaBinhLuan, new List<BinhLuan>())
                    .Select(r => new
                    {
                        id = r.MaBinhLuan,
                        maBinhLuan = r.MaBinhLuan,
                        author = r.NguoiDung?.HoTen ?? ("Người dùng " + r.MaNguoiDung),
                        hoTen = r.NguoiDung?.HoTen ?? ("Người dùng " + r.MaNguoiDung),
                        role = r.NguoiDung?.VaiTroChinh ?? "user",
                        content = r.NoiDung,
                        noiDung = r.NoiDung,
                        time = r.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        ngayTao = r.NgayTao
                    }).ToList();

                var hasTeacherReplied = repliesList.Any(r => r.role == "giao_vien" || r.role == "Teacher")
                    || repliesGrouped.GetValueOrDefault(c.MaBinhLuan, new List<BinhLuan>()).Any(r => r.MaNguoiDung == userId);

                return new
                {
                    id = c.MaBinhLuan,
                    maBinhLuan = c.MaBinhLuan,
                    commentId = c.MaBinhLuan,
                    studentName = c.NguoiDung?.HoTen ?? ("Sinh viên " + c.MaNguoiDung),
                    hoTen = c.NguoiDung?.HoTen ?? ("Sinh viên " + c.MaNguoiDung),
                    author = c.NguoiDung?.HoTen ?? ("Sinh viên " + c.MaNguoiDung),
                    lessonTitle = c.BaiHoc?.TieuDe ?? "Bài học",
                    baiHoc = c.BaiHoc?.TieuDe ?? "Bài học",
                    lesson = c.BaiHoc?.TieuDe ?? "Bài học",
                    tenBaiHoc = c.BaiHoc?.TieuDe ?? "Bài học",
                    subjectId = c.BaiHoc?.Chuong?.MaMonHoc ?? 0,
                    subjectName = c.BaiHoc?.Chuong?.MonHoc?.TenMonHoc ?? "",
                    monHoc = c.BaiHoc?.Chuong?.MonHoc?.TenMonHoc ?? "",
                    subjectCode = c.BaiHoc?.Chuong?.MonHoc?.MaCodeMonHoc ?? "",
                    content = c.NoiDung,
                    noiDung = c.NoiDung,
                    createdAt = c.NgayTao,
                    ngayTao = c.NgayTao,
                    time = c.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    replied = hasTeacherReplied,
                    pinned = c.DaGhim,
                    likes = likeService != null ? likeService.GetLikesCount(c.MaBinhLuan) : 0,
                    isLiked = likeService != null && likeService.HasUserLiked(c.MaBinhLuan, userId),
                    replies = repliesList
                };
            }).ToList();

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
                NoiDung = request.Content.Trim(),
                MaBinhLuanCha = commentId,
                NgayTao = DateTime.UtcNow
            };

            _context.BinhLuans.Add(reply);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho sinh viên tác giả câu hỏi
            if (comment.MaNguoiDung != userId)
            {
                var teacherUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
                var teacherName = teacherUser?.HoTen ?? "Giảng viên";
                var lessonTitle = comment.BaiHoc?.TieuDe ?? "Bài học";

                var thongBao = new ThongBao
                {
                    MaNhomThongBao = Guid.NewGuid(),
                    MaNguoiNhan = comment.MaNguoiDung,
                    MaDonVi = teacherUser?.MaDonVi ?? 1,
                    TieuDe = "Giảng viên đã phản hồi câu hỏi của bạn",
                    TomTat = $"Thầy/Cô {teacherName} vừa phản hồi thảo luận của bạn",
                    NoiDung = $"Thầy/Cô {teacherName} vừa phản hồi thảo luận của bạn trong bài học \"{lessonTitle}\": \"{request.Content.Trim()}\"",
                    NoiDungText = $"Thầy/Cô {teacherName} vừa phản hồi thảo luận của bạn trong bài học \"{lessonTitle}\": \"{request.Content.Trim()}\"",
                    LoaiThongBao = "hoc_vu",
                    PhamViGui = "nguoi_dung",
                    NgayTao = DateTime.UtcNow,
                    NguoiTao = userId
                };
                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                _context.ThongBaoNguoiNhans.Add(new ThongBaoNguoiNhan
                {
                    MaThongBao = thongBao.MaThongBao,
                    MaNguoiNhan = comment.MaNguoiDung,
                    MaDonVi = teacherUser?.MaDonVi ?? 1,
                    DaDoc = false,
                    NhanLuc = DateTime.UtcNow,
                    NgayTao = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

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

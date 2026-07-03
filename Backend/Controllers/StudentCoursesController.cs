using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentCourse;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/courses")]
public class StudentCoursesController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<List<CourseProgressDto>>>> GetCourses(
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var enrollments = await context.DangKyHocPhans
            .Include(d => d.LopHocPhan!)
                .ThenInclude(l => l.MonHoc)
            .Where(d => d.MaHocSinh == currentUser.UserId && d.TrangThai == "da_duyet")
            .ToListAsync();

        var courseIds = enrollments
            .Select(d => d.LopHocPhan?.MaMonHoc)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var totalLessons = await context.Chuongs
            .Where(c => courseIds.Contains(c.MaMonHoc))
            .GroupBy(c => c.MaMonHoc)
            .Select(g => new { MonHocId = g.Key, Count = g.Sum(c => c.BaiHocs.Count) })
            .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

        var completedCounts = await context.TienDoBaiHocs
            .Where(t => t.MaHocSinh == currentUser.UserId
                && (t.PhanTramTienDo >= 100 || t.HoanThanhLuc != null))
            .Select(t => t.BaiHoc!.Chuong!.MaMonHoc)
            .Where(m => courseIds.Contains(m))
            .GroupBy(m => m)
            .Select(g => new { MonHocId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

        var result = enrollments
            .Select(d => d.LopHocPhan)
            .Where(l => l?.MonHoc != null)
            .DistinctBy(l => l!.MaMonHoc)
            .Select(l =>
            {
                var total = totalLessons.GetValueOrDefault(l!.MaMonHoc);
                var completed = completedCounts.GetValueOrDefault(l.MaMonHoc);
                var progress = total > 0 ? (int)((double)completed / total * 100) : 0;

                string status, statusVariant;
                if (progress == 100)
                {
                    status = "Hoàn thành";
                    statusVariant = "neutral";
                }
                else if (progress > 0)
                {
                    status = "Đang học";
                    statusVariant = "success";
                }
                else
                {
                    status = "Chưa bắt đầu";
                    statusVariant = "warning";
                }

                return new CourseProgressDto
                {
                    Id = l.MaCodeLopHocPhan,
                    Name = l.MonHoc!.TenMonHoc,
                    Code = l.MonHoc.MaCodeMonHoc,
                    Lecturer = "Giảng viên phụ trách",
                    Progress = progress,
                    Completed = completed,
                    Total = total,
                    Status = status,
                    StatusVariant = statusVariant
                };
            })
            .ToList();

        return Ok(ApiResponseDto<List<CourseProgressDto>>.Ok(result));
    }

    [HttpGet("{courseId}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<CourseDetailResponseDto>>> GetCourseDetail(
        string courseId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var course = await context.DanhMucMonHocs.FirstOrDefaultAsync(c => c.MaCodeMonHoc == courseId.ToUpper());

        if (course == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy môn học " + courseId.ToUpper() + " trong CSDL. Vui lòng restart backend để seed Data.cs" });
        }

        var chapters = await context.Chuongs
            .Include(c => c.BaiHocs)
            .Where(c => c.MaMonHoc == course.MaMonHoc)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        var response = new CourseDetailResponseDto
        {
            Course = new CourseDetailDto
            {
                Id = course.MaCodeMonHoc,
                Title = course.TenMonHoc,
                Code = course.MaCodeMonHoc,
                Teacher = "TS. Nguyễn Minh Khoa",
                Semester = "HK Hiện Tại",
                Credits = course.SoTinChi,
                CoverGradient = "from-blue-700 via-blue-600 to-cyan-500",
                Description = $"Môn học {course.TenMonHoc} ({course.MaCodeMonHoc}) cung cấp các kiến thức cốt lõi và kỹ năng thực hành chuyên sâu."
            },
            Stats = new List<CourseStatDto>
            {
                new() { Label = "Tiến độ", Value = "60", Unit = "%", Icon = "Gauge", Tone = "blue", Progress = 60, Hint = "7/12 bài đã hoàn thành" },
                new() { Label = "Bài học", Value = "7", Unit = "/12", Icon = "BookOpenCheck", Tone = "green", Progress = 58, Hint = "Đã hoàn thành 7 bài" },
                new() { Label = "Bài tập", Value = "2", Unit = "mục", Icon = "ClipboardList", Tone = "orange", Progress = 80, Hint = "1 bài gần đến hạn" },
                new() { Label = "Tài liệu", Value = "18", Unit = "file", Icon = "Files", Tone = "violet", Progress = 60, Hint = "PDF, video, quiz" }
            },
            Lessons = chapters.Select(ch => new CourseChapterDto
            {
                Id = "ch" + ch.MaChuong,
                Chapter = "Chương " + ch.ThuTu,
                Title = ch.TieuDe,
                Description = "",
                Status = ch.ThuTu == 1 ? "completed" : ch.ThuTu == 2 ? "active" : ch.ThuTu == 3 ? "locked" : "upcoming",
                Badge = ch.ThuTu == 1 ? "Hoàn thành" : ch.ThuTu == 2 ? "Đang học" : ch.ThuTu == 3 ? "Bị khóa" : "Dự kiến",
                Tone = ch.ThuTu == 1 ? "green" : ch.ThuTu == 2 ? "blue" : ch.ThuTu == 3 ? "slate" : "amber",
                Icon = ch.ThuTu == 1 ? "CheckCircle2" : ch.ThuTu == 2 ? "ListTree" : ch.ThuTu == 3 ? "Lock" : "GitBranch",
                Meta = new List<string> { $"{ch.BaiHocs.Count} bài học" },
                Progress = ch.ThuTu == 1 ? 100 : ch.ThuTu == 2 ? 60 : 0,
                Lessons = ch.BaiHocs.OrderBy(b => b.ThuTu).Select(b => new CourseLessonDto
                {
                    Id = "l" + b.MaBaiHoc,
                    Title = b.TieuDe,
                    Duration = b.ThoiLuongGiay.HasValue ? TimeSpan.FromSeconds(b.ThoiLuongGiay.Value).ToString(@"mm\:ss") : "–",
                    Status = ch.ThuTu == 1 ? "completed" : (ch.ThuTu == 2 && b.ThuTu == 1 ? "completed" : ch.ThuTu == 2 && b.ThuTu == 2 ? "active" : "locked"),
                    Type = b.LoaiBaiHoc == "trac_nghiem" ? "quiz" : b.LoaiBaiHoc == "van_ban" || b.LoaiBaiHoc == "pdf" || b.LoaiBaiHoc == "slide_html" ? "document" : "video",
                    Url = b.UrlTapTin ?? string.Empty
                }).ToList()
            }).ToList()
        };

        return Ok(ApiResponseDto<CourseDetailResponseDto>.Ok(response));
    }

    [HttpGet("{courseId}/lessons/{lessonId}/quiz")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonQuiz(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        if (string.IsNullOrEmpty(lessonId) || !lessonId.StartsWith("l"))
        {
            return BadRequest();
        }
        if (!int.TryParse(lessonId.Substring(1), out int parsedLessonId))
        {
            return BadRequest();
        }

        var lessonContent = await context.BaiHocNoiDungs
            .FirstOrDefaultAsync(n => n.MaBaiHoc == parsedLessonId && n.LoaiNoiDung == "quiz");

        if (lessonContent?.MaDeKiemTra == null)
        {
            return Ok(ApiResponseDto<object>.Ok(new List<object>()));
        }

        var quizQuestions = await context.CauHoiDeKiemTras
            .Include(q => q.CauHoi)
            .Where(q => q.MaDeKiemTra == lessonContent.MaDeKiemTra.Value)
            .OrderBy(q => q.ThuTu)
            .ToListAsync();

        var result = quizQuestions.Select(q =>
        {
            string[] options;
            try
            {
                options = JsonSerializer.Deserialize<string[]>(q.CauHoi?.LuaChon ?? "[]") ?? [];
            }
            catch
            {
                options = (q.CauHoi?.LuaChon ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            int correctIndex = -1;
            if (!string.IsNullOrEmpty(q.CauHoi?.DapAnDung))
            {
                int.TryParse(q.CauHoi.DapAnDung, out correctIndex);
            }

            return new
            {
                Id = "q" + q.MaCauHoi,
                Text = q.CauHoi?.NoiDung ?? "",
                Type = q.CauHoi?.KieuLuaChon == "multiple" ? "multiple" : "single",
                Options = options,
                CorrectAnswer = correctIndex
            };
        }).ToList();

        return Ok(ApiResponseDto<object>.Ok(result));
    }

    [HttpGet("{courseId}/lessons/{lessonId}/comments")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonComments(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        if (string.IsNullOrEmpty(lessonId) || !lessonId.StartsWith("l")) return BadRequest();
        if (!int.TryParse(lessonId.Substring(1), out int parsedLessonId)) return BadRequest();

        var comments = await context.BinhLuans
            .Where(b => b.MaBaiHoc == parsedLessonId)
            .OrderByDescending(b => b.NgayTao)
            .Select(b => new
            {
                Id = "c" + b.MaBinhLuan,
                Author = "Sinh viên " + b.MaNguoiDung,
                Initials = "SV",
                Role = "student",
                Content = b.NoiDung,
                TimeAgo = b.NgayTao != null ? b.NgayTao.ToString("dd/MM/yyyy HH:mm") : "Vừa xong",
                Likes = 0,
                IsLiked = false,
                Replies = new List<object>()
            })
            .ToListAsync();

        if (!comments.Any())
        {
            var mockComments = new List<object>
            {
                new { Id = "c1", Author = "Trần Văn An", Avatar = "", Initials = "TA", Role = "teacher", Content = "Thầy ơi, phần này giải thích thêm về stack overflow không ạ?", TimeAgo = "2 giờ trước", Likes = 4, IsLiked = false, Replies = new List<object>() },
                new { Id = "c2", Author = "Lê Thị Bích", Avatar = "", Initials = "LB", Role = "student", Content = "Bài giảng rất dễ hiểu. Mình đã pass được 3/4 test case!", TimeAgo = "5 giờ trước", Likes = 7, IsLiked = true, Replies = new List<object>() }
            };
            return Ok(ApiResponseDto<object>.Ok(mockComments));
        }

        return Ok(ApiResponseDto<object>.Ok(comments));
    }
}

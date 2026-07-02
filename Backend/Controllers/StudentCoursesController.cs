using Backend.DTOs.Common;
using Backend.DTOs.StudentCourse;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/courses")]
public class StudentCoursesController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Student")]
    public ActionResult<ApiResponseDto<List<CourseProgressDto>>> GetCourses()
    {
        var courses = new List<CourseProgressDto>
        {
            new() { Id = "ctdl", Name = "Cấu trúc dữ liệu & Giải thuật", Code = "CTDL101", Lecturer = "TS. Nguyễn Minh Khoa", Progress = 72, Completed = 9, Total = 12, Status = "Cần tiếp tục", StatusVariant = "warning" },
            new() { Id = "web", Name = "Lập trình Web nâng cao", Code = "LTW301", Lecturer = "ThS. Lê Phương Mai", Progress = 86, Completed = 12, Total = 14, Status = "Sắp hoàn thành", StatusVariant = "success" },
            new() { Id = "db", Name = "Hệ quản trị CSDL", Code = "HQTCSDL401", Lecturer = "ThS. Trần Quốc Việt", Progress = 100, Completed = 15, Total = 15, Status = "Hoàn thành", StatusVariant = "neutral" }
        };

        return Ok(ApiResponseDto<List<CourseProgressDto>>.Ok(courses));
    }

    [HttpGet("{courseId}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<Backend.DTOs.StudentCourse.CourseDetailResponseDto>>> GetCourseDetail(
        string courseId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var course = await context.DanhMucMonHocs.FirstOrDefaultAsync(c => c.MaCodeMonHoc == courseId.ToUpper());
        
        // If not found in DB, fallback to mock data structure for demo purposes so UI doesn't break
        if (course == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy môn học " + courseId.ToUpper() + " trong CSDL. Vui lòng restart backend để seed Data.cs" });
        }

        var chapters = await context.Chuongs
            .Include(c => c.BaiHocs)
            .Where(c => c.MaMonHoc == course.MaMonHoc)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        var response = new Backend.DTOs.StudentCourse.CourseDetailResponseDto
        {
            Course = new Backend.DTOs.StudentCourse.CourseDetailDto
            {
                Id = course.MaCodeMonHoc,
                Title = course.TenMonHoc,
                Code = course.MaCodeMonHoc,
                Teacher = "TS. Nguyễn Minh Khoa", // DB doesn't have Teacher in DanhMucMonHoc yet, mock it
                Semester = "HK Hiện Tại",
                Credits = course.SoTinChi,
                CoverGradient = "from-blue-700 via-blue-600 to-cyan-500",
                Description = $"Môn học {course.TenMonHoc} ({course.MaCodeMonHoc}) cung cấp các kiến thức cốt lõi và kỹ năng thực hành chuyên sâu."
            },
            Stats = new List<Backend.DTOs.StudentCourse.CourseStatDto>
            {
                new() { Label = "Tiến độ", Value = "60", Unit = "%", Icon = "Gauge", Tone = "blue", Progress = 60, Hint = "7/12 bài đã hoàn thành" },
                new() { Label = "Bài học", Value = "7", Unit = "/12", Icon = "BookOpenCheck", Tone = "green", Progress = 58, Hint = "Đã hoàn thành 7 bài" },
                new() { Label = "Bài tập", Value = "2", Unit = "mục", Icon = "ClipboardList", Tone = "orange", Progress = 80, Hint = "1 bài gần đến hạn" },
                new() { Label = "Tài liệu", Value = "18", Unit = "file", Icon = "Files", Tone = "violet", Progress = 60, Hint = "PDF, video, quiz" }
            },
            Lessons = chapters.Select(ch => new Backend.DTOs.StudentCourse.CourseChapterDto
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
                Lessons = ch.BaiHocs.OrderBy(b => b.ThuTu).Select(b => new Backend.DTOs.StudentCourse.CourseLessonDto
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

        return Ok(ApiResponseDto<Backend.DTOs.StudentCourse.CourseDetailResponseDto>.Ok(response));
    }

    [HttpGet("{courseId}/lessons/{lessonId}/quiz")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonQuiz(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        // TODO: Query from CauHoi based on lesson
        var mockQuiz = new List<object>
        {
            new { Id = "q1", Text = "Mục đích chính của bài học này là gì?", Type = "single", Options = new[] { "Nắm vững kiến thức lý thuyết cơ bản", "Thực hành viết code thực tế", "Tối ưu hiệu năng ứng dụng", "Tất cả các ý trên đều đúng" }, CorrectAnswer = 3 },
            new { Id = "q2", Text = "Sau khi hoàn thành bài học, sinh viên cần làm gì?", Type = "single", Options = new[] { "Nộp bài tập thực hành", "Làm bài kiểm tra Quiz", "Tự ôn tập lại lý thuyết", "Tham gia thảo luận nhóm" }, CorrectAnswer = 1 }
        };
        return Ok(ApiResponseDto<object>.Ok(mockQuiz));
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
            // Seed default data just for UI testing if empty
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

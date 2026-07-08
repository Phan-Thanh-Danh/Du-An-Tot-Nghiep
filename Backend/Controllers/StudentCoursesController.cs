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

        var student = await context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUser.UserId);

        if (student?.MaLop is null)
        {
            return Ok(ApiResponseDto<List<CourseProgressDto>>.Ok([]));
        }

        var courses = await context.KhoaHocs
            .AsNoTracking()
            .Include(k => k.MonHoc)
            .Include(k => k.GiaoVien)
            .Include(k => k.HocKy)
            .Where(k => k.MaLop == student.MaLop.Value && k.TrangThai == "da_xuat_ban")
            .OrderBy(k => k.HocKy != null ? k.HocKy.NgayBatDau : DateOnly.MinValue)
            .ThenBy(k => k.MonHoc != null ? k.MonHoc.TenMonHoc : k.TieuDe)
            .ToListAsync();

        var subjectIds = courses
            .Select(c => c.MaMonHoc)
            .Distinct()
            .ToList();

        var totalLessons = await context.Chuongs
            .Where(c => subjectIds.Contains(c.MaMonHoc))
            .GroupBy(c => c.MaMonHoc)
            .Select(g => new { MonHocId = g.Key, Count = g.Sum(c => c.BaiHocs.Count) })
            .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

        var completedCounts = await context.TienDoBaiHocs
            .Where(t => t.MaHocSinh == currentUser.UserId
                && (t.PhanTramTienDo >= 100 || t.HoanThanhLuc != null))
            .Select(t => t.BaiHoc!.Chuong!.MaMonHoc)
            .Where(m => subjectIds.Contains(m))
            .GroupBy(m => m)
            .Select(g => new { MonHocId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

        var result = courses
            .Where(c => c.MonHoc != null)
            .Select(course =>
            {
                var total = totalLessons.GetValueOrDefault(course.MaMonHoc);
                var completed = completedCounts.GetValueOrDefault(course.MaMonHoc);
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
                    Id = course.MonHoc!.MaCodeMonHoc,
                    Name = course.MonHoc.TenMonHoc,
                    Code = course.MonHoc.MaCodeMonHoc,
                    Lecturer = course.GiaoVien?.HoTen ?? "Chưa phân công",
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
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        var student = await context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUser.UserId);

        var courseCode = courseId.ToUpper();

        // 1. Tìm KhoaHoc (Khóa học được phân công) dựa trên Lớp của sinh viên và Mã môn học
        var assignedCourse = student?.MaLop != null 
            ? await context.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .Include(k => k.HocKy)
                .FirstOrDefaultAsync(k => k.MaLop == student.MaLop.Value && k.MonHoc!.MaCodeMonHoc == courseCode && k.TrangThai == "da_xuat_ban")
            : null;

        // 2. Nếu không tìm thấy phân công, fallback về DanhMucMonHoc gốc để vẫn xem được đề cương
        var baseSubject = assignedCourse?.MonHoc ?? await context.DanhMucMonHocs.FirstOrDefaultAsync(c => c.MaCodeMonHoc == courseCode);

        if (baseSubject == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy môn học " + courseCode + " trong CSDL." });
        }

        var chapters = await context.Chuongs
            .Include(c => c.BaiHocs)
            .Where(c => c.MaMonHoc == baseSubject.MaMonHoc)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        var teacherName = assignedCourse?.GiaoVien?.HoTen ?? "Chưa phân công giảng viên";
        var semesterName = assignedCourse?.HocKy?.TenHocKy ?? "Chưa xếp học kỳ";

        var response = new CourseDetailResponseDto
        {
            Course = new CourseDetailDto
            {
                Id = baseSubject.MaCodeMonHoc,
                Title = baseSubject.TenMonHoc,
                Code = baseSubject.MaCodeMonHoc,
                Teacher = teacherName,
                Semester = semesterName,
                Credits = baseSubject.SoTinChi,
                CoverGradient = "from-blue-700 via-blue-600 to-cyan-500",
                Description = $"Môn học {baseSubject.TenMonHoc} ({baseSubject.MaCodeMonHoc}) cung cấp các kiến thức cốt lõi và kỹ năng thực hành chuyên sâu."
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
            return Ok(ApiResponseDto<object>.Ok(Array.Empty<object>()));
        }

        return Ok(ApiResponseDto<object>.Ok(comments));
    }
}

using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentCourse;
using Backend.DTOs.StudentDashboard;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/courses")]
public class StudentCoursesController : ControllerBase
{
    private static bool TryParseLessonId(string? lessonId, out int parsedLessonId)
    {
        parsedLessonId = 0;
        if (string.IsNullOrWhiteSpace(lessonId)) return false;
        if (int.TryParse(lessonId, out parsedLessonId)) return true;
        if ((lessonId.StartsWith("l", StringComparison.OrdinalIgnoreCase) || lessonId.StartsWith("L", StringComparison.OrdinalIgnoreCase)) &&
            int.TryParse(lessonId[1..], out parsedLessonId))
        {
            return true;
        }
        return false;
    }

    private static string ResolveMediaUrl(string? rawUrl, Backend.Services.Storage.IR2StorageService? storageService)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return string.Empty;
        if (storageService == null) return rawUrl;

        try
        {
            if (rawUrl.Contains("key="))
            {
                var match = System.Text.RegularExpressions.Regex.Match(rawUrl, @"key=([^&]+)");
                if (match.Success)
                {
                    var rawKey = Uri.UnescapeDataString(match.Groups[1].Value);
                    var directUrl = storageService.GetPresignedStreamUrl(rawKey);
                    if (!string.IsNullOrEmpty(directUrl)) return directUrl;
                }
            }
            else if (rawUrl.StartsWith("videos/", StringComparison.OrdinalIgnoreCase) ||
                     rawUrl.StartsWith("documents/", StringComparison.OrdinalIgnoreCase))
            {
                var directUrl = storageService.GetPresignedStreamUrl(rawUrl);
                if (!string.IsNullOrEmpty(directUrl)) return directUrl;
            }
        }
        catch { /* fallback to rawUrl */ }

        return rawUrl;
    }

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

        // Lấy danh sách khóa học duy nhất theo từng môn học của lớp sinh viên (ưu tiên bản ghi phân công mới nhất)
        var distinctCourses = courses
            .Where(c => c.MonHoc != null)
            .GroupBy(c => c.MaMonHoc)
            .Select(g => g.OrderByDescending(k => k.MaKhoaHoc).First())
            .ToList();

        var subjectIds = distinctCourses
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

        var result = distinctCourses
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
        [FromServices] Backend.Data.ApplicationDbContext context,
        [FromServices] Backend.Services.Storage.IR2StorageService storageService)
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
                .Where(k => k.MaLop == student.MaLop.Value && k.MonHoc!.MaCodeMonHoc == courseCode && k.TrangThai == "da_xuat_ban")
                .OrderByDescending(k => k.MaKhoaHoc)
                .FirstOrDefaultAsync()
            : null;

        // 2. Nếu không tìm thấy phân công, chỉ cho xem đề cương khi mã môn học thật tồn tại.
        var baseSubject = assignedCourse?.MonHoc ?? await context.DanhMucMonHocs.FirstOrDefaultAsync(c => c.MaCodeMonHoc == courseCode);

        if (baseSubject == null)
        {
            return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học."));
        }

        var chapters = await context.Chuongs
            .Include(c => c.BaiHocs.Where(b => b.TrangThai == "da_xuat_ban" || b.TrangThai == "published" || b.BaiHocNoiDungs.Any(n => n.TrangThai == "da_xuat_ban" || n.TrangThai == "published")))
            .Where(c => c.MaMonHoc == baseSubject.MaMonHoc)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        var studentProgress = await context.TienDoBaiHocs
            .AsNoTracking()
            .Where(t => t.MaHocSinh == currentUser.UserId)
            .ToDictionaryAsync(t => t.MaBaiHoc, t => t);

        var allBaiHocs = chapters.SelectMany(c => c.BaiHocs).ToList();
        var totalLessonCount = allBaiHocs.Count;
        var completedLessonCount = allBaiHocs.Count(b =>
            studentProgress.TryGetValue(b.MaBaiHoc, out var tp) && tp.PhanTramTienDo >= 100);

        var totalProgressSum = allBaiHocs.Sum(b => (double)(studentProgress.GetValueOrDefault(b.MaBaiHoc)?.PhanTramTienDo ?? 0m));
        var overallProgressPercent = totalLessonCount > 0 ? (int)Math.Round(totalProgressSum / totalLessonCount) : 0;

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
                new() { Label = "Tiến độ", Value = $"{overallProgressPercent}", Unit = "%", Icon = "Gauge", Tone = "blue", Progress = overallProgressPercent, Hint = $"{completedLessonCount}/{totalLessonCount} bài đã hoàn thành" },
                new() { Label = "Bài học", Value = $"{completedLessonCount}", Unit = $"/{totalLessonCount}", Icon = "BookOpenCheck", Tone = "green", Progress = overallProgressPercent, Hint = $"Đã hoàn thành {completedLessonCount} bài" },
                new() { Label = "Bài tập", Value = "2", Unit = "mục", Icon = "ClipboardList", Tone = "orange", Progress = 80, Hint = "1 bài gần đến hạn" },
                new() { Label = "Tài liệu", Value = "18", Unit = "file", Icon = "Files", Tone = "violet", Progress = 60, Hint = "PDF, video, quiz" }
            },
            Lessons = chapters.Select(ch =>
            {
                var chLessons = ch.BaiHocs.OrderBy(b => b.ThuTu).ToList();
                var chCompleted = chLessons.Count(b => studentProgress.TryGetValue(b.MaBaiHoc, out var tp) && tp.PhanTramTienDo >= 100);
                var chTotal = chLessons.Count;
                var chProgressSum = chLessons.Sum(b => (double)(studentProgress.GetValueOrDefault(b.MaBaiHoc)?.PhanTramTienDo ?? 0m));
                var chProgress = chTotal > 0 ? (int)Math.Round(chProgressSum / chTotal) : 0;
                var isChapterDone = chTotal > 0 && chCompleted == chTotal;

                var rawTitle = ch.TieuDe ?? string.Empty;
                var cleanTitle = System.Text.RegularExpressions.Regex.Replace(rawTitle, @"^(Chương|Phần|Bài)\s*\d+\s*[:\-]\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                if (string.IsNullOrEmpty(cleanTitle)) cleanTitle = rawTitle;

                return new CourseChapterDto
                {
                    Id = "ch" + ch.MaChuong,
                    Chapter = "Chương " + ch.ThuTu,
                    Title = cleanTitle,
                    Description = "",
                    Status = isChapterDone ? "completed" : "active",
                    Badge = isChapterDone ? "Hoàn thành" : "Đang học",
                    Tone = isChapterDone ? "green" : "blue",
                    Icon = isChapterDone ? "CheckCircle2" : "ListTree",
                    Meta = new List<string> { $"{chTotal} bài học" },
                    Progress = chProgress,
                    Lessons = chLessons.Select(b =>
                    {
                        var prog = studentProgress.GetValueOrDefault(b.MaBaiHoc);
                        var progVal = (int)(prog?.PhanTramTienDo ?? 0m);
                        var isDone = progVal >= 100;
                        var isSeekDisabled = b.DieuKienMoKhoa != null && (b.DieuKienMoKhoa.Contains("\"allowSeek\":false") || b.DieuKienMoKhoa.Contains("khoa_tua") || b.DieuKienMoKhoa.Contains("no_seek"));
                        return new CourseLessonDto
                        {
                            Id = "l" + b.MaBaiHoc,
                            Title = b.TieuDe,
                            Duration = b.ThoiLuongGiay.HasValue && b.ThoiLuongGiay.Value > 0 ? TimeSpan.FromSeconds(b.ThoiLuongGiay.Value).ToString(@"mm\:ss") : "15:00",
                            Status = isDone ? "completed" : "active",
                            ProgressPercent = isDone ? 100 : progVal,
                            Type = b.LoaiBaiHoc == "trac_nghiem" ? "quiz" : b.LoaiBaiHoc == "van_ban" || b.LoaiBaiHoc == "pdf" || b.LoaiBaiHoc == "slide_html" ? "document" : "video",
                            Url = ResolveMediaUrl(b.UrlTapTin, storageService),
                            AllowSeek = !isSeekDisabled
                        };
                    }).ToList()
                };
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
        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        // Tìm content block có quiz — fallback cả hai loại "quiz" và "trac_nghiem"
        var lessonContent = await context.BaiHocNoiDungs
            .FirstOrDefaultAsync(n => n.MaBaiHoc == parsedLessonId
                && (n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null)
                && (n.TrangThai == "da_xuat_ban" || n.TrangThai == "published" || n.TrangThai == "dang_mo" || n.TrangThai == "active" || n.TrangThai == "hoat_dong" || n.TrangThai == null));

        if (lessonContent?.MaDeKiemTra == null)
        {
            return Ok(ApiResponseDto<object>.Ok(new List<object>()));
        }

        var quizEntry = await context.DeKiemTras
            .FirstOrDefaultAsync(d => d.MaDeKiemTra == lessonContent.MaDeKiemTra.Value);

        var quizQuestions = await context.CauHoiDeKiemTras
            .Include(q => q.CauHoi)
            .Where(q => q.MaDeKiemTra == lessonContent.MaDeKiemTra.Value && q.CauHoi != null && q.CauHoi.LoaiCauHoi != "tu_luan")
            .OrderBy(q => q.ThuTu)
            .ToListAsync();

        // Parse CauHinhDeThi để lấy thông tin điểm đạt và xáo trộn
        var cauHinh = new { diemDat = 5, tongDiem = 10, cachTinhDat = "phai_dat", xaoTronCauHoi = false, xaoTronDapAn = false };
        if (!string.IsNullOrEmpty(quizEntry?.CauHinhDeThi))
        {
            try
            {
                using var doc = JsonDocument.Parse(quizEntry.CauHinhDeThi);
                var root = doc.RootElement;
                int diemDat = root.TryGetProperty("diemDat", out var dd) ? dd.GetInt32() :
                              root.TryGetProperty("DiemDat", out dd) ? dd.GetInt32() : 5;
                int tongDiem = root.TryGetProperty("tongDiem", out var td) ? td.GetInt32() :
                               root.TryGetProperty("TongDiem", out td) ? td.GetInt32() : 10;
                string cachTinh = root.TryGetProperty("cachTinhDat", out var ct) ? ct.GetString() ?? "phai_dat" :
                                  root.TryGetProperty("CachTinhDat", out ct) ? ct.GetString() ?? "phai_dat" : "phai_dat";
                bool xaoTronCauHoi = root.TryGetProperty("xaoTronCauHoi", out var sq) ? sq.GetBoolean() :
                                    root.TryGetProperty("XaoTronCauHoi", out sq) ? sq.GetBoolean() :
                                    root.TryGetProperty("shuffleQuestions", out sq) ? sq.GetBoolean() : false;
                bool xaoTronDapAn = root.TryGetProperty("xaoTronDapAn", out var sa) ? sa.GetBoolean() :
                                  root.TryGetProperty("XaoTronDapAn", out sa) ? sa.GetBoolean() :
                                  root.TryGetProperty("shuffleAnswers", out sa) ? sa.GetBoolean() : false;
                cauHinh = new { diemDat, tongDiem, cachTinhDat = cachTinh, xaoTronCauHoi, xaoTronDapAn };
            }
            catch { /* dùng giá trị mặc định */ }
        }

        // Xáo trộn thứ tự câu hỏi nếu xaoTronCauHoi = true
        if (cauHinh.xaoTronCauHoi && quizQuestions.Count > 1)
        {
            var rng = new Random();
            quizQuestions = quizQuestions.OrderBy(_ => rng.Next()).ToList();
        }

        var result = quizQuestions.Select(q =>
        {
            // LuaChon được lưu dạng JSON array of objects: [{"id":"A","content":"..."},...]
            // Cần extract text từ field "content" của mỗi option
            string[] options = [];
            try
            {
                using var doc = JsonDocument.Parse(q.CauHoi?.LuaChon ?? "[]");
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    options = doc.RootElement.EnumerateArray().Select(el =>
                    {
                        if (el.ValueKind == JsonValueKind.String)
                            return el.GetString() ?? "";
                        // Dạng object {id, content}
                        if (el.TryGetProperty("content", out var c)) return c.GetString() ?? "";
                        if (el.TryGetProperty("Content", out c)) return c.GetString() ?? "";
                        return el.ToString();
                    }).ToArray();
                }
            }
            catch
            {
                options = (q.CauHoi?.LuaChon ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            // DapAnDung lưu dạng JSON array: ["A"] hoặc ["A","B"]
            // Cần map từ letter (A,B,C,D) sang index (0,1,2,3) cho frontend
            int correctIndex = -1;
            List<string> correctIds = [];
            try
            {
                using var doc = JsonDocument.Parse(q.CauHoi?.DapAnDung ?? "[]");
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    correctIds = doc.RootElement.EnumerateArray()
                        .Select(el => el.GetString() ?? "")
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
                // Map letter A->0, B->1, ...
                if (correctIds.Count > 0)
                {
                    var firstId = correctIds[0].ToUpperInvariant();
                    if (firstId.Length == 1 && firstId[0] >= 'A')
                        correctIndex = firstId[0] - 'A';
                    else
                        int.TryParse(firstId, out correctIndex);
                }
            }
            catch
            {
                int.TryParse(q.CauHoi?.DapAnDung, out correctIndex);
            }

            var correctIndices = correctIds.Select(id =>
            {
                var u = id.Trim().ToUpperInvariant();
                if (u.Length == 1 && u[0] >= 'A' && u[0] <= 'Z') return u[0] - 'A';
                if (int.TryParse(u, out int val)) return val;
                return -1;
            }).Where(idx => idx >= 0).ToList();

            // Xáo trộn đáp án nếu xaoTronDapAn = true
            if (cauHinh.xaoTronDapAn && options.Length > 1)
            {
                var rngOpt = new Random();
                var indexedOptions = options.Select((text, idx) => (text, origIdx: idx)).OrderBy(_ => rngOpt.Next()).ToList();
                options = indexedOptions.Select(x => x.text).ToArray();

                // Cập nhật lại correctIndices & correctIndex theo vị trí mới sau khi xáo trộn
                correctIndices = correctIndices
                    .Select(origIdx => indexedOptions.FindIndex(x => x.origIdx == origIdx))
                    .Where(i => i >= 0)
                    .ToList();
                correctIndex = correctIndices.Count > 0 ? correctIndices[0] : -1;
            }

            var kieu = q.CauHoi?.KieuLuaChon ?? "";
            return new
            {
                Id = "q" + q.MaCauHoi,
                Text = q.CauHoi?.NoiDung ?? "",
                QuestionType = "trac_nghiem",
                Type = (kieu == "chon_nhieu" || kieu == "multiple") ? "multiple" : "single",
                Options = options
            };
        }).ToList();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            quizId = lessonContent.MaDeKiemTra,
            title = quizEntry?.TieuDe ?? "",
            durationMinutes = quizEntry?.ThoiGianPhut ?? 15,
            passScore = cauHinh.diemDat,
            totalScore = cauHinh.tongDiem,
            completionRule = cauHinh.cachTinhDat,
            questions = result
        }));
    }

    [HttpGet("{courseId}/lessons/{lessonId}/content")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonContent(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context,
        [FromServices] Backend.Services.Storage.IR2StorageService storageService)
    {
        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        // Trả về các content blocks đã xuất bản cho sinh viên
        var contents = await context.BaiHocNoiDungs
            .Where(n => n.MaBaiHoc == parsedLessonId && (n.TrangThai == "da_xuat_ban" || n.TrangThai == "published"))
            .OrderBy(n => n.ThuTu)
            .ToListAsync();

        var result = contents.Select(c => new
        {
            Id = c.MaNoiDung,
            Type = c.LoaiNoiDung,   // video, slide_html, tai_lieu, quiz, trac_nghiem, van_ban
            Title = c.LoaiNoiDung == "slide_html" ? "Slide HTML" : (c.LoaiNoiDung == "video" ? "Video bài học" : "Tài liệu bài học"),
            VideoUrl = c.LoaiNoiDung == "video" ? ResolveMediaUrl(c.UrlTapTin, storageService) : null,
            DocumentUrl = (c.LoaiNoiDung == "tai_lieu" || c.LoaiNoiDung == "pdf" || c.LoaiNoiDung == "document") ? ResolveMediaUrl(c.UrlTapTin, storageService) : null,
            SlideHtml = c.LoaiNoiDung == "slide_html" ? c.NoiDungHtml : null,
            NoiDungJson = c.NoiDungJson,
            QuizId = c.MaDeKiemTra,
            DurationSeconds = c.ThoiLuongGiay,
            Order = c.ThuTu
        }).ToList();

        return Ok(ApiResponseDto<object>.Ok(result));
    }

    [HttpGet("{courseId}/lessons/{lessonId}/comments")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonComments(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

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

    [HttpPost("{courseId}/lessons/{lessonId}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> CompleteLesson(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context,
        [FromQuery] int percent = 100)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        var existing = await context.TienDoBaiHocs
            .FirstOrDefaultAsync(t => t.MaHocSinh == currentUser.UserId && t.MaBaiHoc == parsedLessonId);

        if (existing == null)
        {
            context.TienDoBaiHocs.Add(new TienDoBaiHoc
            {
                MaHocSinh = currentUser.UserId,
                MaBaiHoc = parsedLessonId,
                PhanTramTienDo = percent,
                HoanThanhLuc = percent >= 100 ? DateTime.UtcNow : null
            });
        }
        else
        {
            existing.PhanTramTienDo = Math.Max((int)existing.PhanTramTienDo, percent);
            if (existing.PhanTramTienDo >= 100)
            {
                existing.HoanThanhLuc ??= DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
        return Ok(ApiResponseDto<object>.Ok(new { message = "Lưu tiến độ bài học thành công." }));
    }

    [HttpPost("{courseId}/reset-progress")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> ResetCourseProgress(
        string courseId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        var baseSubject = await context.DanhMucMonHocs
            .FirstOrDefaultAsync(m => m.MaCodeMonHoc == courseId || m.MaMonHoc.ToString() == courseId);

        if (baseSubject == null) return NotFound();

        var lessonIds = await context.BaiHocs
            .Where(b => b.Chuong != null && b.Chuong.MaMonHoc == baseSubject.MaMonHoc)
            .Select(b => b.MaBaiHoc)
            .ToListAsync();

        var records = await context.TienDoBaiHocs
            .Where(t => t.MaHocSinh == currentUser.UserId && lessonIds.Contains(t.MaBaiHoc))
            .ToListAsync();

        context.TienDoBaiHocs.RemoveRange(records);

        var gradeRecords = await context.DiemSos
            .Where(d => d.MaHocSinh == currentUser.UserId && d.MaMonHoc == baseSubject.MaMonHoc)
            .ToListAsync();

        context.DiemSos.RemoveRange(gradeRecords);

        await context.SaveChangesAsync();

        return Ok(ApiResponseDto<object>.Ok(new { message = $"Đã reset tiến độ môn {courseId} về 0%" }));
    }
}

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
                    Lecturer = course.GiaoVien?.HoTen ?? "Giảng viên phụ trách",
                    Credits = course.MonHoc.SoTinChi,
                    Semester = course.HocKy?.TenHocKy ?? "Học kỳ 1 năm 2026",
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
            .Include(c => c.BaiHocs.Where(b => !b.DaAn && (b.TrangThai == "da_xuat_ban" || b.TrangThai == "published" || b.TrangThai == "dang_mo" || b.TrangThai == "active" || b.TrangThai == "hoat_dong" || b.BaiHocNoiDungs.Any(n => n.TrangThai == "da_xuat_ban" || n.TrangThai == "published" || n.TrangThai == "dang_mo"))))
                .ThenInclude(b => b.BaiHocNoiDungs)
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
                        var rawVideoUrl = b.UrlTapTin ?? b.BaiHocNoiDungs?.Where(n => n.LoaiNoiDung == "video" && n.UrlTapTin != null).Select(n => n.UrlTapTin).FirstOrDefault();
                        var rawDocUrl = b.BaiHocNoiDungs?.Where(n => (n.LoaiNoiDung == "tai_lieu" || n.LoaiNoiDung == "pdf" || n.LoaiNoiDung == "document") && n.UrlTapTin != null).Select(n => n.UrlTapTin).FirstOrDefault();
                        var hasVid = !string.IsNullOrEmpty(rawVideoUrl) || b.LoaiBaiHoc == "video" || (b.BaiHocNoiDungs != null && b.BaiHocNoiDungs.Any(n => n.LoaiNoiDung == "video"));
                        var hasDoc = !string.IsNullOrEmpty(rawDocUrl) || b.LoaiBaiHoc == "tai_lieu" || b.LoaiBaiHoc == "pdf" || b.LoaiBaiHoc == "document" || b.LoaiBaiHoc == "van_ban";
                        var hasSlide = b.LoaiBaiHoc == "slide_html" || b.LoaiBaiHoc == "slide" || (b.BaiHocNoiDungs != null && b.BaiHocNoiDungs.Any(n => n.LoaiNoiDung == "slide_html"));
                        var hasQuiz = b.LoaiBaiHoc == "quiz" || b.LoaiBaiHoc == "trac_nghiem" || (b.BaiHocNoiDungs != null && b.BaiHocNoiDungs.Any(n => n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null));

                        var resolvedType = hasVid ? "video" : (hasDoc ? "document" : (hasSlide ? "slide" : (hasQuiz ? "quiz" : (b.LoaiBaiHoc == "trac_nghiem" ? "quiz" : b.LoaiBaiHoc == "slide_html" ? "slide" : "video"))));

                        return new CourseLessonDto
                        {
                            Id = "l" + b.MaBaiHoc,
                            Title = b.TieuDe,
                            Duration = b.ThoiLuongGiay.HasValue && b.ThoiLuongGiay.Value > 0 ? TimeSpan.FromSeconds(b.ThoiLuongGiay.Value).ToString(@"mm\:ss") : "15:00",
                            DurationSeconds = b.ThoiLuongGiay.GetValueOrDefault(),
                            Status = isDone ? "completed" : "active",
                            ProgressPercent = isDone ? 100 : progVal,
                            Type = resolvedType,
                            Url = ResolveMediaUrl(rawVideoUrl ?? rawDocUrl, storageService),
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
                && (n.TrangThai == "da_xuat_ban" || n.TrangThai == "published" || n.TrangThai == "dang_mo" || n.TrangThai == "active" || n.TrangThai == "hoat_dong" || n.TrangThai == "nhap" || n.TrangThai == null));

        DeKiemTra? quizEntry = null;
        if (lessonContent?.MaDeKiemTra != null)
        {
            quizEntry = await context.DeKiemTras
                .FirstOrDefaultAsync(d => d.MaDeKiemTra == lessonContent.MaDeKiemTra.Value);
        }
        else
        {
            // Fallback: Tìm đề thi trắc nghiệm của môn học nếu chưa được gán tường minh vào BaiHocNoiDungs
            var baiHoc = await context.BaiHocs.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.MaBaiHoc == parsedLessonId);
            if (baiHoc?.Chuong != null)
            {
                quizEntry = await context.DeKiemTras
                    .Where(d => d.MaMonHoc == baiHoc.Chuong.MaMonHoc && (d.TrangThai == "dang_mo" || d.TrangThai == "da_xuat_ban" || d.TrangThai == "published"))
                    .OrderByDescending(d => d.MaDeKiemTra)
                    .FirstOrDefaultAsync();
            }
        }

        if (quizEntry == null)
        {
            return Ok(ApiResponseDto<object>.Ok(new List<object>()));
        }

        var targetQuizId = quizEntry.MaDeKiemTra;

        var quizQuestions = await context.CauHoiDeKiemTras
            .Include(q => q.CauHoi)
            .Where(q => q.MaDeKiemTra == targetQuizId && q.CauHoi != null && q.CauHoi.LoaiCauHoi != "tu_luan")
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
                Options = options,
                DiemSo = q.DiemSo,
                Points = q.DiemSo
            };
        }).ToList();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            quizId = targetQuizId,
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
            .Where(n => n.MaBaiHoc == parsedLessonId && (n.TrangThai == "da_xuat_ban" || n.TrangThai == "published" || n.TrangThai == "dang_mo" || n.TrangThai == "active" || n.TrangThai == "hoat_dong" || n.TrangThai == "nhap" || n.TrangThai == null))
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
        [FromServices] Backend.Data.ApplicationDbContext context,
        [FromServices] Backend.Services.Comments.ICommentLikeService likeService)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var currentUserId = currentUser?.UserId ?? 0;

        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        var rootComments = await context.BinhLuans
            .Include(b => b.NguoiDung)
            .Where(b => b.MaBaiHoc == parsedLessonId && b.MaBinhLuanCha == null && !b.DaGhim)
            .OrderByDescending(b => b.NgayTao)
            .ToListAsync();

        var rootIds = rootComments.Select(b => b.MaBinhLuan).ToList();
        var replies = await context.BinhLuans
            .Include(b => b.NguoiDung)
            .Where(b => b.MaBinhLuanCha != null && rootIds.Contains(b.MaBinhLuanCha.Value) && !b.DaGhim)
            .OrderBy(b => b.NgayTao)
            .ToListAsync();

        var repliesByParent = replies.GroupBy(r => r.MaBinhLuanCha!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = rootComments.Select(b =>
        {
            var authorName = b.NguoiDung?.HoTen ?? ("Sinh viên " + b.MaNguoiDung);
            var isTeacher = b.NguoiDung?.VaiTroChinh == "giao_vien" || b.NguoiDung?.VaiTroChinh == "Teacher";
            var initials = !string.IsNullOrWhiteSpace(b.NguoiDung?.HoTen)
                ? string.Concat(b.NguoiDung.HoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries).TakeLast(2).Select(s => s[0])).ToUpper()
                : (isTeacher ? "GV" : "SV");

            var childReplies = repliesByParent.GetValueOrDefault(b.MaBinhLuan, new List<BinhLuan>())
                .Select(r =>
                {
                    var rAuthor = r.NguoiDung?.HoTen ?? ("Người dùng " + r.MaNguoiDung);
                    var rIsTeacher = r.NguoiDung?.VaiTroChinh == "giao_vien" || r.NguoiDung?.VaiTroChinh == "Teacher";
                    var rInitials = !string.IsNullOrWhiteSpace(r.NguoiDung?.HoTen)
                        ? string.Concat(r.NguoiDung.HoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries).TakeLast(2).Select(s => s[0])).ToUpper()
                        : (rIsTeacher ? "GV" : "SV");

                    return new
                    {
                        Id = "c" + r.MaBinhLuan,
                        MaBinhLuan = r.MaBinhLuan,
                        Author = rAuthor,
                        Initials = rInitials,
                        Role = rIsTeacher ? "teacher" : "student",
                        Content = r.NoiDung,
                        TimeAgo = r.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                        CreatedAt = r.NgayTao
                    };
                }).ToList();

            return new
            {
                Id = "c" + b.MaBinhLuan,
                MaBinhLuan = b.MaBinhLuan,
                Author = authorName,
                Initials = initials,
                Role = isTeacher ? "teacher" : "student",
                Content = b.NoiDung,
                TimeAgo = b.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                CreatedAt = b.NgayTao,
                Likes = likeService.GetLikesCount(b.MaBinhLuan),
                IsLiked = likeService.HasUserLiked(b.MaBinhLuan, currentUserId),
                Replies = childReplies
            };
        }).ToList();

        return Ok(ApiResponseDto<object>.Ok(result));
    }

    [HttpPost("{courseId}/lessons/{lessonId}/comments/{commentId}/like")]
    [Authorize(Roles = "Student,Teacher")]
    public ActionResult<ApiResponseDto<object>> ToggleCommentLike(
        string courseId, string lessonId, int commentId,
        [FromServices] Backend.Services.Comments.ICommentLikeService likeService)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        var (likesCount, isLiked) = likeService.ToggleLike(commentId, currentUser.UserId);
        return Ok(ApiResponseDto<object>.Ok(new { Likes = likesCount, IsLiked = isLiked }));
    }

    [HttpPost("{courseId}/lessons/{lessonId}/comments")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> CreateLessonComment(
        string courseId, string lessonId,
        [FromBody] CreateLessonCommentRequestDto request,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponseDto.Fail("Nội dung thảo luận không được để trống."));
        }

        var baiHoc = await context.BaiHocs
            .Include(b => b.Chuong)
            .FirstOrDefaultAsync(b => b.MaBaiHoc == parsedLessonId);

        if (baiHoc == null)
        {
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài học."));
        }

        var studentUser = await context.NguoiDungs
            .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUser.UserId);

        var comment = new BinhLuan
        {
            MaBaiHoc = parsedLessonId,
            MaNguoiDung = currentUser.UserId,
            NoiDung = request.Content.Trim(),
            MaBinhLuanCha = request.ParentId,
            NgayTao = DateTime.UtcNow,
            DaGhim = false
        };

        context.BinhLuans.Add(comment);
        await context.SaveChangesAsync();

        // Gửi thông báo cho Giảng viên phụ trách môn học
        if (baiHoc.Chuong != null)
        {
            var monHocId = baiHoc.Chuong.MaMonHoc;
            var course = await context.KhoaHocs
                .FirstOrDefaultAsync(k => k.MaMonHoc == monHocId && (studentUser == null || k.MaLop == studentUser.MaLop));

            var teacherId = course?.MaGiaoVien;
            if (teacherId.HasValue && teacherId.Value != currentUser.UserId)
            {
                var studentName = studentUser?.HoTen ?? "Một sinh viên";
                var thongBao = new ThongBao
                {
                    MaNhomThongBao = Guid.NewGuid(),
                    MaNguoiNhan = teacherId.Value,
                    MaDonVi = studentUser?.MaDonVi ?? 1,
                    TieuDe = $"Sinh viên {studentName} vừa thảo luận trong bài học",
                    TomTat = $"Sinh viên {studentName} vừa đăng thảo luận trong bài học \"{baiHoc.TieuDe}\"",
                    NoiDung = $"Sinh viên {studentName} vừa đăng thảo luận trong bài học \"{baiHoc.TieuDe}\": \"{request.Content.Trim()}\"",
                    NoiDungText = $"Sinh viên {studentName} vừa đăng thảo luận trong bài học \"{baiHoc.TieuDe}\": \"{request.Content.Trim()}\"",
                    LoaiThongBao = "hoc_vu",
                    PhamViGui = "nguoi_dung",
                    NgayTao = DateTime.UtcNow,
                    NguoiTao = currentUser.UserId
                };
                context.ThongBaos.Add(thongBao);
                await context.SaveChangesAsync();

                context.ThongBaoNguoiNhans.Add(new ThongBaoNguoiNhan
                {
                    MaThongBao = thongBao.MaThongBao,
                    MaNguoiNhan = teacherId.Value,
                    MaDonVi = studentUser?.MaDonVi ?? 1,
                    DaDoc = false,
                    NhanLuc = DateTime.UtcNow,
                    NgayTao = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        var authorName = studentUser?.HoTen ?? ("Sinh viên " + currentUser.UserId);
        var initials = !string.IsNullOrWhiteSpace(studentUser?.HoTen)
            ? string.Concat(studentUser.HoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries).TakeLast(2).Select(s => s[0])).ToUpper()
            : "SV";

        var responseData = new
        {
            Id = "c" + comment.MaBinhLuan,
            MaBinhLuan = comment.MaBinhLuan,
            Author = authorName,
            Initials = initials,
            Role = "student",
            Content = comment.NoiDung,
            TimeAgo = "Vừa xong",
            CreatedAt = comment.NgayTao,
            Likes = 0,
            IsLiked = false,
            Replies = new List<object>()
        };

        return Ok(ApiResponseDto<object>.Ok(responseData));
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

        percent = Math.Clamp(percent, 0, 100);

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

    [HttpGet("{courseId}/lessons/{lessonId}/note")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetLessonNote(
        string courseId, string lessonId,
        [FromServices] Backend.Data.ApplicationDbContext context)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        if (!TryParseLessonId(lessonId, out int parsedLessonId))
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài học không hợp lệ."));
        }

        var progress = await context.TienDoBaiHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.MaHocSinh == currentUser.UserId && t.MaBaiHoc == parsedLessonId);

        return Ok(ApiResponseDto<object>.Ok(new { note = progress?.GhiChu ?? "" }));
    }

    [HttpPost("{courseId}/lessons/{lessonId}/note")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<object>>> SaveLessonNote(
        string courseId, string lessonId,
        [FromBody] SaveLessonNoteRequestDto request,
        [FromServices] Backend.Data.ApplicationDbContext context)
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
                PhanTramTienDo = 0,
                GhiChu = request.Note ?? ""
            });
        }
        else
        {
            existing.GhiChu = request.Note ?? "";
        }

        await context.SaveChangesAsync();
        return Ok(ApiResponseDto<object>.Ok(new { message = "Lưu ghi chú thành công.", note = request.Note ?? "" }));
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

public class SaveLessonNoteRequestDto
{
    public string? Note { get; set; }
}

public class CreateLessonCommentRequestDto
{
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

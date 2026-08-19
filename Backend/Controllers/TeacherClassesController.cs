using System.Text.Json;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Attendance;
using Backend.DTOs.Grading;
using Backend.DTOs.QuizManagement;
using Backend.Models;
using Backend.Services.Grading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher,giao_vien")]
public class TeacherClassesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IGradeAggregationService _gradeService;

    public TeacherClassesController(ApplicationDbContext context, IGradeAggregationService gradeService)
    {
        _context = context;
        _gradeService = gradeService;
    }

    [HttpGet("classes")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClasses([FromQuery] string? semesterId = null, [FromQuery] string? keyword = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var query = _context.KhoaHocs
                .Include(k => k.Lop)
                .Include(k => k.MonHoc)
                .Include(k => k.HocKy)
                .Where(k => k.MaGiaoVien == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(semesterId) && int.TryParse(semesterId, out int semId))
            {
                query = query.Where(k => k.MaHocKy == semId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(k => k.TieuDe.Contains(keyword) || (k.Lop != null && k.Lop.TenLop.Contains(keyword)) || (k.MonHoc != null && k.MonHoc.TenMonHoc.Contains(keyword)));
            }

            var courses = await query.ToListAsync();
            var classIds = courses.Select(k => k.MaLop).Distinct().ToList();
            var studentCounts = await _context.NguoiDungs
                .Where(n => n.MaLop != null && classIds.Contains(n.MaLop.Value))
                .GroupBy(n => n.MaLop!.Value)
                .Select(g => new { ClassId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClassId, x => x.Count);

            var result = courses
                .GroupBy(k => new { k.MaLop, TenLop = k.Lop != null ? k.Lop.TenLop : "" })
                .Select(g => new
                {
                    ClassId = g.Key.MaLop,
                    ClassName = g.Key.TenLop,
                    CourseCount = g.Count(),
                    StudentCount = studentCounts.TryGetValue(g.Key.MaLop, out int count) ? count : 0
                })
                .ToList();

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách lớp: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/attendance")]
    public async Task<ActionResult<ApiResponseDto<ClassAttendanceSummaryDto>>> GetClassAttendance(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            // 1. Lấy danh sách các KhoaHoc thuộc lớp id hoặc course id do giáo viên này phụ trách
            var khoaHocs = await _context.KhoaHocs
                .Where(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId)
                .ToListAsync();

            if (!khoaHocs.Any())
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp học hoặc bạn không được phân công giảng dạy lớp này."));

            int classId = khoaHocs.First().MaLop;
            var courseIds = khoaHocs.Select(k => k.MaKhoaHoc).ToList();

            // 2. Lấy danh sách các BuoiHoc đã diễn ra của các khóa học này
            var completedSessions = await _context.BuoiHocs
                .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.TrangThaiBuoi == "da_dien_ra")
                .Select(b => b.MaBuoiHoc)
                .ToListAsync();

            int totalSessions = completedSessions.Count;

            // 3. Lấy danh sách sinh viên trong lớp
            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .ToListAsync();

            // 4. Lấy dữ liệu điểm danh
            var diemDanhs = await _context.DiemDanhs
                .Where(d => completedSessions.Contains(d.MaBuoiHoc) && d.MaHocSinh != null)
                .ToListAsync();

            var resultStudents = new List<ClassAttendanceStudentDto>();

            foreach (var student in students)
            {
                var studentDiemDanhs = diemDanhs.Where(d => d.MaHocSinh == student.MaNguoiDung).ToList();
                int present = studentDiemDanhs.Count(d => d.TrangThai == "co_mat" || d.TrangThai == "di_muon");
                int absent = studentDiemDanhs.Count(d => d.TrangThai == "vang" || d.TrangThai == "vang_co_phep" || d.TrangThai == "co_phep");
                
                int percent = totalSessions > 0 ? (int)Math.Round((double)present / totalSessions * 100) : 0;
                
                string status = "good";
                if (totalSessions > 0)
                {
                    if (percent < 50) status = "danger";
                    else if (percent < 70) status = "warning";
                    else if (percent >= 90) status = "excellent";
                }

                resultStudents.Add(new ClassAttendanceStudentDto
                {
                    Id = student.MaNguoiDung,
                    Name = student.HoTen,
                    Present = present,
                    Absent = absent,
                    Percent = percent,
                    Status = status
                });
            }

            var result = new ClassAttendanceSummaryDto
            {
                TotalSessions = totalSessions,
                Students = resultStudents
            };

            return Ok(ApiResponseDto<ClassAttendanceSummaryDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chuyên cần lớp học: " + ex.Message));
        }
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetCourses([FromQuery] string? semesterId = null, [FromQuery] string? keyword = null, [FromQuery] int? classId = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var query = _context.KhoaHocs
                .Include(k => k.Lop)
                .Include(k => k.MonHoc)
                .Include(k => k.HocKy)
                .Where(k => k.MaGiaoVien == userId)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(k => k.MaLop == classId.Value);
            }

            if (!string.IsNullOrEmpty(semesterId) && int.TryParse(semesterId, out int semId))
            {
                query = query.Where(k => k.MaHocKy == semId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(k => k.TieuDe.Contains(keyword) || (k.Lop != null && k.Lop.TenLop.Contains(keyword)) || (k.MonHoc != null && k.MonHoc.TenMonHoc.Contains(keyword)));
            }

            var courses = await query
                .Select(k => new
                {
                    CourseId = k.MaKhoaHoc,
                    SubjectId = k.MaMonHoc,
                    CourseName = k.TieuDe,
                    SubjectCode = k.MonHoc != null ? k.MonHoc.MaCodeMonHoc : "",
                    SubjectName = k.MonHoc != null ? k.MonHoc.TenMonHoc : k.TieuDe,
                    ClassName = k.Lop != null ? k.Lop.TenLop : "",
                    ClassId = k.MaLop,
                    StudentCount = _context.NguoiDungs.Count(n => n.MaLop == k.MaLop),
                    Semester = k.HocKy != null ? k.HocKy.TenHocKy : "Học kỳ 1 năm 2026"
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(courses));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách khóa học: " + ex.Message));
        }
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetSubjects([FromQuery] string? keyword = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var teacherCourses = await _context.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Include(k => k.HocKy)
                .Where(k => k.MaGiaoVien == userId)
                .ToListAsync();

            var subjects = teacherCourses
                .GroupBy(k => k.MaMonHoc)
                .Select(g => {
                    var first = g.First();
                    var monHoc = first.MonHoc;
                    var classNames = g.Select(k => k.Lop?.TenLop).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
                    var classIds = g.Select(k => (int?)k.MaLop).Distinct().ToList();
                    var studentCount = _context.NguoiDungs.Count(n => classIds.Contains(n.MaLop) && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"));
                    var lessonCount = _context.Chuongs.Where(c => c.MaMonHoc == g.Key && !c.DaAn).SelectMany(c => c.BaiHocs.Where(b => !b.DaAn)).Count();

                    return new
                    {
                        SubjectId = g.Key,
                        CourseId = first.MaKhoaHoc,
                        SubjectCode = monHoc?.MaCodeMonHoc ?? ("MH" + g.Key),
                        SubjectName = monHoc?.TenMonHoc ?? first.TieuDe,
                        CourseName = monHoc?.TenMonHoc ?? first.TieuDe,
                        ClassName = classNames.Count > 0 ? string.Join(", ", classNames) : "Chưa có lớp",
                        ClassCount = classNames.Count,
                        StudentCount = studentCount,
                        LessonCount = lessonCount,
                        Semester = first.HocKy?.TenHocKy ?? "Học kỳ 1 năm 2026"
                    };
                })
                .ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                var k = keyword.ToLower();
                subjects = subjects.Where(s => s.SubjectCode.ToLower().Contains(k) || s.SubjectName.ToLower().Contains(k)).ToList();
            }

            return Ok(ApiResponseDto<object>.Ok(subjects));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách môn học: " + ex.Message));
        }
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
        catch { /* fallback */ }

        return rawUrl;
    }

    [HttpGet("subjects/{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetSubjectLessonsDetail(
        string id,
        [FromServices] Backend.Services.Storage.IR2StorageService storageService)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            int.TryParse(id, out int numericId);
            var cleanId = (id ?? "").Trim().ToLower();

            var monHoc = await _context.DanhMucMonHocs.FirstOrDefaultAsync(m =>
                m.MaCodeMonHoc.ToLower() == cleanId || (numericId > 0 && m.MaMonHoc == numericId));

            if (monHoc == null && numericId > 0)
            {
                var kh = await _context.KhoaHocs.Include(k => k.MonHoc).FirstOrDefaultAsync(k => k.MaKhoaHoc == numericId);
                if (kh?.MonHoc != null) monHoc = kh.MonHoc;
            }

            if (monHoc == null)
            {
                return NotFound(ApiResponseDto.Fail($"Không tìm thấy môn học với mã hoặc định danh '{id}'."));
            }

            int monHocId = monHoc.MaMonHoc;

            var teacherKhoaHocs = await _context.KhoaHocs
                .Include(k => k.Lop)
                .Where(k => k.MaMonHoc == monHocId && k.MaGiaoVien == userId)
                .ToListAsync();

            if (!teacherKhoaHocs.Any())
            {
                teacherKhoaHocs = await _context.KhoaHocs
                    .Include(k => k.Lop)
                    .Where(k => k.MaMonHoc == monHocId)
                    .ToListAsync();
            }

            var classNames = teacherKhoaHocs.Select(k => k.Lop?.TenLop).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
            var classIds = teacherKhoaHocs.Select(k => (int?)k.MaLop).Distinct().ToList();
            var studentCount = _context.NguoiDungs.Count(n => classIds.Contains(n.MaLop) && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"));

            var chuongHocList = await _context.Chuongs
                .Where(c => c.MaMonHoc == monHocId && !c.DaAn)
                .OrderBy(c => c.ThuTu)
                .Select(c => new
                {
                    id = c.MaChuong,
                    tieuDe = c.TieuDe,
                    baiHoc = _context.BaiHocs
                        .Where(b => b.MaChuong == c.MaChuong && !b.DaAn)
                        .OrderBy(b => b.ThuTu)
                        .Select(b => new
                        {
                            id = b.MaBaiHoc,
                            tieuDe = b.TieuDe,
                            hasVideo = _context.BaiHocNoiDungs.Any(n => n.MaBaiHoc == b.MaBaiHoc && n.LoaiNoiDung == "video") || !string.IsNullOrEmpty(b.UrlTapTin) || b.LoaiBaiHoc == "video",
                            hasDoc = _context.BaiHocNoiDungs.Any(n => n.MaBaiHoc == b.MaBaiHoc && (n.LoaiNoiDung == "tai_lieu" || n.LoaiNoiDung == "pdf" || n.LoaiNoiDung == "document")) || b.LoaiBaiHoc == "tai_lieu" || b.LoaiBaiHoc == "pdf" || b.LoaiBaiHoc == "document",
                            hasSlide = _context.BaiHocNoiDungs.Any(n => n.MaBaiHoc == b.MaBaiHoc && n.LoaiNoiDung == "slide_html") || b.LoaiBaiHoc == "slide_html" || b.LoaiBaiHoc == "slide",
                            hasQuiz = _context.BaiHocNoiDungs.Any(n => n.MaBaiHoc == b.MaBaiHoc && (n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null)) || b.LoaiBaiHoc == "quiz" || b.LoaiBaiHoc == "trac_nghiem",
                            rawLoai = b.LoaiBaiHoc.ToLower(),
                            thoiLuong = b.ThoiLuongGiay.HasValue && b.ThoiLuongGiay.Value > 0 ? $"{b.ThoiLuongGiay.Value / 60} phút" : "15 phút",
                            rawUrlTapTin = b.UrlTapTin ?? _context.BaiHocNoiDungs.Where(n => n.MaBaiHoc == b.MaBaiHoc && n.LoaiNoiDung == "video" && n.UrlTapTin != null).Select(n => n.UrlTapTin).FirstOrDefault(),
                            rawDocumentUrl = _context.BaiHocNoiDungs.Where(n => n.MaBaiHoc == b.MaBaiHoc && (n.LoaiNoiDung == "tai_lieu" || n.LoaiNoiDung == "pdf" || n.LoaiNoiDung == "document") && n.UrlTapTin != null).Select(n => n.UrlTapTin).FirstOrDefault(),
                            slideHtml = _context.BaiHocNoiDungs.Where(n => n.MaBaiHoc == b.MaBaiHoc && n.LoaiNoiDung == "slide_html").Select(n => n.NoiDungHtml).FirstOrDefault(),
                            noiDung = b.NoiDungVanBan,
                            allowSeek = b.DieuKienMoKhoa == null || !b.DieuKienMoKhoa.Contains("\"allowSeek\":false"),
                            trangThai = "published",
                            quizInfo = _context.BaiHocNoiDungs
                                .Where(n => n.MaBaiHoc == b.MaBaiHoc && n.MaDeKiemTra != null)
                                .Select(n => new
                                {
                                    contentId = n.MaNoiDung,
                                    quizId = n.DeKiemTra!.MaDeKiemTra,
                                    title = n.DeKiemTra.TieuDe,
                                    durationMinutes = n.DeKiemTra.ThoiGianPhut,
                                    trangThai = n.DeKiemTra.TrangThai,
                                    questionsCount = _context.CauHoiDeKiemTras.Count(cd => cd.MaDeKiemTra == n.MaDeKiemTra),
                                    totalScore = _context.CauHoiDeKiemTras.Where(cd => cd.MaDeKiemTra == n.MaDeKiemTra).Sum(cd => (double)cd.DiemSo)
                                })
                                .FirstOrDefault(),
                            quizQuestions = _context.BaiHocNoiDungs
                                .Where(n => n.MaBaiHoc == b.MaBaiHoc && n.MaDeKiemTra != null)
                                .SelectMany(n => _context.CauHoiDeKiemTras
                                    .Where(cd => cd.MaDeKiemTra == n.MaDeKiemTra)
                                    .Select(cd => new
                                    {
                                        id = cd.CauHoi!.MaCauHoi,
                                        question = cd.CauHoi.NoiDung,
                                        options = cd.CauHoi.LuaChon,
                                        answer = cd.CauHoi.DapAnDung,
                                        explanation = cd.CauHoi.GiaiThichDapAn,
                                        diemSo = cd.DiemSo,
                                        thuTu = cd.ThuTu
                                    }))
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            // Resolve real streaming URLs via IR2StorageService
            var formattedChuongHoc = chuongHocList.Select(c => new
            {
                id = c.id,
                tieuDe = c.tieuDe,
                baiHoc = c.baiHoc.Select(b => new
                {
                    id = b.id,
                    tieuDe = b.tieuDe,
                    loai = b.hasVideo ? "video" : (b.hasDoc ? "pdf" : (b.hasSlide ? "slide" : (b.hasQuiz ? "quiz" : b.rawLoai))),
                    hasVideo = b.hasVideo,
                    hasDoc = b.hasDoc,
                    hasSlide = b.hasSlide,
                    hasQuiz = b.hasQuiz,
                    thoiLuong = b.thoiLuong,
                    urlTapTin = ResolveMediaUrl(b.rawUrlTapTin, storageService),
                    documentUrl = ResolveMediaUrl(b.rawDocumentUrl, storageService),
                    slideHtml = b.slideHtml,
                    noiDung = b.noiDung,
                    allowSeek = b.allowSeek,
                    trangThai = b.trangThai,
                    quizInfo = b.quizInfo,
                    quizQuestions = b.quizQuestions
                }).ToList()
            }).ToList();

            var baiTapList = await _context.BaiTaps
                .Where(b => b.MaMonHoc == monHocId)
                .OrderBy(b => b.MaBaiTap)
                .Select(b => new
                {
                    id = b.MaBaiTap,
                    tieuDe = b.TieuDe,
                    moTa = b.MoTa,
                    hanNop = b.HanNop,
                    soLanNopToiDa = b.SoLanNopToiDa,
                    dinhDangChoPhep = b.DinhDangChoPhep,
                    huongDanChamDiem = b.HuongDanChamDiem,
                    trangThai = b.TrangThai
                })
                .ToListAsync();

            var questionBankCount = await _context.CauHois.CountAsync(c => c.MaMonHoc == monHocId && c.ConHoatDong);

            var allLessons = formattedChuongHoc.SelectMany(c => c.baiHoc).ToList();
            bool isAllLocked = allLessons.Count > 0 && allLessons.All(l => !l.allowSeek);

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassId = teacherKhoaHocs.FirstOrDefault()?.MaLop ?? 0,
                ClassName = classNames.Count > 0 ? string.Join(", ", classNames) : "Tất cả các lớp",
                ClassNames = classNames,
                Code = monHoc.MaCodeMonHoc ?? "MH" + monHocId,
                Name = monHoc.TenMonHoc,
                CourseId = monHocId,
                CourseName = monHoc.TenMonHoc,
                ChuongHoc = formattedChuongHoc,
                BaiTaps = baiTapList,
                QuestionBankCount = questionBankCount,
                StudentCount = studentCount,
                IsAllLocked = isAllLocked
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chi tiết môn học: " + ex.Message));
        }
    }

    public class ToggleAllSeekRequest
    {
        public bool? LockAll { get; set; }
    }

    [HttpPost("subjects/{id}/toggle-seek-all")]
    public async Task<ActionResult<ApiResponseDto<object>>> ToggleSubjectSeekAll(string id, [FromBody] ToggleAllSeekRequest? req = null)
    {
        try
        {
            int.TryParse(id, out int numericId);
            var cleanId = (id ?? "").Trim().ToLower();

            var monHoc = await _context.DanhMucMonHocs.FirstOrDefaultAsync(m =>
                m.MaCodeMonHoc.ToLower() == cleanId || (numericId > 0 && m.MaMonHoc == numericId));

            if (monHoc == null && numericId > 0)
            {
                var kh = await _context.KhoaHocs.Include(k => k.MonHoc).FirstOrDefaultAsync(k => k.MaKhoaHoc == numericId);
                if (kh?.MonHoc != null) monHoc = kh.MonHoc;
            }

            if (monHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học"));
            }

            int monHocId = monHoc.MaMonHoc;
            var lessons = await _context.BaiHocs
                .Where(b => _context.Chuongs.Any(c => c.MaMonHoc == monHocId && c.MaChuong == b.MaChuong))
                .ToListAsync();

            if (!lessons.Any())
            {
                return Ok(ApiResponseDto<object>.Ok(new { message = "Không có bài học nào trong môn học" }));
            }

            bool shouldLock = req?.LockAll ?? !lessons.All(l => l.DieuKienMoKhoa != null && l.DieuKienMoKhoa.Contains("\"allowSeek\":false"));

            string newSetting = shouldLock ? "{\"allowSeek\":false}" : "{\"allowSeek\":true}";
            foreach (var lesson in lessons)
            {
                lesson.DieuKienMoKhoa = newSetting;
                lesson.NgayCapNhat = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                isLocked = shouldLock,
                allowSeek = !shouldLock,
                updatedCount = lessons.Count,
                message = shouldLock
                    ? $"Đã khóa tính năng tua nhanh của sinh viên cho toàn bộ {lessons.Count} bài học môn {monHoc.TenMonHoc}"
                    : $"Đã bật cho phép sinh viên tua video cho toàn bộ {lessons.Count} bài học môn {monHoc.TenMonHoc}"
            }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi cập nhật cấu hình tua video toàn môn học: " + msg));
        }
    }

    [HttpPost("lessons/{lessonId}/toggle-seek")]
    public async Task<ActionResult<ApiResponseDto<object>>> ToggleLessonSeek(int lessonId)
    {
        try
        {
            var baiHoc = await _context.BaiHocs.FindAsync(lessonId);
            if (baiHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy bài học"));
            }

            bool currentlyDisabled = baiHoc.DieuKienMoKhoa != null && baiHoc.DieuKienMoKhoa.Contains("\"allowSeek\":false");
            baiHoc.DieuKienMoKhoa = currentlyDisabled ? "{\"allowSeek\":true}" : "{\"allowSeek\":false}";
            baiHoc.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                lessonId = lessonId,
                allowSeek = currentlyDisabled,
                message = currentlyDisabled ? "Đã bật cho phép sinh viên tua video" : "Đã khóa tính năng tua nhanh của sinh viên"
            }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi cập nhật cấu hình tua video: " + msg));
        }
    }

    [HttpGet("subjects/{id}/question-bank")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetSubjectQuestionBank(string id)
    {
        try
        {
            int.TryParse(id, out int numericId);
            var cleanId = (id ?? "").Trim().ToLower();

            var monHoc = await _context.DanhMucMonHocs.FirstOrDefaultAsync(m =>
                m.MaCodeMonHoc.ToLower() == cleanId || (numericId > 0 && m.MaMonHoc == numericId));

            if (monHoc == null && numericId > 0)
            {
                var kh = await _context.KhoaHocs.Include(k => k.MonHoc).FirstOrDefaultAsync(k => k.MaKhoaHoc == numericId);
                if (kh?.MonHoc != null) monHoc = kh.MonHoc;
            }

            if (monHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học"));
            }

            int monHocId = monHoc.MaMonHoc;

            var questions = await _context.CauHois
                .Where(c => c.MaMonHoc == monHocId && c.ConHoatDong)
                .OrderBy(c => c.MaCauHoi)
                .Select(c => new
                {
                    id = c.MaCauHoi,
                    noiDung = c.NoiDung,
                    loaiCauHoi = c.LoaiCauHoi,
                    luaChon = c.LuaChon,
                    dapAnDung = c.DapAnDung,
                    doKho = c.DoKho,
                    giaiThichDapAn = c.GiaiThichDapAn
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(questions));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải ngân hàng câu hỏi: " + ex.Message));
        }
    }

    public class AddQuizQuestionRequest
    {
        public int QuestionId { get; set; }
    }

    [HttpPost("lessons/{lessonId}/add-quiz-question")]
    public async Task<ActionResult<ApiResponseDto<object>>> AddQuizQuestionToLesson(int lessonId, [FromBody] AddQuizQuestionRequest request)
    {
        try
        {
            var baiHoc = await _context.BaiHocs.FindAsync(lessonId);
            if (baiHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy bài học"));
            }

            var chuong = await _context.Chuongs.FindAsync(baiHoc.MaChuong);
            int monHocId = chuong?.MaMonHoc ?? 0;

            var question = await _context.CauHois.FindAsync(request.QuestionId);
            if (question == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy câu hỏi trong ngân hàng"));
            }

            var content = await _context.BaiHocNoiDungs
                .FirstOrDefaultAsync(n => n.MaBaiHoc == lessonId && (n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null));

            DeKiemTra deKiemTra;
            if (content == null || content.MaDeKiemTra == null)
            {
                deKiemTra = new DeKiemTra
                {
                    TieuDe = "Bài trắc nghiệm: " + baiHoc.TieuDe,
                    MaMonHoc = monHocId,
                    ThoiGianPhut = 15,
                    CauHinhDeThi = "{}",
                    TrangThai = "dang_mo",
                    NgayTao = DateTime.UtcNow
                };
                _context.DeKiemTras.Add(deKiemTra);
                await _context.SaveChangesAsync();

                if (content == null)
                {
                    content = new BaiHocNoiDung
                    {
                        MaBaiHoc = lessonId,
                        LoaiNoiDung = "quiz",
                        MaDeKiemTra = deKiemTra.MaDeKiemTra,
                        TrangThai = "da_xuat_ban",
                        ThuTu = 1,
                        NgayTao = DateTime.UtcNow
                    };
                    _context.BaiHocNoiDungs.Add(content);
                }
                else
                {
                    content.MaDeKiemTra = deKiemTra.MaDeKiemTra;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                deKiemTra = (await _context.DeKiemTras.FindAsync(content.MaDeKiemTra))!;
            }

            bool alreadyLinked = await _context.CauHoiDeKiemTras
                .AnyAsync(cd => cd.MaDeKiemTra == deKiemTra.MaDeKiemTra && cd.MaCauHoi == request.QuestionId);

            if (!alreadyLinked)
            {
                _context.CauHoiDeKiemTras.Add(new CauHoiDeKiemTra
                {
                    MaDeKiemTra = deKiemTra.MaDeKiemTra,
                    MaCauHoi = request.QuestionId,
                    DiemSo = 1m,
                    ThuTu = 1
                });
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponseDto<object>.Ok(new { message = "Thêm câu hỏi vào bài học thành công" }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            if (ex.InnerException?.InnerException != null) msg += $" -> {ex.InnerException.InnerException.Message}";
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi thêm câu hỏi vào bài học: " + msg));
        }
    }

    [HttpGet("subjects/{id}/quizzes")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetSubjectQuizzes(string id)
    {
        try
        {
            int.TryParse(id, out int numericId);
            var cleanId = (id ?? "").Trim().ToLower();

            var monHoc = await _context.DanhMucMonHocs.FirstOrDefaultAsync(m =>
                m.MaCodeMonHoc.ToLower() == cleanId || (numericId > 0 && m.MaMonHoc == numericId));

            if (monHoc == null && numericId > 0)
            {
                var kh = await _context.KhoaHocs.Include(k => k.MonHoc).FirstOrDefaultAsync(k => k.MaKhoaHoc == numericId);
                if (kh?.MonHoc != null) monHoc = kh.MonHoc;
            }

            if (monHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học"));
            }

            int monHocId = monHoc.MaMonHoc;

            // Giảng viên CHỈ được xem các đề kiểm tra đã được Hội đồng quản lý nội dung xuất bản (TrangThai != 'nhap')
            var quizzes = await _context.DeKiemTras
                .Where(d => d.MaMonHoc == monHocId && d.TrangThai != "nhap")
                .OrderByDescending(d => d.MaDeKiemTra)
                .Select(d => new
                {
                    quizId = d.MaDeKiemTra,
                    code = "QZ-" + d.MaDeKiemTra,
                    title = d.TieuDe,
                    description = d.HinhThucThi ?? d.LoaiDeThi ?? "",
                    durationMinutes = d.ThoiGianPhut,
                    loaiDeThi = d.LoaiDeThi,
                    trangThai = d.TrangThai,
                    questionsCount = _context.CauHoiDeKiemTras.Count(cd => cd.MaDeKiemTra == d.MaDeKiemTra),
                    totalScore = _context.CauHoiDeKiemTras.Where(cd => cd.MaDeKiemTra == d.MaDeKiemTra).Sum(cd => (double)cd.DiemSo),
                    questions = _context.CauHoiDeKiemTras
                        .Where(cd => cd.MaDeKiemTra == d.MaDeKiemTra)
                        .OrderBy(cd => cd.ThuTu)
                        .Select(cd => new
                        {
                            id = cd.CauHoi!.MaCauHoi,
                            question = cd.CauHoi.NoiDung,
                            options = cd.CauHoi.LuaChon,
                            answer = cd.CauHoi.DapAnDung,
                            explanation = cd.CauHoi.GiaiThichDapAn,
                            diemSo = cd.DiemSo,
                            thuTu = cd.ThuTu
                        })
                        .ToList(),
                    createdAt = d.NgayTao
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(quizzes));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách đề Quiz: " + ex.Message));
        }
    }

    public class AttachQuizRequest
    {
        public int QuizId { get; set; }
    }

    [HttpPost("lessons/{lessonId}/attach-quiz")]
    public async Task<ActionResult<ApiResponseDto<object>>> AttachQuizToLesson(int lessonId, [FromBody] AttachQuizRequest request)
    {
        try
        {
            var baiHoc = await _context.BaiHocs.FindAsync(lessonId);
            if (baiHoc == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy bài học"));
            }

            var quiz = await _context.DeKiemTras.FindAsync(request.QuizId);
            if (quiz == null)
            {
                return NotFound(ApiResponseDto.Fail("Không tìm thấy đề Quiz"));
            }

            // Nghiệp vụ: Giảng viên không được phép gán đề chưa xuất bản (bản nháp)
            if (quiz.TrangThai == "nhap")
            {
                return BadRequest(ApiResponseDto.Fail("Đề kiểm tra đang ở trạng thái bản nháp. Chỉ có thể gán đề đã được Hội đồng quản lý nội dung xuất bản."));
            }

            var content = await _context.BaiHocNoiDungs
                .FirstOrDefaultAsync(n => n.MaBaiHoc == lessonId && (n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null));

            if (content == null)
            {
                var maxOrder = await _context.BaiHocNoiDungs
                    .Where(n => n.MaBaiHoc == lessonId)
                    .MaxAsync(n => (int?)n.ThuTu) ?? 0;

                content = new BaiHocNoiDung
                {
                    MaBaiHoc = lessonId,
                    LoaiNoiDung = "quiz",
                    MaDeKiemTra = request.QuizId,
                    TrangThai = "da_xuat_ban",
                    ThuTu = maxOrder + 1,
                    NgayTao = DateTime.UtcNow
                };
                _context.BaiHocNoiDungs.Add(content);
            }
            else
            {
                content.MaDeKiemTra = request.QuizId;
                content.LoaiNoiDung = "quiz";
                content.TrangThai = "da_xuat_ban";
                content.NgayCapNhat = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                message = "Gán đề Quiz vào bài học thành công",
                quizId = quiz.MaDeKiemTra,
                title = quiz.TieuDe
            }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi gán đề Quiz vào bài học: " + msg));
        }
    }

    [HttpDelete("lessons/{lessonId}/quiz")]
    public async Task<ActionResult<ApiResponseDto<object>>> RemoveQuizFromLesson(int lessonId)
    {
        try
        {
            var contents = await _context.BaiHocNoiDungs
                .Where(n => n.MaBaiHoc == lessonId && (n.LoaiNoiDung == "quiz" || n.LoaiNoiDung == "trac_nghiem" || n.MaDeKiemTra != null))
                .ToListAsync();

            if (contents.Count > 0)
            {
                _context.BaiHocNoiDungs.RemoveRange(contents);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponseDto<object>.Ok(new { message = "Đã gỡ đề Quiz khỏi bài học" }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi gỡ đề Quiz khỏi bài học: " + ex.Message));
        }
    }

    [HttpDelete("lessons/{lessonId}/quiz-questions/{questionId}")]
    public async Task<ActionResult<ApiResponseDto<object>>> RemoveQuizQuestionFromLesson(int lessonId, int questionId)
    {
        try
        {
            var content = await _context.BaiHocNoiDungs
                .FirstOrDefaultAsync(n => n.MaBaiHoc == lessonId && n.MaDeKiemTra != null);

            if (content?.MaDeKiemTra == null)
            {
                return NotFound(ApiResponseDto.Fail("Bài học chưa có đề Quiz"));
            }

            var relation = await _context.CauHoiDeKiemTras
                .FirstOrDefaultAsync(cd => cd.MaDeKiemTra == content.MaDeKiemTra.Value && cd.MaCauHoi == questionId);

            if (relation != null)
            {
                _context.CauHoiDeKiemTras.Remove(relation);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponseDto<object>.Ok(new { message = "Đã xóa câu hỏi khỏi đề Quiz của bài học" }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi xóa câu hỏi: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClassDetail(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.Lop)
                .Include(k => k.MonHoc)
                .FirstOrDefaultAsync(k => (k.MaKhoaHoc == id || k.MaLop == id || k.MaMonHoc == id) && k.MaGiaoVien == userId);

            int monHocId = khoaHoc != null ? khoaHoc.MaMonHoc : id;
            var monHoc = khoaHoc?.MonHoc ?? await _context.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaMonHoc == id);

            if (khoaHoc == null && monHoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp/khóa học hoặc môn học này."));

            var classId = khoaHoc?.MaLop ?? 0;
            var className = khoaHoc?.Lop?.TenLop ?? "Tất cả các lớp";
            var subjectCode = monHoc?.MaCodeMonHoc ?? "";
            var subjectName = monHoc?.TenMonHoc ?? khoaHoc?.TieuDe ?? "Môn học";

            var chuongHocList = await _context.Chuongs
                .Where(c => c.MaMonHoc == monHocId && !c.DaAn)
                .OrderBy(c => c.ThuTu)
                .Select(c => new
                {
                    id = c.MaChuong,
                    tieuDe = c.TieuDe,
                    baiHoc = _context.BaiHocs
                        .Where(b => b.MaChuong == c.MaChuong && !b.DaAn)
                        .OrderBy(b => b.ThuTu)
                        .Select(b => new
                        {
                            id = b.MaBaiHoc,
                            tieuDe = b.TieuDe,
                            loai = b.LoaiBaiHoc.ToLower(),
                            thoiLuong = b.ThoiLuongGiay.HasValue && b.ThoiLuongGiay.Value > 0 ? $"{b.ThoiLuongGiay.Value / 60} phút" : "15 phút",
                            urlTapTin = b.UrlTapTin,
                            noiDung = b.NoiDungVanBan,
                            trangThai = "published"
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassId = classId,
                ClassName = className,
                Code = subjectCode,
                Name = subjectName,
                CourseId = id,
                CourseName = subjectName,
                ChuongHoc = chuongHocList,
                StudentCount = khoaHoc != null ? _context.NguoiDungs.Count(n => n.MaLop == classId) : 0
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chi tiết lớp: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/workspace")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClassWorkspace(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var courses = await _context.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Where(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId)
                .ToListAsync();

            if (!courses.Any())
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp hoặc bạn không dạy lớp này."));

            int classId = courses.First().MaLop;
            var lop = await _context.LopHanhChinhs
                .Include(l => l.ChuongTrinh)
                    .ThenInclude(c => c.ChuyenNganh)
                .Where(l => l.MaLop == classId)
                .Select(l => new 
                { 
                    l.MaLop, 
                    l.TenLop,
                    ChuyenNganh = l.ChuongTrinh != null && l.ChuongTrinh.ChuyenNganh != null ? l.ChuongTrinh.ChuyenNganh.TenChuyenNganh : ""
                })
                .FirstOrDefaultAsync();

            if (lop == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp học."));

            var courseIds = courses.Select(c => c.MaKhoaHoc).ToList();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var currentSession = await _context.BuoiHocs
                .Include(b => b.Phong)
                .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.MaGiaoVien == userId && b.NgayHoc == today)
                .OrderBy(b => b.MaCaHoc)
                .FirstOrDefaultAsync();

            // If no session today, find the most recent recorded/past session
            if (currentSession == null)
            {
                currentSession = await _context.BuoiHocs
                    .Include(b => b.Phong)
                    .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.MaGiaoVien == userId && b.NgayHoc <= today)
                    .OrderByDescending(b => b.NgayHoc)
                    .ThenByDescending(b => b.MaCaHoc)
                    .FirstOrDefaultAsync();
            }

            // If still null, take the earliest upcoming session
            if (currentSession == null)
            {
                currentSession = await _context.BuoiHocs
                    .Include(b => b.Phong)
                    .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.MaGiaoVien == userId)
                    .OrderBy(b => b.NgayHoc)
                    .ThenBy(b => b.MaCaHoc)
                    .FirstOrDefaultAsync();
            }

            var attendanceRecords = currentSession != null
                ? await _context.DiemDanhs
                    .Where(d => d.MaBuoiHoc == currentSession.MaBuoiHoc)
                    .ToListAsync()
                : [];

            var hasAttendanceRecord = attendanceRecords.Count > 0;
            var attendanceMap = attendanceRecords.ToDictionary(d => d.MaHocSinh, d => d.TrangThai == "co_mat");

            var studentList = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .OrderBy(n => n.HoTen)
                .ThenBy(n => n.MaNguoiDung)
                .Select(n => new
                {
                    Id = n.MaNguoiDung,
                    Name = n.HoTen,
                    Email = n.Email,
                    Avatar = ""
                })
                .ToListAsync();

            var students = studentList.Select(n => new
            {
                n.Id,
                n.Name,
                n.Email,
                n.Avatar,
                Present = attendanceMap.TryGetValue(n.Id, out var isPresent) ? isPresent : false
            }).ToList();

            var monHocIds = courses.Select(c => c.MaMonHoc).ToList();
            var baiHocs = monHocIds.Count > 0
                ? await _context.BaiHocs
                    .Where(b => b.Chuong != null && monHocIds.Contains(b.Chuong.MaMonHoc))
                    .OrderBy(b => b.Chuong != null ? b.Chuong.ThuTu : 0)
                    .ThenBy(b => b.ThuTu)
                    .Take(15)
                    .ToListAsync()
                : [];

            var baiHocIds = baiHocs.Select(b => b.MaBaiHoc).ToList();
            var studentIds = studentList.Select(s => s.Id).ToList();

            var tienDos = await _context.TienDoBaiHocs
                .Where(t => baiHocIds.Contains(t.MaBaiHoc) && studentIds.Contains(t.MaHocSinh) && t.HoanThanhLuc != null)
                .ToListAsync();

            int completedModulesCount = 0;
            var modules = baiHocs.Select((b, idx) =>
            {
                var completedStudentCount = tienDos.Count(t => t.MaBaiHoc == b.MaBaiHoc);
                var isCompleted = studentIds.Count > 0 && completedStudentCount >= Math.Max(1, studentIds.Count * 0.4);
                if (isCompleted) completedModulesCount++;

                string status = isCompleted ? "completed" : (idx == completedModulesCount ? "playing" : "available");
                return new
                {
                    Id = b.MaBaiHoc,
                    Title = b.TieuDe,
                    Duration = "45 phút",
                    Status = status,
                    Type = "video"
                };
            }).ToList();

            var progressPercent = baiHocs.Count > 0
                ? (int)Math.Round((decimal)completedModulesCount / baiHocs.Count * 100)
                : 0;

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassName = lop.TenLop,
                ChuyenNganh = lop.ChuyenNganh,
                PhongHoc = currentSession?.Phong?.TenPhong,
                SessionId = currentSession?.MaBuoiHoc,
                SessionStatus = currentSession?.TrangThaiDiemDanh,
                SessionDate = currentSession?.NgayHoc.ToDateTime(new TimeOnly(0, 0)),
                HasAttendanceRecord = hasAttendanceRecord,
                PresentCount = attendanceMap.Count(x => x.Value),
                AbsentCount = attendanceMap.Count(x => !x.Value),
                ProgressPercent = progressPercent,
                CompletedModules = completedModulesCount,
                TotalModules = baiHocs.Count,
                Students = students,
                Courses = courses.Select(c => new 
                { 
                    CourseId = c.MaKhoaHoc,
                    CourseName = c.TieuDe,
                    SubjectCode = c.MonHoc != null ? c.MonHoc.MaCodeMonHoc : ""
                }),
                Modules = modules
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải workspace: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/progress")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClassProgress(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var courses = await _context.KhoaHocs
                .Where(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId)
                .ToListAsync();

            if (!courses.Any())
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp hoặc bạn không dạy lớp này."));

            int classId = courses.First().MaLop;
            var lop = await _context.LopHanhChinhs
                .Where(l => l.MaLop == classId)
                .Select(l => new { l.MaLop, l.TenLop })
                .FirstAsync();

            var monHocIds = courses.Select(k => k.MaMonHoc).Distinct().ToList();

            var lessonIds = await _context.BaiHocs
                .Where(b => b.Chuong != null && monHocIds.Contains(b.Chuong.MaMonHoc))
                .Select(b => b.MaBaiHoc)
                .ToListAsync();

            var totalLessons = lessonIds.Count;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    Email = n.Email,
                    CoursesCompleted = lessonIds.Count > 0
                        ? _context.TienDoBaiHocs.Count(t => t.MaHocSinh == n.MaNguoiDung && lessonIds.Contains(t.MaBaiHoc) && t.HoanThanhLuc != null)
                        : 0,
                    Absent = _context.DiemDanhs.Count(d => d.MaHocSinh == n.MaNguoiDung && d.TrangThai == "vang"),
                    Diem = _context.DiemSos.FirstOrDefault(d => d.MaHocSinh == n.MaNguoiDung && monHocIds.Contains(d.MaMonHoc))
                })
                .ToListAsync();

            var result = students.Select(s => {
                var prog = totalLessons > 0 ? (int)Math.Round((decimal)s.CoursesCompleted / totalLessons * 100) : 0;
                var status = "good";
                if (prog >= 90) status = "excellent";
                else if (prog < 50) status = "danger";
                else if (prog < 70) status = "warning";

                return new
                {
                    id = s.StudentId,
                    name = s.StudentName,
                    email = s.Email,
                    progress = prog,
                    completedLessons = s.CoursesCompleted,
                    totalLessons = totalLessons,
                    gpa = s.Diem != null ? (decimal?)s.Diem.GpaMonHoc : null,
                    absent = s.Absent,
                    status = status
                };
            }).ToList();

            var overallProgress = result.Count > 0 ? (int)Math.Round(result.Average(r => r.progress)) : 0;
            var completedLessons = result.Sum(r => r.progress == 100 ? 1 : 0);

            var chartData = new List<object>
            {
                new { range = "0-20%", value = result.Count(r => r.progress <= 20), height = 20 },
                new { range = "21-50%", value = result.Count(r => r.progress > 20 && r.progress <= 50), height = 40 },
                new { range = "51-80%", value = result.Count(r => r.progress > 50 && r.progress <= 80), height = 70 },
                new { range = "81-100%", value = result.Count(r => r.progress > 80), height = 100 }
            };

            return Ok(ApiResponseDto<object>.Ok(new
            {
                classId = lop.MaLop,
                className = lop.TenLop,
                students = result,
                overallProgress = overallProgress,
                completedLessons = students.Sum(s => s.CoursesCompleted),
                courseTotalLessons = totalLessons,
                totalLessons = totalLessons * (students.Count > 0 ? students.Count : 1),
                activeStudents = result.Count,
                chartData = chartData
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải tiến độ lớp: " + ex.Message));
        }
    }

    [HttpGet("courses/{id}/progress")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetCourseProgress(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .FirstOrDefaultAsync(k => k.MaKhoaHoc == id && k.MaGiaoVien == userId);
            
            if (khoaHoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy khóa học hoặc bạn không dạy khóa học này."));

            var monHocId = khoaHoc.MaMonHoc;
            var lopId = khoaHoc.MaLop;

            var lessonIds = await _context.BaiHocs
                .Where(b => b.Chuong != null && b.Chuong.MaMonHoc == monHocId)
                .Select(b => b.MaBaiHoc)
                .ToListAsync();

            var totalLessons = lessonIds.Count;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == lopId && n.VaiTroChinh == "hoc_sinh")
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    Email = n.Email,
                    CoursesCompleted = lessonIds.Count > 0
                        ? _context.TienDoBaiHocs.Count(t => t.MaHocSinh == n.MaNguoiDung && lessonIds.Contains(t.MaBaiHoc) && t.HoanThanhLuc != null)
                        : 0,
                    Absent = _context.DiemDanhs.Count(d => d.MaHocSinh == n.MaNguoiDung && d.TrangThai == "vang" && d.BuoiHoc != null && d.BuoiHoc.MaKhoaHoc == id),
                    Diem = _context.DiemSos.FirstOrDefault(d => d.MaHocSinh == n.MaNguoiDung && d.MaMonHoc == monHocId)
                })
                .ToListAsync();

            var result = students.Select(s => {
                var prog = totalLessons > 0 ? (int)Math.Round((decimal)s.CoursesCompleted / totalLessons * 100) : 0;
                var status = "good";
                if (prog >= 90) status = "excellent";
                else if (prog < 50) status = "danger";
                else if (prog < 70) status = "warning";

                return new
                {
                    id = s.StudentId,
                    name = s.StudentName,
                    email = s.Email,
                    progress = prog,
                    completedLessons = s.CoursesCompleted,
                    totalLessons = totalLessons,
                    gpa = s.Diem != null ? (decimal?)s.Diem.GpaMonHoc : null,
                    absent = s.Absent,
                    status = status
                };
            }).ToList();

            var overallProgress = result.Count > 0 ? (int)Math.Round(result.Average(r => r.progress)) : 0;
            var completedLessons = result.Sum(r => r.progress == 100 ? 1 : 0);

            var chartData = new List<object>
            {
                new { range = "0-20%", value = result.Count(r => r.progress <= 20), height = 20 },
                new { range = "21-50%", value = result.Count(r => r.progress > 20 && r.progress <= 50), height = 40 },
                new { range = "51-80%", value = result.Count(r => r.progress > 50 && r.progress <= 80), height = 70 },
                new { range = "81-100%", value = result.Count(r => r.progress > 80), height = 100 }
            };

            return Ok(ApiResponseDto<object>.Ok(new
            {
                courseId = khoaHoc.MaKhoaHoc,
                courseName = !string.IsNullOrEmpty(khoaHoc.TieuDe) ? khoaHoc.TieuDe : (khoaHoc.MonHoc != null ? khoaHoc.MonHoc.TenMonHoc : ""),
                className = khoaHoc.Lop != null ? khoaHoc.Lop.TenLop : "",
                students = result,
                overallProgress = overallProgress,
                completedLessons = students.Sum(s => s.CoursesCompleted),
                courseTotalLessons = totalLessons,
                totalLessons = totalLessons * (students.Count > 0 ? students.Count : 1),
                activeStudents = result.Count,
                chartData = chartData
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải tiến độ khóa học: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/grades")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClassGrades(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoahoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            int classId = khoahoc.MaLop;
            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    Diem = _context.DiemSos.FirstOrDefault(d => d.MaHocSinh == n.MaNguoiDung && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId)
                })
                .ToListAsync();

            var result = students.Select(s => new
            {
                id = s.StudentId.ToString(),
                name = s.StudentName,
                assignment = s.Diem?.DiemQuaTrinh,
                exam = s.Diem?.DiemCuoiKy,
                total = s.Diem != null ? s.Diem.GpaMonHoc : 0m,
                isEditing = false
            });

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải điểm: " + ex.Message));
        }
    }

    [HttpPut("classes/{id}/grades/{studentId}")]
    public async Task<ActionResult<ApiResponseDto<object>>> UpdateStudentGrade(int id, int studentId, [FromBody] UpdateGradeRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoahoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            var diem = await _context.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId);

            if (diem == null)
            {
                var hocSinh = await _context.NguoiDungs.FindAsync(studentId);
                var donViId = hocSinh?.MaDonVi ?? 1;

                diem = new Backend.Models.DiemSo
                {
                    MaDonVi = donViId,
                    MaHocSinh = studentId,
                    MaMonHoc = monHocId,
                    MaHocKy = hocKyId ?? 1,
                    DiemQuaTrinh = request.Assignment,
                    DiemCuoiKy = request.Exam,
                    TrangThai = "draft",
                    DaKhoa = false
                };
                _context.DiemSos.Add(diem);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Assign manual input first as fallback, CalculateGradeAsync will override if configs exist.
                diem.DiemQuaTrinh = request.Assignment;
                diem.DiemCuoiKy = request.Exam;
                await _context.SaveChangesAsync();
            }

            var gradeService = HttpContext.RequestServices.GetService<Backend.Services.Grading.IGradeAggregationService>();
            if (gradeService != null)
            {
                try 
                {
                    await gradeService.CalculateGradeAsync(studentId, monHocId, hocKyId ?? 1);
                }
                catch (Backend.Exceptions.ApiException ex)
                {
                    // If no config, fallback to manual GPA calculation
                    if (ex.StatusCode == 400 && ex.Message.Contains("chưa cấu hình"))
                    {
                        await gradeService.CalculateFallbackGradeAsync(studentId, monHocId, hocKyId ?? 1, request.Assignment, request.Exam);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return Ok(ApiResponseDto<object>.Ok(new { message = "Cập nhật điểm thành công" }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi cập nhật điểm: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/grades/export")]
    public async Task<IActionResult> ExportClassGrades(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoahoc = await _context.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .FirstOrDefaultAsync(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
                
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            int classId = khoahoc.MaLop;
            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    Diem = _context.DiemSos.FirstOrDefault(d => d.MaHocSinh == n.MaNguoiDung && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId)
                })
                .ToListAsync();

            ExcelPackage.License.SetNonCommercialPersonal("Phan Thanh Danh");
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Bang_Diem");

            // Header
            string className = khoahoc.Lop?.TenLop ?? $"Lớp {classId}";
            string subjectName = khoahoc.MonHoc?.TenMonHoc ?? "Môn học";
            worksheet.Cells["A1"].Value = $"BẢNG ĐIỂM - {className.ToUpper()} - {subjectName.ToUpper()}";
            worksheet.Cells["A1:F1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Columns Header
            string[] headers = { "STT", "MSSV", "Họ và Tên", "Điểm QT", "Điểm CK", "GPA", "Trạng thái" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[3, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            int row = 4;
            int passCount = 0;
            int failCount = 0;

            foreach (var s in students)
            {
                decimal qt = s.Diem?.DiemQuaTrinh ?? 0;
                decimal ck = s.Diem?.DiemCuoiKy ?? 0;
                decimal total = s.Diem != null ? s.Diem.GpaMonHoc : 0;
                bool isPass = total >= 5;

                if (isPass) passCount++;
                else failCount++;

                worksheet.Cells[row, 1].Value = row - 3;
                worksheet.Cells[row, 2].Value = s.StudentId.ToString();
                worksheet.Cells[row, 3].Value = s.StudentName;
                worksheet.Cells[row, 4].Value = qt;
                worksheet.Cells[row, 5].Value = ck;
                worksheet.Cells[row, 6].Value = total;
                worksheet.Cells[row, 7].Value = isPass ? "Đạt" : "Rớt";
                
                if (isPass) worksheet.Cells[row, 7].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                else worksheet.Cells[row, 7].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                for (int col = 1; col <= 7; col++)
                {
                    worksheet.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }
                row++;
            }

            worksheet.Column(1).Width = 5; // STT
            worksheet.Column(2).Width = 15; // MSSV
            worksheet.Column(3).Width = 25; // Tên
            worksheet.Column(4).Width = 10; // QT
            worksheet.Column(5).Width = 10; // CK
            worksheet.Column(6).Width = 10; // GPA
            worksheet.Column(7).Width = 15; // Trạng thái

            // Add Pie Chart for Pass/Fail
            var pieChart = worksheet.Drawings.AddPieChart("PassFailChart", ePieChartType.Pie);
            pieChart.Title.Text = "Tỷ lệ Đạt/Rớt";
            pieChart.SetPosition(2, 0, 8, 0);
            pieChart.SetSize(400, 300);

            // Create some hidden cells to hold chart data
            worksheet.Cells["Z1"].Value = "Đạt";
            worksheet.Cells["Z2"].Value = "Rớt";
            worksheet.Cells["AA1"].Value = passCount;
            worksheet.Cells["AA2"].Value = failCount;

            var series = pieChart.Series.Add(worksheet.Cells["AA1:AA2"], worksheet.Cells["Z1:Z2"]);
            var dataLabel = pieChart.DataLabel;
            dataLabel.ShowPercent = true;
            dataLabel.ShowLeaderLines = true;

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"BangDiem_{className}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi xuất bảng điểm: " + ex.Message));
        }
    }

    // ===== Phase 3: New Grading Board Endpoints (read-only + lock/unlock) =====

    [HttpGet("classes/{id}/grades/v2")]
    public async Task<ActionResult<ApiResponseDto<ClassGradesSummaryDto>>> GetClassGradesV2(int id, [FromQuery] int? courseId = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            // Resolve KhoaHoc: use courseId if provided, otherwise first match
            KhoaHoc? khoahoc;
            if (courseId.HasValue)
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .Include(k => k.Lop)
                    .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId.Value && (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
            }
            else
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .Include(k => k.Lop)
                    .FirstOrDefaultAsync(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
            }

            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            int classId = khoahoc.MaLop;
            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
            {
                var peerCourseTerm = await _context.KhoaHocs
                    .Where(k => k.MaLop == khoahoc.MaLop && k.MaMonHoc == khoahoc.MaMonHoc && k.MaHocKy != null)
                    .Select(k => k.MaHocKy)
                    .FirstOrDefaultAsync();

                if (peerCourseTerm.HasValue)
                {
                    hocKyId = peerCourseTerm.Value;
                }
                else
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
                    var currentTerm = await _context.HocKys
                        .Where(h => (h.MaDonVi == khoahoc.MaDonVi || h.MaDonVi == 3) && h.NgayBatDau <= today && h.NgayKetThuc >= today && !h.DaKhoa)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync()
                        ?? await _context.HocKys
                        .Where(h => (h.MaDonVi == khoahoc.MaDonVi || h.MaDonVi == 3) && h.NgayBatDau <= today && !h.DaKhoa)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync()
                        ?? await _context.HocKys
                        .Where(h => !h.DaKhoa && h.NgayBatDau <= today)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync();

                    hocKyId = currentTerm?.MaHocKy ?? 3;
                }
            }

            // Load grade type configs dynamically
            var configs = await _context.CauHinhDauDiemQuaTrinhs
                .Include(x => x.LoaiDauDiem)
                .Where(x => x.MaMonHoc == monHocId && x.MaHocKy == hocKyId.Value)
                .OrderBy(x => x.LoaiDauDiem != null ? x.LoaiDauDiem.ThuTuHienThi : 0)
                .ToListAsync();

            var gradeColumns = configs.Select(c => new GradeTypeColumnDto
            {
                Code = c.LoaiDauDiem?.MaCode ?? "",
                Name = c.LoaiDauDiem?.TenLoai ?? "",
                Weight = c.TrongSoNoiBo,
                ColumnCount = c.SoLuongCot
            }).ToList();

            if (gradeColumns.Count == 0)
            {
                gradeColumns = new List<GradeTypeColumnDto>
                {
                    new() { Code = "chuyen_can", Name = "Chuyên cần", Weight = 10, ColumnCount = 1 },
                    new() { Code = "assignment", Name = "Bài tập & Thực hành", Weight = 40, ColumnCount = 1 },
                    new() { Code = "quiz", Name = "Kiểm tra & Quiz", Weight = 50, ColumnCount = 1 }
                };
            }

            // Load students
            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == classId && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .OrderBy(n => n.HoTen)
                .Select(n => new { n.MaNguoiDung, n.HoTen })
                .ToListAsync();

            // Load all DiemSo records for this class/subject/current term in one query
            var studentIds = students.Select(s => s.MaNguoiDung).ToList();
            var diemRecords = await _context.DiemSos
                .Where(d => studentIds.Contains(d.MaHocSinh) && d.MaMonHoc == monHocId && (d.MaHocKy == hocKyId.Value || d.MaHocKy == 0))
                .ToListAsync();

            var studentGrades = new List<StudentGradeSummaryDto>();

            foreach (var student in students)
            {
                var diemRecord = diemRecords.FirstOrDefault(d => d.MaHocSinh == student.MaNguoiDung && (d.MaHocKy == hocKyId.Value || d.MaHocKy == 0))
                                 ?? diemRecords.FirstOrDefault(d => d.MaHocSinh == student.MaNguoiDung);

                var typeGrades = new Dictionary<string, decimal?>();
                if (configs.Count > 0)
                {
                    foreach (var config in configs)
                    {
                        var loaiCode = config.LoaiDauDiem?.MaCode ?? "";
                        decimal? typeGrade = null;

                        if (loaiCode == "chuyen_can")
                        {
                            typeGrade = await _gradeService.CalculateAttendanceGradeAsync(student.MaNguoiDung, monHocId, hocKyId.Value);
                        }
                        else if (loaiCode == "lab" || loaiCode == "assignment")
                        {
                            typeGrade = await _gradeService.CalculateAssignmentGradeAsync(student.MaNguoiDung, monHocId, config);
                        }
                        else if (loaiCode == "quiz" || loaiCode == "progress_test")
                        {
                            typeGrade = await _gradeService.CalculateQuizGradeAsync(student.MaNguoiDung, monHocId, hocKyId.Value, loaiCode, config);
                        }

                        typeGrades[loaiCode] = typeGrade.HasValue ? Math.Round(typeGrade.Value, 2) : null;
                    }
                }
                else
                {
                    var ccGrade = await _gradeService.CalculateAttendanceGradeAsync(student.MaNguoiDung, monHocId, hocKyId.Value);
                    typeGrades["chuyen_can"] = ccGrade.HasValue ? Math.Round(ccGrade.Value, 2) : null;

                    var studentSubs = await _context.BaiNops
                        .Where(b => b.MaHocSinh == student.MaNguoiDung && b.BaiTap != null && b.BaiTap.MaMonHoc == monHocId && b.DiemSo.HasValue)
                        .Select(b => b.DiemSo!.Value)
                        .ToListAsync();
                    typeGrades["assignment"] = studentSubs.Count > 0 ? Math.Round(studentSubs.Average(), 2) : null;

                    var testIds = await _context.DeKiemTras
                        .Where(d => d.MaMonHoc == monHocId && (d.MaHocKy == null || d.MaHocKy == hocKyId.Value))
                        .Select(d => d.MaDeKiemTra)
                        .ToListAsync();

                    if (testIds.Count > 0)
                    {
                        var attempts = await _context.PhienThiHocSinhs
                            .Where(p => p.MaHocSinh == student.MaNguoiDung && testIds.Contains(p.MaDeKiemTra) && p.TrangThaiLuong == "da_dung" && (p.DiemCuoiCung.HasValue || p.DiemTuDong.HasValue))
                            .GroupBy(p => p.MaDeKiemTra)
                            .Select(g => g.Max(p => p.DiemCuoiCung ?? p.DiemTuDong ?? 0))
                            .ToListAsync();
                        typeGrades["quiz"] = attempts.Count > 0 ? Math.Round(attempts.Average(), 2) : null;
                    }
                    else
                    {
                        typeGrades["quiz"] = null;
                    }
                }

                decimal? dQT = diemRecord?.DiemQuaTrinh;
                if (!dQT.HasValue)
                {
                    var validTg = typeGrades.Values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                    if (validTg.Count > 0)
                    {
                        dQT = Math.Round(validTg.Average(), 2);
                    }
                }

                decimal? gpa = diemRecord?.GpaMonHoc;
                if (!gpa.HasValue && dQT.HasValue && diemRecord?.DiemGiuaKy != null && diemRecord?.DiemCuoiKy != null)
                {
                    gpa = Math.Round(dQT.Value * 0.3m + diemRecord.DiemGiuaKy.Value * 0.2m + diemRecord.DiemCuoiKy.Value * 0.5m, 2);
                }

                studentGrades.Add(new StudentGradeSummaryDto
                {
                    StudentId = student.MaNguoiDung,
                    StudentName = student.HoTen,
                    TypeGrades = typeGrades,
                    DiemQuaTrinh = dQT,
                    DiemGiuaKy = diemRecord?.DiemGiuaKy,
                    DiemCuoiKy = diemRecord?.DiemCuoiKy,
                    GpaMonHoc = gpa,
                    TrangThai = diemRecord?.TrangThai == "dat" ? "Đạt" : (diemRecord?.TrangThai == "rot" ? "Rớt" : (gpa.HasValue ? (gpa.Value >= 5.0m ? "Đạt" : "Rớt") : (diemRecord?.TrangThai != "draft" ? diemRecord?.TrangThai : null))),
                    DaKhoa = diemRecord?.DaKhoa ?? false
                });
            }

            var result = new ClassGradesSummaryDto
            {
                ClassId = id,
                ClassName = khoahoc.Lop?.TenLop ?? "",
                CourseId = khoahoc.MaKhoaHoc,
                SubjectName = khoahoc.MonHoc?.TenMonHoc ?? "",
                GradeColumns = gradeColumns,
                Students = studentGrades
            };

            return Ok(ApiResponseDto<ClassGradesSummaryDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải bảng điểm tổng hợp: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}/grades/{studentId}/detail")]
    public async Task<ActionResult<ApiResponseDto<StudentGradeDetailDto>>> GetStudentGradeDetail(int id, int studentId, [FromQuery] int? courseId = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            KhoaHoc? khoahoc;
            if (courseId.HasValue)
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId.Value && (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
            }
            else
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .FirstOrDefaultAsync(k => (k.MaLop == id || k.MaKhoaHoc == id) && k.MaGiaoVien == userId);
            }

            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
            {
                var peerCourseTerm = await _context.KhoaHocs
                    .Where(k => k.MaLop == khoahoc.MaLop && k.MaMonHoc == khoahoc.MaMonHoc && k.MaHocKy != null)
                    .Select(k => k.MaHocKy)
                    .FirstOrDefaultAsync();

                if (peerCourseTerm.HasValue)
                {
                    hocKyId = peerCourseTerm.Value;
                }
                else
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
                    var currentTerm = await _context.HocKys
                        .Where(h => (h.MaDonVi == khoahoc.MaDonVi || h.MaDonVi == 3) && h.NgayBatDau <= today && h.NgayKetThuc >= today && !h.DaKhoa)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync()
                        ?? await _context.HocKys
                        .Where(h => (h.MaDonVi == khoahoc.MaDonVi || h.MaDonVi == 3) && h.NgayBatDau <= today && !h.DaKhoa)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync()
                        ?? await _context.HocKys
                        .Where(h => !h.DaKhoa && h.NgayBatDau <= today)
                        .OrderByDescending(h => h.NgayBatDau)
                        .FirstOrDefaultAsync();

                    hocKyId = currentTerm?.MaHocKy ?? 3;
                }
            }

            // Verify student belongs to this class or course
            var student = await _context.NguoiDungs
                .Where(n => n.MaNguoiDung == studentId && (n.MaLop == id || n.MaLop == khoahoc.MaLop))
                .Select(n => new { n.MaNguoiDung, n.HoTen })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy học sinh trong lớp này."));

            var diemRecord = await _context.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && (d.MaHocKy == hocKyId.Value || d.MaHocKy == 0));

            var configs = await _context.CauHinhDauDiemQuaTrinhs
                .Include(x => x.LoaiDauDiem)
                .Where(x => x.MaMonHoc == monHocId && x.MaHocKy == hocKyId.Value)
                .OrderBy(x => x.LoaiDauDiem != null ? x.LoaiDauDiem.ThuTuHienThi : 0)
                .ToListAsync();

            var gradeTypes = new List<GradeTypeDetailDto>();

            if (configs.Count == 0)
            {
                // Fallback default breakdown when no formal grade configs are stored
                var ccDetail = new GradeTypeDetailDto
                {
                    Code = "chuyen_can",
                    Name = "Chuyên cần",
                    Weight = 10
                };
                ccDetail.AverageGrade = await _gradeService.CalculateAttendanceGradeAsync(studentId, monHocId, hocKyId.Value);
                if (ccDetail.AverageGrade.HasValue)
                {
                    ccDetail.AverageGrade = Math.Round(ccDetail.AverageGrade.Value, 2);
                }
                gradeTypes.Add(ccDetail);

                var btDetail = new GradeTypeDetailDto
                {
                    Code = "assignment",
                    Name = "Bài tập & Thực hành",
                    Weight = 40
                };
                var allBaiTaps = await _context.BaiTaps
                    .Where(b => b.MaMonHoc == monHocId)
                    .OrderBy(b => b.MaBaiTap)
                    .Select(b => new { b.MaBaiTap, b.TieuDe })
                    .ToListAsync();

                foreach (var bt in allBaiTaps)
                {
                    var allSubs = await _context.BaiNops
                        .Where(b => b.MaBaiTap == bt.MaBaiTap && b.MaHocSinh == studentId)
                        .OrderByDescending(b => b.ThoiDiemNop)
                        .Select(b => new { b.DiemSo, b.ThoiDiemNop })
                        .ToListAsync();

                    var latestSub = allSubs.FirstOrDefault();
                    var gradedSub = allSubs.FirstOrDefault(b => b.DiemSo.HasValue);
                    decimal? finalScore = latestSub?.DiemSo ?? gradedSub?.DiemSo;

                    string itemStatus = "chua_nop";
                    if (latestSub != null)
                    {
                        itemStatus = finalScore.HasValue ? "da_cham" : "cho_cham";
                    }

                    btDetail.Items.Add(new GradeItemDto
                    {
                        ItemId = bt.MaBaiTap,
                        ItemName = bt.TieuDe,
                        Grade = finalScore,
                        Status = itemStatus,
                        SubmittedAt = latestSub?.ThoiDiemNop,
                        IsSubmitted = latestSub != null
                    });
                }
                var scoredBt = btDetail.Items.Where(i => i.Grade.HasValue).ToList();
                if (scoredBt.Any())
                {
                    btDetail.AverageGrade = Math.Round(scoredBt.Average(i => i.Grade!.Value), 2);
                }
                gradeTypes.Add(btDetail);

                var quizDetail = new GradeTypeDetailDto
                {
                    Code = "quiz",
                    Name = "Kiểm tra & Quiz",
                    Weight = 50
                };
                var allDeKiemTras = await _context.DeKiemTras
                    .Where(d => d.MaMonHoc == monHocId && (d.MaHocKy == null || d.MaHocKy == hocKyId.Value))
                    .OrderBy(d => d.MaDeKiemTra)
                    .Select(d => new { d.MaDeKiemTra, d.TieuDe, d.CauHinhDeThi })
                    .ToListAsync();

                foreach (var dk in allDeKiemTras)
                {
                    var attempts = await _context.PhienThiHocSinhs
                        .Where(x => x.MaDeKiemTra == dk.MaDeKiemTra && x.MaHocSinh == studentId && x.TrangThaiLuong == "da_dung")
                        .ToListAsync();

                    var scoredAttempts = attempts.Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue).ToList();
                    decimal? testScore = null;
                    if (scoredAttempts.Any())
                    {
                        testScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                    }

                    quizDetail.Items.Add(new GradeItemDto
                    {
                        ItemId = dk.MaDeKiemTra,
                        ItemName = dk.TieuDe,
                        Grade = testScore,
                        Status = attempts.Any() ? (testScore.HasValue ? "da_cham" : "cho_cham") : "chua_nop",
                        IsSubmitted = attempts.Any(),
                        SubmittedAt = attempts.OrderByDescending(a => a.NopLuc).Select(a => a.NopLuc ?? a.BatDauLuc).FirstOrDefault()
                    });
                }
                var scoredQuiz = quizDetail.Items.Where(i => i.Grade.HasValue).ToList();
                if (scoredQuiz.Any())
                {
                    quizDetail.AverageGrade = Math.Round(scoredQuiz.Average(i => i.Grade!.Value), 2);
                }
                gradeTypes.Add(quizDetail);
            }
            else
            {
                var processedBaiTapIds = new HashSet<int>();

                foreach (var config in configs)
                {
                    var loaiCode = config.LoaiDauDiem?.MaCode ?? "";
                    var detail = new GradeTypeDetailDto
                    {
                        Code = loaiCode,
                        Name = config.LoaiDauDiem?.TenLoai ?? "",
                        Weight = config.TrongSoNoiBo
                    };

                    if (loaiCode == "chuyen_can")
                    {
                        detail.AverageGrade = await _gradeService.CalculateAttendanceGradeAsync(studentId, monHocId, hocKyId.Value);
                        if (detail.AverageGrade.HasValue)
                        {
                            detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);
                        }
                    }
                    else if (loaiCode == "lab" || loaiCode == "assignment")
                    {
                        detail.AverageGrade = await _gradeService.CalculateAssignmentGradeAsync(studentId, monHocId, config);
                        if (detail.AverageGrade.HasValue)
                        {
                            detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);
                        }

                        var baiTaps = await _context.BaiTaps
                            .Where(b => b.MaMonHoc == monHocId && (b.MaCauHinhDauDiem == config.MaCauHinhDauDiem || b.MaCauHinhDauDiem == null))
                            .OrderBy(b => b.MaBaiTap)
                            .Select(b => new { b.MaBaiTap, b.TieuDe })
                            .ToListAsync();

                        foreach (var bt in baiTaps)
                        {
                            if (processedBaiTapIds.Contains(bt.MaBaiTap)) continue;
                            processedBaiTapIds.Add(bt.MaBaiTap);

                            var allSubs = await _context.BaiNops
                                .Where(b => b.MaBaiTap == bt.MaBaiTap && b.MaHocSinh == studentId)
                                .OrderByDescending(b => b.ThoiDiemNop)
                                .Select(b => new { b.DiemSo, b.ThoiDiemNop })
                                .ToListAsync();

                            var latestSub = allSubs.FirstOrDefault();
                            var gradedSub = allSubs.FirstOrDefault(b => b.DiemSo.HasValue);
                            decimal? finalScore = latestSub?.DiemSo ?? gradedSub?.DiemSo;

                            string itemStatus = "chua_nop";
                            if (latestSub != null)
                            {
                                itemStatus = finalScore.HasValue ? "da_cham" : "cho_cham";
                            }

                            detail.Items.Add(new GradeItemDto
                            {
                                ItemId = bt.MaBaiTap,
                                ItemName = bt.TieuDe,
                                Grade = finalScore,
                                Status = itemStatus,
                                SubmittedAt = latestSub?.ThoiDiemNop,
                                IsSubmitted = latestSub != null
                            });
                        }
                    }
                    else if (loaiCode == "quiz" || loaiCode == "progress_test")
                    {
                        detail.AverageGrade = await _gradeService.CalculateQuizGradeAsync(studentId, monHocId, hocKyId.Value, loaiCode, config);
                        if (detail.AverageGrade.HasValue)
                        {
                            detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);
                        }

                        string expectedLoaiDeThi = loaiCode == "quiz" ? "quiz_bai_hoc" : "progress_test";
                        var deKiemTras = await _context.DeKiemTras
                            .Where(d => d.MaMonHoc == monHocId && (d.MaHocKy == null || d.MaHocKy == hocKyId.Value) && (d.LoaiDeThi == expectedLoaiDeThi || d.LoaiDeThi == null))
                            .OrderBy(d => d.MaDeKiemTra)
                            .Select(d => new { d.MaDeKiemTra, d.TieuDe, d.CauHinhDeThi })
                            .ToListAsync();

                        foreach (var dk in deKiemTras)
                        {
                            var attempts = await _context.PhienThiHocSinhs
                                .Where(x => x.MaDeKiemTra == dk.MaDeKiemTra && x.MaHocSinh == studentId && x.TrangThaiLuong == "da_dung")
                                .ToListAsync();

                            var scoredAttempts = attempts.Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue).ToList();

                            decimal? testScore = null;
                            if (scoredAttempts.Any())
                            {
                                var quizConfig = QuizConfigurationDto.Parse(dk.CauHinhDeThi);
                                switch (quizConfig.CachTinhDiemCuoi)
                                {
                                    case "lan_cuoi":
                                        var last = scoredAttempts.OrderByDescending(x => x.LanThu).First();
                                        testScore = last.DiemCuoiCung ?? last.DiemTuDong ?? 0;
                                        break;
                                    case "trung_binh":
                                        testScore = scoredAttempts.Average(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                                        break;
                                    default:
                                        testScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                                        break;
                                }
                            }

                            detail.Items.Add(new GradeItemDto
                            {
                                ItemId = dk.MaDeKiemTra,
                                ItemName = dk.TieuDe,
                                Grade = testScore,
                                Status = attempts.Any() ? (testScore.HasValue ? "da_cham" : "cho_cham") : "chua_nop",
                                IsSubmitted = attempts.Any(),
                                SubmittedAt = attempts.OrderByDescending(a => a.NopLuc).Select(a => a.NopLuc ?? a.BatDauLuc).FirstOrDefault()
                            });
                        }
                    }

                    gradeTypes.Add(detail);
                }
            }

            // Populate Student Activities in this subject / class
            var activities = new List<StudentActivityDto>();

            // 1. Submissions
            var studentSubs = await _context.BaiNops
                .Include(b => b.BaiTap)
                .Where(b => b.MaHocSinh == studentId && b.BaiTap != null && b.BaiTap.MaMonHoc == monHocId)
                .OrderByDescending(b => b.ThoiDiemNop)
                .Take(20)
                .ToListAsync();

            foreach (var sub in studentSubs)
            {
                activities.Add(new StudentActivityDto
                {
                    Type = "assignment",
                    Title = $"Nộp bài: {sub.BaiTap?.TieuDe ?? "Bài tập"}",
                    Description = sub.DiemSo.HasValue ? $"Đã chấm điểm: {sub.DiemSo.Value:0.#} / 10" : "Đã nộp thành công, chờ giảng viên chấm bài",
                    Status = sub.DiemSo.HasValue ? "da_cham" : "cho_cham",
                    Score = sub.DiemSo,
                    Timestamp = sub.ThoiDiemNop
                });
            }

            // 2. Attendance
            var attendances = await _context.DiemDanhs
                .Include(d => d.BuoiHoc)
                .Where(d => d.MaHocSinh == studentId && d.BuoiHoc != null && (d.BuoiHoc.MaKhoaHoc == khoahoc.MaKhoaHoc || (d.BuoiHoc.KhoaHoc != null && d.BuoiHoc.KhoaHoc.MaLop == khoahoc.MaLop)))
                .OrderByDescending(d => d.GhiNhanLuc)
                .Take(15)
                .ToListAsync();

            foreach (var att in attendances)
            {
                string statusDesc = att.TrangThai switch
                {
                    "co_mat" => "Có mặt đầy đủ",
                    "muon" => "Đi muộn",
                    "vang" => "Vắng mặt không phép",
                    "phep" => "Vắng có phép",
                    _ => att.TrangThai
                };
                activities.Add(new StudentActivityDto
                {
                    Type = "attendance",
                    Title = "Điểm danh buổi học",
                    Description = statusDesc,
                    Status = att.TrangThai,
                    Timestamp = att.GhiNhanLuc
                });
            }

            // 3. Quiz / Exam attempts
            var quizAttempts = await _context.PhienThiHocSinhs
                .Include(p => p.DeKiemTra)
                .Where(p => p.MaHocSinh == studentId && p.DeKiemTra != null && p.DeKiemTra.MaMonHoc == monHocId)
                .OrderByDescending(p => p.BatDauLuc)
                .Take(10)
                .ToListAsync();

            foreach (var qa in quizAttempts)
            {
                decimal? score = qa.DiemCuoiCung ?? qa.DiemTuDong;
                activities.Add(new StudentActivityDto
                {
                    Type = "quiz",
                    Title = $"Làm bài kiểm tra: {qa.DeKiemTra?.TieuDe ?? "Bài kiểm tra"}",
                    Description = score.HasValue ? $"Hoàn thành đạt: {score.Value:0.#} / 10" : "Đang thực hiện bài thi",
                    Status = qa.TrangThaiLuong ?? "da_thi",
                    Score = score,
                    Timestamp = qa.NopLuc ?? qa.BatDauLuc
                });
            }

            decimal? dQT = diemRecord?.DiemQuaTrinh;
            if (!dQT.HasValue)
            {
                var scoredGrades = gradeTypes.Where(g => g.AverageGrade.HasValue).Select(g => g.AverageGrade!.Value).ToList();
                if (scoredGrades.Any())
                {
                    dQT = Math.Round(scoredGrades.Average(), 2);
                }
            }

            decimal? gpa = diemRecord?.GpaMonHoc;
            if (!gpa.HasValue && dQT.HasValue && diemRecord?.DiemGiuaKy != null && diemRecord?.DiemCuoiKy != null)
            {
                gpa = Math.Round(dQT.Value * 0.3m + diemRecord.DiemGiuaKy.Value * 0.2m + diemRecord.DiemCuoiKy.Value * 0.5m, 2);
            }

            var result = new StudentGradeDetailDto
            {
                StudentId = student.MaNguoiDung,
                StudentName = student.HoTen,
                GradeTypes = gradeTypes,
                Activities = activities.OrderByDescending(a => a.Timestamp).ToList(),
                DiemQuaTrinh = dQT,
                DiemGiuaKy = diemRecord?.DiemGiuaKy,
                DiemCuoiKy = diemRecord?.DiemCuoiKy,
                GpaMonHoc = gpa,
                TrangThai = diemRecord?.TrangThai == "dat" ? "Đạt" : (diemRecord?.TrangThai == "rot" ? "Rớt" : (gpa.HasValue ? (gpa.Value >= 5.0m ? "Đạt" : "Rớt") : (diemRecord?.TrangThai != "draft" ? diemRecord?.TrangThai : null))),
                DaKhoa = diemRecord?.DaKhoa ?? false
            };

            return Ok(ApiResponseDto<StudentGradeDetailDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải chi tiết điểm: " + ex.Message));
        }
    }

    [HttpPost("classes/{id}/grades/{studentId}/lock")]
    public async Task<ActionResult<ApiResponseDto<object>>> LockStudentGrade(int id, int studentId)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoahoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
                return BadRequest(ApiResponseDto.Fail("Khóa học chưa được gán học kỳ."));

            var diemRecord = await _context.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId.Value);

            if (diemRecord == null)
                return BadRequest(ApiResponseDto.Fail("Học sinh chưa có dữ liệu điểm. Cần tính điểm trước khi khoá."));

            if (diemRecord.TrangThai == "draft" || string.IsNullOrEmpty(diemRecord.TrangThai))
                return BadRequest(ApiResponseDto.Fail("Bảng điểm chưa được tính toán hoàn chỉnh (trạng thái draft). Cần chạy tính điểm trước khi khoá."));

            if (diemRecord.DaKhoa)
                return Conflict(ApiResponseDto.Fail("Bảng điểm đã được khoá trước đó."));

            // Lock
            diemRecord.DaKhoa = true;

            // Audit log
            var auditLog = new NhatKyThayDoiDiem
            {
                MaDiemSo = diemRecord.MaDiemSo,
                NguoiThayDoi = userId,
                GiaTriCu = JsonSerializer.Serialize(new { DaKhoa = false }),
                GiaTriMoi = JsonSerializer.Serialize(new { DaKhoa = true }),
                LyDo = "Giáo viên khoá bảng điểm",
                ThayDoiLuc = DateTime.UtcNow
            };
            _context.NhatKyThayDoiDiems.Add(auditLog);

            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { message = "Đã khoá bảng điểm thành công." }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi khoá bảng điểm: " + ex.Message));
        }
    }

    [HttpPost("classes/{id}/grades/{studentId}/unlock")]
    public async Task<ActionResult<ApiResponseDto<object>>> UnlockStudentGrade(int id, int studentId, [FromBody] UnlockGradeRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var khoahoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
                return BadRequest(ApiResponseDto.Fail("Khóa học chưa được gán học kỳ."));

            var diemRecord = await _context.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId.Value);

            if (diemRecord == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy dữ liệu điểm của học sinh."));

            if (!diemRecord.DaKhoa)
                return BadRequest(ApiResponseDto.Fail("Bảng điểm chưa được khoá, không cần yêu cầu mở khoá."));

            // Check for existing pending unlock request
            var existingRequest = await _context.YeuCauSuaDiems
                .AnyAsync(y => y.MaDiemSo == diemRecord.MaDiemSo && y.TrangThai == "cho_duyet" && y.LoaiYeuCau == "mo_khoa_bang_diem");

            if (existingRequest)
                return Conflict(ApiResponseDto.Fail("Đã có yêu cầu mở khoá đang chờ duyệt cho bảng điểm này."));

            if (string.IsNullOrWhiteSpace(request.LyDo))
                return BadRequest(ApiResponseDto.Fail("Vui lòng cung cấp lý do yêu cầu mở khoá."));

            // Create YeuCauSuaDiem for approval
            var yeuCau = new YeuCauSuaDiem
            {
                MaDiemSo = diemRecord.MaDiemSo,
                NguoiYeuCau = userId,
                LyDo = request.LyDo,
                TrangThai = "cho_duyet",
                LoaiYeuCau = "mo_khoa_bang_diem"
            };
            _context.YeuCauSuaDiems.Add(yeuCau);

            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                message = "Đã gửi yêu cầu mở khoá bảng điểm. Vui lòng chờ duyệt.",
                requestId = yeuCau.MaYcSuaDiem
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi gửi yêu cầu mở khoá: " + ex.Message));
        }
    }
}

public class UpdateGradeRequest
{
    public decimal? Assignment { get; set; }
    public decimal? Exam { get; set; }
}

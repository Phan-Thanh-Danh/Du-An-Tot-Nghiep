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
[Authorize(Roles = "Teacher")]
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
    public async Task<ActionResult<ApiResponseDto<object>>> GetClasses()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var result = await _context.KhoaHocs
                .Where(k => k.MaGiaoVien == userId)
                .GroupBy(k => new { k.MaLop, TenLop = k.Lop != null ? k.Lop.TenLop : "" })
                .Select(g => new
                {
                    ClassId = g.Key.MaLop,
                    ClassName = g.Key.TenLop,
                    CourseCount = g.Count(),
                    StudentCount = _context.NguoiDungs.Count(n => n.MaLop == g.Key.MaLop)
                })
                .ToListAsync();

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

            // 1. Lấy danh sách các KhoaHoc thuộc lớp id do giáo viên này phụ trách
            var khoaHocs = await _context.KhoaHocs
                .Where(k => k.MaLop == id && k.MaGiaoVien == userId)
                .Select(k => k.MaKhoaHoc)
                .ToListAsync();

            if (!khoaHocs.Any())
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp học hoặc bạn không được phân công giảng dạy lớp này."));

            // 2. Lấy danh sách các BuoiHoc đã diễn ra của các khóa học này
            var completedSessions = await _context.BuoiHocs
                .Where(b => khoaHocs.Contains(b.MaKhoaHoc) && b.TrangThaiBuoi == "da_dien_ra")
                .Select(b => b.MaBuoiHoc)
                .ToListAsync();

            int totalSessions = completedSessions.Count;

            // 3. Lấy danh sách sinh viên trong lớp
            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id)
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
                
                int percent = totalSessions > 0 ? (int)Math.Round((double)present / totalSessions * 100) : 100;
                
                string status = "excellent";
                if (percent < 50) status = "danger";
                else if (percent < 70) status = "warning";
                else if (percent < 90) status = "good";

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
                .Where(k => k.MaGiaoVien == userId)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(k => k.MaLop == classId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(k => k.TieuDe.Contains(keyword) || (k.Lop != null && k.Lop.TenLop.Contains(keyword)));
            }

            var courses = await query
                .Select(k => new
                {
                    CourseId = k.MaKhoaHoc,
                    CourseName = k.TieuDe,
                    SubjectCode = k.MonHoc != null ? k.MonHoc.MaCodeMonHoc : "",
                    ClassName = k.Lop != null ? k.Lop.TenLop : "",
                    ClassId = k.MaLop,
                    StudentCount = _context.NguoiDungs.Count(n => n.MaLop == k.MaLop),
                    Semester = "N/A"
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(courses));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải danh sách khóa học: " + ex.Message));
        }
    }

    [HttpGet("classes/{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetClassDetail(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var hasCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (!hasCourse)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp hoặc bạn không dạy lớp này."));

            var lop = await _context.LopHanhChinhs
                .Where(l => l.MaLop == id)
                .Select(l => new { l.MaLop, l.TenLop })
                .FirstAsync();

            var courses = await _context.KhoaHocs
                .Where(k => k.MaLop == id && k.MaGiaoVien == userId)
                .Select(k => new
                {
                    CourseId = k.MaKhoaHoc,
                    CourseName = k.TieuDe,
                    SubjectCode = k.MonHoc != null ? k.MonHoc.MaCodeMonHoc : "",
                    StudentCount = _context.NguoiDungs.Count(n => n.MaLop == id)
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassId = lop.MaLop,
                ClassName = lop.TenLop,
                Courses = courses
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

            var hasCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (!hasCourse)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp hoặc bạn không dạy lớp này."));

            var lop = await _context.LopHanhChinhs
                .Include(l => l.ChuongTrinh)
                    .ThenInclude(c => c.ChuyenNganh)
                .Where(l => l.MaLop == id)
                .Select(l => new 
                { 
                    l.MaLop, 
                    l.TenLop,
                    ChuyenNganh = l.ChuongTrinh != null && l.ChuongTrinh.ChuyenNganh != null ? l.ChuongTrinh.ChuyenNganh.TenChuyenNganh : ""
                })
                .FirstOrDefaultAsync();

            if (lop == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp học."));

            var courses = await _context.KhoaHocs
                .Include(k => k.MonHoc)
                .Where(k => k.MaLop == id && k.MaGiaoVien == userId)
                .ToListAsync();

            if (!courses.Any())
                return NotFound(ApiResponseDto.Fail("Bạn không dạy khoá học nào trong lớp này."));

            var courseIds = courses.Select(c => c.MaKhoaHoc).ToList();

            var currentSession = await _context.BuoiHocs
                .Include(b => b.Phong)
                .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.MaGiaoVien == userId && b.NgayHoc == DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(b => b.MaCaHoc)
                .FirstOrDefaultAsync();

            if (currentSession == null)
            {
                currentSession = await _context.BuoiHocs
                    .Include(b => b.Phong)
                    .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.MaGiaoVien == userId)
                    .OrderByDescending(b => b.NgayHoc)
                    .ThenBy(b => b.MaCaHoc)
                    .FirstOrDefaultAsync();
            }

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id && n.VaiTroChinh == "hoc_sinh")
                .OrderBy(n => n.HoTen)
                .ThenBy(n => n.MaNguoiDung)
                .Select(n => new
                {
                    Id = n.MaNguoiDung,
                    Name = n.HoTen,
                    Email = n.Email,
                    Avatar = "",
                    Present = true
                })
                .ToListAsync();

            var monHocIds = courses.Select(c => c.MaMonHoc).ToList();
            var modules = new List<object>();
            if (monHocIds.Count > 0)
            {
                modules = await _context.BaiHocs
                    .Where(b => b.Chuong != null && monHocIds.Contains(b.Chuong.MaMonHoc))
                    .Select(b => new
                    {
                        Id = b.MaBaiHoc,
                        Title = b.TieuDe,
                        Duration = "45 phút",
                        Status = "locked",
                        Type = "video"
                    })
                    .Take(10)
                    .ToListAsync<object>();
            }

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassName = lop.TenLop,
                ChuyenNganh = lop.ChuyenNganh,
                PhongHoc = currentSession?.Phong?.TenPhong,
                SessionId = currentSession?.MaBuoiHoc,
                SessionStatus = currentSession?.TrangThaiDiemDanh,
                SessionDate = currentSession?.NgayHoc.ToDateTime(new TimeOnly(0, 0)),
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

            var hasCourse = await _context.KhoaHocs
                .AnyAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (!hasCourse)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp hoặc bạn không dạy lớp này."));

            var lop = await _context.LopHanhChinhs
                .Where(l => l.MaLop == id)
                .Select(l => new { l.MaLop, l.TenLop })
                .FirstAsync();

            var monHocIds = await _context.KhoaHocs
                .Where(k => k.MaLop == id && k.MaGiaoVien == userId)
                .Select(k => k.MaMonHoc)
                .Distinct()
                .ToListAsync();

            var lessonIds = await _context.BaiHocs
                .Where(b => b.Chuong != null && monHocIds.Contains(b.Chuong.MaMonHoc))
                .Select(b => b.MaBaiHoc)
                .ToListAsync();

            var totalLessons = lessonIds.Count;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id && n.VaiTroChinh == "hoc_sinh")
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    Email = n.Email,
                    CoursesCompleted = lessonIds.Count > 0
                        ? _context.TienDoBaiHocs.Count(t => t.MaHocSinh == n.MaNguoiDung && lessonIds.Contains(t.MaBaiHoc) && t.HoanThanhLuc != null)
                        : 0,
                    Absent = _context.DiemDanhs.Count(d => d.MaHocSinh == n.MaNguoiDung && d.TrangThai == "vang")
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
                    gpa = 0, // Mock GPA for now
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
                    Absent = _context.DiemDanhs.Count(d => d.MaHocSinh == n.MaNguoiDung && d.TrangThai == "vang" && d.BuoiHoc != null && d.BuoiHoc.MaKhoaHoc == id)
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
                    gpa = 0, // Mock GPA for now
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
                .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id)
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
                .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
                
            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id)
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
            string className = khoahoc.Lop?.TenLop ?? $"Lớp {id}";
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
                    .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId.Value && k.MaLop == id && k.MaGiaoVien == userId);
            }
            else
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .Include(k => k.Lop)
                    .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            }

            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
                return BadRequest(ApiResponseDto.Fail("Khóa học chưa được gán học kỳ, không thể xem bảng điểm."));

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

            // Load students
            var students = await _context.NguoiDungs
                .Where(n => n.MaLop == id && n.VaiTroChinh == "hoc_sinh")
                .OrderBy(n => n.HoTen)
                .Select(n => new { n.MaNguoiDung, n.HoTen })
                .ToListAsync();

            // Load all DiemSo records for this class/subject/term in one query
            var studentIds = students.Select(s => s.MaNguoiDung).ToList();
            var diemRecords = await _context.DiemSos
                .Where(d => studentIds.Contains(d.MaHocSinh) && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId.Value)
                .ToListAsync();

            var studentGrades = new List<StudentGradeSummaryDto>();

            foreach (var student in students)
            {
                var diemRecord = diemRecords.FirstOrDefault(d => d.MaHocSinh == student.MaNguoiDung);

                var typeGrades = new Dictionary<string, decimal?>();
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

                studentGrades.Add(new StudentGradeSummaryDto
                {
                    StudentId = student.MaNguoiDung,
                    StudentName = student.HoTen,
                    TypeGrades = typeGrades,
                    DiemQuaTrinh = diemRecord?.DiemQuaTrinh,
                    DiemGiuaKy = diemRecord?.DiemGiuaKy,
                    DiemCuoiKy = diemRecord?.DiemCuoiKy,
                    GpaMonHoc = diemRecord != null ? diemRecord.GpaMonHoc : null,
                    TrangThai = diemRecord?.TrangThai == "dat" ? "Đạt" : (diemRecord?.TrangThai == "rot" ? "Rớt" : (diemRecord?.TrangThai != "draft" ? diemRecord?.TrangThai : null)),
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
                    .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId.Value && k.MaLop == id && k.MaGiaoVien == userId);
            }
            else
            {
                khoahoc = await _context.KhoaHocs
                    .Include(k => k.MonHoc)
                    .FirstOrDefaultAsync(k => k.MaLop == id && k.MaGiaoVien == userId);
            }

            if (khoahoc == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy môn học bạn dạy trong lớp này."));

            var monHocId = khoahoc.MaMonHoc;
            var hocKyId = khoahoc.MaHocKy;

            if (!hocKyId.HasValue)
                return BadRequest(ApiResponseDto.Fail("Khóa học chưa được gán học kỳ."));

            // Verify student belongs to this class
            var student = await _context.NguoiDungs
                .Where(n => n.MaNguoiDung == studentId && n.MaLop == id)
                .Select(n => new { n.MaNguoiDung, n.HoTen })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy học sinh trong lớp này."));

            var diemRecord = await _context.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == studentId && d.MaMonHoc == monHocId && d.MaHocKy == hocKyId.Value);

            var configs = await _context.CauHinhDauDiemQuaTrinhs
                .Include(x => x.LoaiDauDiem)
                .Where(x => x.MaMonHoc == monHocId && x.MaHocKy == hocKyId.Value)
                .OrderBy(x => x.LoaiDauDiem != null ? x.LoaiDauDiem.ThuTuHienThi : 0)
                .ToListAsync();

            var gradeTypes = new List<GradeTypeDetailDto>();

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
                    // Attendance has no sub-items breakdown at assignment level
                }
                else if (loaiCode == "lab" || loaiCode == "assignment")
                {
                    detail.AverageGrade = await _gradeService.CalculateAssignmentGradeAsync(studentId, monHocId, config);
                    if (detail.AverageGrade.HasValue)
                    {
                        detail.AverageGrade = Math.Round(detail.AverageGrade.Value, 2);
                    }

                    // Get per-item breakdown
                    var baiTaps = await _context.BaiTaps
                        .Where(b => b.MaMonHoc == monHocId && b.MaCauHinhDauDiem == config.MaCauHinhDauDiem)
                        .OrderBy(b => b.MaBaiTap)
                        .Select(b => new { b.MaBaiTap, b.TieuDe })
                        .ToListAsync();

                    foreach (var bt in baiTaps)
                    {
                        // Latest submission per assignment
                        var latestSub = await _context.BaiNops
                            .Where(b => b.MaBaiTap == bt.MaBaiTap && b.MaHocSinh == studentId)
                            .OrderByDescending(b => b.ThoiDiemNop)
                            .Select(b => new { b.DiemSo })
                            .FirstOrDefaultAsync();

                        detail.Items.Add(new GradeItemDto
                        {
                            ItemId = bt.MaBaiTap,
                            ItemName = bt.TieuDe,
                            Grade = latestSub?.DiemSo // null = chưa nộp, 0 = có nộp nhưng 0 điểm
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
                        .Where(d => d.MaMonHoc == monHocId && d.MaHocKy == hocKyId.Value && d.LoaiDeThi == expectedLoaiDeThi)
                        .OrderBy(d => d.MaDeKiemTra)
                        .Select(d => new { d.MaDeKiemTra, d.TieuDe, d.CauHinhDeThi })
                        .ToListAsync();

                    foreach (var dk in deKiemTras)
                    {
                        var attempts = await _context.PhienThiHocSinhs
                            .Where(x => x.MaDeKiemTra == dk.MaDeKiemTra && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "da_dung")
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
                            Grade = testScore // null = chưa làm
                        });
                    }
                }

                gradeTypes.Add(detail);
            }

            var result = new StudentGradeDetailDto
            {
                StudentId = student.MaNguoiDung,
                StudentName = student.HoTen,
                GradeTypes = gradeTypes,
                DiemQuaTrinh = diemRecord?.DiemQuaTrinh,
                DiemGiuaKy = diemRecord?.DiemGiuaKy,
                DiemCuoiKy = diemRecord?.DiemCuoiKy,
                GpaMonHoc = diemRecord != null ? diemRecord.GpaMonHoc : null,
                TrangThai = diemRecord?.TrangThai != "draft" ? diemRecord?.TrangThai : null,
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

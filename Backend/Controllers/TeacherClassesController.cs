using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherClassesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherClassesController(ApplicationDbContext context)
    {
        _context = context;
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
                    GpaMonHoc = ((request.Assignment ?? 0) * 0.4m) + ((request.Exam ?? 0) * 0.6m),
                    TrangThai = "draft",
                    DaKhoa = false
                };
                _context.DiemSos.Add(diem);
            }
            else
            {
                diem.DiemQuaTrinh = request.Assignment;
                diem.DiemCuoiKy = request.Exam;
                diem.GpaMonHoc = ((request.Assignment ?? 0) * 0.4m) + ((request.Exam ?? 0) * 0.6m);
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { message = "Cập nhật điểm thành công" }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi cập nhật điểm: " + ex.Message));
        }
    }
}

public class UpdateGradeRequest
{
    public decimal? Assignment { get; set; }
    public decimal? Exam { get; set; }
}

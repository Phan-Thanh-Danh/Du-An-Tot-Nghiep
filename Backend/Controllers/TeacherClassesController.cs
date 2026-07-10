using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
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

            var monHocIds = await _context.KhoaHocs
                .Where(k => k.MaLop == id && k.MaGiaoVien == userId)
                .Select(k => k.MaMonHoc)
                .Distinct()
                .ToListAsync();

            var submissions = new List<object>();
            var comments = new List<object>();

            if (monHocIds.Count > 0)
            {
                submissions = await _context.BaiNops
                    .Where(b => b.BaiTap != null && monHocIds.Contains(b.BaiTap.MaMonHoc))
                    .OrderByDescending(b => b.ThoiDiemNop)
                    .Take(5)
                    .Select(b => new
                    {
                        Type = "nop_bai",
                        Description = (b.HocSinh != null ? b.HocSinh.HoTen : "") + " nộp bài " + (b.BaiTap != null ? b.BaiTap.TieuDe : ""),
                        Time = b.ThoiDiemNop
                    })
                    .ToListAsync<object>();

                comments = await _context.BinhLuans
                    .Where(c => c.BaiHoc != null && c.BaiHoc.Chuong != null && monHocIds.Contains(c.BaiHoc.Chuong.MaMonHoc))
                    .OrderByDescending(c => c.NgayTao)
                    .Take(5)
                    .Select(c => new
                    {
                        Type = "binh_luan",
                        Description = (c.NguoiDung != null ? c.NguoiDung.HoTen : "") + " bình luận: " + c.NoiDung,
                        Time = c.NgayTao
                    })
                    .ToListAsync<object>();
            }

            var recentActivity = submissions
                .Concat(comments)
                .OrderByDescending(a => ((dynamic)a).Time)
                .Take(10)
                .ToList();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassId = lop.MaLop,
                ClassName = lop.TenLop,
                Courses = courses,
                RecentActivity = recentActivity
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
                .Where(n => n.MaLop == id)
                .Select(n => new
                {
                    StudentId = n.MaNguoiDung,
                    StudentName = n.HoTen,
                    CoursesCompleted = lessonIds.Count > 0
                        ? _context.TienDoBaiHocs.Count(t => t.MaHocSinh == n.MaNguoiDung && lessonIds.Contains(t.MaBaiHoc) && t.HoanThanhLuc != null)
                        : 0
                })
                .ToListAsync();

            var result = students.Select(s => new
            {
                StudentId = s.StudentId,
                StudentName = s.StudentName,
                CoursesCompleted = s.CoursesCompleted,
                CoursesTotal = totalLessons,
                OverallProgress = totalLessons > 0 ? Math.Round((decimal)s.CoursesCompleted / totalLessons * 100, 1) : 0m
            }).ToList();

            return Ok(ApiResponseDto<object>.Ok(new
            {
                ClassId = lop.MaLop,
                ClassName = lop.TenLop,
                Students = result
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải tiến độ lớp: " + ex.Message));
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

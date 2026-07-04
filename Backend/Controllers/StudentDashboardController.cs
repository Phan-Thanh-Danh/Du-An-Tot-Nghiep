using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = "Student")]
public class StudentDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<StudentDashboardDto>>> GetDashboard()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = currentUser!.UserId;

        var student = await _context.NguoiDungs
            .Include(n => n.Lop)
            .FirstOrDefaultAsync(n => n.MaNguoiDung == userId);

        if (student == null) return Unauthorized();

        var courses = await _context.KhoaHocs
            .Include(k => k.MonHoc)
            .Include(k => k.GiaoVien)
            .Include(k => k.HocKy)
            .Where(k => k.MaLop == student.MaLop)
            .ToListAsync();

        var courseIds = courses.Select(c => c.MaKhoaHoc).ToList();
        var subjectIds = courses.Select(c => c.MaMonHoc).Distinct().ToList();

        // 1. Assignments (BaiTap & BaiNop)
        var assignments = await _context.BaiTaps
            .Include(b => b.MonHoc)
            .Where(b => subjectIds.Contains(b.MaMonHoc))
            .ToListAsync();

        var assignmentIds = assignments.Select(a => a.MaBaiTap).ToList();
        
        var submissions = await _context.BaiNops
            .Where(b => b.MaHocSinh == userId && assignmentIds.Contains(b.MaBaiTap))
            .ToListAsync();

        // 2. Schedule (Today's BuoiHoc)
        // Adjust for Vietnam time +7
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); 
        var todaySessions = await _context.BuoiHocs
            .Include(b => b.KhoaHoc)
                .ThenInclude(k => k!.MonHoc)
            .Include(b => b.Phong)
            .Include(b => b.CaHoc)
            .Where(b => courseIds.Contains(b.MaKhoaHoc) && b.NgayHoc == today && b.TrangThaiBuoi != "da_huy")
            .OrderBy(b => b.CaHoc != null ? b.CaHoc.GioBatDau : default)
            .ToListAsync();

        // 3. Attendance
        var attendanceRecords = await _context.DiemDanhs
            .Include(d => d.BuoiHoc)
            .Where(d => d.MaHocSinh == userId && courseIds.Contains(d.BuoiHoc!.MaKhoaHoc))
            .ToListAsync();
            
        // Calculate Attendance Health
        var totalSessions = attendanceRecords.Count;
        var presentSessions = attendanceRecords.Count(a => a.TrangThai == "co_mat" || a.TrangThai == "di_tre");
        var attendanceScore = totalSessions > 0 ? (int)Math.Round((double)presentSessions / totalSessions * 100) : 100;

        // 4. Grades
        var grades = await _context.DiemSos
            .Include(d => d.MonHoc)
            .Where(d => d.MaHocSinh == userId)
            .OrderByDescending(d => d.MaDiemSo)
            .Take(5)
            .ToListAsync();

        // Focus Summary Math
        var dueAssignmentsCount = assignments.Count(a => a.HanNop >= DateTime.UtcNow && a.HanNop <= DateTime.UtcNow.AddDays(7) && !submissions.Any(s => s.MaBaiTap == a.MaBaiTap));
        var nextDeadline = assignments.Where(a => a.HanNop >= DateTime.UtcNow && !submissions.Any(s => s.MaBaiTap == a.MaBaiTap)).OrderBy(a => a.HanNop).Select(a => a.HanNop).FirstOrDefault();

        // Compose DTO
        var dashboardData = new StudentDashboardDto
        {
            Student = new StudentInfoDto
            {
                Name = student.HoTen,
                Code = student.MaNguoiDung.ToString(),
                ClassName = student.Lop?.TenLop ?? "Không có lớp",
                Semester = courses.FirstOrDefault()?.HocKy?.TenHocKy ?? "Đang học",
            },
            WeekProgress = 50,
            FocusSummary = new FocusSummaryDto
            {
                ClassesToday = todaySessions.Count,
                AssignmentsDue = dueAssignmentsCount,
                CompletedThisWeek = submissions.Count(s => s.ThoiDiemNop >= DateTime.UtcNow.AddDays(-7)),
                NearestDeadline = nextDeadline != default ? nextDeadline.ToString("dd/MM/yyyy HH:mm") : "Không có",
                Gpa = "8.5", 
            },
            Kpis = new List<KpiDto>
            {
                new() { Id = "courses", Label = "Khóa học đang học", Value = courses.Count.ToString(), Trend = "", Tone = "blue", Route = "/student/courses" },
                new() { Id = "assignments", Label = "Bài tập cần xử lý", Value = dueAssignmentsCount.ToString(), Trend = "", Tone = "amber", Route = "/student/assignments" },
                new() { Id = "gpa", Label = "GPA học kỳ", Value = "8.5", Trend = "", Tone = "violet", Route = "/student/grades" },
                new() { Id = "attendance", Label = "Chuyên cần", Value = $"{attendanceScore}%", Trend = "", Tone = "teal", Route = "/student/attendance" }
            },
            Courses = courses.Select(c => new CourseProgressDto
            {
                Id = c.MaKhoaHoc.ToString(),
                Name = c.MonHoc?.TenMonHoc ?? "",
                Code = c.MonHoc?.MaCodeMonHoc ?? "",
                Lecturer = c.GiaoVien?.HoTen ?? "",
                Progress = 50,
                Completed = 5,
                Total = 10,
                Status = "Đang học",
                StatusVariant = "info"
            }).ToList(),
            Assignments = assignments.OrderBy(a => a.HanNop).Take(5).Select(a => {
                var isSubmitted = submissions.Any(s => s.MaBaiTap == a.MaBaiTap);
                return new AssignmentDto
                {
                    Id = a.MaBaiTap.ToString(),
                    Title = a.TieuDe ?? "",
                    Course = a.MonHoc?.TenMonHoc ?? "",
                    Deadline = a.HanNop.ToString("dd/MM/yyyy HH:mm"),
                    Status = isSubmitted ? "Đã nộp" : "Chưa làm",
                    Variant = isSubmitted ? "success" : "danger",
                    Priority = "high"
                };
            }).ToList(),
            Schedule = todaySessions.Select(s => new ScheduleDto
            {
                Id = s.MaBuoiHoc.ToString(),
                Course = s.KhoaHoc?.MonHoc?.TenMonHoc ?? "",
                Code = s.KhoaHoc?.MonHoc?.MaCodeMonHoc ?? "",
                Time = s.CaHoc != null ? $"{s.CaHoc.GioBatDau:HH:mm} - {s.CaHoc.GioKetThuc:HH:mm}" : "",
                Room = s.Phong?.TenPhong ?? "",
                Type = "Lý thuyết",
                TypeVariant = "info",
                Status = s.CaHoc?.GioBatDau <= TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(7)) && s.CaHoc?.GioKetThuc >= TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(7)) ? "Đang diễn ra" : "Sắp tới",
                StatusVariant = "success"
            }).ToList(),
            Grades = grades.Select(g => new GradeDto
            {
                Id = g.MaDiemSo.ToString(),
                Course = g.MonHoc?.TenMonHoc ?? "",
                Code = g.MonHoc?.MaCodeMonHoc ?? "",
                ExamType = "Tổng kết",
                Score = (double)(g.DiemCuoiKy ?? 0),
                Date = "N/A",
                Status = (g.DiemCuoiKy ?? 0) >= 5 ? "passed" : "failed"
            }).ToList(),
            Tuition = new TuitionDto
            {
                TotalDue = "0",
                Deadline = "N/A",
                Progress = 100,
                Status = "Đã thanh toán",
                StatusVariant = "success"
            },
            Registration = new RegistrationDto
            {
                Semester = "HK3 2026",
                StartDate = "N/A",
                Status = "Đóng",
                Action = "Xem"
            },
            Attendance = new AttendanceHealthDto
            {
                Score = attendanceScore,
                Status = attendanceScore > 80 ? "Tốt" : "Cần chú ý",
                Tone = attendanceScore > 80 ? "teal" : "warning",
                Advice = attendanceScore > 80 ? "Duy trì tốt!" : "Bạn cần đi học đều đặn hơn.",
                Risks = []
            },
            Notifications = new List<NotificationDto>()
        };

        return Ok(ApiResponseDto<StudentDashboardDto>.Ok(dashboardData));
    }
}

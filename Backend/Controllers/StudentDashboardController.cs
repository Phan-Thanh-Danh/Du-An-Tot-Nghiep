using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StudentDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<StudentDashboardDto>>> GetDashboard()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var studentId = currentUser!.UserId;

        var student = await _db.NguoiDungs
            .Include(n => n.Lop)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaNguoiDung == studentId);

        if (student == null)
            return NotFound(new ApiResponseDto<StudentDashboardDto> { Success = false, Message = "User not found" });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayDayOfWeek = (int)DateTime.UtcNow.DayOfWeek == 0 ? 7 : (int)DateTime.UtcNow.DayOfWeek;

        // 1. Courses
        var courses = await _db.KhoaHocs
            .Include(k => k.MonHoc)
            .Include(k => k.GiaoVien)
            .Where(k => k.MaLop == student.MaLop)
            .AsNoTracking()
            .ToListAsync();

        var courseDtos = courses.Select(c => new CourseProgressDto
        {
            Id = c.MaKhoaHoc.ToString(),
            Name = c.MonHoc?.TenMonHoc ?? "",
            Code = c.MonHoc?.MaMonHoc.ToString() ?? "",
            Lecturer = c.GiaoVien?.HoTen ?? "",
            Progress = 50, // Simplified for MVP
            Completed = 7,
            Total = 15,
            Status = c.TrangThai == "dang_hoc" ? "Đang học" : "Chờ học",
            StatusVariant = c.TrangThai == "dang_hoc" ? "info" : "secondary"
        }).ToList();

        // 2. Assignments
        var assignments = await _db.BaiNops
            .Include(b => b.BaiTap)
            .ThenInclude(bt => bt!.MonHoc)
            .Where(b => b.MaHocSinh == studentId)
            .OrderByDescending(b => b.ThoiDiemNop)
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        var assignmentDtos = assignments.Where(a => a.BaiTap != null).Select(a => new AssignmentDto
        {
            Id = a.MaBaiNop.ToString(),
            Title = a.BaiTap!.TieuDe,
            Course = a.BaiTap.MonHoc?.TenMonHoc ?? "",
            Deadline = a.BaiTap.HanNop.ToString("dd/MM/yyyy HH:mm"),
            Status = a.DiemSo.HasValue ? "Đã chấm" : "Đã nộp",
            Variant = a.DiemSo.HasValue ? "success" : "info",
            Priority = "medium"
        }).ToList();

        var pendingAssignments = await _db.BaiTaps
            .Where(bt => bt.HanNop >= DateTime.UtcNow && courses.Select(c => c.MaMonHoc).Contains(bt.MaMonHoc))
            .CountAsync();

        // 3. Schedule
        var schedules = await _db.ThoiKhoaBieus
            .Include(t => t.KhoaHoc)
            .ThenInclude(k => k!.MonHoc)
            .Include(t => t.Phong)
            .Include(t => t.CaHoc)
            .Where(t => t.KhoaHoc != null && t.KhoaHoc.MaLop == student.MaLop && t.ThuTrongTuan == todayDayOfWeek)
            .AsNoTracking()
            .ToListAsync();

        var scheduleDtos = schedules.Select(s => new ScheduleDto
        {
            Id = s.MaTkb.ToString(),
            Course = s.KhoaHoc?.MonHoc?.TenMonHoc ?? "",
            Code = s.KhoaHoc?.MonHoc?.MaMonHoc.ToString() ?? "",
            Time = s.CaHoc != null ? $"{s.CaHoc.GioBatDau:HH:mm} - {s.CaHoc.GioKetThuc:HH:mm}" : "",
            Room = s.Phong?.TenPhong ?? "",
            Type = "Lý thuyết",
            TypeVariant = "info",
            Status = "Hôm nay",
            StatusVariant = "success"
        }).ToList();

        // 4. Grades
        var grades = await _db.DiemSos
            .Include(d => d.MonHoc)
            .Where(d => d.MaHocSinh == studentId)
            .OrderByDescending(d => d.MaDiemSo)
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        var gradeDtos = grades.Select(g => new GradeDto
        {
            Id = g.MaDiemSo.ToString(),
            Course = g.MonHoc?.TenMonHoc ?? "",
            Code = g.MonHoc?.MaMonHoc.ToString() ?? "",
            ExamType = "Điểm môn học",
            Score = (double)g.GpaMonHoc,
            Date = "", 
            Status = g.TrangThai == "passed" ? "passed" : "failed"
        }).ToList();

        // 5. Notifications
        var notifications = await _db.ThongBaos
            .Where(t => t.MaNguoiNhan == studentId || t.PhamViGui == "toan_truong" || (t.LoaiDoiTuongLienKet == "LopHanhChinh" && t.MaDoiTuongLienKet == student.MaLop))
            .OrderByDescending(t => t.NgayTao)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        var notificationDtos = notifications.Select(n => new NotificationDto
        {
            Id = n.MaThongBao.ToString(),
            Title = n.TieuDe ?? "",
            Content = n.TomTat ?? n.TomTatNoiDung ?? "Không có nội dung",
            Time = n.NgayTao.ToString("dd/MM/yyyy HH:mm"),
            Category = n.LoaiThongBao,
            Unread = !n.DaDoc
        }).ToList();
        
        decimal avgGpa = grades.Any() ? grades.Average(g => g.GpaMonHoc) : 0;

        var dashboardData = new StudentDashboardDto
        {
            Student = new StudentInfoDto
            {
                Name = student.HoTen,
                Code = $"SV{student.NamNhapHoc ?? 2026}{student.MaNguoiDung.ToString("D4")}",
                ClassName = student.Lop?.TenLop ?? "Chưa xếp lớp",
                Semester = "HK Hiện tại",
            },
            WeekProgress = 50,
            FocusSummary = new FocusSummaryDto
            {
                ClassesToday = schedules.Count,
                AssignmentsDue = pendingAssignments,
                CompletedThisWeek = 0,
                NearestDeadline = assignments.FirstOrDefault()?.BaiTap?.HanNop.ToString("dd/MM/yyyy") ?? "Không có",
                Gpa = avgGpa.ToString("0.0")
            },
            Kpis = new List<KpiDto>
            {
                new() { Id = "courses", Label = "Khóa học đang học", Value = courses.Count.ToString(), Trend = "Đang diễn ra", Tone = "blue", Route = "/student/courses" },
                new() { Id = "assignments", Label = "Bài tập cần xử lý", Value = pendingAssignments.ToString(), Trend = "Đang chờ", Tone = "amber", Route = "/student/assignments" },
                new() { Id = "gpa", Label = "GPA tổng", Value = avgGpa.ToString("0.0"), Trend = "", Tone = "violet", Route = "/student/grades" },
            },
            Courses = courseDtos,
            Assignments = assignmentDtos,
            Schedule = scheduleDtos,
            Grades = gradeDtos,
            Notifications = notificationDtos,
            Tuition = new TuitionDto { TotalDue = "0", Deadline = "-", Progress = 100, Status = "Hoàn tất", StatusVariant = "success" },
            Registration = new RegistrationDto { Semester = "N/A", StartDate = "-", Status = "Chưa mở", Action = "" },
            Attendance = new AttendanceHealthDto { Score = 100, Status = "Tốt", Tone = "teal", Advice = "Vui lòng theo dõi lịch học." }
        };

        return Ok(ApiResponseDto<StudentDashboardDto>.Ok(dashboardData));
    }
}

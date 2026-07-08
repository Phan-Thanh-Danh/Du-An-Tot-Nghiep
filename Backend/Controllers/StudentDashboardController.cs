using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentDashboard;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = "Student")]
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
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            if (currentUser == null)
                return Unauthorized();

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // 1. Student info
            var user = await _db.NguoiDungs
                .Include(u => u.Lop)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUser.UserId);

            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông tin người dùng" });

            // 2. Current semester
            var currentHocKy = await _db.HocKys
                .Where(h => h.MaDonVi == currentUser.CampusId
                    && h.NgayBatDau <= today
                    && h.NgayKetThuc >= today)
                .OrderByDescending(h => h.NgayBatDau)
                .FirstOrDefaultAsync();

            // 3. Enrolled subjects (via DangKyHocPhan)
            var enrollments = await _db.DangKyHocPhans
                .Include(d => d.LopHocPhan!)
                    .ThenInclude(l => l.MonHoc)
                .Where(d => d.MaHocSinh == currentUser.UserId && d.TrangThai == "da_duyet")
                .ToListAsync();

            var enrolledMonHocIds = enrollments
                .Select(d => d.LopHocPhan?.MaMonHoc)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            // 4. Courses via KhoaHoc (student's class)
            var khoaHocs = await _db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .Include(k => k.Lop)
                .Where(k => k.MaLop == user.MaLop
                    && (currentHocKy == null || k.MaHocKy == currentHocKy.MaHocKy)
                    && k.TrangThai == "dang_mo")
                .ToListAsync();

            var allMonHocIds = enrolledMonHocIds
                .Union(khoaHocs.Select(k => k.MaMonHoc))
                .Distinct()
                .ToList();

            // 5. Lesson progress data
            var totalLessons = await _db.Chuongs
                .Where(c => allMonHocIds.Contains(c.MaMonHoc))
                .GroupBy(c => c.MaMonHoc)
                .Select(g => new { MonHocId = g.Key, Count = g.Sum(c => c.BaiHocs.Count) })
                .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

            var completedLessonCounts = await _db.TienDoBaiHocs
                .Where(t => t.MaHocSinh == currentUser.UserId
                    && (t.PhanTramTienDo >= 100 || t.HoanThanhLuc != null)
                    && t.BaiHoc!.Chuong != null
                    && allMonHocIds.Contains(t.BaiHoc.Chuong.MaMonHoc))
                .GroupBy(t => t.BaiHoc!.Chuong!.MaMonHoc)
                .Select(g => new { MonHocId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.MonHocId, g => g.Count);

            // 6. Focus summary
            var classesToday = await _db.BuoiHocs
                .Include(b => b.KhoaHoc)
                .Where(b => b.NgayHoc == today
                    && b.KhoaHoc != null
                    && b.KhoaHoc.MaLop == user.MaLop
                    && b.TrangThaiBuoi != "da_huy")
                .CountAsync();

            var assignmentsDue = await _db.BaiTaps
                .Where(bt => bt.HanNop >= now
                    && bt.TrangThai == "da_xuat_ban"
                    && allMonHocIds.Contains(bt.MaMonHoc))
                .CountAsync();

            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var completedThisWeek = await _db.TienDoBaiHocs
                .CountAsync(t => t.MaHocSinh == currentUser.UserId
                    && t.HoanThanhLuc >= weekStart);

            var nearestAssignment = await _db.BaiTaps
                .Where(bt => bt.HanNop >= now
                    && bt.TrangThai == "da_xuat_ban"
                    && allMonHocIds.Contains(bt.MaMonHoc))
                .OrderBy(bt => bt.HanNop)
                .Select(bt => bt.HanNop)
                .FirstOrDefaultAsync();

            var nearestDeadlineStr = nearestAssignment == default
                ? "Không có"
                : FormatDeadline(nearestAssignment);

            var diemSoList = await _db.DiemSos
                .Where(ds => ds.MaHocSinh == currentUser.UserId
                    && (currentHocKy == null || ds.MaHocKy == currentHocKy.MaHocKy))
                .ToListAsync();

            var gpaValue = diemSoList.Count != 0
                ? diemSoList.Average(ds => (double)ds.GpaMonHoc).ToString("F1")
                : "0.0";

            // 7. Week progress
            var weekProgress = CalculateWeekProgress(currentHocKy, today);

            // 8. Courses
            var courses = new List<CourseProgressDto>();

            foreach (var lhp in enrollments
                .Select(d => d.LopHocPhan)
                .Where(l => l?.MonHoc != null)
                .DistinctBy(l => l!.MaMonHoc))
            {
                var total = totalLessons.GetValueOrDefault(lhp!.MaMonHoc);
                var completed = completedLessonCounts.GetValueOrDefault(lhp.MaMonHoc);
                var progress = total > 0 ? (int)((double)completed / total * 100) : 0;
                var lecturer = khoaHocs.FirstOrDefault(k => k.MaMonHoc == lhp.MaMonHoc)?.GiaoVien?.HoTen ?? "Giảng viên";

                courses.Add(new CourseProgressDto
                {
                    Id = lhp.MaCodeLopHocPhan,
                    Name = lhp.MonHoc!.TenMonHoc,
                    Code = lhp.MonHoc.MaCodeMonHoc,
                    Lecturer = lecturer,
                    Progress = progress,
                    Completed = completed,
                    Total = total,
                    Status = progress switch { 100 => "Hoàn thành", > 0 => "Đang học", _ => "Chưa bắt đầu" },
                    StatusVariant = progress switch { 100 => "neutral", > 0 => "success", _ => "warning" }
                });
            }

            foreach (var kh in khoaHocs.Where(k => !enrolledMonHocIds.Contains(k.MaMonHoc)))
            {
                if (kh.MonHoc == null) continue;
                var total = totalLessons.GetValueOrDefault(kh.MaMonHoc);
                var completed = completedLessonCounts.GetValueOrDefault(kh.MaMonHoc);
                var progress = total > 0 ? (int)((double)completed / total * 100) : 0;

                courses.Add(new CourseProgressDto
                {
                    Id = kh.MaKhoaHoc.ToString(),
                    Name = kh.MonHoc.TenMonHoc,
                    Code = kh.MonHoc.MaCodeMonHoc,
                    Lecturer = kh.GiaoVien?.HoTen ?? "Giảng viên",
                    Progress = progress,
                    Completed = completed,
                    Total = total,
                    Status = progress switch { 100 => "Hoàn thành", > 0 => "Đang học", _ => "Chưa bắt đầu" },
                    StatusVariant = progress switch { 100 => "neutral", > 0 => "success", _ => "warning" }
                });
            }

            // 9. Assignments
            var assignments = await _db.BaiTaps
                .Include(bt => bt.MonHoc)
                .Where(bt => allMonHocIds.Contains(bt.MaMonHoc) && bt.TrangThai == "da_xuat_ban")
                .OrderBy(bt => bt.HanNop)
                .Select(bt => new AssignmentDto
                {
                    Id = bt.MaBaiTap.ToString(),
                    Title = bt.TieuDe,
                    Course = bt.MonHoc != null ? bt.MonHoc.TenMonHoc : "",
                    Deadline = bt.HanNop < now ? "Quá hạn" : FormatDeadline(bt.HanNop),
                    Status = bt.HanNop < now ? "Quá hạn" : bt.HanNop <= now.AddDays(2) ? "Sắp đến hạn" : "Chưa làm",
                    Variant = bt.HanNop < now ? "danger" : bt.HanNop <= now.AddDays(2) ? "warning" : "info",
                    Priority = bt.HanNop < now || bt.HanNop <= now.AddDays(1) ? "high" : "medium"
                })
                .ToListAsync();

            // 10. Schedule
            var scheduleRaw = await _db.ThoiKhoaBieus
                .Include(t => t.KhoaHoc!)
                    .ThenInclude(k => k.MonHoc)
                .Include(t => t.CaHoc)
                .Include(t => t.Phong)
                .Where(t => t.KhoaHoc != null
                    && t.KhoaHoc.MaLop == user.MaLop
                    && (currentHocKy == null || t.KhoaHoc.MaHocKy == currentHocKy.MaHocKy)
                    && t.TrangThai == "da_xuat_ban")
                .OrderBy(t => t.ThuTrongTuan)
                .ThenBy(t => t.CaHoc != null ? t.CaHoc.ThuTu : 0)
                .ToListAsync();

            var schedule = scheduleRaw.Select(t => new ScheduleDto
            {
                Id = t.MaTkb.ToString(),
                Course = t.KhoaHoc?.MonHoc?.TenMonHoc ?? "",
                Code = t.KhoaHoc?.MonHoc?.MaCodeMonHoc ?? "",
                Time = t.CaHoc != null ? $"{t.CaHoc.GioBatDau:hh\\:mm} - {t.CaHoc.GioKetThuc:hh\\:mm}" : "",
                Room = t.Phong?.TenPhong ?? "",
                Type = t.CaHoc != null && t.CaHoc.Buoi == "sang" ? "Lý thuyết" : "Thực hành",
                TypeVariant = "info",
                Status = "Sắp tới",
                StatusVariant = "secondary"
            }).ToList();

            // 11. Grades
            var grades = await _db.DiemSos
                .Include(ds => ds.MonHoc)
                .Where(ds => ds.MaHocSinh == currentUser.UserId)
                .OrderByDescending(ds => ds.MaHocKy)
                .Select(ds => new GradeDto
                {
                    Id = ds.MaDiemSo.ToString(),
                    Course = ds.MonHoc != null ? ds.MonHoc.TenMonHoc : "",
                    Code = ds.MonHoc != null ? ds.MonHoc.MaCodeMonHoc : "",
                    ExamType = "Điểm tổng kết",
                    Score = (double)ds.GpaMonHoc,
                    Date = ds.MaHocKy.ToString(),
                    Status = ds.GpaMonHoc >= 5m ? "passed" : "failed"
                })
                .Take(10)
                .ToListAsync();

            // 12. Tuition
            var hoaDon = await _db.HoaDons
                .Where(hd => hd.MaHocSinh == currentUser.UserId
                    && (currentHocKy == null || hd.MaHocKy == currentHocKy.MaHocKy))
                .OrderByDescending(hd => hd.NgayTao)
                .FirstOrDefaultAsync();

            var tuition = new TuitionDto();
            if (hoaDon != null)
            {
                var conLai = hoaDon.SoTien - hoaDon.GiamTru - hoaDon.DaThanhToan;
                var daDong = hoaDon.DaThanhToan + hoaDon.GiamTru;
                var tienDo = hoaDon.SoTien > 0 ? (int)(daDong / hoaDon.SoTien * 100) : 0;

                tuition.TotalDue = conLai.ToString("N0");
                tuition.Deadline = hoaDon.HanThanhToan.ToString("dd/MM/yyyy");
                tuition.Progress = tienDo;
                tuition.Status = conLai <= 0 ? "Đã thanh toán" : "Chưa thanh toán";
                tuition.StatusVariant = conLai <= 0 ? "success" : "danger";
            }

            // 13. Registration period
            var registrationPeriod = await _db.GiaiDoanDangKys
                .Include(g => g.HocKy)
                .Where(g => g.MaDonVi == currentUser.CampusId && g.TrangThai != "da_dong")
                .OrderBy(g => g.BatDauLuc)
                .FirstOrDefaultAsync();

            var registration = new RegistrationDto();
            if (registrationPeriod != null)
            {
                registration.Semester = registrationPeriod.HocKy?.TenHocKy ?? "HK mới";
                registration.StartDate = registrationPeriod.BatDauLuc.ToString("dd/MM/yyyy");
                var isOpen = now >= registrationPeriod.BatDauLuc && now <= registrationPeriod.KetThucLuc;
                registration.Status = isOpen ? "Đang mở" : now < registrationPeriod.BatDauLuc ? "Sắp mở" : "Đã đóng";
                registration.Action = isOpen ? "Đăng ký ngay" : "Xem trước lịch";
            }

            // 14. Attendance
            var diemDanhRecords = await _db.DiemDanhs
                .Include(dd => dd.BuoiHoc!)
                    .ThenInclude(b => b.KhoaHoc!)
                        .ThenInclude(k => k.MonHoc)
                .Where(dd => dd.MaHocSinh == currentUser.UserId
                    && dd.BuoiHoc != null
                    && dd.BuoiHoc.TrangThaiBuoi == "da_dien_ra")
                .ToListAsync();

            var totalSessions = diemDanhRecords.Count;
            var presentSessions = diemDanhRecords.Count(dd => dd.TrangThai is "co_mat" or "di_muon");
            var absentSessions = diemDanhRecords.Count(dd => dd.TrangThai is "vang" or "vang_co_phep");
            var attendanceScore = totalSessions > 0 ? (int)((double)presentSessions / totalSessions * 100) : 0;

            var attendanceRisks = diemDanhRecords
                .GroupBy(dd => dd.BuoiHoc!.MaKhoaHoc)
                .Select(g =>
                {
                    var kh = g.First().BuoiHoc?.KhoaHoc;
                    var absent = g.Count(dd => dd.TrangThai == "vang");
                    return new AttendanceRiskDto
                    {
                        Id = $"risk_{g.Key}",
                        Course = kh?.MonHoc?.TenMonHoc ?? "",
                        Code = kh?.MonHoc?.MaCodeMonHoc ?? "",
                        Absent = absent,
                        Limit = g.Count(),
                        Percent = g.Count() > 0 ? (int)((double)absent / g.Count() * 100) : 0
                    };
                })
                .Where(r => r.Absent > 0)
                .ToList();

            var attendance = new AttendanceHealthDto
            {
                Score = attendanceScore,
                Status = attendanceScore >= 80 ? "Tốt" : attendanceScore >= 50 ? "Trung bình" : "Kém",
                Tone = attendanceScore >= 80 ? "teal" : attendanceScore >= 50 ? "amber" : "danger",
                Advice = attendanceScore >= 80
                    ? "Tiếp tục duy trì tiến độ học tập và đi học đầy đủ bạn nhé!"
                    : attendanceScore >= 50
                        ? "Bạn cần đi học đều đặn hơn để tránh ảnh hưởng kết quả."
                        : "Tình trạng nghỉ học quá nhiều. Hãy gặp cố vấn học tập ngay!",
                Risks = attendanceRisks
            };

            // 15. Notifications
            var notifications = await _db.ThongBaoNguoiNhans
                .Include(nn => nn.ThongBao)
                .Where(nn => nn.MaNguoiNhan == currentUser.UserId && !nn.DaAn)
                .OrderByDescending(nn => nn.NgayTao)
                .Take(10)
                .Select(nn => new NotificationDto
                {
                    Id = nn.MaThongBaoNguoiNhan.ToString(),
                    Title = nn.ThongBao != null ? nn.ThongBao.TieuDe ?? "" : "",
                    Content = nn.ThongBao != null ? (nn.ThongBao.TomTat ?? nn.ThongBao.NoiDung) : "",
                    Time = nn.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    Category = nn.ThongBao != null ? nn.ThongBao.LoaiThongBao : "",
                    Unread = !nn.DaDoc
                })
                .ToListAsync();

            // 16. KPIs
            var kpis = new List<KpiDto>
            {
                new()
                {
                    Id = "courses",
                    Label = "Khóa học đang học",
                    Value = courses.Count.ToString(),
                    Trend = $"{enrolledMonHocIds.Count} khóa đã đăng ký",
                    Tone = "blue",
                    Route = "/student/courses"
                },
                new()
                {
                    Id = "assignments",
                    Label = "Bài tập cần xử lý",
                    Value = assignmentsDue.ToString(),
                    Trend = $"{assignments.Count(a => a.Deadline.Contains("hôm nay") || a.Deadline.Contains("ngày mai"))} bài sắp đến hạn",
                    Tone = "amber",
                    Route = "/student/assignments"
                },
                new()
                {
                    Id = "gpa",
                    Label = "GPA học kỳ",
                    Value = gpaValue,
                    Trend = diemSoList.Count != 0 ? $"{diemSoList.Count} môn đã có điểm" : "Chưa có điểm",
                    Tone = "violet",
                    Route = "/student/grades"
                },
                new()
                {
                    Id = "attendance",
                    Label = "Chuyên cần",
                    Value = $"{attendanceScore}%",
                    Trend = $"{absentSessions} buổi vắng",
                    Tone = "teal",
                    Route = "/student/attendance"
                }
            };

            // 17. Build response
            var data = new StudentDashboardDto
            {
                Student = new StudentInfoDto
                {
                    Name = user.HoTen,
                    Code = $"SV{user.MaNguoiDung:D6}",
                    ClassName = user.Lop?.TenLop ?? user.Lop?.MaCodeLop ?? "",
                    Semester = currentHocKy?.TenHocKy ?? "Chưa có học kỳ"
                },
                WeekProgress = weekProgress,
                FocusSummary = new FocusSummaryDto
                {
                    ClassesToday = classesToday,
                    AssignmentsDue = assignmentsDue,
                    CompletedThisWeek = completedThisWeek,
                    NearestDeadline = nearestDeadlineStr,
                    Gpa = gpaValue
                },
                Kpis = kpis,
                Courses = courses,
                Assignments = assignments,
                Schedule = schedule,
                Grades = grades,
                Tuition = tuition,
                Registration = registration,
                Attendance = attendance,
                Notifications = notifications
            };

            return Ok(ApiResponseDto<StudentDashboardDto>.Ok(data));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponseDto.Fail("Lỗi khi tải dữ liệu dashboard: " + ex.Message));
        }
    }

    private static int CalculateWeekProgress(HocKy? semester, DateOnly today)
    {
        if (semester == null) return 0;
        var totalDays = semester.NgayKetThuc.DayNumber - semester.NgayBatDau.DayNumber;
        if (totalDays <= 0) return 0;
        var elapsedDays = today.DayNumber - semester.NgayBatDau.DayNumber;
        if (elapsedDays <= 0) return 0;
        return Math.Clamp((int)((double)elapsedDays / totalDays * 100), 0, 100);
    }

    private static string FormatDeadline(DateTime deadline)
    {
        var now = DateTime.UtcNow;
        var diff = deadline - now;
        if (diff.TotalHours <= 0) return "Quá hạn";
        if (diff.TotalHours < 1) return "Sắp hết hạn";
        if (diff.TotalHours < 24) return $"Hôm nay · {deadline:HH\\:mm}";
        if (diff.TotalDays < 2) return $"Ngày mai · {deadline:HH\\:mm}";
        return diff.TotalDays <= 7 ? $"{(int)diff.TotalDays} ngày nữa" : deadline.ToString("dd/MM/yyyy");
    }
}

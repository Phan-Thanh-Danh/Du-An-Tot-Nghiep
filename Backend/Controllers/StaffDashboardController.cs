using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = AuthRoles.AcademicStaff)]
public class StaffDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StaffDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponseDto<StaffDashboardDto>>> GetDashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayDayOfWeek = (int)DateTime.Today.DayOfWeek == 0 ? 7 : (int)DateTime.Today.DayOfWeek;

        var todaySchedules = await _db.ThoiKhoaBieus
            .CountAsync(t => t.ThuTrongTuan == todayDayOfWeek
                && t.TrangThai != "da_huy"
                && (!t.NgayBatDau.HasValue || t.NgayBatDau <= today)
                && (!t.NgayKetThuc.HasValue || t.NgayKetThuc >= today));

        var conflicts = await _db.ThoiKhoaBieus
            .Where(t => t.TrangThai != "da_huy")
            .GroupBy(t => new { t.MaPhong, t.ThuTrongTuan, t.MaCaHoc })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1))
            .CountAsync();

        var activeClasses = await _db.LopHocPhans
            .CountAsync(l => l.TrangThai == "mo");

        var pendingRequests = await _db.DonTus
            .CountAsync(d => d.TrangThai == "da_nop" || d.TrangThai == "dang_xem_xet");

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var newNotices = await _db.ThongBaos
            .CountAsync(t => t.NgayTao >= sevenDaysAgo);

        var recentSchedules = await _db.ThoiKhoaBieus
            .Where(t => t.TrangThai != "da_huy")
            .OrderByDescending(t => t.NgayTao)
            .Take(10)
            .Select(t => new ScheduleEntryDto
            {
                Id = t.MaTkb,
                RoomId = t.MaPhong,
                RoomName = t.Phong != null ? t.Phong.TenPhong : null,
                ThuTrongTuan = t.ThuTrongTuan,
                MaCaHoc = t.MaCaHoc,
                TrangThai = t.TrangThai,
                NgayBatDau = t.NgayBatDau,
                NgayKetThuc = t.NgayKetThuc,
                NgayTao = t.NgayTao
            })
            .ToListAsync();

        var recentRequests = await _db.DonTus
            .OrderByDescending(d => d.NgayTao)
            .Take(10)
            .Select(d => new RequestEntryDto
            {
                Id = d.MaDonTu,
                TieuDe = d.TieuDe,
                LoaiDon = d.LoaiDon,
                TrangThai = d.TrangThai,
                NgayTao = d.NgayTao,
                HocSinhName = d.HocSinh != null ? d.HocSinh.HoTen : null
            })
            .ToListAsync();

        var announcements = await _db.ThongBaos
            .Where(t => t.NgayTao >= sevenDaysAgo && t.TrangThai == "da_gui")
            .OrderByDescending(t => t.NgayTao)
            .Take(5)
            .Select(t => new AnnouncementDto
            {
                Id = t.MaThongBao,
                TieuDe = t.TieuDe ?? "",
                NoiDung = t.NoiDung,
                NgayTao = t.NgayTao
            })
            .ToListAsync();

        var data = new StaffDashboardDto
        {
            TodaySchedules = todaySchedules,
            Conflicts = conflicts,
            ActiveClasses = activeClasses,
            PendingRequests = pendingRequests,
            NewNotices = newNotices,
            RecentSchedules = recentSchedules,
            RecentRequests = recentRequests,
            Announcements = announcements
        };

        return Ok(ApiResponseDto<StaffDashboardDto>.Ok(data));
    }
}

public class StaffDashboardDto
{
    public int TodaySchedules { get; set; }
    public int Conflicts { get; set; }
    public int ActiveClasses { get; set; }
    public int PendingRequests { get; set; }
    public int NewNotices { get; set; }
    public List<ScheduleEntryDto> RecentSchedules { get; set; } = [];
    public List<RequestEntryDto> RecentRequests { get; set; } = [];
    public List<AnnouncementDto> Announcements { get; set; } = [];
}

public class ScheduleEntryDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public DateTime NgayTao { get; set; }
}

public class RequestEntryDto
{
    public int Id { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string LoaiDon { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string? HocSinhName { get; set; }
}

public class AnnouncementDto
{
    public int Id { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}

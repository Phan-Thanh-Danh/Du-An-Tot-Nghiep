using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Constants;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
public class BghDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponseDto<BghDashboardDto>>> GetDashboard()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin || user?.Role == AuthRoles.Admin;
        var campusId = user?.CampusId ?? 0;

        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => isGlobal || l.MaDonVi == campusId);
        
        // Note: ThoiKhoaBieu links to KhoaHoc which has MaDonVi
        var pendingSchedules = await _db.ThoiKhoaBieus.CountAsync(t => t.TrangThai == "nhap" && (isGlobal || (t.KhoaHoc != null && t.KhoaHoc.MaDonVi == campusId)));
        var pendingApplicationStatuses = new[] { "da_nop", "dang_xem_xet", "yeu_cau_bo_sung" };
        var pendingRequests = await _db.DonTus.CountAsync(d =>
            pendingApplicationStatuses.Contains(d.TrangThai) &&
            (isGlobal || (d.HocSinh != null && d.HocSinh.MaDonVi == campusId)));

        var pendingScheduleItems = await _db.ThoiKhoaBieus
            .AsNoTracking()
            .Where(t => t.TrangThai == "nhap" && (isGlobal || (t.KhoaHoc != null && t.KhoaHoc.MaDonVi == campusId)))
            .OrderByDescending(t => t.NgayTao)
            .Take(3)
            .Select(t => new PendingScheduleItemDto
            {
                Id = t.MaTkb,
                Title = t.KhoaHoc != null && t.KhoaHoc.MonHoc != null
                    ? t.KhoaHoc.MonHoc.TenMonHoc
                    : $"Thời khóa biểu #{t.MaTkb}",
                Badge = "MỚI",
                Description = t.KhoaHoc != null && t.KhoaHoc.Lop != null
                    ? $"{t.KhoaHoc.Lop.MaCodeLop} · {t.NgayTao:dd/MM/yyyy HH:mm}"
                    : $"{t.NgayTao:dd/MM/yyyy HH:mm}"
            })
            .ToListAsync();

        var recentAuditLogs = await _db.NhatKyKiemToans
            .AsNoTracking()
            .Where(a => isGlobal || a.MaDonVi == campusId)
            .OrderByDescending(a => a.ThoiDiemThayDoi)
            .Take(10)
            .Select(a => new AuditLogEntryDto
            {
                Id = a.MaKiemToan,
                Action = a.HanhDong,
                Entity = a.LoaiDoiTuong,
                EntityId = a.MaDoiTuong,
                Timestamp = a.ThoiDiemThayDoi,
                Description = a.MoTa ?? "",
                PerformedBy = a.NguoiThayDoiNavigation != null ? a.NguoiThayDoiNavigation.HoTen : null
            })
            .ToListAsync();

        var data = new BghDashboardDto
        {
            TotalTeachers = totalTeachers,
            TotalStudents = totalStudents,
            TotalClasses = totalClasses,
            PendingSchedules = pendingSchedules,
            PendingRequests = pendingRequests,
            PendingScheduleItems = pendingScheduleItems,
            RecentAuditLogs = recentAuditLogs
        };

        return Ok(ApiResponseDto<BghDashboardDto>.Ok(data));
    }
}

public class BghDashboardDto
{
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalClasses { get; set; }
    public int PendingSchedules { get; set; }
    public int PendingRequests { get; set; }
    public List<PendingScheduleItemDto> PendingScheduleItems { get; set; } = [];
    public List<AuditLogEntryDto> RecentAuditLogs { get; set; } = [];
}

public class PendingScheduleItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AuditLogEntryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
}

using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Constants;
using Backend.Services.Bgh;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
public class BghDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IBghPerformanceCache _cache;

    public BghDashboardController(ApplicationDbContext db, IBghPerformanceCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponseDto<BghDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin || user?.Role == AuthRoles.Admin;
        var campusId = user?.CampusId ?? 0;

        Response.Headers.CacheControl = "private, max-age=15, stale-while-revalidate=45";
        var cacheKey = BghCacheKey.For(HttpContext, "dashboard-summary");
        var data = await _cache.GetOrCreateAsync(
            cacheKey,
            TimeSpan.FromSeconds(45),
            async ct =>
            {
        var roleCounts = await _db.NguoiDungs
            .AsNoTracking()
            .Where(u => (u.VaiTroChinh == "giao_vien" || u.VaiTroChinh == "hoc_sinh") &&
                        (isGlobal || u.MaDonVi == campusId))
            .GroupBy(u => u.VaiTroChinh)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, ct);
        var totalTeachers = roleCounts.GetValueOrDefault("giao_vien");
        var totalStudents = roleCounts.GetValueOrDefault("hoc_sinh");
        var totalClasses = await _db.LopHanhChinhs
            .AsNoTracking()
            .CountAsync(l => isGlobal || l.MaDonVi == campusId, ct);
        
        // Note: ThoiKhoaBieu links to KhoaHoc which has MaDonVi
        var pendingSchedules = await _db.ThoiKhoaBieus
            .AsNoTracking()
            .CountAsync(t => t.TrangThai == "nhap" && (isGlobal || (t.KhoaHoc != null && t.KhoaHoc.MaDonVi == campusId)), ct);
        var pendingApplicationStatuses = new[] { "da_nop", "dang_xem_xet", "yeu_cau_bo_sung" };
        var pendingRequests = await _db.DonTus.CountAsync(d =>
            pendingApplicationStatuses.Contains(d.TrangThai) &&
            (isGlobal || (d.HocSinh != null && d.HocSinh.MaDonVi == campusId)), ct);

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
            .ToListAsync(ct);

        var recentAuditLogs = await _db.NhatKyKiemToans
            .AsNoTracking()
            .Where(a => isGlobal || a.MaDonVi == campusId)
            .OrderByDescending(a => a.ThoiDiemThayDoi)
            .Take(5)
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
            .ToListAsync(ct);

        var riskAggregates = _db.DiemSos
            .AsNoTracking()
            .Where(d => isGlobal || d.MaDonVi == campusId)
            .GroupBy(d => d.MaHocSinh)
            .Select(g => new
            {
                StudentId = g.Key,
                AvgGpa = g.Average(d => d.GpaMonHoc),
                FailCount = g.Count(d => d.GpaMonHoc < 4)
            })
            .Where(x => x.FailCount > 0);

        var riskStudents = await (
                from risk in riskAggregates
                join student in _db.NguoiDungs.AsNoTracking()
                    on risk.StudentId equals student.MaNguoiDung
                join academicClass in _db.LopHanhChinhs.AsNoTracking()
                    on student.MaLop equals (int?)academicClass.MaLop into classJoin
                from academicClass in classJoin.DefaultIfEmpty()
                orderby risk.AvgGpa, risk.FailCount descending, student.MaNguoiDung
                select new DashboardRiskStudentDto
                {
                    Id = student.MaNguoiDung,
                    Name = student.HoTen,
                    Email = student.Email,
                    ClassCode = academicClass != null ? academicClass.MaCodeLop : "",
                    AvgGpa = Math.Round(risk.AvgGpa, 2),
                    FailCount = risk.FailCount
                })
            .Take(5)
            .ToListAsync(ct);

        return new BghDashboardDto
        {
            TotalTeachers = totalTeachers,
            TotalStudents = totalStudents,
            TotalClasses = totalClasses,
            PendingSchedules = pendingSchedules,
            PendingRequests = pendingRequests,
            PendingScheduleItems = pendingScheduleItems,
            RecentAuditLogs = recentAuditLogs,
            RiskStudents = riskStudents
        };
            },
            cancellationToken);

        return Ok(ApiResponseDto<BghDashboardDto>.Ok(data));
    }

    [HttpGet("performance/cache-stats")]
    public ActionResult<ApiResponseDto<BghCacheMetrics>> GetCacheStats()
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(ApiResponseDto<BghCacheMetrics>.Ok(_cache.GetMetrics()));
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
    public List<DashboardRiskStudentDto> RiskStudents { get; set; } = [];
}

public class DashboardRiskStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public decimal AvgGpa { get; set; }
    public int FailCount { get; set; }
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

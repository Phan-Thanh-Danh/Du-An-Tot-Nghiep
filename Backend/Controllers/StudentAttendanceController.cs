using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentAttendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/attendance")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentAttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentAttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentAttendanceItemDto>>>> GetAttendance(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 1000,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return Unauthorized();

        var query = _context.DiemDanhs
            .AsNoTracking()
            .Include(d => d.BuoiHoc)
                .ThenInclude(b => b!.CaHoc)
            .Include(d => d.BuoiHoc)
                .ThenInclude(b => b!.Phong)
            .Include(d => d.BuoiHoc)
                .ThenInclude(b => b!.KhoaHoc)
                    .ThenInclude(k => k!.MonHoc)
            .Where(d => d.MaHocSinh == currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(d => d.TrangThai == status);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var records = await query
            .OrderByDescending(d => d.BuoiHoc!.NgayHoc)
            .ThenByDescending(d => d.BuoiHoc!.MaCaHoc)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new StudentAttendanceItemDto
            {
                MaDiemDanh = d.MaDiemDanh,
                MaBuoiHoc = d.MaBuoiHoc,
                TenMonHoc = d.BuoiHoc!.KhoaHoc!.MonHoc != null ? d.BuoiHoc.KhoaHoc.MonHoc.TenMonHoc : d.BuoiHoc.KhoaHoc!.TieuDe,
                TieuDeKhoaHoc = d.BuoiHoc!.KhoaHoc!.TieuDe,
                TenCa = d.BuoiHoc.CaHoc != null ? d.BuoiHoc.CaHoc.TenCa : "Ca học",
                GioBatDau = d.BuoiHoc.CaHoc != null ? d.BuoiHoc.CaHoc.GioBatDau.ToString(@"hh\:mm") : "",
                GioKetThuc = d.BuoiHoc.CaHoc != null ? d.BuoiHoc.CaHoc.GioKetThuc.ToString(@"hh\:mm") : "",
                TenPhong = d.BuoiHoc.Phong != null ? d.BuoiHoc.Phong.TenPhong : "",
                TrangThai = d.TrangThai,
                NgayHoc = d.BuoiHoc.NgayHoc.ToString("yyyy-MM-dd"),
                GhiNhanLuc = d.GhiNhanLuc
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResultDto<StudentAttendanceItemDto>
        {
            Items = records,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        return Ok(ApiResponseDto<PagedResultDto<StudentAttendanceItemDto>>.Ok(result));
    }
}

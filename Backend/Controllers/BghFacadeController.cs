using Backend.Constants;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
public class BghFacadeController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghFacadeController(ApplicationDbContext db)
    {
        _db = db;
    }

    private (int CampusId, bool IsGlobal) GetUserScope()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var campusId = user?.CampusId ?? 0;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin || user?.Role == AuthRoles.Admin;
        return (campusId, isGlobal);
    }

    [HttpGet("master-data/training-programs")]
    public async Task<IActionResult> GetTrainingPrograms()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.ChuongTrinhDaoTaos
            .Select(x => new { Id = x.MaChuongTrinh, MaCode = x.MaCodeChuongTrinh, TenChuongTrinh = x.TenChuongTrinh, TrangThai = x.TrangThai })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/academic-terms")]
    public async Task<IActionResult> GetAcademicTerms()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.HocKys
            .Select(x => new { Id = x.MaHocKy, MaCode = x.MaCodeHocKy, TenKyHoc = x.TenHocKy, NamHoc = x.NamHoc, TrangThai = x.DaKhoa ? "Đã khóa" : "Đang mở" })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/buildings")]
    public async Task<IActionResult> GetBuildings()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.ToaNhas
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .Select(x => new { Id = x.MaToaNha, MaCode = x.MaCodeToaNha, TenToaNha = x.TenToaNha })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/floors")]
    public async Task<IActionResult> GetFloors()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.Tangs
            .Where(x => isGlobal || x.ToaNha!.MaDonVi == campusId)
            .Select(x => new { Id = x.MaTang, TenTang = x.TenTang })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.PhongHocs
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .Select(x => new { Id = x.MaPhong, MaCode = x.MaCodePhong, TenPhong = x.TenPhong, LoaiPhong = x.LoaiPhong, SucChua = x.SucChua })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.ThoiKhoaBieus
            .Select(x => new { Id = x.MaTkb, TrangThai = x.TrangThai })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.NhatKyKiemToans
            .Select(x => new { Id = x.MaKiemToan, HanhDong = x.HanhDong })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.DanhMucMonHocs
            .Select(x => new { Id = x.MaMonHoc, MaCode = x.MaCodeMonHoc, TenMonHoc = x.TenMonHoc, TrangThai = x.ConHoatDong ? "Hoạt động" : "Ngừng" })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("rbac/roles")]
    public async Task<IActionResult> GetRoles()
    {
        var data = await _db.VaiTros
            .Select(x => new { Id = x.MaVaiTro, MaCode = x.MaCodeVaiTro, TenVaiTro = x.TenVaiTro })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

}

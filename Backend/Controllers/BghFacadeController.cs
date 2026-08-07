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
            .AsNoTracking()
            .Where(x => isGlobal || x.LopHanhChinhs.Any(l => l.MaDonVi == campusId))
            .OrderBy(x => x.TenChuongTrinh)
            .Select(x => new { Id = x.MaChuongTrinh, MaCode = x.MaCodeChuongTrinh, TenChuongTrinh = x.TenChuongTrinh, TrangThai = x.TrangThai })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/academic-terms")]
    public async Task<IActionResult> GetAcademicTerms()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.HocKys
            .AsNoTracking()
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .OrderBy(x => x.NgayBatDau)
            .ThenBy(x => x.ThuTuTrongNam)
            .Select(x => new
            {
                Id = x.MaHocKy,
                MaHocKy = x.MaHocKy,
                MaCode = x.MaCodeHocKy,
                MaCodeHocKy = x.MaCodeHocKy,
                TenKyHoc = x.TenHocKy,
                TenHocKy = x.TenHocKy,
                x.NamHoc,
                x.NgayBatDau,
                x.NgayKetThuc,
                x.ThuTuTrongNam,
                x.SoTinChiToiDa,
                x.DaKhoa,
                TrangThai = x.DaKhoa ? "Đã khóa" : "Đang mở"
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/cohorts")]
    public async Task<IActionResult> GetCohorts()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.KhoaTuyenSinhs
            .AsNoTracking()
            .Where(x => isGlobal || _db.ChuongTrinhDaoTaos.Any(p =>
                p.MaKhoaTuyenSinh == x.MaKhoaTuyenSinh &&
                p.LopHanhChinhs.Any(l => l.MaDonVi == campusId)))
            .OrderByDescending(x => x.NamBatDau)
            .Select(x => new
            {
                x.MaKhoaTuyenSinh,
                x.MaCodeKhoa,
                x.TenKhoa,
                x.NamBatDau,
                x.NamKetThucDuKien,
                x.MoTa,
                x.ConHoatDong
            })
            .ToListAsync();
        return Ok(new { data, message = "Success" });
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

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? keyword = null)
    {
        var (campusId, isGlobal) = GetUserScope();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query =
            from user in _db.NguoiDungs.AsNoTracking()
            join organization in _db.DonVis.AsNoTracking()
                on user.MaDonVi equals organization.MaDonVi
            join role in _db.VaiTros.AsNoTracking()
                on user.VaiTroChinh equals role.MaCodeVaiTro
            where isGlobal || user.MaDonVi == campusId
            select new
            {
                user.MaNguoiDung,
                user.HoTen,
                user.Email,
                user.SoDienThoai,
                VaiTroChinh = role.MaCodeVaiTro,
                role.TenVaiTro,
                user.MaDonVi,
                organization.TenDonVi,
                user.TrangThai,
                user.NgayTao
            };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.HoTen.ToLower().Contains(normalizedKeyword) ||
                x.Email.ToLower().Contains(normalizedKeyword) ||
                (x.SoDienThoai != null && x.SoDienThoai.Contains(normalizedKeyword)));
        }

        var data = await query
            .OrderByDescending(x => x.NgayTao)
            .ThenBy(x => x.HoTen)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules([FromQuery] string? status = null)
    {
        var (campusId, isGlobal) = GetUserScope();
        var useClientTimeCalculation =
            _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        var query = _db.ThoiKhoaBieus
            .AsNoTracking()
            .Join(
                _db.KhoaHocs.AsNoTracking(),
                schedule => schedule.MaKhoaHoc,
                course => course.MaKhoaHoc,
                (schedule, course) => new { Schedule = schedule, Course = course })
            .Where(x => isGlobal || x.Course.MaDonVi == campusId);

        query = status?.ToLowerInvariant() switch
        {
            "published" => query.Where(x => x.Schedule.TrangThai == "da_xuat_ban"),
            "cancelled" => query.Where(x => x.Schedule.TrangThai == "da_huy"),
            _ => query.Where(x => x.Schedule.TrangThai == "nhap")
        };

        var data = await query
            .OrderByDescending(x => x.Schedule.NgayTao)
            .Select(x => new
            {
                Id = $"TKB-{x.Schedule.MaTkb:D5}",
                ScheduleId = x.Schedule.MaTkb,
                Department = x.Course.Lop != null &&
                    x.Course.Lop.ChuongTrinh != null &&
                    x.Course.Lop.ChuongTrinh.ChuyenNganh != null
                        ? x.Course.Lop.ChuongTrinh.ChuyenNganh.TenChuyenNganh
                        : "Chưa xác định",
                Dept = x.Course.Lop != null &&
                    x.Course.Lop.ChuongTrinh != null &&
                    x.Course.Lop.ChuongTrinh.ChuyenNganh != null
                        ? x.Course.Lop.ChuongTrinh.ChuyenNganh.TenChuyenNganh
                        : "Chưa xác định",
                Semester = x.Course.HocKy != null ? x.Course.HocKy.TenHocKy : "",
                Term = x.Course.HocKy != null ? x.Course.HocKy.TenHocKy : "",
                Subject = x.Course.MonHoc != null ? x.Course.MonHoc.TenMonHoc : "",
                ClassCode = x.Course.Lop != null ? x.Course.Lop.MaCodeLop : "",
                Room = x.Schedule.Phong != null ? x.Schedule.Phong.MaCodePhong : "",
                Shift = x.Schedule.CaHoc != null ? x.Schedule.CaHoc.TenCa : "",
                x.Schedule.ThuTrongTuan,
                x.Schedule.NgayBatDau,
                x.Schedule.NgayKetThuc,
                Status = x.Schedule.TrangThai == "da_xuat_ban" ? "published" : x.Schedule.TrangThai == "da_huy" ? "cancelled" : "pending",
                Submitter = x.Course.GiaoVien != null ? x.Course.GiaoVien.HoTen : "",
                Sender = x.Course.GiaoVien != null ? x.Course.GiaoVien.HoTen : "",
                Conflicts = _db.ThoiKhoaBieus.Count(other =>
                    other.MaTkb != x.Schedule.MaTkb &&
                    other.TrangThai != "da_huy" &&
                    other.ThuTrongTuan == x.Schedule.ThuTrongTuan &&
                    other.MaCaHoc == x.Schedule.MaCaHoc &&
                    other.KhoaHoc != null &&
                    other.KhoaHoc.MaHocKy == x.Course.MaHocKy &&
                    (other.MaPhong == x.Schedule.MaPhong ||
                     other.KhoaHoc.MaGiaoVien == x.Course.MaGiaoVien)),
                Type = "Lịch học",
                Classes = 1,
                Hours = x.Schedule.CaHoc != null
                    ? useClientTimeCalculation
                        ? (x.Schedule.CaHoc.GioKetThuc.ToTimeSpan() -
                           x.Schedule.CaHoc.GioBatDau.ToTimeSpan()).TotalHours
                        : EF.Functions.DateDiffMinute(
                            x.Schedule.CaHoc.GioBatDau,
                            x.Schedule.CaHoc.GioKetThuc) / 60.0
                    : 0,
                Campus = x.Course.DonVi != null ? x.Course.DonVi.TenDonVi : ""
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.NhatKyKiemToans
            .AsNoTracking()
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .OrderByDescending(x => x.ThoiDiemThayDoi)
            .Take(500)
            .Select(x => new
            {
                Id = x.MaKiemToan,
                x.LoaiDoiTuong,
                x.MaDoiTuong,
                x.HanhDong,
                x.GiaTriCu,
                x.GiaTriMoi,
                x.ThoiDiemThayDoi,
                x.DiaChiIp,
                x.MoTa,
                TenNguoiThayDoi = x.NguoiThayDoiNavigation != null ? x.NguoiThayDoiNavigation.HoTen : null
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.DanhMucMonHocs
            .AsNoTracking()
            .Where(x => isGlobal || _db.MonHocTrongChuongTrinhs.Any(p =>
                p.MaMonHoc == x.MaMonHoc &&
                p.ChuongTrinhDaoTao != null &&
                p.ChuongTrinhDaoTao.LopHanhChinhs.Any(l => l.MaDonVi == campusId)))
            .OrderBy(x => x.TenMonHoc)
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

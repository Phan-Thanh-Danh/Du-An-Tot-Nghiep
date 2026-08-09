using Backend.Constants;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Bgh;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
public class BghFacadeController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IBghPerformanceCache _cache;

    public BghFacadeController(ApplicationDbContext db, IBghPerformanceCache cache)
    {
        _db = db;
        _cache = cache;
    }

    private (int CampusId, bool IsGlobal) GetUserScope()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var campusId = user?.CampusId ?? 0;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin ||
                       user?.Role == AuthRoles.Admin ||
                       (user?.Email != null && user.Email.Contains("bgh_all"));
        return (campusId, isGlobal);
    }

    [HttpGet("master-data/training-programs")]
    [BghResponseCache(600)]
    public async Task<IActionResult> GetTrainingPrograms([FromQuery] string? keyword = null)
    {
        var (campusId, isGlobal) = GetUserScope();
        var normalizedKeyword = keyword?.Trim();
        var data = await _db.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Where(x =>
                (isGlobal || x.LopHanhChinhs.Any(l => l.MaDonVi == campusId)) &&
                (string.IsNullOrEmpty(normalizedKeyword) ||
                 x.TenChuongTrinh.Contains(normalizedKeyword) ||
                 x.MaCodeChuongTrinh.Contains(normalizedKeyword)))
            .OrderBy(x => x.TenChuongTrinh)
            .Select(x => new
            {
                Id = x.MaChuongTrinh,
                MaChuongTrinh = x.MaChuongTrinh,
                MaCode = x.MaCodeChuongTrinh,
                MaCodeChuongTrinh = x.MaCodeChuongTrinh,
                TenChuongTrinh = x.TenChuongTrinh,
                TrangThai = x.TrangThai,
                TenChuyenNganh = x.ChuyenNganh != null ? x.ChuyenNganh.TenChuyenNganh : "",
                TenKhoa = x.KhoaTuyenSinh != null ? x.KhoaTuyenSinh.TenKhoa : "",
                SoHocKy = _db.ChuongTrinhHocKys.Count(term => term.MaChuongTrinh == x.MaChuongTrinh),
                TongTinChiYeuCau = _db.MonHocTrongChuongTrinhs
                    .Where(subject => subject.MaChuongTrinh == x.MaChuongTrinh && subject.ConHoatDong)
                    .Sum(subject => (int?)subject.SoTinChi) ?? 0,
                SoHocKyKhaiBao = x.SoHocKy,
                TongTinChiKhaiBao = x.TongTinChiYeuCau,
                SoMonHoc = _db.MonHocTrongChuongTrinhs.Count(subject =>
                    subject.MaChuongTrinh == x.MaChuongTrinh && subject.ConHoatDong),
                x.ThoiGianDaoTaoThang,
                x.Version,
                NgayHieuLuc = x.NgayHieuLuc,
                NgayHetHieuLuc = x.NgayHetHieuLuc,
                NguoiGuiDuyet = x.NguoiGuiDuyetId != null
                    ? _db.NguoiDungs
                        .Where(user => user.MaNguoiDung == x.NguoiGuiDuyetId)
                        .Select(user => user.HoTen)
                        .FirstOrDefault() ?? ""
                    : "",
                NguoiDuyet = x.NguoiDuyetId != null
                    ? _db.NguoiDungs
                        .Where(user => user.MaNguoiDung == x.NguoiDuyetId)
                        .Select(user => user.HoTen)
                        .FirstOrDefault() ?? ""
                    : "",
                NgayTao = x.NgayTao,
                x.MoTa
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/training-programs/{programId:int}/curriculum")]
    [BghResponseCache(300)]
    public async Task<IActionResult> GetTrainingProgramCurriculum(int programId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var program = await _db.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == programId &&
                        (isGlobal || x.LopHanhChinhs.Any(l => l.MaDonVi == campusId)))
            .Select(x => new
            {
                x.MaChuongTrinh,
                x.MaCodeChuongTrinh,
                x.TenChuongTrinh,
                TenChuyenNganh = x.ChuyenNganh != null ? x.ChuyenNganh.TenChuyenNganh : "",
                x.TrangThai
            })
            .FirstOrDefaultAsync();

        if (program == null)
            return NotFound(new { message = "Không tìm thấy chương trình trong phạm vi quản lý." });

        var terms = await _db.ChuongTrinhHocKys
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == programId)
            .OrderBy(x => x.ThuTuHocKy)
            .Select(x => new
            {
                x.MaChuongTrinhHocKy,
                x.MaHocKy,
                x.ThuTuHocKy,
                TenHocKy = x.HocKy != null ? x.HocKy.TenHocKy : "",
                NamHoc = x.HocKy != null ? x.HocKy.NamHoc : ""
            })
            .ToListAsync();

        var subjects = await _db.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == programId && x.ConHoatDong)
            .OrderBy(x => x.HocKyDuKien)
            .ThenBy(x => x.ThuTu)
            .Select(x => new
            {
                x.MaChuongTrinhMonHoc,
                x.MaMonHoc,
                MaCodeMonHoc = x.DanhMucMonHoc != null ? x.DanhMucMonHoc.MaCodeMonHoc : "",
                TenMonHoc = x.DanhMucMonHoc != null ? x.DanhMucMonHoc.TenMonHoc : "",
                x.HocKyDuKien,
                x.SoTinChi,
                x.LoaiMonHoc,
                x.BatBuoc,
                x.ThuTu,
                x.ConHoatDong
            })
            .ToListAsync();

        return Ok(new
        {
            data = new
            {
                program,
                terms,
                subjects,
                semesterCount = terms.Count,
                subjectCount = subjects.Count,
                totalCredits = subjects.Sum(x => x.SoTinChi)
            },
            message = "Success"
        });
    }

    [HttpGet("master-data/academic-terms")]
    [BghResponseCache(600)]
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
    [BghResponseCache(600)]
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
    [BghResponseCache(600)]
    public async Task<IActionResult> GetBuildings()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.ToaNhas
            .AsNoTracking()
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .OrderBy(x => x.TenToaNha)
            .Select(x => new
            {
                Id = x.MaToaNha,
                x.MaToaNha,
                MaCode = x.MaCodeToaNha,
                x.MaCodeToaNha,
                x.TenToaNha,
                x.MaDonVi,
                x.SoTang,
                x.ConHoatDong
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/floors")]
    [BghResponseCache(600)]
    public async Task<IActionResult> GetFloors()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.Tangs
            .AsNoTracking()
            .Where(x => isGlobal || x.ToaNha!.MaDonVi == campusId)
            .OrderBy(x => x.MaToaNha)
            .ThenBy(x => x.ThuTuTang)
            .Select(x => new
            {
                Id = x.MaTang,
                x.MaTang,
                x.MaToaNha,
                MaCodeToaNha = x.ToaNha != null ? x.ToaNha.MaCodeToaNha : "",
                TenToaNha = x.ToaNha != null ? x.ToaNha.TenToaNha : "",
                MaDonVi = x.ToaNha != null ? x.ToaNha.MaDonVi : 0,
                x.TenTang,
                x.ThuTuTang,
                x.ConHoatDong
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("master-data/rooms")]
    [BghResponseCache(600)]
    public async Task<IActionResult> GetRooms()
    {
        var (campusId, isGlobal) = GetUserScope();
        var data = await _db.PhongHocs
            .AsNoTracking()
            .Where(x => isGlobal || x.MaDonVi == campusId)
            .OrderBy(x => x.MaToaNha)
            .ThenBy(x => x.MaTang)
            .ThenBy(x => x.MaCodePhong)
            .Select(x => new
            {
                Id = x.MaPhong,
                x.MaPhong,
                x.MaDonVi,
                x.MaToaNha,
                MaCodeToaNha = x.ToaNha != null ? x.ToaNha.MaCodeToaNha : "",
                TenToaNha = x.ToaNha != null ? x.ToaNha.TenToaNha : "",
                x.MaTang,
                TenTang = x.Tang != null ? x.Tang.TenTang : "",
                ThuTuTang = x.Tang != null ? x.Tang.ThuTuTang : 0,
                MaCode = x.MaCodePhong,
                x.MaCodePhong,
                x.TenPhong,
                x.LoaiPhong,
                x.SucChua,
                x.TrangThaiPhong,
                Equipment = _db.ThietBiPhongs
                    .Where(equipment => equipment.MaPhong == x.MaPhong)
                    .OrderBy(equipment => equipment.TenThietBi)
                    .Select(equipment => new
                    {
                        equipment.MaThietBi,
                        equipment.TenThietBi,
                        equipment.SoLuong
                    })
                    .ToList()
            })
            .ToListAsync();
        return Ok(new { data = data, message = "Success" });
    }

    [HttpGet("users")]
    [BghResponseCache(20)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? keyword = null,
        [FromQuery(Name = "role")] string? roleCode = null,
        [FromQuery] string? status = null)
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

        if (!string.IsNullOrWhiteSpace(roleCode))
            query = query.Where(x => x.VaiTroChinh == roleCode);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.TrangThai == status);

        var totalItems = await query.CountAsync();
        var data = await query
            .OrderBy(x => x.VaiTroChinh == "hoc_sinh" ? 1 : 0)
            .ThenBy(x => x.VaiTroChinh)
            .ThenByDescending(x => x.NgayTao)
            .ThenBy(x => x.HoTen)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data,
            pagination = new
            {
                pageIndex,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            message = "Success"
        });
    }

    [HttpGet("schedules")]
    [BghResponseCache(20)]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] string? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50)
    {
        var (campusId, isGlobal) = GetUserScope();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
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

        var st = status?.ToLowerInvariant().Trim();
        if (st == "published" || st == "da_xuat_ban" || st == "da_duyet" || st == "approved" || st == "cong_bo")
        {
            query = query.Where(x => x.Schedule.TrangThai == "da_xuat_ban" || x.Schedule.TrangThai == "da_duyet" || x.Schedule.TrangThai == "cong_bo");
        }
        else if (st == "cancelled" || st == "da_huy" || st == "tu_choi" || st == "rejected")
        {
            query = query.Where(x => x.Schedule.TrangThai == "da_huy" || x.Schedule.TrangThai == "tu_choi");
        }
        else
        {
            query = query.Where(x => x.Schedule.TrangThai == "nhap" || x.Schedule.TrangThai == "cho_duyet" || string.IsNullOrEmpty(x.Schedule.TrangThai));
        }

        var totalItems = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.Schedule.NgayTao)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
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
                ShiftId = x.Schedule.MaCaHoc,
                ShiftStart = x.Schedule.CaHoc != null ? x.Schedule.CaHoc.GioBatDau : (TimeOnly?)null,
                ShiftEnd = x.Schedule.CaHoc != null ? x.Schedule.CaHoc.GioKetThuc : (TimeOnly?)null,
                x.Schedule.ThuTrongTuan,
                x.Schedule.NgayBatDau,
                x.Schedule.NgayKetThuc,
                Status = (x.Schedule.TrangThai == "da_xuat_ban" || x.Schedule.TrangThai == "da_duyet" || x.Schedule.TrangThai == "cong_bo") ? "approved" : (x.Schedule.TrangThai == "da_huy" || x.Schedule.TrangThai == "tu_choi") ? "rejected" : "pending",
                Submitter = x.Course.GiaoVien != null ? x.Course.GiaoVien.HoTen : "Giáo vụ vận hành",
                Sender = x.Course.GiaoVien != null ? x.Course.GiaoVien.HoTen : "Giáo vụ vận hành",
                Conflicts = _db.ThoiKhoaBieus.Count(other =>
                    other.MaTkb != x.Schedule.MaTkb &&
                    other.TrangThai != "da_huy" &&
                    other.ThuTrongTuan == x.Schedule.ThuTrongTuan &&
                    other.MaCaHoc == x.Schedule.MaCaHoc &&
                    other.KhoaHoc != null &&
                    other.KhoaHoc.MaHocKy == x.Course.MaHocKy &&
                    (other.MaPhong == x.Schedule.MaPhong ||
                     other.KhoaHoc.MaGiaoVien == x.Course.MaGiaoVien)),
                Type = "Lịch học chính khóa",
                Classes = 1,
                Teachers = 1,
                Hours = x.Schedule.CaHoc != null
                    ? useClientTimeCalculation
                        ? (x.Schedule.CaHoc.GioKetThuc.ToTimeSpan() -
                           x.Schedule.CaHoc.GioBatDau.ToTimeSpan()).TotalHours
                        : EF.Functions.DateDiffMinute(
                            x.Schedule.CaHoc.GioBatDau,
                            x.Schedule.CaHoc.GioKetThuc) / 60.0
                    : 0,
                Campus = x.Course.DonVi != null ? x.Course.DonVi.TenDonVi : "",
                Created = x.Schedule.NgayTao
            })
            .ToListAsync();
        return Ok(new
        {
            data,
            pagination = new
            {
                pageIndex,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            message = "Success"
        });
    }

    /// <summary>
    /// Phê duyệt (xuất bản) một bộ TKB — chuyển trạng thái sang "da_xuat_ban" hoặc "da_duyet".
    /// </summary>
    [HttpPost("schedules/{scheduleId}/approve")]
    public async Task<IActionResult> ApproveSchedule(string scheduleId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var rawIdStr = scheduleId.Replace("TKB-", "");
        if (!int.TryParse(rawIdStr, out var id))
            return BadRequest(new { message = "Mã TKB không hợp lệ.", scheduleId });

        var tkb = await _db.ThoiKhoaBieus
            .Include(t => t.KhoaHoc)
            .FirstOrDefaultAsync(t =>
                t.MaTkb == id && (isGlobal || (t.KhoaHoc != null && t.KhoaHoc.MaDonVi == campusId)));
        if (tkb == null)
            return NotFound(new { message = "Không tìm thấy TKB.", scheduleId });

        tkb.TrangThai = "da_xuat_ban";
        tkb.NgayCapNhat = DateTime.UtcNow;

        _db.NhatKyKiemToans.Add(new Models.NhatKyKiemToan
        {
            MaDonVi = tkb.KhoaHoc?.MaDonVi ?? campusId,
            LoaiDoiTuong = "ThoiKhoaBieu",
            MaDoiTuong = id.ToString(),
            HanhDong = "APPROVE",
            MoTa = $"BGH phê duyệt Thời khóa biểu #{id}",
            ThoiDiemThayDoi = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _cache.RemoveByPrefix("bgh:");

        return Ok(new { message = "Đã phê duyệt TKB thành công.", scheduleId, status = "approved" });
    }

    /// <summary>
    /// Từ chối (hủy) một bộ TKB — chuyển trạng thái sang "tu_choi" hoặc "da_huy".
    /// </summary>
    [HttpPost("schedules/{scheduleId}/reject")]
    public async Task<IActionResult> RejectSchedule(string scheduleId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var rawIdStr = scheduleId.Replace("TKB-", "");
        if (!int.TryParse(rawIdStr, out var id))
            return BadRequest(new { message = "Mã TKB không hợp lệ.", scheduleId });

        var tkb = await _db.ThoiKhoaBieus
            .Include(t => t.KhoaHoc)
            .FirstOrDefaultAsync(t =>
                t.MaTkb == id && (isGlobal || (t.KhoaHoc != null && t.KhoaHoc.MaDonVi == campusId)));
        if (tkb == null)
            return NotFound(new { message = "Không tìm thấy TKB.", scheduleId });

        tkb.TrangThai = "da_huy";
        tkb.NgayCapNhat = DateTime.UtcNow;

        _db.NhatKyKiemToans.Add(new Models.NhatKyKiemToan
        {
            MaDonVi = tkb.KhoaHoc?.MaDonVi ?? campusId,
            LoaiDoiTuong = "ThoiKhoaBieu",
            MaDoiTuong = id.ToString(),
            HanhDong = "REJECT",
            MoTa = $"BGH trả về Thời khóa biểu #{id}",
            ThoiDiemThayDoi = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _cache.RemoveByPrefix("bgh:");

        return Ok(new { message = "Đã từ chối TKB.", scheduleId, status = "rejected" });
    }

    [HttpGet("audit-logs")]
    [BghResponseCache(20)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? keyword = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var (campusId, isGlobal) = GetUserScope();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.NhatKyKiemToans
            .AsNoTracking()
            .Where(x => isGlobal || x.MaDonVi == campusId);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(x =>
                (x.MoTa != null && x.MoTa.Contains(normalizedKeyword)) ||
                (x.NguoiThayDoiNavigation != null && x.NguoiThayDoiNavigation.HoTen.Contains(normalizedKeyword)));
        }
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.LoaiDoiTuong == entityType);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.HanhDong == action);
        if (fromDate.HasValue)
            query = query.Where(x => x.ThoiDiemThayDoi >= fromDate.Value.Date);
        if (toDate.HasValue)
        {
            var exclusiveEnd = toDate.Value.Date.AddDays(1);
            query = query.Where(x => x.ThoiDiemThayDoi < exclusiveEnd);
        }
        var totalItems = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ThoiDiemThayDoi)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
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
        return Ok(new
        {
            data,
            pagination = new
            {
                pageIndex,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            message = "Success"
        });
    }

    [HttpGet("master-data/subjects")]
    [BghResponseCache(600)]
    public async Task<IActionResult> GetSubjects([FromQuery] string? keyword = null)
    {
        var (campusId, isGlobal) = GetUserScope();
        var normalizedKeyword = keyword?.Trim();
        var data = await _db.DanhMucMonHocs
            .AsNoTracking()
            .Where(x => isGlobal || _db.MonHocTrongChuongTrinhs.Any(p =>
                p.MaMonHoc == x.MaMonHoc &&
                p.ChuongTrinhDaoTao != null &&
                p.ChuongTrinhDaoTao.LopHanhChinhs.Any(l => l.MaDonVi == campusId)))
            .Where(x => string.IsNullOrEmpty(normalizedKeyword) ||
                        x.TenMonHoc.Contains(normalizedKeyword) ||
                        x.MaCodeMonHoc.Contains(normalizedKeyword))
            .OrderBy(x => x.TenMonHoc)
            .Select(x => new { Id = x.MaMonHoc, MaCode = x.MaCodeMonHoc, TenMonHoc = x.TenMonHoc, TrangThai = x.ConHoatDong ? "Hoạt động" : "Ngừng" })
            .ToListAsync();
        return Ok(new { data, message = "Success" });
    }

    [HttpGet("rbac/roles")]
    [BghResponseCache(600)]
    public async Task<IActionResult> GetRoles()
    {
        var data = await _db.VaiTros
            .AsNoTracking()
            .Select(x => new { Id = x.MaVaiTro, MaVaiTro = x.MaVaiTro, MaCode = x.MaCodeVaiTro, MaCodeVaiTro = x.MaCodeVaiTro, TenVaiTro = x.TenVaiTro })
            .ToListAsync();
        return Ok(new { data, message = "Success" });
    }

}

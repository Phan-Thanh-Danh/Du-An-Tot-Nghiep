using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/parent")]
[Authorize(Roles = AuthRoles.Parent)]
public class ParentController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ParentController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponseDto<ParentDashboardDto>>> GetDashboard(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var childrenIds = await GetLinkedChildrenIds(userId, ct);
        var children = await _db.NguoiDungs
            .Where(n => childrenIds.Contains(n.MaNguoiDung))
            .Select(n => new ParentChildSummaryDto
            {
                Id = n.MaNguoiDung,
                Name = n.HoTen,
                ClassName = n.Lop != null ? n.Lop.TenLop : "",
                Status = n.TrangThai
            })
            .ToListAsync(ct);

        var unreadNotifications = await _db.ThongBaoNguoiNhans
            .CountAsync(t => t.MaNguoiNhan == userId && !t.DaDoc, ct);

        var data = new ParentDashboardDto
        {
            Children = children,
            UnreadNotifications = unreadNotifications
        };

        return Ok(ApiResponseDto<ParentDashboardDto>.Ok(data));
    }

    [HttpGet("children")]
    public async Task<ActionResult<ApiResponseDto<List<ParentChildSummaryDto>>>> GetChildren(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var childrenIds = await GetLinkedChildrenIds(userId, ct);

        var children = await _db.NguoiDungs
            .Where(n => childrenIds.Contains(n.MaNguoiDung))
            .Select(n => new ParentChildSummaryDto
            {
                Id = n.MaNguoiDung,
                Name = n.HoTen,
                ClassName = n.Lop != null ? n.Lop.TenLop : "",
                Status = n.TrangThai
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<List<ParentChildSummaryDto>>.Ok(children));
    }

    [HttpGet("children/{childId:int}")]
    public async Task<ActionResult<ApiResponseDto<ParentChildDetailDto>>> GetChildDetail(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var child = await _db.NguoiDungs
            .Include(n => n.Lop)
            .FirstOrDefaultAsync(n => n.MaNguoiDung == childId, ct);
        if (child == null) return NotFound();

        var enrollments = await _db.DangKyHocPhans
            .CountAsync(d => d.MaHocSinh == childId && d.TrangThai == "da_duyet", ct);
        var gpa = await _db.DiemSos
            .Where(d => d.MaHocSinh == childId)
            .AverageAsync(d => (double?)d.GpaMonHoc) ?? 0;

        var data = new ParentChildDetailDto
        {
            Id = child.MaNguoiDung,
            Name = child.HoTen,
            Email = child.Email,
            Phone = child.SoDienThoai ?? "",
            ClassName = child.Lop?.TenLop ?? "",
            EnrolledCourses = enrollments,
            Gpa = Math.Round(gpa, 2)
        };

        return Ok(ApiResponseDto<ParentChildDetailDto>.Ok(data));
    }

    [HttpGet("children/{childId:int}/grades")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildGrades(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var grades = await _db.DiemSos
            .Include(d => d.MonHoc)
            .Include(d => d.HocKy)
            .Where(d => d.MaHocSinh == childId)
            .Select(d => new
            {
                Subject = d.MonHoc != null ? d.MonHoc.TenMonHoc : "",
                Code = d.MonHoc != null ? d.MonHoc.MaCodeMonHoc : "",
                ProcessScore = d.DiemQuaTrinh,
                MidtermScore = d.DiemGiuaKy,
                FinalScore = d.DiemCuoiKy,
                Total = d.GpaMonHoc,
                Semester = d.HocKy != null ? d.HocKy.TenHocKy : ""
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(grades));
    }

    [HttpGet("children/{childId:int}/schedule")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildSchedule(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var enrolledLopHocPhanIds = await _db.DangKyHocPhans
            .Where(d => d.MaHocSinh == childId && d.TrangThai == "da_duyet")
            .Select(d => d.MaLopHocPhan)
            .ToListAsync(ct);

        var courseIds = await _db.KhoaHocs
            .Where(k => k.MaLopHocPhan != null && enrolledLopHocPhanIds.Contains(k.MaLopHocPhan.Value))
            .Select(k => k.MaKhoaHoc)
            .ToListAsync(ct);

        var schedule = await _db.ThoiKhoaBieus
            .Include(t => t.KhoaHoc!)
                .ThenInclude(k => k.MonHoc)
            .Include(t => t.KhoaHoc!)
                .ThenInclude(k => k.GiaoVien)
            .Include(t => t.Phong)
            .Include(t => t.CaHoc)
            .Where(t => courseIds.Contains(t.MaKhoaHoc))
            .Select(t => new
            {
                Day = t.ThuTrongTuan,
                Time = t.CaHoc != null ? t.CaHoc.TenCa : "",
                Subject = t.KhoaHoc!.MonHoc != null ? t.KhoaHoc.MonHoc.TenMonHoc : "",
                Room = t.Phong != null ? t.Phong.MaCodePhong : "",
                Teacher = t.KhoaHoc.GiaoVien != null ? t.KhoaHoc.GiaoVien.HoTen : ""
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(schedule));
    }

    [HttpGet("children/{childId:int}/attendance")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildAttendance(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var attendance = await _db.DiemDanhs
            .Include(d => d.BuoiHoc!)
                .ThenInclude(b => b.KhoaHoc!)
                .ThenInclude(k => k.MonHoc)
            .Where(d => d.MaHocSinh == childId)
            .Select(d => new
            {
                Subject = d.BuoiHoc!.KhoaHoc!.MonHoc != null ? d.BuoiHoc.KhoaHoc.MonHoc.TenMonHoc : "",
                Date = d.BuoiHoc.NgayHoc,
                Status = d.TrangThai
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(attendance));
    }

    [HttpGet("children/{childId:int}/alerts")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildAlerts(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var absentCount = await _db.DiemDanhs
            .CountAsync(d => d.MaHocSinh == childId && d.TrangThai == "vang", ct);

        var lowGrades = await _db.DiemSos
            .CountAsync(d => d.MaHocSinh == childId && d.GpaMonHoc < 5, ct);

        var alerts = new List<object>();

        if (absentCount > 3)
        {
            alerts.Add(new { Type = "attendance", Message = $"Sinh viên đã vắng {absentCount} buổi.", Severity = "warning" });
        }

        if (lowGrades > 0)
        {
            alerts.Add(new { Type = "grade", Message = $"Có {lowGrades} môn dưới điểm trung bình.", Severity = "danger" });
        }

        return Ok(ApiResponseDto<object>.Ok(new { Alerts = alerts }));
    }

    [HttpGet("children/{childId:int}/tuition")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildTuition(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var invoices = await _db.HoaDons
            .Where(h => h.MaHocSinh == childId)
            .Select(h => new
            {
                Id = h.MaHoaDon,
                Amount = h.SoTien,
                DueDate = h.HanThanhToan,
                Status = h.TrangThai ?? ""
            })
            .ToListAsync(ct);

        var totalDue = invoices.Where(i => i.Status != "da_thanh_toan").Sum(i => i.Amount);

        return Ok(ApiResponseDto<object>.Ok(new { Invoices = invoices, TotalDue = totalDue }));
    }

    [HttpGet("children/{childId:int}/transactions")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildTransactions(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var transactions = await _db.GiaoDichs
            .Include(g => g.HoaDon)
            .Where(g => g.HoaDon != null && g.HoaDon.MaHocSinh == childId)
            .OrderByDescending(g => g.NgayTao)
            .Select(g => new
            {
                Id = g.MaGiaoDich,
                Amount = g.SoTien,
                Date = g.NgayTao,
                Method = g.NhaCungCapThanhToan ?? "",
                Status = g.TrangThai ?? ""
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(transactions));
    }

    [HttpGet("children/{childId:int}/invoices")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetChildInvoices(
        int childId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, childId, ct);

        var invoices = await _db.HoaDons
            .Where(h => h.MaHocSinh == childId)
            .OrderByDescending(h => h.NgayTao)
            .Select(h => new
            {
                Id = h.MaHoaDon,
                Amount = h.SoTien,
                CreatedAt = h.NgayTao,
                DueDate = h.HanThanhToan,
                Status = h.TrangThai ?? ""
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(invoices));
    }

    [HttpPost("payment")]
    public async Task<ActionResult<ApiResponseDto<object>>> MakePayment(
        [FromBody] ParentPaymentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await VerifyChildLinked(userId, request.ChildId, ct);

        var invoice = await _db.HoaDons
            .FirstOrDefaultAsync(h => h.MaHoaDon == request.InvoiceId, ct);
        if (invoice == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy hóa đơn."));

        return Ok(ApiResponseDto<object>.Ok(new { Success = true, Message = "Yêu cầu thanh toán đã được ghi nhận." }));
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetNotifications(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var notifications = await _db.ThongBaoNguoiNhans
            .Include(t => t.ThongBao)
            .Where(t => t.MaNguoiNhan == userId)
            .OrderByDescending(t => t.ThongBao != null ? t.ThongBao.NgayTao : DateTime.MinValue)
            .Select(t => new
            {
                Id = t.MaThongBao,
                Title = t.ThongBao != null ? t.ThongBao.TieuDe : "",
                Content = t.ThongBao != null ? t.ThongBao.NoiDung : "",
                IsRead = t.DaDoc,
                CreatedAt = t.ThongBao != null ? t.ThongBao.NgayTao : DateTime.MinValue
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(notifications));
    }

    [HttpGet("notifications/history")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetNotificationHistory(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var history = await _db.ThongBaoNguoiNhans
            .Include(t => t.ThongBao)
            .Where(t => t.MaNguoiNhan == userId && t.DaDoc)
            .OrderByDescending(t => t.ThongBao != null ? t.ThongBao.NgayTao : DateTime.MinValue)
            .Select(t => new
            {
                Id = t.MaThongBao,
                Title = t.ThongBao != null ? t.ThongBao.TieuDe : "",
                Content = t.ThongBao != null ? t.ThongBao.NoiDung : "",
                ReadAt = t.DocLuc,
                CreatedAt = t.ThongBao != null ? t.ThongBao.NgayTao : DateTime.MinValue
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(history));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetProfile(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var user = await _db.NguoiDungs
            .Include(n => n.DonVi)
            .FirstOrDefaultAsync(n => n.MaNguoiDung == userId, ct);
        if (user == null) return NotFound();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Id = user.MaNguoiDung,
            Name = user.HoTen,
            Email = user.Email,
            Phone = user.SoDienThoai ?? "",
            Campus = user.DonVi != null ? user.DonVi.TenDonVi : ""
        }));
    }

    [HttpGet("access-rights")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAccessRights(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var links = await _db.LienKetPhuHuynhs
            .Where(l => l.MaPhuHuynh == userId && l.TrangThai == "hoat_dong")
            .Select(l => new
            {
                ChildId = l.MaHocSinh,
                Permissions = l.QuyenXem
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new { Links = links }));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }

    private async Task<List<int>> GetLinkedChildrenIds(int parentUserId, CancellationToken ct)
    {
        return await _db.LienKetPhuHuynhs
            .Where(l => l.MaPhuHuynh == parentUserId && l.TrangThai == "hoat_dong")
            .Select(l => l.MaHocSinh)
            .ToListAsync(ct);
    }

    private async Task VerifyChildLinked(int parentUserId, int childId, CancellationToken ct)
    {
        var linked = await _db.LienKetPhuHuynhs
            .AnyAsync(l => l.MaPhuHuynh == parentUserId && l.MaHocSinh == childId && l.TrangThai == "hoat_dong", ct);
        if (!linked)
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông tin của học sinh này.");
    }
}

// DTOs
public class ParentDashboardDto
{
    public List<ParentChildSummaryDto> Children { get; set; } = [];
    public int UnreadNotifications { get; set; }
}

public class ParentChildSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ParentChildDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int EnrolledCourses { get; set; }
    public double Gpa { get; set; }
}

public class ParentPaymentRequest
{
    public int ChildId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
}

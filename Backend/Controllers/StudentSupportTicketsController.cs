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
[Route("api/student/support-tickets")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentSupportTicketsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StudentSupportTicketsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<object>>> GetTickets(CancellationToken ct)
    {
        await EnsureHasPermissionAsync("requests.read", ct);
        var userId = GetCurrentUserId();
        var tickets = await _db.PhieuHoTros
            .Where(p => p.MaHocSinh == userId)
            .OrderByDescending(p => p.NgayTao)
            .Select(p => new
            {
                Id = p.MaPhieuHt,
                Code = $"TCK-{p.MaPhieuHt:D3}",
                Title = p.TieuDe,
                CategoryDb = p.DanhMuc,
                StatusDb = p.TrangThai,
                AssignedTo = p.PhanCongChoNavigation != null ? p.PhanCongChoNavigation.HoTen : "",
                CreatedAt = p.NgayTao,
                Deadline = p.HanXuLy
            })
            .ToListAsync(ct);

        var result = tickets.Select(p => new
        {
            p.Id,
            p.Code,
            p.Title,
            Category = MapCategoryToUi(p.CategoryDb),
            Status = MapStatusToUi(p.StatusDb),
            p.AssignedTo,
            p.CreatedAt,
            p.Deadline
        });

        return Ok(ApiResponseDto<object>.Ok(result));
    }

    [HttpGet("{ticketId:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetTicketDetail(
        int ticketId, CancellationToken ct)
    {
        await EnsureHasPermissionAsync("requests.read", ct);
        var userId = GetCurrentUserId();
        var ticket = await _db.PhieuHoTros
            .Include(p => p.HocSinh)
            .Include(p => p.PhanCongChoNavigation)
            .FirstOrDefaultAsync(p => p.MaPhieuHt == ticketId && p.MaHocSinh == userId, ct);

        if (ticket == null) return NotFound();

        var messages = await _db.TinNhanHoTros
            .Where(t => t.MaPhieuHt == ticketId)
            .OrderBy(t => t.NgayTao)
            .Select(t => new
            {
                Sender = t.NguoiGui != null ? t.NguoiGui.HoTen : "",
                Text = t.NoiDung,
                Time = t.NgayTao,
                IsMe = t.MaNguoiGui == userId,
                AttachmentUrl = t.UrlDinhKem
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Id = ticket.MaPhieuHt,
            Code = $"TCK-{ticket.MaPhieuHt:D3}",
            Title = ticket.TieuDe,
            Category = MapCategoryToUi(ticket.DanhMuc),
            Status = MapStatusToUi(ticket.TrangThai),
            Description = ticket.MoTa,
            AssignedTo = ticket.PhanCongChoNavigation?.HoTen ?? "",
            CreatedAt = ticket.NgayTao,
            Urgency = ticket.DoUuTien,
            AttachmentUrl = ticket.UrlDinhKem,
            Messages = messages
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<object>>> CreateTicket(
        [FromBody] CreateSupportTicketRequest request, CancellationToken ct)
    {
        await EnsureHasPermissionAsync("requests.create", ct);
        var userId = GetCurrentUserId();
        var ticket = new Models.PhieuHoTro
        {
            MaHocSinh = userId,
            DanhMuc = MapCategoryToDb(request.Category),
            TieuDe = request.Title,
            MoTa = request.Description ?? "",
            TrangThai = "mo",
            DoUuTien = "normal",
            NgayTao = DateTime.UtcNow,
            UrlDinhKem = request.AttachmentUrl
        };

        _db.PhieuHoTros.Add(ticket);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Id = ticket.MaPhieuHt,
            Code = $"TCK-{ticket.MaPhieuHt:D3}",
            Message = "Ticket đã được tạo thành công."
        }));
    }

    [HttpPost("{ticketId:int}/messages")]
    public async Task<ActionResult<ApiResponseDto<object>>> SendMessage(
        int ticketId, [FromBody] SendTicketMessageRequest request, CancellationToken ct)
    {
        await EnsureHasPermissionAsync("requests.create", ct);
        var userId = GetCurrentUserId();
        var ticket = await _db.PhieuHoTros
            .FirstOrDefaultAsync(p => p.MaPhieuHt == ticketId && p.MaHocSinh == userId, ct);
        if (ticket == null) return NotFound();

        var message = new Models.TinNhanHoTro
        {
            MaPhieuHt = ticketId,
            MaNguoiGui = userId,
            NoiDung = request.Content,
            UrlDinhKem = request.AttachmentUrl,
            NgayTao = DateTime.UtcNow
        };

        _db.TinNhanHoTros.Add(message);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
    }

    [HttpPost("{ticketId:int}/close")]
    public async Task<ActionResult<ApiResponseDto<object>>> CloseTicket(
        int ticketId, [FromBody] CloseTicketRequest? request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var ticket = await _db.PhieuHoTros
            .FirstOrDefaultAsync(p => p.MaPhieuHt == ticketId && p.MaHocSinh == userId, ct);
        if (ticket == null) return NotFound();

        ticket.TrangThai = "da_dong";
        ticket.DanhGiaHaiLong = request?.Satisfaction;
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }

    private static string MapCategoryToDb(string? uiCategory) => uiCategory switch
    {
        "Kỹ thuật" => "ky_thuat",
        "Học vụ" => "hoc_vu",
        "Tài chính" => "tai_chinh",
        _ => "khac"
    };

    private static string MapCategoryToUi(string dbCategory) => dbCategory switch
    {
        "ky_thuat" => "Kỹ thuật",
        "hoc_vu" => "Học vụ",
        "tai_chinh" => "Tài chính",
        _ => "Khác"
    };

    private static string MapStatusToUi(string dbStatus) => dbStatus switch
    {
        "mo" => "Open",
        "dang_xu_ly" => "In progress",
        "da_giai_quyet" => "Resolved",
        "da_dong" => "Closed",
        _ => "Open"
    };

    private async Task EnsureHasPermissionAsync(string permissionCode, CancellationToken ct)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var roleCode = currentUser?.Role ?? "hoc_sinh";

        if (roleCode == "SuperAdmin" || roleCode == "sieu_quan_tri" || roleCode == "Admin" || roleCode == "quan_tri")
            return;

        var hasPerm = await _db.VaiTroQuyenHans
            .AsNoTracking()
            .AnyAsync(vp => vp.VaiTro != null &&
                           (vp.VaiTro.MaCodeVaiTro == roleCode || vp.VaiTro.MaCodeVaiTro == "hoc_sinh") &&
                           vp.QuyenHan != null && vp.QuyenHan.MaCode == permissionCode, ct);

        if (!hasPerm)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, $"Vai trò của bạn chưa được cấp quyền '{permissionCode}' để thực hiện hành động này.");
        }
    }
}

public class CreateSupportTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? AttachmentUrl { get; set; }
}

public class SendTicketMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class CloseTicketRequest
{
    public int? Satisfaction { get; set; }
}

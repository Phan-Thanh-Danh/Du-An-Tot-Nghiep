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
        var userId = GetCurrentUserId();
        var tickets = await _db.PhieuHoTros
            .Where(p => p.MaHocSinh == userId)
            .OrderByDescending(p => p.NgayTao)
            .Select(p => new
            {
                Id = $"TCK-{p.MaPhieuHt:D3}",
                Title = p.TieuDe,
                Category = p.DanhMuc,
                Status = p.TrangThai,
                AssignedTo = p.PhanCongChoNavigation != null ? p.PhanCongChoNavigation.HoTen : "",
                CreatedAt = p.NgayTao,
                Deadline = p.HanXuLy
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(tickets));
    }

    [HttpGet("{ticketId:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetTicketDetail(
        int ticketId, CancellationToken ct)
    {
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
                IsMe = t.MaNguoiGui == userId
            })
            .ToListAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Id = $"TCK-{ticket.MaPhieuHt:D3}",
            Title = ticket.TieuDe,
            Category = ticket.DanhMuc,
            Status = ticket.TrangThai,
            Description = ticket.MoTa,
            AssignedTo = ticket.PhanCongChoNavigation?.HoTen ?? "",
            CreatedAt = ticket.NgayTao,
            Urgency = ticket.DoUuTien,
            Messages = messages
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<object>>> CreateTicket(
        [FromBody] CreateSupportTicketRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var ticket = new Models.PhieuHoTro
        {
            MaHocSinh = userId,
            DanhMuc = request.Category ?? "Khác",
            TieuDe = request.Title,
            MoTa = request.Description ?? "",
            TrangThai = "open",
            DoUuTien = "normal",
            NgayTao = DateTime.UtcNow
        };

        _db.PhieuHoTros.Add(ticket);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Id = ticket.MaPhieuHt,
            Message = "Ticket đã được tạo thành công."
        }));
    }

    [HttpPost("{ticketId:int}/messages")]
    public async Task<ActionResult<ApiResponseDto<object>>> SendMessage(
        int ticketId, [FromBody] SendTicketMessageRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var ticket = await _db.PhieuHoTros
            .FirstOrDefaultAsync(p => p.MaPhieuHt == ticketId && p.MaHocSinh == userId, ct);
        if (ticket == null) return NotFound();

        var message = new Models.TinNhanHoTro
        {
            MaPhieuHt = ticketId,
            MaNguoiGui = userId,
            NoiDung = request.Content,
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

        ticket.TrangThai = "closed";
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
}

public class CreateSupportTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}

public class SendTicketMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class CloseTicketRequest
{
    public int? Satisfaction { get; set; }
}

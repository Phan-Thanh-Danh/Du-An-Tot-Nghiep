using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Finance;
using Backend.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;
    private readonly ILogger<FinanceController> _logger;

    public FinanceController(
        IFinanceService financeService,
        ILogger<FinanceController> logger)
    {
        _financeService = financeService;
        _logger = logger;
    }

    private CurrentUserContext? GetCurrentUser()
    {
        return HttpContext.Items["CurrentUser"] as CurrentUserContext;
    }

    [HttpGet("hoa-don")]
    [HttpGet("invoices")]
    public async Task<ActionResult> GetInvoices(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (user == null || user.CampusId <= 0)
            return Forbid();

        var (items, total) = await _financeService.GetInvoicesAsync(
            user.CampusId, pageIndex, pageSize, cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách hóa đơn thành công.",
            data = items,
            pagination = new
            {
                pageIndex = Math.Max(pageIndex, 1),
                pageSize = Math.Clamp(pageSize, 1, 100),
                total,
                totalPages = (total + Math.Clamp(pageSize, 1, 100) - 1) / Math.Clamp(pageSize, 1, 100)
            }
        });
    }

    [HttpGet("hoa-don/{id:int}")]
    [HttpGet("invoices/{id:int}")]
    public async Task<ActionResult> GetInvoiceDetail(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (user == null || user.CampusId <= 0)
            return Forbid();

        var detail = await _financeService.GetInvoiceDetailAsync(
            id, user.CampusId, cancellationToken);

        if (detail == null)
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy hóa đơn hoặc không có quyền truy cập.",
                errors = new[] { "Không tìm thấy hóa đơn hoặc không có quyền truy cập." }
            });

        return Ok(new
        {
            success = true,
            message = "Lấy chi tiết hóa đơn thành công.",
            data = detail
        });
    }

    [HttpGet("monitor")]
    public async Task<ActionResult> GetMonitor(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (user == null || user.CampusId <= 0)
            return Forbid();

        var monitor = await _financeService.GetMonitorAsync(
            user.CampusId, fromDate, toDate, cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Lấy dữ liệu tổng quan tài chính thành công.",
            data = monitor
        });
    }

    [HttpGet("giao-dich")]
    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (user == null || user.CampusId <= 0)
            return Forbid();

        var (items, total) = await _financeService.GetTransactionsAsync(
            user.CampusId, pageIndex, pageSize, cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách giao dịch thành công.",
            data = items,
            pagination = new
            {
                pageIndex = Math.Max(pageIndex, 1),
                pageSize = Math.Clamp(pageSize, 1, 100),
                total,
                totalPages = (total + Math.Clamp(pageSize, 1, 100) - 1) / Math.Clamp(pageSize, 1, 100)
            }
        });
    }

    [HttpPost("payment/create-link")]
    public async Task<ActionResult> CreatePaymentLink(
        [FromBody] CreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (user == null || user.CampusId <= 0)
            return Forbid();

        return Ok(new
        {
            success = true,
            message = "Tạo link thanh toán thành công",
            data = new
            {
                maHoaDon = request.MaHoaDon,
                soTien = request.SoTien,
                paymentLink = $"https://pay.payos.vn/web/{request.MaHoaDon}",
                qrCode = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=PAYOS_{request.MaHoaDon}_{request.SoTien}"
            }
        });
    }

    [HttpPost("hoa-don")]
    [HttpPost("invoices")]
    [Authorize(Roles = "FinanceAdmin,CampusAccountant,CampusChiefAccountant,CampusAdmin,Principal,SuperAdmin,Admin")]
    public async Task<ActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var maDonVi = user?.CampusId ?? 0;
        var maNguoiDung = user?.UserId ?? 0;
        if (maDonVi <= 0 || maNguoiDung <= 0)
            return Forbid();

        try
        {
            var result = await _financeService.CreateInvoiceAsync(
                maDonVi, request, cancellationToken);
            return CreatedAtAction(nameof(GetInvoiceDetail), new { id = result.MaHoaDon },
                new { data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("hoa-don/{id:int}/status")]
    [HttpPatch("invoices/{id:int}/status")]
    [Authorize(Roles = "FinanceAdmin,CampusAccountant,CampusChiefAccountant,CampusAdmin,Principal,SuperAdmin,Admin")]
    public async Task<ActionResult> UpdateInvoiceStatus(
        [FromRoute] int id,
        [FromBody] UpdateInvoiceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var maDonVi = user?.CampusId ?? 0;
        var maNguoiDung = user?.UserId ?? 0;
        if (maDonVi <= 0 || maNguoiDung <= 0)
            return Forbid();

        try
        {
            var success = await _financeService.UpdateInvoiceStatusAsync(
                id, maDonVi, request, maNguoiDung, cancellationToken);

            if (!success)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy hóa đơn hoặc không có quyền truy cập.",
                    errors = new[] { "Không tìm thấy hóa đơn hoặc không có quyền truy cập." }
                });

            return Ok(new { message = "Cập nhật trạng thái hóa đơn thành công." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refund-requests")]
    [Authorize(Roles = "FinanceAdmin,CampusAccountant,CampusChiefAccountant,CampusAdmin,Principal,SuperAdmin,Admin")]
    public async Task<ActionResult> CreateRefundRequest(
        [FromBody] CreateRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var maDonVi = user?.CampusId ?? 0;
        var maNguoiDung = user?.UserId ?? 0;
        if (maDonVi <= 0 || maNguoiDung <= 0)
            return Forbid();

        try
        {
            var result = await _financeService.CreateRefundRequestAsync(
                maDonVi, request, maNguoiDung, cancellationToken);
            return CreatedAtAction(nameof(GetRefundRequests),
                new { data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("refund-requests")]
    [Authorize(Roles = "FinanceAdmin,CampusAccountant,CampusChiefAccountant,CampusAdmin,Principal,SuperAdmin,Admin")]
    public async Task<ActionResult> GetRefundRequests(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var maDonVi = user?.CampusId ?? 0;
        if (maDonVi <= 0)
            return Forbid();

        var (items, total) = await _financeService.GetRefundRequestsAsync(
            maDonVi, pageIndex, pageSize, cancellationToken);

        return Ok(new
        {
            data = items,
            pagination = new
            {
                pageIndex = Math.Max(pageIndex, 1),
                pageSize = Math.Clamp(pageSize, 1, 100),
                total,
                totalPages = (total + Math.Clamp(pageSize, 1, 100) - 1) / Math.Clamp(pageSize, 1, 100)
            }
        });
    }

    [HttpPatch("refund-requests/{id:int}")]
    [Authorize(Roles = "FinanceAdmin,CampusChiefAccountant,CampusAdmin,Principal,SuperAdmin,Admin")]
    public async Task<ActionResult> ApproveRefundRequest(
        [FromRoute] int id,
        [FromBody] ApproveRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var maDonVi = user?.CampusId ?? 0;
        var maNguoiDung = user?.UserId ?? 0;
        if (maDonVi <= 0 || maNguoiDung <= 0)
            return Forbid();

        try
        {
            var success = await _financeService.ApproveRefundRequestAsync(
                id, maDonVi, request, maNguoiDung, cancellationToken);

            if (!success)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy yêu cầu hoàn phí hoặc không có quyền truy cập.",
                    errors = new[] { "Không tìm thấy yêu cầu hoàn phí hoặc không có quyền truy cập." }
                });

            return Ok(new
            {
                message = request.DuaVao
                    ? "Duyệt yêu cầu hoàn phí thành công."
                    : "Từ chối yêu cầu hoàn phí thành công."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreatePaymentLinkRequest
{
    public int MaHoaDon { get; set; }
    public decimal SoTien { get; set; }
    public string? GhiChu { get; set; }
}

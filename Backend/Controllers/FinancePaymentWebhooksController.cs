using System.Text;
using Backend.DTOs.Common;
using Backend.DTOs.Finance.TuitionPayments;
using Backend.Services.Finance.TuitionPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/finance/payments/webhooks")]
public class FinancePaymentWebhooksController : ControllerBase
{
    private readonly ITuitionPaymentService _tuitionPaymentService;

    public FinancePaymentWebhooksController(ITuitionPaymentService tuitionPaymentService)
    {
        _tuitionPaymentService = tuitionPaymentService;
    }

    [HttpPost("payos")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<PayOsWebhookResultDto>>> HandlePayOsWebhook(
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        var result = await _tuitionPaymentService.HandlePayOsWebhookAsync(rawBody, cancellationToken);

        return Ok(ApiResponseDto<PayOsWebhookResultDto>.Ok(result, "Webhook PayOS đã được xử lý."));
    }
}

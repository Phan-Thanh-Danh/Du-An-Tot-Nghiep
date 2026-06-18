using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Finance.TuitionPayments;
using Backend.Exceptions;
using Backend.Services.Finance.TuitionPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/tuition")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentTuitionController : ControllerBase
{
    private readonly ITuitionPaymentService _tuitionPaymentService;

    public StudentTuitionController(ITuitionPaymentService tuitionPaymentService)
    {
        _tuitionPaymentService = tuitionPaymentService;
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<StudentTuitionInvoiceDto>>>> GetInvoices(
        CancellationToken cancellationToken)
    {
        var invoices = await _tuitionPaymentService.GetStudentInvoicesAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<StudentTuitionInvoiceDto>>.Ok(
            invoices,
            "Lấy danh sách hóa đơn học phí thành công."));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<StudentTuitionTransactionDto>>>> GetTransactions(
        CancellationToken cancellationToken)
    {
        var transactions = await _tuitionPaymentService.GetStudentTransactionsAsync(GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<StudentTuitionTransactionDto>>.Ok(
            transactions,
            "Lấy lịch sử giao dịch học phí thành công."));
    }

    [HttpPost("invoices/{invoiceId:int}/payments")]
    public async Task<ActionResult<ApiResponseDto<CreateTuitionPaymentResponse>>> CreatePayment(
        int invoiceId,
        CreateTuitionPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _tuitionPaymentService.CreatePaymentAsync(
            invoiceId,
            GetCurrentUserId(),
            request.Provider,
            cancellationToken);

        return Ok(ApiResponseDto<CreateTuitionPaymentResponse>.Ok(
            payment,
            "Tạo giao dịch thanh toán học phí thành công."));
    }

    [HttpGet("payments/{transactionId:int}")]
    public async Task<ActionResult<ApiResponseDto<CreateTuitionPaymentResponse>>> GetPayment(
        int transactionId,
        CancellationToken cancellationToken)
    {
        var payment = await _tuitionPaymentService.GetPaymentAsync(
            transactionId,
            GetCurrentUserId(),
            cancellationToken);

        return Ok(ApiResponseDto<CreateTuitionPaymentResponse>.Ok(
            payment,
            "Lấy trạng thái giao dịch học phí thành công."));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
        {
            return currentUser.UserId;
        }

        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

using Backend.DTOs.Finance.TuitionPayments;

namespace Backend.Services.Finance.TuitionPayments;

public interface ITuitionPaymentService
{
    Task<IReadOnlyList<StudentTuitionInvoiceDto>> GetStudentInvoicesAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentTuitionTransactionDto>> GetStudentTransactionsAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<CreateTuitionPaymentResponse> CreatePaymentAsync(
        int invoiceId,
        int currentUserId,
        string provider,
        CancellationToken cancellationToken = default);

    Task<CreateTuitionPaymentResponse> GetPaymentAsync(
        int transactionId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<PayOsWebhookResultDto> HandlePayOsWebhookAsync(
        string rawBody,
        CancellationToken cancellationToken = default);
}

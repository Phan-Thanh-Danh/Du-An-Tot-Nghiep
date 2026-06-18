using System.Text.Json;

namespace Backend.Services.Finance.TuitionPayments;

public interface IPayOsService
{
    Task<PayOsCreatePaymentLinkResult> CreatePaymentLinkAsync(
        PayOsCreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<PayOsPaymentLinkStatusResult> GetPaymentLinkAsync(
        long orderCode,
        CancellationToken cancellationToken = default);

    bool VerifyWebhookSignature(JsonElement data, string signature);
}

public class PayOsCreatePaymentLinkRequest
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ItemName { get; set; } = "Học phí";
}

public class PayOsCreatePaymentLinkResult
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? QrPayload { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? BankCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public long Amount { get; set; }
    public string? Description { get; set; }
    public string RequestPayloadJson { get; set; } = string.Empty;
    public string ResponsePayloadJson { get; set; } = string.Empty;
}

public class PayOsPaymentLinkStatusResult
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRemaining { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string ResponsePayloadJson { get; set; } = string.Empty;
}

public class PayOsProviderException : Exception
{
    public string? ResponsePayloadJson { get; }

    public PayOsProviderException(string message, string? responsePayloadJson = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ResponsePayloadJson = responsePayloadJson;
    }
}

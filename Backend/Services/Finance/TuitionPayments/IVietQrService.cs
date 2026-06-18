namespace Backend.Services.Finance.TuitionPayments;

public interface IVietQrService
{
    string CreateQrUrl(
        string bankCode,
        string accountNo,
        string accountName,
        decimal amount,
        string transferContent,
        string templateId);
}

using System.Globalization;
using Backend.Exceptions;

namespace Backend.Services.Finance.TuitionPayments;

public class VietQrService : IVietQrService
{
    public string CreateQrUrl(
        string bankCode,
        string accountNo,
        string accountName,
        decimal amount,
        string transferContent,
        string templateId)
    {
        if (string.IsNullOrWhiteSpace(bankCode)
            || string.IsNullOrWhiteSpace(accountNo)
            || string.IsNullOrWhiteSpace(accountName)
            || string.IsNullOrWhiteSpace(templateId))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thông tin tài khoản VietQR chưa đầy đủ.");
        }

        var normalizedAmount = ToVndAmount(amount);
        var encodedContent = Uri.EscapeDataString(transferContent);
        var encodedAccountName = Uri.EscapeDataString(accountName);

        return "https://api.vietqr.io/image/"
            + $"{Uri.EscapeDataString(bankCode)}-{Uri.EscapeDataString(accountNo)}-{Uri.EscapeDataString(templateId)}.jpg"
            + $"?amount={normalizedAmount.ToString(CultureInfo.InvariantCulture)}"
            + $"&addInfo={encodedContent}"
            + $"&accountName={encodedAccountName}";
    }

    private static long ToVndAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tiền thanh toán phải lớn hơn 0.");
        }

        if (amount != decimal.Truncate(amount))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tiền thanh toán VietQR phải là số VND nguyên.");
        }

        return decimal.ToInt64(amount);
    }
}

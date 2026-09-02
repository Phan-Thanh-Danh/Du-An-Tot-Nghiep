namespace Backend.Helpers;

public static class FinanceMaskHelper
{
    /// <summary>
    /// Mask số tài khoản ngân hàng dạng "****" + 4 số cuối.
    /// Ví dụ: "9704001234567890" -> "************7890".
    /// </summary>
    public static string MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return string.Empty;

        var trimmed = accountNumber.Trim();
        if (trimmed.Length <= 4)
            return new string('*', trimmed.Length);

        var last4 = trimmed[^4..];
        var prefix = new string('*', trimmed.Length - 4);
        return prefix + last4;
    }
}

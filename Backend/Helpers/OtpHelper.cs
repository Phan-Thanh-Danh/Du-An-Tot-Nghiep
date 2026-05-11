using System.Security.Cryptography;

namespace Backend.Helpers;

public static class OtpHelper
{
    public static string GenerateSixDigitOtp()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }
}

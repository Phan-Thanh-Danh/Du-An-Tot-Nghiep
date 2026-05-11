using System.Security.Cryptography;

namespace Backend.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            '.',
            "PBKDF2",
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static string? GetPasswordStrengthError(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Mật khẩu mới không được để trống.";
        }

        if (password.Length < 8)
        {
            return "Mật khẩu mới phải có tối thiểu 8 ký tự.";
        }

        if (password.Any(char.IsWhiteSpace))
        {
            return "Mật khẩu mới không được chứa khoảng trắng.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Mật khẩu mới phải có ít nhất 1 chữ hoa.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Mật khẩu mới phải có ít nhất 1 chữ thường.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Mật khẩu mới phải có ít nhất 1 chữ số.";
        }

        if (!password.Any(ch => char.IsPunctuation(ch) || char.IsSymbol(ch)))
        {
            return "Mật khẩu mới phải có ít nhất 1 ký tự đặc biệt.";
        }

        return null;
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('.', 4);
        if (parts.Length != 4 || parts[0] != "PBKDF2")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedHash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

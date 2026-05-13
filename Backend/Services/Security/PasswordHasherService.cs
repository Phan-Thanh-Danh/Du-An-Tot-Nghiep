using Backend.Helpers;

namespace Backend.Services.Security;

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string password)
    {
        return PasswordHelper.HashPassword(password);
    }

    public string? GetPasswordStrengthError(string password)
    {
        return PasswordHelper.GetPasswordStrengthError(password);
    }
}

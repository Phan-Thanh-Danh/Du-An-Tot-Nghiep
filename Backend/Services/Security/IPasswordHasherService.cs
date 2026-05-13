namespace Backend.Services.Security;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    string? GetPasswordStrengthError(string password);
}

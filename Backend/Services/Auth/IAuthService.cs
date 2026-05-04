using Backend.DTOs.Auth;

namespace Backend.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordDto request);
}

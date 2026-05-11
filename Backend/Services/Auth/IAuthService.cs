using Backend.DTOs.Auth;

namespace Backend.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(RevokeTokenRequestDto request);
    Task RevokeTokenAsync(RevokeTokenRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordDto request);
}

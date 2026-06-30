namespace Backend.DTOs.Auth;

public class LoginResponseDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Đăng nhập thành công";
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public AuthUserDto User { get; set; } = new();
}

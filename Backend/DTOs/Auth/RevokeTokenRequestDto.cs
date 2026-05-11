using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class RevokeTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token không được để trống.")]
    public string RefreshToken { get; set; } = string.Empty;
}

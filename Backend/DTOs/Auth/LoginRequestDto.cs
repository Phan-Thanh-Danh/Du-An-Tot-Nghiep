using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có tối thiểu 6 ký tự.")]
    public string Password { get; set; } = string.Empty;
}

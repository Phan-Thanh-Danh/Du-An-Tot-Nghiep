using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class LoginRequestDto
{
    private string _usernameOrEmail = string.Empty;

    [Required(ErrorMessage = "Tên đăng nhập hoặc email không được để trống.")]
    public string UsernameOrEmail
    {
        get => _usernameOrEmail;
        set => _usernameOrEmail = value ?? string.Empty;
    }

    public string Email
    {
        get => UsernameOrEmail;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                UsernameOrEmail = value;
            }
        }
    }

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có tối thiểu 6 ký tự.")]
    public string Password { get; set; } = string.Empty;
}

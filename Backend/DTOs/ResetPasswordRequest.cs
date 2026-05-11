using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP không được để trống.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải gồm đúng 6 chữ số.")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

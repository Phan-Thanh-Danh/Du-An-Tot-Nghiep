using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

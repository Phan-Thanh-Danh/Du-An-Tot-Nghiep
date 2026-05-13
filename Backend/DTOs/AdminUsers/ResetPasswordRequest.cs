using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AdminUsers;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;
}

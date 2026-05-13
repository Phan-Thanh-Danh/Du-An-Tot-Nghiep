using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Rbac;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "Mã vai trò không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã vai trò không được vượt quá 50 ký tự.")]
    public string MaCodeVaiTro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên vai trò không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên vai trò không được vượt quá 100 ký tự.")]
    public string TenVaiTro { get; set; } = string.Empty;
}

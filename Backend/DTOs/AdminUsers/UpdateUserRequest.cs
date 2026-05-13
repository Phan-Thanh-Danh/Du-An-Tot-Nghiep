using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AdminUsers;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [MaxLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự.")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
    public string? SoDienThoai { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vai trò không hợp lệ.")]
    public int MaVaiTro { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    public int? MaLopHanhChinh { get; set; }
}

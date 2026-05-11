using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class UpdateProfileRequest
{
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }

    public string? HoTen { get; set; }

    [RegularExpression(@"^$|^[0-9+\-\s]{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? SoDienThoai { get; set; }
}

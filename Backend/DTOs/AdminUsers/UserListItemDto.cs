namespace Backend.DTOs.AdminUsers;

public class UserListItemDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string TenDonVi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}

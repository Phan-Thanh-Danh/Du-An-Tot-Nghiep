namespace Backend.DTOs.AdminUsers;

public class UserDetailDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public int MaVaiTro { get; set; }
    public string MaCodeVaiTro { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int? MaLopHanhChinh { get; set; }
    public string? TenLopHanhChinh { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? LanDangNhapCuoi { get; set; }
}

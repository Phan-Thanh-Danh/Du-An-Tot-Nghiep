namespace Backend.DTOs.Rbac;

public class RoleMemberDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

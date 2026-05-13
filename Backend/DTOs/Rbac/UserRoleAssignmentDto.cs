namespace Backend.DTOs.Rbac;

public class UserRoleAssignmentDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public RoleDto VaiTroChinh { get; set; } = new();
    public IReadOnlyList<RoleDto> VaiTroDuocGan { get; set; } = [];
}

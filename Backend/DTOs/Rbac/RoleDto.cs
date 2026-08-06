namespace Backend.DTOs.Rbac;

public class RoleDto
{
    public int MaVaiTro { get; set; }
    public string MaCodeVaiTro { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;
    /// <summary>
    /// "System" nếu là vai trò hệ thống đã định nghĩa, "Custom" nếu do người dùng tạo.
    /// </summary>
    public string Type { get; set; } = "System";
    /// <summary>
    /// Số lượng người dùng được gán vai trò này.
    /// </summary>
    public int MemberCount { get; set; }
}

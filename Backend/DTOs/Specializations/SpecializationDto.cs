namespace Backend.DTOs.Specializations;

public class SpecializationDto
{
    public int MaChuyenNganh { get; set; }
    public int MaNganh { get; set; }
    public string MaCodeNganh { get; set; } = string.Empty;
    public string TenNganh { get; set; } = string.Empty;
    public string MaCodeChuyenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

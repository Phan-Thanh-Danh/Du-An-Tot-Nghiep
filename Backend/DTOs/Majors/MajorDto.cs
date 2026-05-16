namespace Backend.DTOs.Majors;

public class MajorDto
{
    public int MaNganh { get; set; }
    public string MaCodeNganh { get; set; } = string.Empty;
    public string TenNganh { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

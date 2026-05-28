namespace Backend.DTOs.Buildings;

public class BuildingDto
{
    public int MaToaNha { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string MaCodeToaNha { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public int? SoTang { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

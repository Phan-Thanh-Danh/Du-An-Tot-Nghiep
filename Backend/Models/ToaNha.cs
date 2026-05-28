namespace Backend.Models;

public class ToaNha
{
    public int MaToaNha { get; set; }
    public int MaDonVi { get; set; }
    public string MaCodeToaNha { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public int? SoTang { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DonVi? DonVi { get; set; }
}

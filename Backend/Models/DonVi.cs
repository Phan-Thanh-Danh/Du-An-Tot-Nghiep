namespace Backend.Models;

public class DonVi
{
    public int MaDonVi { get; set; }
    public int? MaDonViCha { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string CapDonVi { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }

    public DonVi? DonViCha { get; set; }
}

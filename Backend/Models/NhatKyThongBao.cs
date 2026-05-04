namespace Backend.Models;

public class NhatKyThongBao
{
    public int MaNkThongBao { get; set; }
    public int? MaThongBao { get; set; }
    public int MaNguoiNhan { get; set; }
    public int MaDonVi { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string KenhGui { get; set; } = string.Empty;
    public DateTime? GuiLuc { get; set; }

    public ThongBao? ThongBao { get; set; }
    public NguoiDung? NguoiNhan { get; set; }
    public DonVi? DonVi { get; set; }
}

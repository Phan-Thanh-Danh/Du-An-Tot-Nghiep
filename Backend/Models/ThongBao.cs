namespace Backend.Models;

public class ThongBao
{
    public int MaThongBao { get; set; }
    public int MaNguoiNhan { get; set; }
    public int MaDonVi { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string? TieuDe { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiNhan { get; set; }
    public DonVi? DonVi { get; set; }
}

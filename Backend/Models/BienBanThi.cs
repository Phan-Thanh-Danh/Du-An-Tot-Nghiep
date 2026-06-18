namespace Backend.Models;

public class BienBanThi
{
    public int MaBienBan { get; set; }
    public int MaCaThi { get; set; }
    public int? MaPhienThi { get; set; }
    public string LoaiBienBan { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public int MaNguoiLap { get; set; }
    public DateTime ThoiDiemLap { get; set; }
    public string TrangThaiXuLy { get; set; } = "cho_xu_ly";
    public DateTime NgayTao { get; set; }

    public CaThi? CaThi { get; set; }
    public PhienThiHocSinh? PhienThi { get; set; }
    public NguoiDung? NguoiLap { get; set; }
}

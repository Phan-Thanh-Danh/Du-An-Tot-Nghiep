namespace Backend.Models;

public class TinNhanHoTro
{
    public int MaTinNhanHt { get; set; }
    public int MaPhieuHt { get; set; }
    public int MaNguoiGui { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? UrlDinhKem { get; set; }
    public DateTime NgayTao { get; set; }

    public PhieuHoTro? PhieuHt { get; set; }
    public NguoiDung? NguoiGui { get; set; }
}

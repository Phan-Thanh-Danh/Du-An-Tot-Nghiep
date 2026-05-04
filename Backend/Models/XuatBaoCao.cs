namespace Backend.Models;

public class XuatBaoCao
{
    public int MaXuatBaoCao { get; set; }
    public int NguoiYeuCau { get; set; }
    public int MaDonVi { get; set; }
    public string LoaiBaoCao { get; set; } = string.Empty;
    public string? ThamSoJson { get; set; }
    public string? UrlTapTin { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiYeuCauNavigation { get; set; }
    public DonVi? DonVi { get; set; }
}

namespace Backend.Models;

public class NhatKyViPhamThi
{
    public int MaViPham { get; set; }
    public int? MaPhienThi { get; set; }
    public int MaHocSinh { get; set; }
    public int MaCaThi { get; set; }
    public string LoaiViPham { get; set; } = string.Empty;
    public string MucDo { get; set; } = "nhac_nho";
    public string? ChiTietJson { get; set; }
    public DateTime ThoiDiem { get; set; }
    public DateTime NgayTao { get; set; }

    public PhienThiHocSinh? PhienThi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public CaThi? CaThi { get; set; }
    public ICollection<XuLyViPhamThi> XuLyViPhams { get; set; } = new List<XuLyViPhamThi>();
}

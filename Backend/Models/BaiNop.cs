namespace Backend.Models;

public class BaiNop
{
    public int MaBaiNop { get; set; }
    public int MaBaiTap { get; set; }
    public int MaHocSinh { get; set; }
    public string UrlTapTin { get; set; } = string.Empty;
    public int SoLanNop { get; set; }
    public bool NopTre { get; set; }
    public decimal? DiemDaoVan { get; set; }
    public decimal? DiemSo { get; set; }
    public decimal? DiemAiDeXuat { get; set; }
    public string? NhanXet { get; set; }
    public DateTime ThoiDiemNop { get; set; }
    public bool DaCongBo { get; set; }

    public BaiTap? BaiTap { get; set; }
    public NguoiDung? HocSinh { get; set; }
}

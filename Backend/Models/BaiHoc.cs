namespace Backend.Models;

public class BaiHoc
{
    public int MaBaiHoc { get; set; }
    public int MaChuong { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string LoaiBaiHoc { get; set; } = string.Empty;
    public string? UrlTapTin { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public string? NoiDungVanBan { get; set; }
    public string? DieuKienMoKhoa { get; set; }
    public string? TomTatAi { get; set; }
    public int ThuTu { get; set; }
    public bool DaAn { get; set; }

    public Chuong? Chuong { get; set; }
}

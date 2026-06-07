namespace Backend.Models;

public class BaiHocNoiDung
{
    public int MaNoiDung { get; set; }
    public int MaBaiHoc { get; set; }
    public string LoaiNoiDung { get; set; } = string.Empty; // video, slide_html, tai_lieu, quiz, van_ban
    public string? NoiDungHtml { get; set; }
    public string? NoiDungJson { get; set; }
    public string? UrlTapTin { get; set; }
    public string? StorageKey { get; set; }
    public long? KichThuocByte { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public string? TrangThai { get; set; } // nhap, da_xuat_ban
    public int ThuTu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public BaiHoc? BaiHoc { get; set; }
}

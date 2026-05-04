namespace Backend.Models;

public class Chuong
{
    public int MaChuong { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThuTu { get; set; }
    public bool DaAn { get; set; }

    public KhoaHoc? KhoaHoc { get; set; }
}

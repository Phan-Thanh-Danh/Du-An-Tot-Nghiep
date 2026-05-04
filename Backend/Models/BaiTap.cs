namespace Backend.Models;

public class BaiTap
{
    public int MaBaiTap { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public DateTime HanNop { get; set; }
    public int SoLanNopToiDa { get; set; }
    public string DinhDangChoPhep { get; set; } = string.Empty;
    public string? HuongDanChamDiem { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public KhoaHoc? KhoaHoc { get; set; }
}

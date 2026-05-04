namespace Backend.Models;

public class YeuCauSuaDiem
{
    public int MaYcSuaDiem { get; set; }
    public int MaDiemSo { get; set; }
    public int NguoiYeuCau { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public string? UrlBangChung { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime? MoDenLuc { get; set; }
    public string LoaiYeuCau { get; set; } = string.Empty;
    public DateTime? UnlockExpiresAt { get; set; }
    public string? CotDiemDuocMo { get; set; }

    public DiemSo? DiemSo { get; set; }
    public NguoiDung? NguoiYeuCauNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

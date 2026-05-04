namespace Backend.Models;

public class MauThongBao
{
    public int MaMauTb { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string KenhGui { get; set; } = string.Empty;
    public string? MauTieuDe { get; set; }
    public string MauNoiDung { get; set; } = string.Empty;
}

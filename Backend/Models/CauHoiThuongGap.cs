namespace Backend.Models;

public class CauHoiThuongGap
{
    public int MaCauHoiFaq { get; set; }
    public string DanhMuc { get; set; } = string.Empty;
    public string CauHoi { get; set; } = string.Empty;
    public string TraLoi { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
}

namespace Backend.Models;

public class CauHoi
{
    public int MaCauHoi { get; set; }
    public int? MaMonHoc { get; set; }
    public int? NguoiTao { get; set; }
    public string LoaiCauHoi { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string? LuaChon { get; set; }
    public string? DapAnDung { get; set; }
    public string DoKho { get; set; } = string.Empty;

    public DanhMucMonHoc? MonHoc { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
}

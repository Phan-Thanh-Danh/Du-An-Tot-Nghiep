namespace Backend.Models;

public class MauBangKhen
{
    public int MaMauBangKhen { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public string FileNenUrl { get; set; } = string.Empty;
    public int ChieuRong { get; set; }
    public int ChieuCao { get; set; }
    public string HuongGiay { get; set; } = string.Empty;
    public string? CauHinhJson { get; set; }
    public bool ConHoatDong { get; set; }
    public int NguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public NguoiDung? NguoiTaoNavigation { get; set; }
}

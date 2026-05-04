namespace Backend.Models;

public class YeuCauMoKhoaDiemDanh
{
    public int MaYcMoKhoa { get; set; }
    public int MaBuoiHoc { get; set; }
    public int NguoiYeuCau { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime? MoKhoaDenLuc { get; set; }
    public DateTime NgayTao { get; set; }

    public BuoiHoc? BuoiHoc { get; set; }
    public NguoiDung? NguoiYeuCauNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

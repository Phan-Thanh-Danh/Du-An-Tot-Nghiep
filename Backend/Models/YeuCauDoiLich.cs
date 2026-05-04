namespace Backend.Models;

public class YeuCauDoiLich
{
    public int MaYcDoiLich { get; set; }
    public int MaTkb { get; set; }
    public int GiaoVienDeXuat { get; set; }
    public int GiaoVienNhanDoi { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime? GvNhanPhanHoiLuc { get; set; }
    public DateTime? AdminDuyetLuc { get; set; }
    public DateTime NgayTao { get; set; }

    public ThoiKhoaBieu? Tkb { get; set; }
    public NguoiDung? GiaoVienDeXuatNavigation { get; set; }
    public NguoiDung? GiaoVienNhanDoiNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

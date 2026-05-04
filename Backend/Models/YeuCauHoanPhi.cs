namespace Backend.Models;

public class YeuCauHoanPhi
{
    public int MaHoanPhi { get; set; }
    public int MaHoaDon { get; set; }
    public int MaHocSinh { get; set; }
    public int MaDonVi { get; set; }
    public decimal SoTienYeuCau { get; set; }
    public string LoaiHoanPhi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime? XuLyLuc { get; set; }

    public HoaDon? HoaDon { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

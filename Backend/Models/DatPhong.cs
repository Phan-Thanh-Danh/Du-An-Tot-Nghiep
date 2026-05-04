namespace Backend.Models;

public class DatPhong
{
    public int MaDatPhong { get; set; }
    public int MaPhong { get; set; }
    public int MaDonVi { get; set; }
    public int NguoiYeuCau { get; set; }
    public string MucDich { get; set; } = string.Empty;
    public DateTime BatDauLuc { get; set; }
    public DateTime KetThucLuc { get; set; }
    public int? SoNguoiThamDu { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime NgayTao { get; set; }

    public PhongHoc? Phong { get; set; }
    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiYeuCauNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

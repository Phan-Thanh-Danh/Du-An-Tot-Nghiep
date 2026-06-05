namespace Backend.Models;

public class HoaDon
{
    public int MaHoaDon { get; set; }

    public int MaDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaHocKy { get; set; }

    public string MaHoaDonCode { get; set; } = string.Empty;
    public string LoaiHoaDon { get; set; } = "hoc_phi";

    public decimal SoTien { get; set; }
    public decimal GiamTru { get; set; }
    public decimal DaThanhToan { get; set; }

    public string TrangThai { get; set; } = "chua_thanh_toan";

    public DateOnly HanThanhToan { get; set; }

    public string? UrlHoaDonPdf { get; set; }
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public int? NguoiTao { get; set; }
    public int? NguoiCapNhat { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public HocKy? HocKy { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiCapNhatNavigation { get; set; }
}

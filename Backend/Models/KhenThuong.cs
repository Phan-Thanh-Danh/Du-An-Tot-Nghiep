namespace Backend.Models;

public class KhenThuong
{
    public int MaKhenThuong { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public int MaHocKy { get; set; }
    public int? MaDotKhenThuong { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string LoaiKhenThuong { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public decimal? GpaDatDuoc { get; set; }
    public decimal? DiemXet { get; set; }
    public int? XepHang { get; set; }
    public string UrlChungTu { get; set; } = string.Empty;
    public string? UrlPdfBangKhen { get; set; }
    public DateTime? NgaySinhPdf { get; set; }
    public string? LoiSinhPdf { get; set; }
    public int SoLanSinhPdf { get; set; }
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    public string? TenHocKySnapshot { get; set; }
    public string? DanhHieuSnapshot { get; set; }
    public DateTime CapLuc { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiCap { get; set; }
    public int? NguoiDuyet { get; set; }
    public bool DaHuy { get; set; }
    public string? LyDoHuy { get; set; }
    public int? NguoiHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public string? GhiChuHuy { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public HocKy? HocKy { get; set; }
    public DotKhenThuong? DotKhenThuong { get; set; }
    public MauBangKhen? MauBangKhen { get; set; }
    public NguoiDung? NguoiCapNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
    public NguoiDung? NguoiHuyNavigation { get; set; }
}

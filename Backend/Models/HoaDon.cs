namespace Backend.Models;

public class HoaDon
{
    public int MaHoaDon { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaHocKy { get; set; }
    public decimal SoTien { get; set; }
    public decimal? GiamTru { get; set; }
    public decimal DaThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? PhuongThucTt { get; set; }
    public string? MaGiaoDichCong { get; set; }
    public DateTime? BatDauTtLuc { get; set; }
    public DateTime? HetHanTtLuc { get; set; }
    public DateOnly HanThanhToan { get; set; }
    public string? UrlHoaDonPdf { get; set; }
    public DateTime? ThoiDiemKhoiTaoTt { get; set; }
    public DateTime? HetHanTt { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public HocKy? HocKy { get; set; }
}

namespace Backend.Models;

public class KhenThuong
{
    public int MaKhenThuong { get; set; }
    public int MaHocSinh { get; set; }
    public int MaHocKy { get; set; }
    public string LoaiKhenThuong { get; set; } = string.Empty;
    public decimal? GpaDatDuoc { get; set; }
    public string UrlChungTu { get; set; } = string.Empty;
    public DateTime CapLuc { get; set; }
    public bool DaHuy { get; set; }
    public string? GhiChuHuy { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public HocKy? HocKy { get; set; }
}

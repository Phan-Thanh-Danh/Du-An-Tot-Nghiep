namespace Backend.Models;

public class UngVienKhenThuong
{
    public int MaUngVienKhenThuong { get; set; }
    public int MaDotKhenThuong { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    
    public int? XepHang { get; set; }
    public decimal DiemXet { get; set; }
    public decimal? GpaHocKy { get; set; }
    public int? TongTinChi { get; set; }
    
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDoLoai { get; set; }
    public string? LyDoLoaiJson { get; set; }
    
    public string? TieuChiSnapshotJson { get; set; }
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    public string? TenHocKySnapshot { get; set; }
    public string? GhiChuDieuChinh { get; set; }
    public int? NguoiDieuChinh { get; set; }
    public DateTime? NgayDieuChinh { get; set; }
    
    public int? NguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DotKhenThuong? DotKhenThuong { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public DonVi? DonVi { get; set; }
    public HocKy? HocKy { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiDieuChinhNavigation { get; set; }
}

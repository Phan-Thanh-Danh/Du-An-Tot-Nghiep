namespace Backend.Models;

public class DotKhenThuong
{
    public int MaDotKhenThuong { get; set; }
    public int MaHocKy { get; set; }
    public int? MaDonVi { get; set; }
    public string TenDot { get; set; } = string.Empty;
    public string LoaiDot { get; set; } = string.Empty;
    public int SoLuongToiDa { get; set; }
    public string? TieuChiXetJson { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int NguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public int? NguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime? NgayCongBo { get; set; }
    public string? GhiChu { get; set; }

    public HocKy? HocKy { get; set; }
    public DonVi? DonVi { get; set; }
    public MauBangKhen? MauBangKhen { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}

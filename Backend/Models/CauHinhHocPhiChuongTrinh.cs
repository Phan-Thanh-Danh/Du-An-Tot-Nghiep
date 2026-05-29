namespace Backend.Models;

public class CauHinhHocPhiChuongTrinh
{
    public int MaCauHinhHocPhi { get; set; }
    public int MaDonVi { get; set; }
    public int MaChuongTrinhDaoTao { get; set; }
    public int MaHocKy { get; set; }
    public int NamHocTrongChuongTrinh { get; set; }
    public int HocKyTrongNam { get; set; }
    public int SoThuTuHocKy { get; set; }
    public string LoaiCachTinhHocPhi { get; set; } = string.Empty;
    public decimal SoTienHocPhi { get; set; }
    public decimal TienHocLieu { get; set; }
    public decimal TongTienDuKien { get; set; }
    public bool ConHoatDong { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DonVi? DonVi { get; set; }
    public ChuongTrinhDaoTao? ChuongTrinhDaoTao { get; set; }
    public HocKy? HocKy { get; set; }
}

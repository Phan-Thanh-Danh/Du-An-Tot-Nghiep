namespace Backend.Models;

public class MonHocTrongChuongTrinh
{
    public int MaChuongTrinhMonHoc { get; set; }
    public int MaChuongTrinh { get; set; }
    public int MaMonHoc { get; set; }
    public int HocKyDuKien { get; set; }
    public int SoTinChi { get; set; }
    public string LoaiMonHoc { get; set; } = string.Empty;
    public bool BatBuoc { get; set; }
    public int ThuTu { get; set; }
    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public ChuongTrinhDaoTao? ChuongTrinhDaoTao { get; set; }
    public DanhMucMonHoc? DanhMucMonHoc { get; set; }
}

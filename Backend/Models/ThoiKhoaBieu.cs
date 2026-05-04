namespace Backend.Models;

public class ThoiKhoaBieu
{
    public int MaTkb { get; set; }
    public int MaDonVi { get; set; }
    public int MaGiaoVien { get; set; }
    public int? MaGiaoVienDayThay { get; set; }
    public int MaMonHoc { get; set; }
    public int MaPhong { get; set; }
    public int MaLop { get; set; }
    public int? MaLopHocPhan { get; set; }
    public int ThuTrongTuan { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? DuongDanHop { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? BuChoBuoi { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public NguoiDung? GiaoVienDayThay { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public PhongHoc? Phong { get; set; }
    public LopHanhChinh? Lop { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }
    public ThoiKhoaBieu? BuChoBuoiNavigation { get; set; }
}

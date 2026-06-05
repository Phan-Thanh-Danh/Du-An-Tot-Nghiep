namespace Backend.Models;

public class BuoiHoc
{
    public int MaBuoiHoc { get; set; }
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public int MaPhong { get; set; }
    public int MaGiaoVien { get; set; }
    public int? MaGiaoVienDayThay { get; set; }
    public string TrangThaiBuoi { get; set; } = string.Empty;
    public string? LoaiThayDoi { get; set; }
    public string? LyDoThayDoi { get; set; }
    public string? GhiChu { get; set; }
    public DateTime? KhoaLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public ThoiKhoaBieu? Tkb { get; set; }
    public KhoaHoc? KhoaHoc { get; set; }
    public CaHoc? CaHoc { get; set; }
    public PhongHoc? Phong { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public NguoiDung? GiaoVienDayThay { get; set; }
}

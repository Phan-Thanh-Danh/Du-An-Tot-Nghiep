namespace Backend.Models;

public class KyThi
{
    public int MaKyThi { get; set; }
    public string TenKyThi { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string LoaiKyThi { get; set; } = "cuoi_ky"; // giua_ky, cuoi_ky
    public string TrangThai { get; set; } = "nhap";
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public HocKy? HocKy { get; set; }
    public ICollection<LichThiTong> LichThiTongs { get; set; } = new List<LichThiTong>();
}

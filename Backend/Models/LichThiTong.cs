namespace Backend.Models;

public class LichThiTong
{
    public int MaLichThiTong { get; set; }
    public int MaKyThi { get; set; }
    public int MaMonHoc { get; set; }
    public int? MaDeKiemTra { get; set; }
    public string HinhThucThi { get; set; } = string.Empty;
    public DateTime NgayThiDuKien { get; set; }
    public string TrangThai { get; set; } = "nhap";
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public KyThi? KyThi { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public DeKiemTra? DeKiemTra { get; set; }
    public ICollection<CaThi> CaThis { get; set; } = new List<CaThi>();
}

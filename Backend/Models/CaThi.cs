namespace Backend.Models;

public class CaThi
{
    public int MaCaThi { get; set; }
    public int MaLichThiTong { get; set; }
    public string TenCaThi { get; set; } = string.Empty;
    public int? MaPhong { get; set; }
    public DateTime NgayThi { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public int MaDonVi { get; set; }
    public string TrangThai { get; set; } = "nhap";
    public string? GhiChu { get; set; }
    public string? LyDoDieuChinh { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public LichThiTong? LichThiTong { get; set; }
    public PhongHoc? Phong { get; set; }
    public DonVi? DonVi { get; set; }
    public ICollection<PhanCongGiamThi> PhanCongGiamThis { get; set; } = new List<PhanCongGiamThi>();
    public ICollection<ThiSinhCaThi> ThiSinhCaThis { get; set; } = new List<ThiSinhCaThi>();
    public ICollection<DiemDanhThi> DiemDanhThis { get; set; } = new List<DiemDanhThi>();
}

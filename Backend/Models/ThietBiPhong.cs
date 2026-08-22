namespace Backend.Models;

public class ThietBiPhong
{
    public int MaThietBi { get; set; }
    public int MaPhong { get; set; }
    public string TenThietBi { get; set; } = string.Empty;
    public string? MaCodeThietBi { get; set; }
    public string? ChungLoai { get; set; }
    public int SoLuong { get; set; }
    public string? TinhTrang { get; set; }
    public DateTime? NgayKiemDinh { get; set; }
    public string? GhiChu { get; set; }

    public PhongHoc? Phong { get; set; }
}

namespace Backend.Models;

public class ThietBiPhong
{
    public int MaThietBi { get; set; }
    public int MaPhong { get; set; }
    public string TenThietBi { get; set; } = string.Empty;
    public int SoLuong { get; set; }

    public PhongHoc? Phong { get; set; }
}

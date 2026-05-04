namespace Backend.Models;

public class PhongHoc
{
    public int MaPhong { get; set; }
    public int MaDonVi { get; set; }
    public string MaCodePhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public string LoaiPhong { get; set; } = string.Empty;
    public string TrangThaiPhong { get; set; } = string.Empty;

    public DonVi? DonVi { get; set; }
}

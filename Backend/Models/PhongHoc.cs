namespace Backend.Models;

public class PhongHoc
{
    public int MaPhong { get; set; }
    public int MaDonVi { get; set; }
    public int? MaToaNha { get; set; }
    public int? MaTang { get; set; }
    public string MaCodePhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public string LoaiPhong { get; set; } = string.Empty;
    public string TrangThaiPhong { get; set; } = string.Empty;
    public string? GhiChu { get; set; }

    public DonVi? DonVi { get; set; }
    public ToaNha? ToaNha { get; set; }
    public Tang? Tang { get; set; }
}

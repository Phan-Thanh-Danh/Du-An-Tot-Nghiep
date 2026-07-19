namespace Backend.Models;

public class LoaiDauDiemQuaTrinh
{
    public int MaLoaiDauDiem { get; set; }
    public string MaCode { get; set; } = string.Empty;
    public string TenLoai { get; set; } = string.Empty;
    public int ThuTuHienThi { get; set; }
}

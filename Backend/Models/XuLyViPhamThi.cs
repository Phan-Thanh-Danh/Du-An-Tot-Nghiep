namespace Backend.Models;

public class XuLyViPhamThi
{
    public int MaXuLy { get; set; }
    public int MaViPham { get; set; }
    public string HanhDongXuLy { get; set; } = string.Empty;
    public int LanNhacNho { get; set; }
    public int MaNguoiXuLy { get; set; }
    public DateTime ThoiDiem { get; set; }
    public string? LyDo { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }

    public NhatKyViPhamThi? ViPham { get; set; }
    public NguoiDung? NguoiXuLy { get; set; }
}

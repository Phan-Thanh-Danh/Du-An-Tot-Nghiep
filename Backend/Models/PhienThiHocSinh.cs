namespace Backend.Models;

public class PhienThiHocSinh
{
    public int MaPhienThi { get; set; }
    public int MaDeKiemTra { get; set; }
    public int MaHocSinh { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? NopLuc { get; set; }
    public string? CauTraLoiJson { get; set; }
    public string? NhatKyViPham { get; set; }
    public string? SaoLuuCucBo { get; set; }
    public string TrangThaiLuong { get; set; } = string.Empty;
    public decimal? DiemTuDong { get; set; }
    public decimal? DiemCuoiCung { get; set; }
    public decimal? DiemTuLuanAiGoiY { get; set; }

    public DeKiemTra? DeKiemTra { get; set; }
    public NguoiDung? HocSinh { get; set; }
}

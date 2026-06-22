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
    public int LanThu { get; set; } = 1;
    public DateTime? HanNopLuc { get; set; }
    public int? SoCauDung { get; set; }
    public bool? KetQuaDat { get; set; }
    public string? DeThiSnapshotJson { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    // Exam module extensions
    public int? MaCaThi { get; set; }
    public string? TrangThaiKyTen { get; set; }
    public DateTime? ThoiDiemKy { get; set; }
    public int? NguoiXacNhanKyTen { get; set; }
    public string? TrangThaiCongBo { get; set; }

    public DeKiemTra? DeKiemTra { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public CaThi? CaThi { get; set; }
    public NguoiDung? NguoiXacNhan { get; set; }
}

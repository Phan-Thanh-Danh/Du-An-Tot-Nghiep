namespace Backend.Models;

public class NguoiDung
{
    public int MaNguoiDung { get; set; }
    public int MaDonVi { get; set; }
    public string Email { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string VaiTroChinh { get; set; } = string.Empty;
    public int? MaLop { get; set; }
    public string? SoDienThoai { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? NamNhapHoc { get; set; }
    public string? MatKhauHash { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? LanDangNhapCuoi { get; set; }
    public int SoLanSaiMatKhau { get; set; }
    public bool DangNhapLanDau { get; set; }

    public DonVi? DonVi { get; set; }
    public LopHanhChinh? Lop { get; set; }
}

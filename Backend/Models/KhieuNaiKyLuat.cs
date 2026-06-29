namespace Backend.Models;

public class KhieuNaiKyLuat
{
    public int MaKhieuNaiKyLuat { get; set; }
    public int MaHoSoKyLuat { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaDonVi { get; set; }
    public string LyDoKhieuNai { get; set; } = string.Empty;
    public string? ChungTuJson { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDoXuLy { get; set; }
    public string? GhiChuXuLy { get; set; }
    public int? NguoiXuLy { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public HoSoKyLuat? HoSoKyLuat { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? NguoiXuLyNavigation { get; set; }
    public DonVi? DonVi { get; set; }
}

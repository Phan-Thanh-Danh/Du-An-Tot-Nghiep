namespace Backend.Models;

public class DonTu
{
    public int MaDonTu { get; set; }
    public int MaHocSinh { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyetHienTai { get; set; }
    public string? DuLieuBieuMau { get; set; }
    public string? UrlBangChung { get; set; }
    public string? LyDoTuChoi { get; set; }
    public string? NhatKyTuDong { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? NguoiDuyetHienTaiNavigation { get; set; }
}

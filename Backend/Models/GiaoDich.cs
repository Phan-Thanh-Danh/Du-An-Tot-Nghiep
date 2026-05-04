namespace Backend.Models;

public class GiaoDich
{
    public int MaGiaoDich { get; set; }
    public int MaHoaDon { get; set; }
    public string? MaThamChieuCong { get; set; }
    public decimal SoTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? DuLieuPhanHoi { get; set; }
    public DateTime NgayTao { get; set; }
    public int? MaNguoiThucHien { get; set; }

    public HoaDon? HoaDon { get; set; }
    public NguoiDung? NguoiThucHien { get; set; }
}

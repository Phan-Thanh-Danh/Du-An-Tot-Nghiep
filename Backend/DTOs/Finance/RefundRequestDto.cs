namespace Backend.DTOs.Finance;

public class RefundRequestDto
{
    public int MaHoanPhi { get; set; }
    public int MaHoaDon { get; set; }
    public string SoHoaDon { get; set; } = string.Empty;
    public int MaHocSinh { get; set; }
    public string HoTenHocSinh { get; set; } = string.Empty;
    public decimal SoTienYeuCau { get; set; }
    public string LoaiHoanPhi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty; // dang_xu_ly, da_duyet, tu_choi
    public string? LyDoYeuCau { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? NguoiDuyetTen { get; set; }
}

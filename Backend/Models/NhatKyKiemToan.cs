namespace Backend.Models;

public class NhatKyKiemToan
{
    public int MaKiemToan { get; set; }
    public int? MaDonVi { get; set; }
    public string LoaiDoiTuong { get; set; } = string.Empty;
    public string MaDoiTuong { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string? GiaTriCu { get; set; }
    public string? GiaTriMoi { get; set; }
    public int? NguoiThayDoi { get; set; }
    public DateTime ThoiDiemThayDoi { get; set; }
    public string? DiaChiIp { get; set; }
    public string? UserAgent { get; set; }
    public string? MoTa { get; set; }
    public string? TraceId { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiThayDoiNavigation { get; set; }
}

namespace Backend.DTOs.Finance;

public class PaymentAccountDto
{
    public int MaTaiKhoanNhanTien { get; set; }
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string TenNganHang { get; set; } = string.Empty;
    public string MaNganHang { get; set; } = string.Empty;
    public string SoTaiKhoanMasked { get; set; } = string.Empty;
    public string TenChuTaiKhoan { get; set; } = string.Empty;
    public string? ChiNhanh { get; set; }
    public string NhaCungCapThanhToan { get; set; } = string.Empty;
    public string TrangThaiDuyet { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayDuyet { get; set; }
}

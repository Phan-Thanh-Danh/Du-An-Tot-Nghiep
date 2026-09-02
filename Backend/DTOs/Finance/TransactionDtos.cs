namespace Backend.DTOs.Finance;

public class TransactionListItemDto
{
    public int MaGiaoDich { get; set; }
    public int MaHoaDon { get; set; }
    public string? MaHoaDonCode { get; set; }
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public int? MaHocSinh { get; set; }
    public string? HoTenHocSinh { get; set; }
    public string? MaThamChieuNoiBo { get; set; }
    public string? MaThamChieuCong { get; set; }
    public decimal SoTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? NhaCungCapThanhToan { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public string? SoTaiKhoanMasked { get; set; }
    public string? TenNganHang { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string? ChuThich { get; set; }
}

public class TransactionQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? TrangThai { get; set; }
    public string? LoaiGiaoDich { get; set; }
    public int? MaHoaDon { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MaDonVi { get; set; }
}

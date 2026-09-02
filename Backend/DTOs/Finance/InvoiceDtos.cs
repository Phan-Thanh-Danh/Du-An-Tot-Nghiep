namespace Backend.DTOs.Finance;

public class InvoiceListItemDto
{
    public int MaHoaDon { get; set; }
    public string MaHoaDonCode { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public string? HoTenHocSinh { get; set; }
    public string? EmailHocSinh { get; set; }
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public string LoaiHoaDon { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public decimal GiamTru { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConLai => Math.Max(0, SoTien - GiamTru - DaThanhToan);
    public string TrangThai { get; set; } = string.Empty;
    public DateOnly HanThanhToan { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class InvoiceDetailDto : InvoiceListItemDto
{
    public string? UrlHoaDonPdf { get; set; }
    public string? GhiChu { get; set; }
    public string? LyDoHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public int? NguoiTao { get; set; }
    public string? TenNguoiTao { get; set; }
    public List<TransactionSummaryDto> Transactions { get; set; } = new();
}

public class TransactionSummaryDto
{
    public int MaGiaoDich { get; set; }
    public string? MaThamChieuNoiBo { get; set; }
    public string? MaThamChieuCong { get; set; }
    public decimal SoTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? NhaCungCapThanhToan { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string? TenNguoiThucHien { get; set; }
}

public class InvoiceQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? TrangThai { get; set; }
    public string? LoaiHoaDon { get; set; }
    public int? MaHocKy { get; set; }
    public int? MaHocSinh { get; set; }
    public string? Keyword { get; set; }
    public int? MaDonVi { get; set; }
}

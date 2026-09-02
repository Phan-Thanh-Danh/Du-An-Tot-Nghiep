namespace Backend.DTOs.Finance;

public class HoaDonDto
{
    public int MaHoaDon { get; set; }
    public string SoHoaDon { get; set; } = string.Empty;
    public int MaHocSinh { get; set; }
    public string HoTenHocSinh { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public decimal DaThu { get; set; }
    public decimal ConNo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}

public class HoaDonDetailDto : HoaDonDto
{
    public List<HoaDonChiTietDto> ChiTiets { get; set; } = new();
    public List<GiaoDichDto> GiaoDiches { get; set; } = new();
}

public class HoaDonChiTietDto
{
    public int MaChiTiet { get; set; }
    public string TenKhoanHoc { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}

public class GiaoDichDto
{
    public int MaGiaoDich { get; set; }
    public decimal SoTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayGiaoDich { get; set; }
    public string MaTaiKhoan { get; set; } = string.Empty; // Masked: ****1234
    public string GhiChu { get; set; } = string.Empty;
}

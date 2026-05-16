namespace Backend.DTOs.CampusSpecializations;

public class CampusSpecializationDto
{
    public int MaChuyenNganhCoSo { get; set; }
    public int MaChuyenNganh { get; set; }
    public string MaCodeChuyenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public int MaNganh { get; set; }
    public string TenNganh { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NamBatDau { get; set; }
    public int? ChiTieuDuKien { get; set; }
    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; }
}

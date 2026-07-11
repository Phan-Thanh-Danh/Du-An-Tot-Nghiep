namespace Backend.DTOs.Courses;

public class KhoaHocDto
{
    public int MaKhoaHoc { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public string? MaMonHocCode { get; set; }
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? UrlAnhBia { get; set; }
    public DateTime NgayTao { get; set; }
    
    // Gợi ý thông số từ QuyDoiTinChi
    public int? SoBlockHoc { get; set; }
    public int? GoiYSoBuoiMoiTuan { get; set; }
    public int? GoiYSoCaMoiBuoi { get; set; }
}

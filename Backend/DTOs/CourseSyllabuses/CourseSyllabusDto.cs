namespace Backend.DTOs.CourseSyllabuses;

public class CourseSyllabusDto
{
    public int MaSyllabus { get; set; }
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public int MaChuyenNganh { get; set; }
    public string MaCodeChuyenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public int MaNganh { get; set; }
    public string TenNganh { get; set; } = string.Empty;
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string TenSyllabus { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int? HocKyDuKien { get; set; }
    public bool BatBuoc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

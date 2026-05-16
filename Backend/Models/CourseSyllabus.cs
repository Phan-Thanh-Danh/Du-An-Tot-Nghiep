namespace Backend.Models;

public class CourseSyllabus
{
    public int MaSyllabus { get; set; }
    public int MaMonHoc { get; set; }
    public int MaChuyenNganh { get; set; }
    public int? MaDonVi { get; set; }
    public string TenSyllabus { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int? HocKyDuKien { get; set; }
    public bool BatBuoc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public ChuyenNganh? ChuyenNganh { get; set; }
    public DonVi? DonVi { get; set; }
}

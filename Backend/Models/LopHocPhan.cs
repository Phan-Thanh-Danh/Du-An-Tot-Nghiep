namespace Backend.Models;

public class LopHocPhan
{
    public int MaLopHocPhan { get; set; }
    public int MaDonVi { get; set; }
    public int MaMonHoc { get; set; }
    // TODO: Add MaSyllabus to LopHocPhan in next migration so each class section can bind to a concrete CourseSyllabus version.
    public int MaHocKy { get; set; }
    public string MaCodeLopHocPhan { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public int? SoDangKyToiThieu { get; set; }
    public int SoDaDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? QuotaVangToiDa { get; set; }

    public DonVi? DonVi { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
}

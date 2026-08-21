namespace Backend.DTOs.TeacherPersonnel;

public class TeacherWorkloadSummaryDto
{
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public int TongSoLopHocPhan { get; set; }
    public int TongSoCaDayTrongTuan { get; set; }
    public int TongSoGioGiangDayQuyDoi { get; set; }
    public int TongSoBuoiDaDienRa { get; set; }
    public int TongSoBuoiChuaDienRa { get; set; }
    public int TongSoBuoiBiHuy { get; set; }
    public int TongSoBuoiDayThay { get; set; }
    public List<TeacherCourseWorkloadItemDto> DanhSachLop { get; set; } = [];
}

public class TeacherCourseWorkloadItemDto
{
    public int MaKhoaHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenLopHanhChinh { get; set; } = string.Empty;
    public int SoLuongSinhVien { get; set; }
    public int SoCaMoiTuan { get; set; }
    public int TongSoBuoi { get; set; }
    public int SoBuoiHoanThanh { get; set; }
}

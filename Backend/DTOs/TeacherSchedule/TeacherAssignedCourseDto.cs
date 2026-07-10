namespace Backend.DTOs.TeacherSchedule;

public class TeacherAssignedCourseDto
{
    public int MaKhoaHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public int SessionsPerWeek { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
}

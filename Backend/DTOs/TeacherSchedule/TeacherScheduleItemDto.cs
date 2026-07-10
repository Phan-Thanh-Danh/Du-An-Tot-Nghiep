namespace Backend.DTOs.TeacherSchedule;

public class TeacherScheduleItemDto
{
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public bool IsSubstitute { get; set; }
    public string TeacherRoleLabel { get; set; } = string.Empty;
    public string TrangThaiBuoi { get; set; } = string.Empty;
    public string TrangThaiDiemDanh { get; set; } = string.Empty;
}

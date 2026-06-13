namespace Backend.DTOs.BuoiHoc;

public class SessionConflictDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TenKhoaHoc { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int? MaGiaoVienDayThay { get; set; }
    public string? TenGiaoVienDayThay { get; set; }
    public int EffectiveTeacherId { get; set; }
    public string EffectiveTeacherName { get; set; } = string.Empty;
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
}

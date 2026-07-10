namespace Backend.DTOs.StudentSchedule;

public class StudentScheduleItemDto
{
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string? TieuDeKhoaHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int? MaLop { get; set; }
    public string? TenLop { get; set; }
    public int? MaMonHoc { get; set; }
    public string? MaCodeMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public DateOnly NgayHoc { get; set; }
    public int? MaCaHoc { get; set; }
    public string? TenCa { get; set; }
    public TimeOnly? GioBatDau { get; set; }
    public TimeOnly? GioKetThuc { get; set; }
    public int? MaPhong { get; set; }
    public string? MaCodePhong { get; set; }
    public string? TenPhong { get; set; }
    public int? MaGiaoVien { get; set; }
    public string? TenGiaoVien { get; set; }
    public int? MaGiaoVienDayThay { get; set; }
    public string? TenGiaoVienDayThay { get; set; }
    public bool IsSubstitute { get; set; }
    public string? TrangThaiBuoi { get; set; }
    public string? TrangThaiDiemDanh { get; set; }
    public string? ChangeType { get; set; }
    public string? ChangeMessage { get; set; }
}

namespace Backend.DTOs.StudentAttendance;

public class StudentAttendanceItemDto
{
    public int MaDiemDanh { get; set; }
    public int MaBuoiHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public string TenCa { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string NgayHoc { get; set; } = string.Empty;
    public DateTime? GhiNhanLuc { get; set; }
}

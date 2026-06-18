namespace Backend.DTOs.Attendance;

public class StudentAttendanceDto
{
    public int MaDiemDanh { get; set; }
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int HeSoVang { get; set; }
    public DateTime GhiNhanLuc { get; set; }
}

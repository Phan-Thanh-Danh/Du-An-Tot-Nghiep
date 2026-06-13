namespace Backend.DTOs.Attendance;

public class AttendanceStudentDto
{
    public int MaDiemDanh { get; set; }
    public int MaBuoiHoc { get; set; }
    public int MaHocSinh { get; set; }
    public string HoTenHocSinh { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? MaLop { get; set; }
    public string? TenLop { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int HeSoVang { get; set; }
    public int NguoiGhiNhan { get; set; }
    public string TenNguoiGhiNhan { get; set; } = string.Empty;
    public DateTime GhiNhanLuc { get; set; }
    public DateTime? KhoaLuc { get; set; }
}

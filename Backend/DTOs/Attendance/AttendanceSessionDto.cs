namespace Backend.DTOs.Attendance;

public class AttendanceSessionDto
{
    public int MaBuoiHoc { get; set; }
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public string MaCodePhong { get; set; } = string.Empty;
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int? MaGiaoVienDayThay { get; set; }
    public string? TenGiaoVienDayThay { get; set; }
    public string TrangThaiBuoi { get; set; } = string.Empty;
    public string TrangThaiDiemDanh { get; set; } = string.Empty;
    public DateTime? DiemDanhBatDauLuc { get; set; }
    public DateTime? DiemDanhHanGuiLuc { get; set; }
    public DateTime? DiemDanhDaGuiLuc { get; set; }
    public DateTime? DiemDanhHanChinhSuaLuc { get; set; }
    public DateTime? DiemDanhKhoaLuc { get; set; }
}

namespace Backend.DTOs.ThoiKhoaBieu;

public class ScheduleConflictDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TenKhoaHoc { get; set; } = string.Empty;
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public int ThuTrongTuan { get; set; }
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
}

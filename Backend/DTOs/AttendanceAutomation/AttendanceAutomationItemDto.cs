namespace Backend.DTOs.AttendanceAutomation;

public class AttendanceAutomationItemDto
{
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public int MaGiaoVien { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

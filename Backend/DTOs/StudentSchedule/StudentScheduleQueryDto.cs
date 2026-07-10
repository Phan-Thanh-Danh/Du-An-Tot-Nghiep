namespace Backend.DTOs.StudentSchedule;

public class StudentScheduleQueryDto
{
    public DateTime? NgayTu { get; set; }
    public DateTime? NgayDen { get; set; }
    public int? MaHocKy { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? TrangThai { get; set; }
}

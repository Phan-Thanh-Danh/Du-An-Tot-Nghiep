namespace Backend.DTOs.StudentSchedule;

public class StudentScheduleTermDto
{
    public int MaHocKy { get; set; }
    public string? MaCodeHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsUpcoming { get; set; }
    public int? DaysUntilStart { get; set; }
    public int SessionCount { get; set; }
}

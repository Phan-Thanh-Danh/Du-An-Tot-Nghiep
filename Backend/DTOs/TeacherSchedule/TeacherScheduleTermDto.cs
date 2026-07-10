namespace Backend.DTOs.TeacherSchedule;

public class TeacherScheduleTermDto
{
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public bool IsCurrent { get; set; }
    public int SessionCount { get; set; }
}

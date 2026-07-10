namespace Backend.DTOs.TeacherSchedule;

public class TeacherScheduleQueryDto
{
    public DateOnly? NgayTu { get; set; }
    public DateOnly? NgayDen { get; set; }
    public int? MaHocKy { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

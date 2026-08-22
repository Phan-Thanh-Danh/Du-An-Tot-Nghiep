namespace Backend.DTOs.Bgh;

public class CampusComparisonDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Students { get; set; }
    public decimal Gpa { get; set; }
    public decimal PassRate { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal Revenue { get; set; }
    public decimal TeacherScore { get; set; }
}

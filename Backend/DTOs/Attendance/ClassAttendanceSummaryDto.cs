namespace Backend.DTOs.Attendance;

public class ClassAttendanceSummaryDto
{
    public int TotalSessions { get; set; }
    public List<ClassAttendanceStudentDto> Students { get; set; } = new();
}

public class ClassAttendanceStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Percent { get; set; }
    public string Status { get; set; }
}

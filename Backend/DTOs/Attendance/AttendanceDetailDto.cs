namespace Backend.DTOs.Attendance;

public class AttendanceDetailDto : AttendanceSessionDto
{
    public IReadOnlyList<AttendanceStudentDto> Students { get; set; } = [];
}

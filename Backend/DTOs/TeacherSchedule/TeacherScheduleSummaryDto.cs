namespace Backend.DTOs.TeacherSchedule;

public class TeacherScheduleSummaryDto
{
    public TeacherScheduleTermDto? CurrentTerm { get; set; }
    public int AssignedCourseCount { get; set; }
    public int AssignedClassCount { get; set; }
    public int SubjectCount { get; set; }
    public int WeeklyShiftCount { get; set; }
    public List<TeacherScheduleItemDto> TodaySessions { get; set; } = new();
    public TeacherScheduleItemDto? NextSession { get; set; }
    public List<TeacherAssignedCourseDto> AssignedCourses { get; set; } = new();
}

namespace Backend.DTOs.StudentSchedule;

public class StudentScheduleSummaryDto
{
    public StudentScheduleTermDto? CurrentTerm { get; set; }
    public StudentScheduleTermDto? UpcomingTerm { get; set; }
    
    public int TodaySessionCount { get; set; }
    public List<StudentScheduleItemDto> TodaySessions { get; set; } = new();
    public StudentScheduleItemDto? NextSession { get; set; }
    
    public int WeeklySessionCount { get; set; }
    public int SubjectCount { get; set; }
    public int ActiveCourseCount { get; set; }
}

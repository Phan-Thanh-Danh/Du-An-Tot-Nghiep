namespace Backend.DTOs.AcademicSchedulingContext;

public class SchedulingReadinessDto
{
    public bool HasCourses { get; set; }
    public bool HasClasses { get; set; }
    public bool HasSubjects { get; set; }
    public bool HasTeachers { get; set; }
    public bool HasRooms { get; set; }
    public bool HasShifts { get; set; }
    public bool HasPublishedSchedule { get; set; }
    public bool HasDraftSchedule { get; set; }
    public List<SchedulingBlockingIssueDto> BlockingIssues { get; set; } = new();
}

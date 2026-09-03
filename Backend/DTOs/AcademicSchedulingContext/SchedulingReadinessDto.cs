namespace Backend.DTOs.AcademicSchedulingContext;

public class SchedulingReadinessItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = "ready"; // ready | warning | blocked | unknown
    public string Message { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? ActionRoute { get; set; }
    public int AffectedCount { get; set; }
    public List<string> AffectedItems { get; set; } = new();
}

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
    public List<SchedulingReadinessItemDto> Items { get; set; } = new();
}

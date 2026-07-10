namespace Backend.DTOs.AcademicSchedulingContext;

public class AcademicSchedulingContextDto
{
    public DateOnly Today { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public SchedulingTermDto? CurrentTerm { get; set; }
    public SchedulingTermDto? NextTerm { get; set; }
    public SchedulingTermDto? SchedulableTerm { get; set; }
    
    public bool CanPrepareSchedule { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonMessage { get; set; } = string.Empty;
    
    public SchedulingReadinessDto Readiness { get; set; } = new();
}

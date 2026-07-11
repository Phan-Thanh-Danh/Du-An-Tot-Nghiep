using Backend.DTOs.AcademicSchedulingContext;

namespace Backend.DTOs.TeachingPreferences;

public class TeachingPreferenceContextDto
{
    public AcademicSchedulingContextDto SchedulingContext { get; set; } = null!;
    public DateTime? OpenTime { get; set; }
    public DateTime? Deadline { get; set; }
    public bool IsOpen { get; set; }
    public bool IsPastDeadline { get; set; }
    
    // Status info for the current teacher (unstarted, draft, submitted)
    public string CurrentStatus { get; set; } = "unstarted";
    public DateTime? LastUpdated { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

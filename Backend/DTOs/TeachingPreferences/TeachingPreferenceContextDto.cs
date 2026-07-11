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

    // Dynamic UI data
    public List<TeachingPreferenceShiftDto> ActiveShifts { get; set; } = new();
    public List<int> SupportedDays { get; set; } = new() { 2, 3, 4, 5, 6, 7 }; // T2 -> T7
    
    // Permissions
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    
    // Reason/Error handling
    public string? ReasonCode { get; set; }
    public string? ReasonMessage { get; set; }
}

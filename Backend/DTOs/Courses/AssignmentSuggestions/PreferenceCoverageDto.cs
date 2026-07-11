namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class PreferenceCoverageDto
{
    public int PreferredCount { get; set; }
    public int AvailableCount { get; set; }
    public int NeutralCount { get; set; }
    public int UnavailableCount { get; set; }
    
    public double CoverageRatio { get; set; }
    public double Score { get; set; }
    
    public bool HasHardConflicts { get; set; }
    public List<string> Warnings { get; set; } = new();
}

namespace Backend.DTOs.TeachingPreferences;

public class StaffTeachingPreferenceSummaryDto
{
    public int MaHocKy { get; set; }
    public int TotalTeachers { get; set; }
    public int SubmittedCount { get; set; }
    public int DraftCount { get; set; }
    public int UnstartedCount { get; set; }
    
    // Aggregated coverage heatmap. Key: "Thu-Ca" (e.g. "2-1"). Value: Count of teachers who preferred/available
    public Dictionary<string, int> PreferredCoverage { get; set; } = new();
    public Dictionary<string, int> AvailableCoverage { get; set; } = new();
}

namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class TeacherAssignmentCandidateDto
{
    public int MaGiaoVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ChuyenNganh { get; set; } = string.Empty;

    public double CapabilityScore { get; set; }
    public double MainSubjectBonus { get; set; }
    public double ExperienceScore { get; set; }
    public double HistoryScore { get; set; }

    public int CurrentClassCount { get; set; }
    public int CurrentWeeklyShiftCount { get; set; }
    public double ClassWorkloadPenalty { get; set; }
    public double WeeklyShiftPenalty { get; set; }

    public double PreferenceCoverage { get; set; }
    public double PreferenceScore { get; set; }

    public double FinalScore { get; set; }
    public bool IsEligible { get; set; }

    public List<string> Reasons { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

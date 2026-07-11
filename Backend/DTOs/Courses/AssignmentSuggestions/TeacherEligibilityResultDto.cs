namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class TeacherEligibilityResultDto
{
    public bool IsEligible { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonMessage { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

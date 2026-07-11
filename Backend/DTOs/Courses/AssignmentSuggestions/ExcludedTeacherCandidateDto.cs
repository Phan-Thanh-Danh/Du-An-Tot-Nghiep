namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class ExcludedTeacherCandidateDto
{
    public int MaGiaoVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonMessage { get; set; } = string.Empty;
}

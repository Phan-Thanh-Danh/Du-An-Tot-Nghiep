namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class CourseAssignmentSuggestionResultDto
{
    public int MaHocKy { get; set; }
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;

    public List<TeacherAssignmentCandidateDto> Candidates { get; set; } = new();
    public List<ExcludedTeacherCandidateDto> ExcludedCandidates { get; set; } = new();
}

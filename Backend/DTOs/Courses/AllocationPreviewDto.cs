namespace Backend.DTOs.Courses;

public class AllocationPreviewDto
{
    public List<BulkAssignCourseSkippedDto> Skipped { get; set; } = [];
    public List<KhoaHocPreviewDto> Valid { get; set; } = [];
    public bool IsTeacherOverloaded { get; set; }
    public string? WarningMessage { get; set; }
}

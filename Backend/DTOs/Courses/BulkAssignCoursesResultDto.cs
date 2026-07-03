namespace Backend.DTOs.Courses;

public class BulkAssignCoursesResultDto
{
    public List<KhoaHocDto> Created { get; set; } = [];
    public List<BulkAssignCourseSkippedDto> Skipped { get; set; } = [];
    public int CreatedCount => Created.Count;
    public int SkippedCount => Skipped.Count;
}

public class BulkAssignCourseSkippedDto
{
    public int MaLop { get; set; }
    public string? TenLop { get; set; }
    public string LyDo { get; set; } = string.Empty;
}

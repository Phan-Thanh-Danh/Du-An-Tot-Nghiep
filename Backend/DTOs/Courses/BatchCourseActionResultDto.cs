namespace Backend.DTOs.Courses;

public class BatchCourseActionResultDto
{
    public List<int> SuccessIds { get; set; } = [];
    public List<BatchCourseActionFailureDto> Failed { get; set; } = [];
    public int Count => SuccessIds.Count;
}

public class BatchCourseActionFailureDto
{
    public int Id { get; set; }
    public string LyDo { get; set; } = string.Empty;
}

namespace Backend.DTOs.Courses;

public class AllocationSuggestionRequest
{
    public int MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public List<int> MaLopIds { get; set; } = [];
}

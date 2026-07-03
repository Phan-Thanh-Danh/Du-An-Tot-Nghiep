namespace Backend.DTOs.Courses;

public class CloneCourseRequest
{
    public int? MaHocKy { get; set; }
    public int? MaLop { get; set; }
    public int? MaGiaoVien { get; set; }
    public string? TieuDe { get; set; }
    public string? MoTa { get; set; }
    public string? UrlAnhBia { get; set; }
}

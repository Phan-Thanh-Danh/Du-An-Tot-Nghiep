namespace Backend.DTOs.Courses;

public class BulkAssignCoursesRequest
{
    public int MaMonHoc { get; set; }
    public int MaGiaoVien { get; set; }
    public int? MaHocKy { get; set; }
    public List<int> MaLopIds { get; set; } = [];
    public string? TieuDe { get; set; }
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "nhap";
    public string? UrlAnhBia { get; set; }
}

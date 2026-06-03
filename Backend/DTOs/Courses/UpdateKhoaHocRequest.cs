namespace Backend.DTOs.Courses;

public class UpdateKhoaHocRequest
{
    public int MaGiaoVien { get; set; }
    public int? MaHocKy { get; set; }
    public int MaLop { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? UrlAnhBia { get; set; }
}

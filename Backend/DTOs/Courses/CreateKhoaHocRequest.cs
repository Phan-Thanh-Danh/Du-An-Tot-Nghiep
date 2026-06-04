namespace Backend.DTOs.Courses;

public class CreateKhoaHocRequest
{
    public int MaDonVi { get; set; }
    public int MaMonHoc { get; set; }
    public int MaGiaoVien { get; set; }
    public int? MaHocKy { get; set; }
    public int MaLop { get; set; }
    public string? TieuDe { get; set; }
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "nhap";
    public string? UrlAnhBia { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Courses;

public class KhoaHocQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? MaMonHoc { get; set; }
    public int? MaGiaoVien { get; set; }
    public int? MaHocKy { get; set; }
    public int? MaLop { get; set; }
    public string? TrangThai { get; set; }
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

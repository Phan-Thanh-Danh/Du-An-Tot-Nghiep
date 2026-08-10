using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Subjects;

public class SubjectQueryParameters
{
    public string? Keyword { get; set; }
    public bool? ConHoatDong { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 1000, ErrorMessage = "pageSize phải từ 1 đến 1000.")]
    public int PageSize { get; set; } = 20;

    public int? MaNganh { get; set; }
    public int? MaChuyenNganh { get; set; }
}

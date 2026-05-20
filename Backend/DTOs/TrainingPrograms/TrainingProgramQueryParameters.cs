using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingPrograms;

public class TrainingProgramQueryParameters
{
    public string? Keyword { get; set; }
    public int? MaKhoaTuyenSinh { get; set; }
    public int? MaDonVi { get; set; }
    public int? MaChuyenNganh { get; set; }
    public int? MaNganh { get; set; }
    public string? TrangThai { get; set; }
    public bool? ConHoatDong { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

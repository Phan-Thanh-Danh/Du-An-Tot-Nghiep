using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingProgramSubjects;

public class TrainingProgramSubjectQueryParameters
{
    public int? MaChuongTrinh { get; set; }
    public int? MaMonHoc { get; set; }
    public int? HocKyDuKien { get; set; }
    public int? SemesterNumber { get; set; }
    public string? LoaiMonHoc { get; set; }
    public bool? BatBuoc { get; set; }
    public bool? ConHoatDong { get; set; }
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

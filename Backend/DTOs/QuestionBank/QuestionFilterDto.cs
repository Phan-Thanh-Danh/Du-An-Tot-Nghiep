using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuestionBank;

public class QuestionFilterDto
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public int? MaMonHoc { get; set; }
    public string? Keyword { get; set; }
    public string? DoKho { get; set; }
    public string? LoaiCauHoi { get; set; }
    public string? KieuLuaChon { get; set; }
    public bool? ConHoatDong { get; set; }
}

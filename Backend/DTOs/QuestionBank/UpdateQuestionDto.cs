using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuestionBank;

public class UpdateQuestionDto
{
    [Required]
    public int MaMonHoc { get; set; }

    [Required]
    public string LoaiCauHoi { get; set; } = string.Empty;

    [Required]
    public string NoiDung { get; set; } = string.Empty;

    public string? KieuLuaChon { get; set; }

    public List<QuestionChoiceDto>? LuaChon { get; set; }

    public List<string>? DapAnDung { get; set; }

    public string? GiaiThichDapAn { get; set; }

    [Required]
    public string DoKho { get; set; } = string.Empty;
}

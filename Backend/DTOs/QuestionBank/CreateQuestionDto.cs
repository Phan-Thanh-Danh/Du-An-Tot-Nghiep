using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuestionBank;

public class CreateQuestionDto
{
    [Required]
    public int MaMonHoc { get; set; }

    [Required]
    public string LoaiCauHoi { get; set; } = string.Empty; // trac_nghiem, tu_luan

    [Required]
    public string NoiDung { get; set; } = string.Empty;

    public string? KieuLuaChon { get; set; } // chon_mot, chon_nhieu

    public List<QuestionChoiceDto>? LuaChon { get; set; }

    public List<string>? DapAnDung { get; set; }

    public string? GiaiThichDapAn { get; set; }

    [Required]
    public string DoKho { get; set; } = string.Empty; // de, trung_binh, kho
}

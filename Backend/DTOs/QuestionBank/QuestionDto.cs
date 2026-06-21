namespace Backend.DTOs.QuestionBank;

public class QuestionDto
{
    public int MaCauHoi { get; set; }
    public int? MaMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public string LoaiCauHoi { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string? KieuLuaChon { get; set; }
    public List<QuestionChoiceDto>? LuaChon { get; set; }
    public List<string>? DapAnDung { get; set; }
    public string? GiaiThichDapAn { get; set; }
    public string DoKho { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

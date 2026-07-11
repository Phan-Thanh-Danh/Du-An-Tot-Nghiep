namespace Backend.DTOs.SmartTimetable;

public class ScheduleDraftItemDto
{
    public int MaDraftItem { get; set; }
    public int MaKhoaHoc { get; set; }
    public string? MaKhoaHocCode { get; set; }
    public int? ThuTrongTuan { get; set; }
    public int? MaCaHoc { get; set; }
    public string? TenCa { get; set; }
    public int? MaPhong { get; set; }
    public string? TenPhong { get; set; }
    public string TrangThai { get; set; } = "pending";
    public double? Score { get; set; }
    public Backend.DTOs.SmartTimetable.Suggestions.ScheduleSlotScoreComponentsDto? ScoreBreakdown { get; set; }
    public List<string> LyDoGoiY { get; set; } = new();
    public string? PreferenceLevel { get; set; }
    public List<string> CanhBao { get; set; } = new();
    public List<string> Loi { get; set; } = new();
}

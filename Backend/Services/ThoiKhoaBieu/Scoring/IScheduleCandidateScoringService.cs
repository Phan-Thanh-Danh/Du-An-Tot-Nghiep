using Backend.DTOs.SmartTimetable.Suggestions;
using Backend.Models;

namespace Backend.Services.ThoiKhoaBieu.Scoring;

public class ScheduleCandidateContext
{
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public KhoaHoc Course { get; set; } = null!;
    public PhongHoc Room { get; set; } = null!;
    public Models.CaHoc Shift { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public int ExpectedStudentCount { get; set; }
    
    // Preferences: level can be "preferred", "available", "unavailable", or null/"unknown"
    public string? PreferenceLevel { get; set; }
    public bool HasDraftPreference { get; set; }
    
    public int TeacherDailyLoad { get; set; }
    public int ClassDailyLoad { get; set; }
}

public interface IScheduleCandidateScoringService
{
    ScheduleSlotSuggestionDto ScoreCandidate(ScheduleCandidateContext context);
    
    /// <summary>
    /// Sắp xếp danh sách candidates theo độ ưu tiên giảm dần một cách deterministic.
    /// </summary>
    List<ScheduleSlotSuggestionDto> SortCandidates(IEnumerable<ScheduleSlotSuggestionDto> candidates);
}

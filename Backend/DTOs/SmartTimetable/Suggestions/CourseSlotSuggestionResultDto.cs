using System.Text.Json.Serialization;

namespace Backend.DTOs.SmartTimetable.Suggestions;

public class ScheduleSlotScoreComponentsDto
{
    public double Base { get; set; }
    public double RoomFit { get; set; }
    public double PreferredShift { get; set; }
    public double AvailableShift { get; set; }
    public double TeacherDayLoadPenalty { get; set; }
    public double ClassDayLoadPenalty { get; set; }
    public double SaturdayPenalty { get; set; }
    public double EveningPenalty { get; set; }
}

public class ScheduleSlotSuggestionDto
{
    public int Rank { get; set; }
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string MaCodePhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    
    public double Score { get; set; }
    public double RawScore { get; set; }
    public bool HardConstraintPassed { get; set; }
    
    public ScheduleSlotScoreComponentsDto Components { get; set; } = new();
    public List<string> Reasons { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class RejectedCandidateSummaryDto
{
    public int TeacherConflicts { get; set; }
    public int ClassConflicts { get; set; }
    public int RoomConflicts { get; set; }
    public int UnavailablePreferences { get; set; }
    public int CapacityRejected { get; set; }
    public int InactiveRooms { get; set; }
    public int InactiveShifts { get; set; }
}

public class CourseSlotSuggestionResultDto
{
    public int MaKhoaHoc { get; set; }
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public string TeacherPreferenceStatus { get; set; } = "unknown";
    public int ExpectedStudentCount { get; set; }
    
    public List<ScheduleSlotSuggestionDto> Candidates { get; set; } = new();
    public RejectedCandidateSummaryDto RejectedSummary { get; set; } = new();
}

using Backend.Configuration;
using Backend.DTOs.SmartTimetable.Suggestions;
using Microsoft.Extensions.Options;

namespace Backend.Services.ThoiKhoaBieu.Scoring;

public class ScheduleCandidateScoringService : IScheduleCandidateScoringService
{
    private readonly SmartTimetableScoringOptions _options;

    public ScheduleCandidateScoringService(IOptions<SmartTimetableScoringOptions> options)
    {
        _options = options.Value;
    }

    public ScheduleSlotSuggestionDto ScoreCandidate(ScheduleCandidateContext context)
    {
        var result = new ScheduleSlotSuggestionDto
        {
            ThuTrongTuan = context.DayOfWeek,
            MaCaHoc = context.Shift.MaCaHoc,
            TenCa = context.Shift.TenCa ?? "",
            GioBatDau = context.Shift.GioBatDau.ToString(),
            GioKetThuc = context.Shift.GioKetThuc.ToString(),
            MaPhong = context.Room.MaPhong,
            MaCodePhong = context.Room.MaCodePhong ?? "",
            TenPhong = context.Room.TenPhong ?? context.Room.MaCodePhong ?? "",
            HardConstraintPassed = true
        };

        // Base score
        result.Components.Base = _options.BaseScore;
        result.RawScore += _options.BaseScore;
        result.Reasons.Add("Điểm cơ sở: " + _options.BaseScore);

        // Preference evaluation
        EvaluatePreferences(context, result);
        
        // Workload evaluation
        EvaluateWorkload(context, result);
        
        // Time & Day evaluation
        EvaluateTimeAndDay(context, result);
        
        // Room fit evaluation
        EvaluateRoomFit(context, result);

        // Calculate final score
        result.Score = result.RawScore;
        // Optional clamp to 0-100 could be done here if strictly needed, but raw is better.
        // If clamped, we should preserve RawScore for tie-breaking.
        
        return result;
    }

    private void EvaluatePreferences(ScheduleCandidateContext context, ScheduleSlotSuggestionDto result)
    {
        if (context.PreferenceLevel == "unavailable")
        {
            result.HardConstraintPassed = false;
            result.Warnings.Add("Giảng viên báo bận vào thời gian này.");
            return;
        }

        if (context.PreferenceLevel == "preferred")
        {
            result.Components.PreferredShift = _options.PreferredShiftBonus;
            result.RawScore += _options.PreferredShiftBonus;
            result.Reasons.Add("Thuộc nguyện vọng ưu tiên của giảng viên.");
        }
        else if (context.PreferenceLevel == "available")
        {
            result.Components.AvailableShift = _options.AvailableShiftBonus;
            result.RawScore += _options.AvailableShiftBonus;
            result.Reasons.Add("Thời gian phù hợp với giảng viên.");
        }

        if (context.HasDraftPreference)
        {
            result.Warnings.Add("Giảng viên có nguyện vọng nháp nhưng chưa gửi chính thức.");
        }
        else if (string.IsNullOrEmpty(context.PreferenceLevel))
        {
            result.Warnings.Add("Giảng viên chưa gửi nguyện vọng.");
        }
    }

    private void EvaluateWorkload(ScheduleCandidateContext context, ScheduleSlotSuggestionDto result)
    {
        if (context.TeacherDailyLoad >= _options.TeacherDailyLoadThreshold)
        {
            result.Components.TeacherDayLoadPenalty = -_options.TeacherDailyLoadPenalty;
            result.RawScore -= _options.TeacherDailyLoadPenalty;
            result.Reasons.Add($"Giảng viên đã có >= {_options.TeacherDailyLoadThreshold} ca trong ngày này.");
        }

        if (context.ClassDailyLoad >= _options.ClassDailyLoadThreshold)
        {
            result.Components.ClassDayLoadPenalty = -_options.ClassDailyLoadPenalty;
            result.RawScore -= _options.ClassDailyLoadPenalty;
            result.Reasons.Add($"Lớp đã có >= {_options.ClassDailyLoadThreshold} ca trong ngày này.");
        }
    }

    private void EvaluateTimeAndDay(ScheduleCandidateContext context, ScheduleSlotSuggestionDto result)
    {
        if (context.DayOfWeek == 7)
        {
            result.Components.SaturdayPenalty = -_options.SaturdayPenalty;
            result.RawScore -= _options.SaturdayPenalty;
            result.Reasons.Add("Thứ 7 bị trừ điểm nhẹ.");
        }

        var isEvening = (context.Shift.Buoi?.Contains("Tối", StringComparison.OrdinalIgnoreCase) == true) ||
                        (context.Shift.TenCa?.Contains("Tối", StringComparison.OrdinalIgnoreCase) == true);
        
        if (isEvening)
        {
            result.Components.EveningPenalty = -_options.EveningPenalty;
            result.RawScore -= _options.EveningPenalty;
            result.Reasons.Add("Ca tối bị trừ điểm nhẹ.");
        }
    }

    private void EvaluateRoomFit(ScheduleCandidateContext context, ScheduleSlotSuggestionDto result)
    {
        if (context.ExpectedStudentCount > 0 && context.Room.SucChua > 0)
        {
            if (context.Room.SucChua < context.ExpectedStudentCount)
            {
                result.HardConstraintPassed = false;
                result.Warnings.Add($"Sức chứa phòng ({context.Room.SucChua}) không đủ cho sĩ số ({context.ExpectedStudentCount}).");
                return;
            }

            var ratio = (double)context.Room.SucChua / context.ExpectedStudentCount;
            if (ratio >= 1.0 && ratio <= _options.OversizedRoomRatio)
            {
                result.Components.RoomFit = _options.GoodRoomFitBonus;
                result.RawScore += _options.GoodRoomFitBonus;
                result.Reasons.Add("Phòng có sức chứa phù hợp.");
            }
            else if (ratio > _options.OversizedRoomRatio)
            {
                result.Components.RoomFit = -_options.OversizedRoomPenalty;
                result.RawScore -= _options.OversizedRoomPenalty;
                result.Reasons.Add("Phòng quá lớn so với sĩ số.");
            }
        }
        else
        {
            result.Warnings.Add("Chưa có dữ liệu sĩ số để đánh giá độ phù hợp phòng.");
        }
    }

    public List<ScheduleSlotSuggestionDto> SortCandidates(IEnumerable<ScheduleSlotSuggestionDto> candidates)
    {
        var rank = 1;
        return candidates
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Warnings.Count)
            .ThenBy(c => c.ThuTrongTuan)
            .ThenBy(c => c.MaCaHoc) // Should technically be ThuTu but MaCaHoc works as a tie-breaker
            .ThenBy(c => c.MaPhong)
            .Select(c =>
            {
                c.Rank = rank++;
                return c;
            })
            .ToList();
    }
}

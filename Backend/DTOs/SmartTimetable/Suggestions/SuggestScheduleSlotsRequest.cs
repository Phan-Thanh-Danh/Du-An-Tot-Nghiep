namespace Backend.DTOs.SmartTimetable.Suggestions;

public class SuggestScheduleSlotsRequest
{
    public int MaKhoaHoc { get; set; }
    public int TopN { get; set; } = 10;
    public List<int>? CandidateDays { get; set; }
    public List<int>? CandidateShiftIds { get; set; }
    public List<int>? CandidateRoomIds { get; set; }
}

public class SuggestScheduleSlotsBatchRequest
{
    public List<int> MaKhoaHocIds { get; set; } = new();
    public int TopNPerCourse { get; set; } = 5;
}

public class AssignedCourseSuggestionDto
{
    public int MaKhoaHoc { get; set; }
    public ScheduleSlotSuggestionDto SelectedCandidate { get; set; } = null!;
    public List<ScheduleSlotSuggestionDto> Alternatives { get; set; } = new();
}

public class UnassignedCourseSuggestionDto
{
    public int MaKhoaHoc { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public List<string> Reasons { get; set; } = new();
}

public class BatchSlotSuggestionSummaryDto
{
    public int Total { get; set; }
    public int Assigned { get; set; }
    public int Unassigned { get; set; }
}

public class BatchSlotSuggestionResultDto
{
    public List<AssignedCourseSuggestionDto> Assigned { get; set; } = new();
    public List<UnassignedCourseSuggestionDto> Unassigned { get; set; } = new();
    public BatchSlotSuggestionSummaryDto Summary { get; set; } = new();
}

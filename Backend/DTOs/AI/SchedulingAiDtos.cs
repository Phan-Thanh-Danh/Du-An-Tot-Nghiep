using System;
using System.Collections.Generic;

namespace Backend.DTOs.AI;

public class AiSchedulingInterpretRequest
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
    public Guid? DraftId { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(8)]
    public List<AiConversationTurn> History { get; set; } = new();
}

public class AiSchedulingInterpretResponse
{
    public string Intent { get; set; } = "clarify"; // prepare_schedule | query_schedule | query_readiness | clarify | unsupported
    public bool ExcludeEvening { get; set; }
    public List<string> UnsupportedPreferences { get; set; } = new();
    public string Profile { get; set; } = "balanced"; // "balanced" | "student_friendly" | "teacher_friendly"
    public string ProfileDisplayName { get; set; } = "Cân bằng toàn diện";
    public string Summary { get; set; } = string.Empty;
    public List<string> RequestedPreferences { get; set; } = new();
    public bool RequiresConfirmation { get; set; } = true;
    public string ContextVersion { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public string CampusName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public int SchedulableCourseCount { get; set; }
    public bool CanPrepareSchedule { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

public class AiExplainDraftRequest
{
    public Guid DraftId { get; set; }
    public int? CampusId { get; set; }
}

public class SchedulingDraftFactsDto
{
    public Guid DraftId { get; set; }
    public int TotalCourses { get; set; }
    public int AssignedCourses { get; set; }
    public int UnassignedCourses { get; set; }
    public int HardConflictsCount { get; set; }
    public int EveningShiftsCount { get; set; }
    public int SaturdayShiftsCount { get; set; }
    public double SuccessRate { get; set; }
    public double? BestFitnessScore { get; set; }
    public string ProfileUsed { get; set; } = "balanced";
    public int TotalSessionsCount { get; set; }
    public double AverageRoomFitRatio { get; set; }
    public List<string> HighlightNotes { get; set; } = new();
}

public class AiExplainDraftResponse
{
    public Guid DraftId { get; set; }
    public bool IsSuccess { get; set; }
    public SchedulingDraftFactsDto Facts { get; set; } = new();
    public string AiExplanation { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AiExplainReadinessRequest
{
    public string ReasonCode { get; set; } = string.Empty;
    public string? RawMessage { get; set; }
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
}

public class AiExplainReadinessResponse
{
    public string ReasonCode { get; set; } = string.Empty;
    public string HumanExplanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string ActionRoute { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
}

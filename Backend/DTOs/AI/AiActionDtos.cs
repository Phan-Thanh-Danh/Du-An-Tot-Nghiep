using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AI;

// ── 1. BGH AI Analytics Request & Response ───────────────────────────────────

public class BghAiReportRequest
{
    [Required]
    public string ReportType { get; set; } = "gpa"; // "gpa", "at_risk", "pass_fail", "teacher_eval", "academic_overview", "detailed_report"

    public int? SemesterId { get; set; }
    public int? DepartmentId { get; set; }
    public int? MajorId { get; set; }
    public int? SpecializationId { get; set; }
    public int? CampusId { get; set; }
    public string Mode { get; set; } = "deep"; // Always default to "deep" (9B) for BGH strategic analytics
    public bool UseRag { get; set; } = false;
    public bool ForceRefresh { get; set; } = false;
    public string? CustomPrompt { get; set; }
}

public class BghAiReportResponse
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public object? Metrics { get; set; }
    public string AiAnalysis { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool Cached { get; set; }
    public List<string> Sources { get; set; } = new();
}

public class GpaAnalyticsContextDto
{
    public int CampusId { get; set; }
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public double AverageGpa { get; set; }
    public double PreviousSemesterGpa { get; set; }
    public double GpaDelta { get; set; }
    public Dictionary<string, int> ScoreRanges { get; set; } = new(); // "<5.0", "5.0-6.9", "7.0-7.9", "8.0-8.9", "9.0-10"
    public List<DepartmentGpaSummaryDto> DepartmentGpas { get; set; } = new();
    public string TrendTrajectory { get; set; } = string.Empty;
}

public class DepartmentGpaSummaryDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public double AverageGpa { get; set; }
    public int StudentCount { get; set; }
}

public class AtRiskAnalyticsContextDto
{
    public int CampusId { get; set; }
    public int SemesterId { get; set; }
    public int TotalAtRiskStudents { get; set; }
    public int CriticalCount { get; set; }
    public int ModerateCount { get; set; }
    public int WatchlistCount { get; set; }
    public List<AtRiskClassSummaryDto> TopAtRiskClasses { get; set; } = new();
    public List<string> RiskSignals { get; set; } = new();
}

public class AtRiskClassSummaryDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int AtRiskCount { get; set; }
    public string PrimaryReason { get; set; } = string.Empty;
}

public class PassFailAnalyticsContextDto
{
    public int CampusId { get; set; }
    public int SemesterId { get; set; }
    public int TotalEnrollments { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public double PassRate { get; set; }
    public double FailRate { get; set; }
    public List<SubjectPassFailSummaryDto> TopFailedSubjects { get; set; } = new();
}

public class SubjectPassFailSummaryDto
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int FailedStudents { get; set; }
    public double FailRate { get; set; }
}

public class TeacherEvaluationContextDto
{
    public int CampusId { get; set; }
    public int SemesterId { get; set; }
    public double AverageRating { get; set; }
    public int TotalResponses { get; set; }
    public Dictionary<string, int> RatingDistribution { get; set; } = new();
    public List<TeacherRatingSummaryDto> TeacherSummaries { get; set; } = new();
}

public class TeacherRatingSummaryDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int TotalClasses { get; set; }
    public int ResponseCount { get; set; }
}

// ── 2. AI Quiz Generator Action DTOs ─────────────────────────────────────────

public class AiGenerateQuizRequest
{
    [Required]
    public int MaMonHoc { get; set; }

    [Required]
    [MaxLength(255)]
    public string TieuDe { get; set; } = string.Empty;

    public string? ChuDe { get; set; }

    [Range(1, 30)]
    public int SoLuongCauHoi { get; set; } = 5;

    [Range(5, 180)]
    public int ThoiGianPhut { get; set; } = 15;

    public string DoKho { get; set; } = "trung_binh"; // "de", "trung_binh", "kho"
    public int? MaBaiHoc { get; set; }
}

public class AiGenerateQuizResponse
{
    public bool Success { get; set; }
    public int MaDeKiemTra { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public int TongSoCau { get; set; }
    public int ThoiGianPhut { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public List<AiGeneratedQuestionDto> DanhSachCauHoi { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class AiGeneratedQuestionDto
{
    public int MaCauHoi { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public List<AiQuestionChoiceDto> LuaChon { get; set; } = new();
    public string DapAnDung { get; set; } = string.Empty;
    public string GiaiThich { get; set; } = string.Empty;
    public string DoKho { get; set; } = string.Empty;
    public decimal DiemSo { get; set; }
}

public class AiQuestionChoiceDto
{
    public string Id { get; set; } = string.Empty; // "A", "B", "C", "D"
    public string Text { get; set; } = string.Empty;
}

// ── 3. Academic Staff (Giáo Vụ) Timetable Conflict DTOs ───────────────────────

public class AiTimetableConflictCheckRequest
{
    public int? SemesterId { get; set; }
    public DateOnly? NgayHoc { get; set; }
}

public class AiTimetableConflictCheckResponse
{
    public int TotalConflictsFound { get; set; }
    public List<AiTimetableConflictItemDto> Conflicts { get; set; } = new();
    public string AiRecommendations { get; set; } = string.Empty;
}

public class AiTimetableConflictItemDto
{
    public string ConflictType { get; set; } = string.Empty; // "TrungPhong", "TrungGiaoVien", "QuaSucChua"
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "high";
    public string SuggestedResolution { get; set; } = string.Empty;
}

// ── 4. Phase 3: BGH Awards, Facilities & Certificate Template Assistant DTOs ────

public class AwardsAnalyticsContextDto
{
    public int CampusId { get; set; }
    public int TotalCampaigns { get; set; }
    public int TotalAwardsIssued { get; set; }
    public int TotalDistinctRewardedStudents { get; set; }
    public double AverageGpaOfAwardees { get; set; }
    public List<TopAwardedStudentDto> TopFrequentAwardees { get; set; } = new();
    public List<Top3GpaHonorStudentDto> Top3AnnualGpaHonors { get; set; } = new();
}

public class TopAwardedStudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int RewardCount { get; set; }
    public double AverageGpa { get; set; }
    public string LatestAwardTitle { get; set; } = string.Empty;
}

public class Top3GpaHonorStudentDto
{
    public int Rank { get; set; } // 1, 2, 3
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public double CumulativeGpa { get; set; }
    public int RewardCount { get; set; }
    public string HonorTitle { get; set; } = string.Empty; // e.g., "Thủ khoa xuất sắc toàn trường"
    public string RecommendationReason { get; set; } = string.Empty;
}

public class FacilitiesAnalyticsContextDto
{
    public int CampusId { get; set; }
    public int TotalBuildings { get; set; }
    public int TotalFloors { get; set; }
    public int TotalRooms { get; set; }
    public int TotalCapacity { get; set; }
    public int ActiveRooms { get; set; }
    public int MaintenanceRooms { get; set; }
    public double UtilizationRate { get; set; }
    public List<BuildingFacilitySummaryDto> BuildingSummaries { get; set; } = new();
    public List<EquipmentIssueDto> EquipmentIssues { get; set; } = new();
}

public class BuildingFacilitySummaryDto
{
    public int BuildingId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int TotalCapacity { get; set; }
    public int ActiveRooms { get; set; }
    public int MaintenanceRooms { get; set; }
    public string OperationalStatus { get; set; } = "Ổn định";
}

public class EquipmentIssueDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string IssueStatus { get; set; } = string.Empty; // "CanBaoTri", "HongHoc", "QuaHanKiemDinh"
    public string Note { get; set; } = string.Empty;
}

public class AiCertificateTemplateEditRequest
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string CurrentHtml { get; set; } = string.Empty;
    public string CurrentCss { get; set; } = string.Empty;
    [Required]
    public string Instruction { get; set; } = string.Empty;
    public string Mode { get; set; } = "deep";
}

public class AiCertificateTemplateEditResponse
{
    public int TemplateId { get; set; }
    public string UpdatedHtml { get; set; } = string.Empty;
    public string UpdatedCss { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> ChangesSummary { get; set; } = new();
}

public class AcademicOverviewContextDto
{
    public int CampusId { get; set; }
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public double AverageGpa { get; set; }
    public double PassRate { get; set; }
    public double FailRate { get; set; }
    public int TotalAtRiskStudents { get; set; }
    public int CriticalCount { get; set; }
    public int ModerateCount { get; set; }
    public int WatchlistCount { get; set; }
    public Dictionary<string, int> ScoreRanges { get; set; } = new();
    public List<SubjectPassFailSummaryDto> TopFailedSubjects { get; set; } = new();
}


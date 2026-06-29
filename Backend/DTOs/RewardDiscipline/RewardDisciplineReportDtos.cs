namespace Backend.DTOs.RewardDiscipline;

public class RewardDisciplineReportQuery
{
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? GroupBy { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RewardReportQuery : RewardDisciplineReportQuery
{
    public string? LoaiDot { get; set; }
    public string? LoaiKhenThuong { get; set; }
    public string? TrangThai { get; set; }
}

public class DisciplineReportQuery : RewardDisciplineReportQuery
{
    public string? MucDoKyLuat { get; set; }
    public string? HinhThucXuLy { get; set; }
    public string? TrangThai { get; set; }
}

public class CertificateReportQuery : RewardDisciplineReportQuery
{
    public int? MaDotKhenThuong { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? TrangThai { get; set; }
}

public class DisciplineAppealReportQuery : RewardDisciplineReportQuery
{
    public string? TrangThai { get; set; }
}

public class RewardDisciplineTrendQuery
{
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Metric { get; set; } = string.Empty;
    public string GroupBy { get; set; } = "month";
}

public class TopStudentReportQuery
{
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public string Mode { get; set; } = "balanced";
    public int Limit { get; set; } = 10;
}

public class RewardDisciplineOverviewReportDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public ReportFilterDto Filters { get; set; } = new();
    public int TotalRewardCampaigns { get; set; }
    public int TotalRewards { get; set; }
    public int TotalIssuedRewards { get; set; }
    public int TotalCertificateGenerated { get; set; }
    public int TotalCertificateFailed { get; set; }
    public int TotalActiveDisciplineRecords { get; set; }
    public int TotalExpiredDisciplineRecords { get; set; }
    public int TotalRemovedDisciplineRecords { get; set; }
    public int TotalDisciplineAppeals { get; set; }
    public int PendingDisciplineAppeals { get; set; }
    public int ApprovedDisciplineAppeals { get; set; }
    public int RejectedDisciplineAppeals { get; set; }
    public IReadOnlyList<StatusCountDto> RewardByStatus { get; set; } = [];
    public IReadOnlyList<StatusCountDto> DisciplineByStatus { get; set; } = [];
    public IReadOnlyList<StatusCountDto> AppealByStatus { get; set; } = [];
    public IReadOnlyList<LatestRewardDisciplineEventDto> LatestRewardEvents { get; set; } = [];
    public IReadOnlyList<LatestRewardDisciplineEventDto> LatestDisciplineEvents { get; set; } = [];
}

public class RewardReportDto
{
    public ReportFilterDto Filters { get; set; } = new();
    public int TotalCampaigns { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalApprovedRewards { get; set; }
    public int TotalIssuedRewards { get; set; }
    public int TotalCanceledRewards { get; set; }
    public int TotalRestoredRewards { get; set; }
    public decimal AverageRewardsPerCampaign { get; set; }
    public IReadOnlyList<StatusCountDto> CampaignsByStatus { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RewardsByType { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RewardsBySemester { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RewardsByCampus { get; set; } = [];
    public IReadOnlyList<StatusCountDto> RewardsByStatus { get; set; } = [];
    public IReadOnlyList<TopStudentReportItemDto> TopRewardedStudents { get; set; } = [];
}

public class DisciplineReportDto
{
    public ReportFilterDto Filters { get; set; } = new();
    public int TotalDisciplineRecords { get; set; }
    public int ActiveRecords { get; set; }
    public int ApprovedRecords { get; set; }
    public int RejectedRecords { get; set; }
    public int ExpiredRecords { get; set; }
    public int RemovedEffectRecords { get; set; }
    public int VoidedRecords { get; set; }
    public decimal? AverageActiveDurationDays { get; set; }
    public IReadOnlyList<GroupCountDto> RecordsBySeverity { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RecordsByHandlingMethod { get; set; } = [];
    public IReadOnlyList<StatusCountDto> RecordsByStatus { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RecordsBySemester { get; set; } = [];
    public IReadOnlyList<GroupCountDto> RecordsByCampus { get; set; } = [];
    public IReadOnlyList<TopStudentReportItemDto> RepeatDisciplineStudents { get; set; } = [];
}

public class CertificateReportDto
{
    public ReportFilterDto Filters { get; set; } = new();
    public int TotalRewardsEligibleForCertificate { get; set; }
    public int TotalCertificatesGenerated { get; set; }
    public int TotalCertificatesFailed { get; set; }
    public int? TotalDownloadedByStudents { get; set; }
    public decimal GenerationFailureRate { get; set; }
    public IReadOnlyList<GroupCountDto> CertificatesByTemplate { get; set; } = [];
    public IReadOnlyList<GroupCountDto> CertificatesByCampaign { get; set; } = [];
    public IReadOnlyList<StatusCountDto> CertificatesByStatus { get; set; } = [];
    public IReadOnlyList<RecentFailedCertificateDto> RecentFailedCertificates { get; set; } = [];
}

public class DisciplineAppealReportDto
{
    public ReportFilterDto Filters { get; set; } = new();
    public int TotalAppeals { get; set; }
    public int PendingAppeals { get; set; }
    public int AcceptedAppeals { get; set; }
    public int RejectedAppeals { get; set; }
    public decimal? AverageResolutionTimeHours { get; set; }
    public int OverdueAppeals { get; set; }
    public int SlaHours { get; set; }
    public IReadOnlyList<StatusCountDto> AppealsByStatus { get; set; } = [];
    public IReadOnlyList<GroupCountDto> AppealsByDisciplineSeverity { get; set; } = [];
    public IReadOnlyList<GroupCountDto> AppealsBySemester { get; set; } = [];
    public IReadOnlyList<GroupCountDto> AppealsByCampus { get; set; } = [];
}

public class RewardDisciplineTrendReportDto
{
    public string Metric { get; set; } = string.Empty;
    public string GroupBy { get; set; } = string.Empty;
    public ReportFilterDto Filters { get; set; } = new();
    public IReadOnlyList<TrendPointDto> Points { get; set; } = [];
}

public class TrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Value { get; set; }
}

public class TopStudentReportItemDto
{
    public int StudentId { get; set; }
    public string? StudentCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public int RewardCount { get; set; }
    public int DisciplineCount { get; set; }
    public int ActiveDisciplineCount { get; set; }
    public decimal Score { get; set; }
    public string? LatestRewardTitle { get; set; }
    public string? LatestDisciplineStatus { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class GroupCountDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LatestRewardDisciplineEventDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class RecentFailedCertificateDto
{
    public int MaKhenThuong { get; set; }
    public int? MaDotKhenThuong { get; set; }
    public string? TenDot { get; set; }
    public int MaHocSinh { get; set; }
    public string? HoTen { get; set; }
    public string? Mssv { get; set; }
    public string SafeError { get; set; } = string.Empty;
    public DateTime? NgaySinhPdf { get; set; }
}

public class ReportFilterDto
{
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? GroupBy { get; set; }
}

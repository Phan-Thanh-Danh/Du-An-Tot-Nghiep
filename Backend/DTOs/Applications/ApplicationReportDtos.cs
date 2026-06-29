namespace Backend.DTOs.Applications;

public class ApplicationReportQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
    public string? LoaiDon { get; set; }
    public string? Type { get; set; }
    public string? TrangThai { get; set; }
    public string? Status { get; set; }
    public string? TrangThaiXuLyNghiepVu { get; set; }
    public string? ProcessingStatus { get; set; }
    public int? NguoiDuyetHienTai { get; set; }
    public int? AssigneeId { get; set; }
    public int? NguoiXuLyCuoi { get; set; }
    public int? ProcessorId { get; set; }
    public DateTime? TuNgayNop { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? DenNgayNop { get; set; }
    public DateTime? SubmittedTo { get; set; }
}

public class ApplicationReportOverviewDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public ApplicationReportFiltersDto Filters { get; set; } = new();
    public ApplicationReportSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ApplicationReportBucketDto> StatusBreakdown { get; set; } = [];
    public IReadOnlyList<ApplicationReportBucketDto> ProcessingStatusBreakdown { get; set; } = [];
    public IReadOnlyList<ApplicationReportTypeBreakdownDto> TypeBreakdown { get; set; } = [];
    public IReadOnlyList<ApplicationReportCampusBreakdownDto> CampusBreakdown { get; set; } = [];
    public ApplicationReportReviewPerformanceDto ReviewPerformance { get; set; } = new();
}

public class ApplicationReportFiltersDto
{
    public int? CampusId { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? ProcessingStatus { get; set; }
    public int? AssigneeId { get; set; }
    public int? ProcessorId { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? SubmittedTo { get; set; }
}

public class ApplicationReportSummaryDto
{
    public int TotalApplications { get; set; }
    public int PendingReview { get; set; }
    public int WaitingForSupplement { get; set; }
    public int Overdue { get; set; }
    public int DueSoon { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int PendingProcessing { get; set; }
    public int ManualRequired { get; set; }
    public int ProcessingRecorded { get; set; }
    public int ProcessingSucceeded { get; set; }
    public int ProcessingFailed { get; set; }
}

public class ApplicationReportBucketDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ApplicationReportTypeBreakdownDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int PendingReview { get; set; }
    public int ManualRequired { get; set; }
}

public class ApplicationReportCampusBreakdownDto
{
    public int CampusId { get; set; }
    public string CampusName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int PendingReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Overdue { get; set; }
}

public class ApplicationReportReviewPerformanceDto
{
    public int DecidedCount { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal RejectionRate { get; set; }
    public decimal? AverageReviewHours { get; set; }
}


public class ApplicationByTypeReportDto
{
    public string ApplicationTypeId { get; set; } = string.Empty;
    public string ApplicationTypeName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Canceled { get; set; }
    public int Overdue { get; set; }
    public decimal? AverageProcessingHours { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal RejectionRate { get; set; }
}

public class PendingApplicationReportQuery : ApplicationReportQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PendingApplicationReportDto
{
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<PendingApplicationReportItemDto> Items { get; set; } = new();
}

public class PendingApplicationReportItemDto
{
    public int ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? AssignedToName { get; set; }
    public string? CurrentStep { get; set; }
    public decimal? AgeHours { get; set; }
    public int? CampusId { get; set; }
    public string? CampusName { get; set; }
}

public class OverdueApplicationReportQuery : ApplicationReportQueryParameters
{
    public int? SlaHours { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class OverdueApplicationReportDto
{
    public int TotalOverdue { get; set; }
    public int DefaultSlaHours { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<OverdueApplicationReportItemDto> Items { get; set; } = new();
}

public class OverdueApplicationReportItemDto
{
    public int ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string? AssignedToName { get; set; }
    public decimal AgeHours { get; set; }
    public decimal OverdueHours { get; set; }
}

public class ProcessingTimeReportDto
{
    public decimal AverageProcessingHours { get; set; }
    public decimal? MedianProcessingHours { get; set; }
    public decimal? MinProcessingHours { get; set; }
    public decimal? MaxProcessingHours { get; set; }
    public List<ProcessingTimeGroupDto> ByType { get; set; } = new();
    public List<ProcessingTimeGroupDto> ByAssignee { get; set; } = new();
    public List<ProcessingTimeGroupDto> ByCampus { get; set; } = new();
    public List<ProcessingTimeGroupDto> ByMonth { get; set; } = new();
}

public class ProcessingTimeGroupDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
    public decimal AverageProcessingHours { get; set; }
}

public class ApplicationByAssigneeReportDto
{
    public int? AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Completed { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Overdue { get; set; }
    public decimal? AverageProcessingHours { get; set; }
}

public class ApplicationTrendQuery : ApplicationReportQueryParameters
{
    public string Metric { get; set; } = string.Empty; // submitted, approved, rejected, canceled, overdue, completed
    public string GroupBy { get; set; } = "month"; // day, month, semester
}

public class ApplicationTrendReportDto
{
    public string Metric { get; set; } = string.Empty;
    public string GroupBy { get; set; } = string.Empty;
    public List<ApplicationTrendPointDto> Points { get; set; } = new();
}

public class ApplicationTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Value { get; set; }
}

public class ApplicationStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ApplicationGroupCountDto
{
    public string GroupKey { get; set; } = string.Empty;
    public int Count { get; set; }
}

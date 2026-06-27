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

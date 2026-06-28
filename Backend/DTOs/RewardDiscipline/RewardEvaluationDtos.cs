using System.Text.Json.Serialization;

namespace Backend.DTOs.RewardDiscipline;

public class EvaluateRewardCampaignRequest
{
    public bool ForceReevaluate { get; set; }
    public bool IncludeExcluded { get; set; }
    public bool DryRun { get; set; }
    
    // Admin override for testing/special cases
    public string? OverrideCriteriaJson { get; set; }
}

public class RewardEvaluationResultDto
{
    public int MaDotKhenThuong { get; set; }
    public int CandidateCount { get; set; }
    public int ExcludedCount { get; set; }
    public bool IsDryRun { get; set; }
    public string StatusAfterEvaluation { get; set; } = string.Empty;
}

public class RewardCandidateDto
{
    public int MaUngVienKhenThuong { get; set; }
    public int MaDotKhenThuong { get; set; }
    public int MaHocSinh { get; set; }
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    
    public int? XepHang { get; set; }
    public decimal DiemXet { get; set; }
    public decimal? GpaHocKy { get; set; }
    
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDoLoai { get; set; }
    public string? GhiChuDieuChinh { get; set; }
    public int? NguoiDieuChinh { get; set; }
    public DateTime? NgayDieuChinh { get; set; }
    public DateTime NgayTao { get; set; }
}

public class ExcludedRewardCandidateDto
{
    public int MaHocSinh { get; set; }
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    public decimal? DiemXet { get; set; }
    public string? LyDoLoai { get; set; }
    public string? LyDoLoaiJson { get; set; }
}

public class RewardCandidateQueryParameters
{
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ExcludedRewardCandidateQueryParameters
{
    public string? ReasonCode { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RewardApprovalSummaryDto
{
    public int MaDotKhenThuong { get; set; }
    public string TenDot { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int SoLuongToiDa { get; set; }
    public int TotalCandidates { get; set; }
    public int SelectedCount { get; set; }
    public int ExcludedCount { get; set; }
    public int ReserveCount { get; set; }
    public int ApprovedCandidateCount { get; set; }
    public int RewardsCreatedCount { get; set; }
    public bool CanAdjust { get; set; }
    public bool CanApprove { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public class AdjustCandidateRequest
{
    public string? TrangThai { get; set; }
    public int? XepHang { get; set; }
    public decimal? DiemXet { get; set; }
    public string? GhiChuDieuChinh { get; set; }
    public string? LyDoDieuChinh { get; set; }
}

public class ManualAddCandidateRequest
{
    public int MaHocSinh { get; set; }
    public decimal? DiemXet { get; set; }
    public int? XepHang { get; set; }
    public string? LyDoThemBatBuoc { get; set; }
}

public class ReorderCandidatesRequest
{
    public IReadOnlyList<ReorderCandidateItemDto> Items { get; set; } = [];
}

public class ReorderCandidateItemDto
{
    public int CandidateId { get; set; }
    public int XepHang { get; set; }
}

public class ApproveRewardCampaignResultDto
{
    public int CampaignId { get; set; }
    public int RewardsCreatedCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

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

namespace Backend.DTOs.RewardDiscipline;

public class GenerateRewardCertificatesRequest
{
    public bool ForceRegenerate { get; set; }
    public bool OnlyFailed { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? GhiChu { get; set; }
}

public class RegenerateRewardCertificatesRequest
{
    public IReadOnlyList<int>? RewardIds { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class GenerateRewardCertificatesResultDto
{
    public int MaDotKhenThuong { get; set; }
    public int Total { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public IReadOnlyList<RewardCertificateGenerationItemDto> Items { get; set; } = [];
}

public class RewardCertificateGenerationItemDto
{
    public int MaKhenThuong { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UrlPdfBangKhen { get; set; }
    public string? Error { get; set; }
    public string? SkippedReason { get; set; }
}

public class RewardCertificateQueryParameters
{
    public string? TrangThaiPdf { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RewardCertificateListItemDto
{
    public int MaKhenThuong { get; set; }
    public int MaDotKhenThuong { get; set; }
    public int MaHocSinh { get; set; }
    public string? HoTen { get; set; }
    public string? Mssv { get; set; }
    public int MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int? XepHang { get; set; }
    public decimal? DiemXet { get; set; }
    public string? DanhHieu { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? TenMauBangKhen { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string TrangThaiSinhPdf { get; set; } = string.Empty;
    public string? UrlPdfBangKhen { get; set; }
    public DateTime? NgaySinhPdf { get; set; }
    public string? LoiSinhPdf { get; set; }
    public int SoLanSinhPdf { get; set; }
}

public class RewardCertificateDownloadDto
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = "certificate.pdf";
    public string ContentType { get; set; } = "application/pdf";
}

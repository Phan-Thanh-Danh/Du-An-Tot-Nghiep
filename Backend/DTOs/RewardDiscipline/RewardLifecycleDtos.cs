using System.ComponentModel.DataAnnotations;
using Backend.DTOs.Common;

namespace Backend.DTOs.RewardDiscipline;

public class AdminRewardQueryParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public int? MaDotKhenThuong { get; set; }
    public int? MaHocKy { get; set; }
    public int? MaHocSinh { get; set; }
    public string? LoaiKhenThuong { get; set; }
    public string? TrangThai { get; set; }
    public bool? HasCertificate { get; set; }
    public string? Keyword { get; set; }
}

public class AdminRewardListItemDto
{
    public int MaKhenThuong { get; set; }
    public int? MaDotKhenThuong { get; set; }
    public int MaHocSinh { get; set; }
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    public string? TenHocKySnapshot { get; set; }
    public string? DanhHieuSnapshot { get; set; }
    public int? XepHang { get; set; }
    public decimal? DiemXet { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool HasCertificate { get; set; }
    public DateTime? NgaySinhPdf { get; set; }
    public int SoLanSinhPdf { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime? NgayCap { get; set; }
    public int MaDonVi { get; set; }
}

public class AdminRewardDetailDto : AdminRewardListItemDto
{
    public decimal? GpaDatDuoc { get; set; }
    public string LoaiKhenThuong { get; set; } = string.Empty;
    public string UrlChungTu { get; set; } = string.Empty;
    public string? UrlPdfBangKhen { get; set; }
    public string? LoiSinhPdf { get; set; }
    public DateTime CapLuc { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiCap { get; set; }
    public int? NguoiDuyet { get; set; }
    public bool DaHuy { get; set; }
    public string? LyDoHuy { get; set; }
    public int? NguoiHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public string? GhiChuHuy { get; set; }
    public string? GhiChuVongDoi { get; set; }
}

public class CancelRewardRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}

public class RestoreRewardRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}

public class MarkRewardIssuedRequest
{
    public DateTime? IssuedAt { get; set; }
    
    [MaxLength(1000)]
    public string? Note { get; set; }
}

public class RegenerateSingleRewardCertificateRequest
{
    public int? MaMauBangKhen { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}

public class RewardLifecycleResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? NewStatus { get; set; }
}

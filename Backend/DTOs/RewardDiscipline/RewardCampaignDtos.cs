using System.Text.Json;

namespace Backend.DTOs.RewardDiscipline;

public class RewardCampaignQueryParameters
{
    public int? MaHocKy { get; set; }
    public int? MaDonVi { get; set; }
    public string? LoaiDot { get; set; }
    public string? TrangThai { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateTop100RewardCampaignRequest
{
    public int MaHocKy { get; set; }
    public int? MaDonVi { get; set; }
    public string TenDot { get; set; } = string.Empty;
    public int? SoLuongToiDa { get; set; }
    public JsonElement? TieuChiXetJson { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? GhiChu { get; set; }
}

public class UpdateRewardCampaignRequest
{
    public int MaHocKy { get; set; }
    public int? MaDonVi { get; set; }
    public string TenDot { get; set; } = string.Empty;
    public int SoLuongToiDa { get; set; }
    public JsonElement? TieuChiXetJson { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? GhiChu { get; set; }
}

public class CancelRewardCampaignRequest
{
    public string? LyDoHuy { get; set; }
    public string? GhiChu { get; set; }
}

public class RewardCampaignListItemDto
{
    public int MaDotKhenThuong { get; set; }
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string TenDot { get; set; } = string.Empty;
    public string LoaiDot { get; set; } = string.Empty;
    public int SoLuongToiDa { get; set; }
    public int? MaMauBangKhen { get; set; }
    public string? TenMauBangKhen { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int NguoiTao { get; set; }
    public string? TenNguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
}

public class RewardCampaignDetailDto : RewardCampaignListItemDto
{
    public string? TieuChiXetJson { get; set; }
    public int? NguoiDuyet { get; set; }
    public string? TenNguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime? NgayCongBo { get; set; }
    public string? GhiChu { get; set; }
    public RewardCampaignTermDto? HocKy { get; set; }
    public RewardCampaignOrganizationDto? DonVi { get; set; }
    public RewardCampaignCertificateTemplateDto? MauBangKhen { get; set; }
}

public class RewardCampaignTermDto
{
    public int MaHocKy { get; set; }
    public string MaCodeHocKy { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public string NamHoc { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
}

public class RewardCampaignOrganizationDto
{
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string CapDonVi { get; set; } = string.Empty;
}

public class RewardCampaignCertificateTemplateDto
{
    public int MaMauBangKhen { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
}

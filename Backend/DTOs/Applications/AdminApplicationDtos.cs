using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.DTOs.Common;

namespace Backend.DTOs.Applications;

public class AdminApplicationQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
    public int? MaHocSinh { get; set; }
    public int? StudentId { get; set; }
    public int? NguoiDuyetHienTai { get; set; }
    public int? AssigneeId { get; set; }
    public string? LoaiDon { get; set; }
    public string? Type { get; set; }
    public string? TrangThai { get; set; }
    public string? Status { get; set; }
    public string? AssignmentState { get; set; }
    public string? SlaStatus { get; set; }
    public DateTime? TuNgayNop { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? DenNgayNop { get; set; }
    public DateTime? SubmittedTo { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminApplicationAssigneeQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminApplicationReceiveRequest
{
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationRequestSupplementRequest
{
    public string Request { get; set; } = string.Empty;
    public string? InternalNote { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationApproveRequest
{
    public string? PublicNote { get; set; }
    public string? InternalNote { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationRejectRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? InternalNote { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationAssignRequest
{
    public int AssigneeId { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public string? LyDo { get; set; }
    public string? Reason { get; set; }
}

public class AdminApplicationQueueItemDto
{
    public int MaDonTu { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TenLoaiDon { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string TenTrangThai { get; set; } = string.Empty;
    public AdminApplicationPersonDto HocSinh { get; set; } = new();
    public AdminApplicationCampusDto DonVi { get; set; } = new();
    public AdminApplicationPersonDto? NguoiDuyetHienTai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public DateTime? NgayNop { get; set; }
    public DateTime? HanXuLyLuc { get; set; }
    public AdminApplicationSlaDto Sla { get; set; } = new();
    public int AttachmentCount { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationDetailDto : AdminApplicationQueueItemDto
{
    public int? MaMauDon { get; set; }
    public int? PhienBanMau { get; set; }
    public string TrangThaiXuLyNghiepVu { get; set; } = string.Empty;
    public JsonElement DuLieuBieuMau { get; set; }
    public bool DuLieuBieuMauHopLe { get; set; }
    public string? NoiDungYeuCauBoSung { get; set; }
    public string? LyDoTuChoi { get; set; }
    public AdminApplicationPersonDto? NguoiXuLyCuoi { get; set; }
    public IReadOnlyList<AdminApplicationAttachmentDto> Attachments { get; set; } = [];
    public IReadOnlyList<AdminApplicationTimelineDto> Timeline { get; set; } = [];
    public AdminApplicationAllowedActionsDto AllowedActions { get; set; } = new();
}

public class AdminApplicationPersonDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
}

public class AdminApplicationCampusDto
{
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
}

public class AdminApplicationSlaDto
{
    public string Status { get; set; } = "none";
    public int? RemainingMinutes { get; set; }
}

public class AdminApplicationAllowedActionsDto
{
    public bool CanReceive { get; set; }
    public bool CanAssign { get; set; }
    public bool CanReassign { get; set; }
    public bool CanRequestSupplement { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanDownloadEvidence { get; set; }
}

public class AdminApplicationAttachmentDto
{
    public int MaTep { get; set; }
    public string TenFileGoc { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long KichThuocByte { get; set; }
    public DateTime NgayTao { get; set; }
}

public class AdminApplicationTimelineDto
{
    public int MaNkDuyet { get; set; }
    public string NguonThucHien { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string? TrangThaiCu { get; set; }
    public string? TrangThaiMoi { get; set; }
    public string? GhiChuCongKhai { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public AdminApplicationTimelineMetadataDto? Metadata { get; set; }
    public bool HienThiChoHocSinh { get; set; }
    public DateTime NgayTao { get; set; }
    public AdminApplicationPersonDto? NguoiThucHien { get; set; }
}

public class AdminApplicationTimelineMetadataDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Operation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FromAssigneeId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ToAssigneeId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReasonProvided { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? TemplateAssigned { get; set; }

    public IReadOnlyList<string> ChangedFields { get; set; } = [];
    public IReadOnlyList<int> AttachmentIds { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AttachmentId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FileCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Decision { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PreviousAssigneeId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ProcessorId { get; set; }
}

public class AdminApplicationQueueSummaryDto
{
    public int Active { get; set; }
    public int TotalActive { get; set; }
    public int Submitted { get; set; }
    public int InReview { get; set; }
    public int NeedSupplement { get; set; }
    public int WaitingForSupplement { get; set; }
    public int Unassigned { get; set; }
    public int Assigned { get; set; }
    public int AssignedToMe { get; set; }
    public int Overdue { get; set; }
    public int DueSoon { get; set; }
}

public class AdminApplicationAssigneeDto : AdminApplicationPersonDto
{
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int UserId => MaNguoiDung;
    public string FullName => HoTen;
    public string Role => VaiTro;
    public int CampusId => MaDonVi;
    public string CampusName => TenDonVi;
}

public class AdminApplicationQueueResponseDto : PagedResultDto<AdminApplicationQueueItemDto>
{
}

public class AdminApplicationAssigneeResponseDto : PagedResultDto<AdminApplicationAssigneeDto>
{
}

using System.Text.Json;
using Backend.DTOs.Common;

namespace Backend.DTOs.Applications;

public class AdminApplicationQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? MaHocSinh { get; set; }
    public int? NguoiDuyetHienTai { get; set; }
    public string? LoaiDon { get; set; }
    public string? TrangThai { get; set; }
    public string? AssignmentState { get; set; }
    public string? SlaStatus { get; set; }
    public DateTime? TuNgayNop { get; set; }
    public DateTime? DenNgayNop { get; set; }
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminApplicationAssigneeQueryParameters
{
    public int? MaDonVi { get; set; }
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminApplicationReceiveRequest
{
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationAssignRequest
{
    public int AssigneeId { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public string? LyDo { get; set; }
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
    public string? SnapshotJson { get; set; }
    public bool HienThiChoHocSinh { get; set; }
    public DateTime NgayTao { get; set; }
    public AdminApplicationPersonDto? NguoiThucHien { get; set; }
}

public class AdminApplicationQueueSummaryDto
{
    public int TotalActive { get; set; }
    public int Submitted { get; set; }
    public int InReview { get; set; }
    public int NeedSupplement { get; set; }
    public int Unassigned { get; set; }
    public int Overdue { get; set; }
    public int DueSoon { get; set; }
}

public class AdminApplicationAssigneeDto : AdminApplicationPersonDto
{
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
}

public class AdminApplicationQueueResponseDto : PagedResultDto<AdminApplicationQueueItemDto>
{
}

public class AdminApplicationAssigneeResponseDto : PagedResultDto<AdminApplicationAssigneeDto>
{
}

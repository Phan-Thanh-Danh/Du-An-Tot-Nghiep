using System.Text.Json;

namespace Backend.DTOs.Applications;

public class StudentApplicationListItemDto
{
    public int MaDonTu { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TenLoaiDon { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string TenTrangThai { get; set; } = string.Empty;
    public string TrangThaiXuLyNghiepVu { get; set; } = string.Empty;
    public string TenTrangThaiXuLyNghiepVu { get; set; } = string.Empty;
    public int? PhienBanMau { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public DateTime? NgayNop { get; set; }
    public DateTime? HanXuLyLuc { get; set; }
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanResubmit { get; set; }
    public bool CanCancel { get; set; }
}

public class StudentApplicationDetailDto : StudentApplicationListItemDto
{
    public int? MaMauDon { get; set; }
    public JsonElement DuLieuBieuMau { get; set; }
    public string? LyDoTuChoi { get; set; }
    public string? NoiDungYeuCauBoSung { get; set; }
    public StudentApplicationTemplateDto? Template { get; set; }
    public IReadOnlyList<StudentApplicationAttachmentDto> Attachments { get; set; } = [];
    public IReadOnlyList<StudentApplicationTimelineDto> Timeline { get; set; } = [];
    public string RowVersion { get; set; } = string.Empty;
}

public class StudentApplicationTemplateDto
{
    public int MaMauDon { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public int PhienBan { get; set; }
    public string CauHinhJson { get; set; } = string.Empty;
    public bool BatBuocMinhChung { get; set; }
    public int SoTepToiDa { get; set; }
    public long DungLuongTepToiDaByte { get; set; }
    public long TongDungLuongToiDaByte { get; set; }
    public int? SlaGio { get; set; }
}

public class StudentApplicationAttachmentDto
{
    public int MaTep { get; set; }
    public string TenFileGoc { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long KichThuocByte { get; set; }
    public DateTime NgayTao { get; set; }
}

public class StudentApplicationTimelineDto
{
    public string HanhDong { get; set; } = string.Empty;
    public string? TrangThaiCu { get; set; }
    public string? TrangThaiMoi { get; set; }
    public string? GhiChuCongKhai { get; set; }
    public string NguonThucHien { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}

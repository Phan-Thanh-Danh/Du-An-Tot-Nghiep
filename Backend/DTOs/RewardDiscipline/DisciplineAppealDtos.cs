using Backend.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Backend.DTOs.RewardDiscipline;

public class CreateDisciplineAppealRequest
{
    [Required(ErrorMessage = "Vui lòng nhập lý do khiếu nại")]
    [StringLength(2000, MinimumLength = 20, ErrorMessage = "Lý do khiếu nại từ 20 đến 2000 ký tự")]
    public string Reason { get; set; } = string.Empty;

    public JsonElement? EvidenceJson { get; set; }
}

public class DisciplineAppealQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public string? TrangThai { get; set; }
    public string? Keyword { get; set; }
}

public class DisciplineAppealListItemDto
{
    public int MaKhieuNaiKyLuat { get; set; }
    public int MaHoSoKyLuat { get; set; }
    public int MaHocSinh { get; set; }
    public string TenHocSinh { get; set; } = string.Empty;
    public string Mssv { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public string? NguoiXuLyName { get; set; }
}

public class DisciplineAppealDetailDto : DisciplineAppealListItemDto
{
    public string LyDoKhieuNai { get; set; } = string.Empty;
    public JsonElement? EvidenceJson { get; set; }
    public string? LyDoXuLy { get; set; }
    public string? GhiChuXuLy { get; set; }
}

public class ResolveDisciplineAppealRequest
{
    [Required(ErrorMessage = "Vui lòng nhập quyết định xử lý (chap_nhan/tu_choi)")]
    public string Decision { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lý do")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do từ 10 đến 1000 ký tự")]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Ghi chú không quá 2000 ký tự")]
    public string? ResolutionNote { get; set; }

    public bool RemoveEffect { get; set; } = false;
}

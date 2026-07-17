using Backend.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Backend.DTOs.RewardDiscipline;

public class DisciplineRecordQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public int? MaHocSinh { get; set; }
    public string? MucDoKyLuat { get; set; }
    public string? HinhThucXuLy { get; set; }
    public string? TrangThai { get; set; }
    public string? Keyword { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DisciplineRecordListItemDto
{
    public int MaHoSoKyLuat { get; set; }
    public int MaHocSinh { get; set; }
    public string HoTenHocSinh { get; set; } = string.Empty;
    public string Mssv { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public string MucDoKyLuat { get; set; } = string.Empty;
    public string HinhThucXuLy { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public DateOnly NgayViPham { get; set; }
    public DateTime NgayTao { get; set; }
    public int NguoiTao { get; set; }
}

public class DisciplineRecordDetailDto : DisciplineRecordListItemDto
{
    public string MoTaViPham { get; set; } = string.Empty;
    public string? CanCuXuLy { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public JsonElement? EvidenceJson { get; set; }
    public string? LyDoHuy { get; set; }
    public int? NguoiHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
    public int? NguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }
    public string? GhiChuDuyet { get; set; }
    public int? NguoiApDung { get; set; }
    public DateTime? NgayApDung { get; set; }
}

public class CreateDisciplineRecordRequest
{
    [Required]
    public int MaHocSinh { get; set; }

    public int? MaHocKy { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 5)]
    public string TieuDe { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string MoTaViPham { get; set; } = string.Empty;

    [Required]
    public DateOnly NgayViPham { get; set; }

    [Required]
    public string MucDoKyLuat { get; set; } = string.Empty;

    [Required]
    public string HinhThucXuLy { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? CanCuXuLy { get; set; }

    [StringLength(2000)]
    public string? GhiChuNoiBo { get; set; }

    public JsonElement? EvidenceJson { get; set; }
}

public class UpdateDisciplineRecordRequest
{
    [Required]
    [StringLength(255, MinimumLength = 5)]
    public string TieuDe { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string MoTaViPham { get; set; } = string.Empty;

    [Required]
    public DateOnly NgayViPham { get; set; }

    [Required]
    public string MucDoKyLuat { get; set; } = string.Empty;

    [Required]
    public string HinhThucXuLy { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? CanCuXuLy { get; set; }

    [StringLength(2000)]
    public string? GhiChuNoiBo { get; set; }

    public JsonElement? EvidenceJson { get; set; }
}

public class CancelDisciplineRecordRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}

public class DisciplineRecordResultDto
{
    public int MaHoSoKyLuat { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class DisciplineApprovalRequest
{
    [StringLength(1000)]
    public string? DecisionNote { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [StringLength(1000)]
    public string? PublicNote { get; set; }

    [StringLength(2000)]
    public string? InternalNote { get; set; }
}

public class DisciplineRejectRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? InternalNote { get; set; }
}

public class DisciplineActivateRequest
{
    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
}

public class DisciplineExpireRequest
{
    [StringLength(1000)]
    public string? Reason { get; set; }
}

public class RemoveDisciplineEffectRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? RemovalNote { get; set; }

    public DateTime? EffectiveEndAt { get; set; }
}

public class VoidApprovedDisciplineRecordRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? InternalNote { get; set; }
}

public class StudentDisciplineRecordQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? MaHocKy { get; set; }
    public string? TrangThai { get; set; }
}

public class StudentDisciplineRecordListItemDto
{
    public int MaHoSoKyLuat { get; set; }
    public int? MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string MucDoKyLuat { get; set; } = string.Empty;
    public string HinhThucXuLy { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateOnly NgayViPham { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateOnly? NgayBatDauHieuLuc { get; set; }
    public DateOnly? NgayKetThucHieuLuc { get; set; }
    public bool CoTheKhieuNai { get; set; }
    public string? AppealStatus { get; set; }
}

public class StudentDisciplineRecordDetailDto : StudentDisciplineRecordListItemDto
{
    public string MoTaViPham { get; set; } = string.Empty;
    public string? CanCuXuLy { get; set; }
}

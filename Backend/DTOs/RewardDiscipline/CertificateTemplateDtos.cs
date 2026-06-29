using System.Text.Json;

namespace Backend.DTOs.RewardDiscipline;

public class CertificateTemplateQueryParameters
{
    public string? LoaiMau { get; set; }
    public bool? ConHoatDong { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateCertificateTemplateRequest
{
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public string FileNenUrl { get; set; } = string.Empty;
    public int ChieuRong { get; set; }
    public int ChieuCao { get; set; }
    public string HuongGiay { get; set; } = string.Empty;
    public JsonElement? CauHinhJson { get; set; }
}

public class UpdateCertificateTemplateRequest
{
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public string FileNenUrl { get; set; } = string.Empty;
    public int ChieuRong { get; set; }
    public int ChieuCao { get; set; }
    public string HuongGiay { get; set; } = string.Empty;
    public JsonElement? CauHinhJson { get; set; }
}

public class CertificateTemplateDto
{
    public int MaMauBangKhen { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public string FileNenUrl { get; set; } = string.Empty;
    public int ChieuRong { get; set; }
    public int ChieuCao { get; set; }
    public string HuongGiay { get; set; } = string.Empty;
    public string CauHinhJson { get; set; } = string.Empty;
    public bool ConHoatDong { get; set; }
    public int NguoiTao { get; set; }
    public string? TenNguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class CertificateTemplatePreviewRequest
{
    public int? MaKhenThuong { get; set; }
    public JsonElement? DuLieuMau { get; set; }
}

public class CertificateTemplatePreviewDto
{
    public int MaMauBangKhen { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string LoaiMau { get; set; } = string.Empty;
    public string FileNenUrl { get; set; } = string.Empty;
    public int ChieuRong { get; set; }
    public int ChieuCao { get; set; }
    public string HuongGiay { get; set; } = string.Empty;
    public IReadOnlyList<CertificateTemplatePreviewFieldDto> Fields { get; set; } = [];
    public IReadOnlyDictionary<string, string?> Data { get; set; } = new Dictionary<string, string?>();
    public bool IsPdfGenerated { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class CertificateTemplatePreviewFieldDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal FontSize { get; set; }
    public string Align { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Bold { get; set; }
}

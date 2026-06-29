using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Backend.DTOs.Common;

namespace Backend.DTOs.Notification;

public class NotificationTemplateQueryParameters
{
    public string? Keyword { get; set; }
    public string? LoaiThongBao { get; set; }
    public bool? DangHoatDong { get; set; }
    public int? MaDonVi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

public class NotificationTemplateListItemDto
{
    public int MaMauThongBao { get; set; }
    public string? MaMau { get; set; }
    public string? TenMau { get; set; }
    public string? LoaiThongBao { get; set; }
    public string? TieuDeMau { get; set; }
    public bool DangHoatDong { get; set; }
    public bool LaHeThong { get; set; }
    public int? MaDonVi { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class NotificationTemplateDetailDto
{
    public int MaMauThongBao { get; set; }
    public int? MaDonVi { get; set; }
    public string? TenMau { get; set; }
    public string? MaMau { get; set; }
    public string? LoaiThongBao { get; set; }
    public string? TieuDeMau { get; set; }
    public string? NoiDungMau { get; set; }
    public string? KenhThongBao { get; set; }
    public string? MucDoUuTien { get; set; }
    public string? DoiTuongMacDinh { get; set; }
    public string? BienChoPhepJson { get; set; }
    public bool DangHoatDong { get; set; }
    public bool LaHeThong { get; set; }
    public DateTime NgayTao { get; set; }
    public int? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiCapNhat { get; set; }
    public List<string> DetectedPlaceholders { get; set; } = new();
}

public class CreateNotificationTemplateRequest
{
    [Required]
    [MaxLength(100)]
    public string MaMau { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TenMau { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LoaiThongBao { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string TieuDeMau { get; set; } = string.Empty;

    [Required]
    public string NoiDungMau { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MucDoUuTien { get; set; }

    [MaxLength(100)]
    public string? DoiTuongMacDinh { get; set; }

    public string? BienChoPhepJson { get; set; }

    public int? MaDonVi { get; set; }

    public bool DangHoatDong { get; set; } = true;
    
    [MaxLength(20)]
    public string KenhThongBao { get; set; } = "in_app";
}

public class UpdateNotificationTemplateRequest
{
    [Required]
    [MaxLength(100)]
    public string MaMau { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TenMau { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LoaiThongBao { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string TieuDeMau { get; set; } = string.Empty;

    [Required]
    public string NoiDungMau { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MucDoUuTien { get; set; }

    [MaxLength(100)]
    public string? DoiTuongMacDinh { get; set; }

    public string? BienChoPhepJson { get; set; }
    
    [MaxLength(20)]
    public string KenhThongBao { get; set; } = "in_app";
    
    public bool? LaHeThong { get; set; } // Only editable by SuperAdmin
}

public class DeactivateNotificationTemplateRequest
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}

public class PreviewNotificationTemplateRequest
{
    public Dictionary<string, string>? Variables { get; set; }
    public string? SampleRecipientType { get; set; }
    public int? SampleRecipientId { get; set; }
}

public class PreviewNotificationTemplateResultDto
{
    public string? RenderedTitle { get; set; }
    public string? RenderedBody { get; set; }
    public List<string> MissingVariables { get; set; } = new();
    public List<string> UnusedVariables { get; set; } = new();
    public List<string> DetectedPlaceholders { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

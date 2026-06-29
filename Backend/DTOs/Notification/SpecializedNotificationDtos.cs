using System.ComponentModel.DataAnnotations;
using Backend.DTOs.Common;

namespace Backend.DTOs.Notification;

public class SpecializedNotificationTargetDto
{
    [Required]
    public string TargetType { get; set; } = string.Empty;
    public int? MaDonVi { get; set; }
    public int? MaLop { get; set; }
    public int? MaNganh { get; set; }
    public List<int>? StudentIds { get; set; }
    public List<string>? RoleCodes { get; set; }
    public string? Keyword { get; set; }
}

public class PreviewSpecializedRecipientsRequest
{
    [Required]
    public SpecializedNotificationTargetDto Target { get; set; } = null!;
}

public class SpecializedRecipientPreviewItemDto
{
    public int MaNguoiDung { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTroChinh { get; set; } = string.Empty;
    public string? TenDonVi { get; set; }
}

public class PreviewSpecializedRecipientsResultDto
{
    public int TotalRecipients { get; set; }
    public List<SpecializedRecipientPreviewItemDto> SampleRecipients { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> UnsupportedFilters { get; set; } = new();
}

public abstract class BaseSpecializedNotificationRequest
{
    public int? TemplateId { get; set; }
    public string? TemplateCode { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }

    [Required]
    public SpecializedNotificationTargetDto Target { get; set; } = null!;
    
    public Dictionary<string, string>? Variables { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string AuditNote { get; set; } = string.Empty;

    public string? ClientRequestId { get; set; }
}

public class SendTuitionNotificationRequest : BaseSpecializedNotificationRequest
{
    public DateTime? DueDate { get; set; }
    public decimal? Amount { get; set; }
    public string? SemesterName { get; set; }
}

public class SendAcademicNotificationRequest : BaseSpecializedNotificationRequest
{
    public string? AcademicCategory { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class SendUrgentNotificationRequest : BaseSpecializedNotificationRequest
{
    [Required]
    public string Priority { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

public class SendMaintenanceNotificationRequest : BaseSpecializedNotificationRequest
{
    public string? Location { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<string>? AffectedServices { get; set; }
}

public class SpecializedNotificationSendResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SpecializedNotificationCategoryDto
{
    public List<string> Categories { get; set; } = new();
}

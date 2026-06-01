namespace Backend.DTOs.Audit;

public class AuditLogDetailDto : AuditLogListItemDto
{
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? UserAgent { get; set; }
    public string? TraceId { get; set; }
}

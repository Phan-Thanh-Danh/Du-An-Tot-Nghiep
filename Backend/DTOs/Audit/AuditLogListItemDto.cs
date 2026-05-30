namespace Backend.DTOs.Audit;

public class AuditLogListItemDto
{
    public int Id { get; set; }
    public int? MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
}

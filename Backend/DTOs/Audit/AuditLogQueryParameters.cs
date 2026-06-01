using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Audit;

public class AuditLogQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "pageNumber phải lớn hơn 0.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;

    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public int? ChangedBy { get; set; }
    public int? MaDonVi { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Keyword { get; set; }
}

namespace Backend.DTOs.Organizations;

public class OrganizationMetricsDto
{
    public int Users { get; set; }
    public int Classes { get; set; }
    public int Rooms { get; set; }
}

public class OrganizationTreeDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string OrganizationLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public OrganizationMetricsDto Metrics { get; set; } = new();
    public List<OrganizationTreeDto> Children { get; set; } = [];
}


namespace Backend.DTOs.TeacherPersonnel;

public class OrganizationHierarchyNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "organization"; // organization, role, user
    public int? EntityId { get; set; }
    public string? Code { get; set; }
    public string? Status { get; set; }
    public int TotalMembers { get; set; }
    public bool IsManageable { get; set; }
    public List<OrganizationHierarchyNodeDto> Children { get; set; } = [];
}

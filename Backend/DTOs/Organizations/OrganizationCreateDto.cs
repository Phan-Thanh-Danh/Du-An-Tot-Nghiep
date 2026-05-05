using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Organizations;

public class OrganizationCreateDto
{
    public int? ParentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string OrganizationLevel { get; set; } = string.Empty;
}

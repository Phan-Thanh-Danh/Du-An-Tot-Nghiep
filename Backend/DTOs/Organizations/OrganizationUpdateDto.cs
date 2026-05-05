using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Organizations;

public class OrganizationUpdateDto
{
    public int? ParentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string OrganizationLevel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

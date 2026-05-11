using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Organizations;

public class OrganizationUpdateDto
{
    public int? ParentId { get; set; }

    [Required(ErrorMessage = "Tên đơn vị không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên đơn vị không được vượt quá 255 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cấp đơn vị không được để trống.")]
    [MaxLength(20, ErrorMessage = "Cấp đơn vị không được vượt quá 20 ký tự.")]
    public string OrganizationLevel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

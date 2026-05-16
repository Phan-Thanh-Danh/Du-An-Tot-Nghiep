using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Majors;

public class UpdateMajorRequest
{
    [Required(ErrorMessage = "Mã ngành không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã ngành không được vượt quá 50 ký tự.")]
    public string MaCodeNganh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên ngành không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên ngành không được vượt quá 255 ký tự.")]
    public string TenNganh { get; set; } = string.Empty;

    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

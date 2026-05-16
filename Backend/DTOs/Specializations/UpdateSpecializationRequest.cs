using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Specializations;

public class UpdateSpecializationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Ngành đào tạo không hợp lệ.")]
    public int MaNganh { get; set; }

    [Required(ErrorMessage = "Mã chuyên ngành không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã chuyên ngành không được vượt quá 50 ký tự.")]
    public string MaCodeChuyenNganh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên chuyên ngành không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên chuyên ngành không được vượt quá 255 ký tự.")]
    public string TenChuyenNganh { get; set; } = string.Empty;

    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

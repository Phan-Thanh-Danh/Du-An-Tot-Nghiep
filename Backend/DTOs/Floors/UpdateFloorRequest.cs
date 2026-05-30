using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Floors;

public class UpdateFloorRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Tòa nhà không hợp lệ.")]
    public int MaToaNha { get; set; }

    [Required(ErrorMessage = "Tên tầng không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên tầng không được vượt quá 100 ký tự.")]
    public string TenTang { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự tầng không hợp lệ.")]
    public int ThuTuTang { get; set; }

    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

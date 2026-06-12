using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.CaHoc;

public class UpdateCaHocRequest
{
    [Required(ErrorMessage = "Tên ca không được để trống.")]
    [MaxLength(50, ErrorMessage = "Tên ca không được vượt quá 50 ký tự.")]
    public string TenCa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Buổi học không được để trống.")]
    [MaxLength(20, ErrorMessage = "Buổi học không được vượt quá 20 ký tự.")]
    public string Buoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giờ bắt đầu không được để trống.")]
    public string GioBatDau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giờ kết thúc không được để trống.")]
    public string GioKetThuc { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn 0.")]
    public int ThuTu { get; set; }

    public bool ConHoatDong { get; set; } = true;
}

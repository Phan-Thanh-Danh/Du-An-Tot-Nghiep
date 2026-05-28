using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Buildings;

public class UpdateBuildingRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Required(ErrorMessage = "Mã tòa nhà không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã tòa nhà không được vượt quá 50 ký tự.")]
    public string MaCodeToaNha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tòa nhà không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên tòa nhà không được vượt quá 255 ký tự.")]
    public string TenToaNha { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
    public string? DiaChi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số tầng phải lớn hơn 0.")]
    public int? SoTang { get; set; }

    public bool ConHoatDong { get; set; } = true;
}

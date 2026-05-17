using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Cohorts;

public class UpdateCohortRequest
{
    [Required(ErrorMessage = "Mã khóa tuyển sinh không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã khóa tuyển sinh không được vượt quá 50 ký tự.")]
    public string MaCodeKhoa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khóa tuyển sinh không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên khóa tuyển sinh không được vượt quá 255 ký tự.")]
    public string TenKhoa { get; set; } = string.Empty;

    public int NamBatDau { get; set; }
    public int? NamKetThucDuKien { get; set; }
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

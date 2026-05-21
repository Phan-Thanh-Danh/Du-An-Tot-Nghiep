using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingPrograms;

public class CloneTrainingProgramRequest
{
    [Required(ErrorMessage = "Khóa tuyển sinh mới không được để trống.")]
    [Range(1, int.MaxValue, ErrorMessage = "Khóa tuyển sinh mới không hợp lệ.")]
    public int MaKhoaTuyenSinhMoi { get; set; }

    [Required(ErrorMessage = "Mã chương trình không được để trống.")]
    [MaxLength(100, ErrorMessage = "Mã chương trình không được vượt quá 100 ký tự.")]
    public string MaCodeChuongTrinh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên chương trình không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên chương trình không được vượt quá 255 ký tự.")]
    public string TenChuongTrinh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phiên bản không được để trống.")]
    [MaxLength(50, ErrorMessage = "Phiên bản không được vượt quá 50 ký tự.")]
    public string Version { get; set; } = string.Empty;

    public string? GhiChuThayDoi { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
}

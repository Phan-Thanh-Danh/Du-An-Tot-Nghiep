using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingProgramSubjects;

public class CreateTrainingProgramSubjectRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã chương trình phải lớn hơn 0.")]
    public int MaChuongTrinh { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã môn học phải lớn hơn 0.")]
    public int MaMonHoc { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Học kỳ dự kiến phải lớn hơn 0.")]
    public int HocKyDuKien { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số tín chỉ phải lớn hơn 0.")]
    public int SoTinChi { get; set; }

    [Required(ErrorMessage = "Loại môn học không được để trống.")]
    [MaxLength(30, ErrorMessage = "Loại môn học không được vượt quá 30 ký tự.")]
    public string LoaiMonHoc { get; set; } = string.Empty;

    public bool? BatBuoc { get; set; }
    public int? ThuTu { get; set; }
    public string? GhiChu { get; set; }
}

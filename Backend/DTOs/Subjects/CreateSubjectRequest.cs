using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Subjects;

public class CreateSubjectRequest
{
    [Required(ErrorMessage = "Mã môn học không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã môn học không được vượt quá 50 ký tự.")]
    public string MaCodeMonHoc { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên môn học không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên môn học không được vượt quá 255 ký tự.")]
    public string TenMonHoc { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Số tín chỉ phải lớn hơn 0.")]
    public int SoTinChi { get; set; }
}

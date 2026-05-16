using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.CourseSyllabuses;

public class CreateCourseSyllabusRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Môn học không hợp lệ.")]
    public int MaMonHoc { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Chuyên ngành không hợp lệ.")]
    public int MaChuyenNganh { get; set; }

    public int? MaDonVi { get; set; }

    [Required(ErrorMessage = "Tên đề cương không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên đề cương không được vượt quá 255 ký tự.")]
    public string TenSyllabus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phiên bản không được để trống.")]
    [MaxLength(50, ErrorMessage = "Phiên bản không được vượt quá 50 ký tự.")]
    public string Version { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Học kỳ dự kiến phải lớn hơn 0.")]
    public int? HocKyDuKien { get; set; }

    public bool BatBuoc { get; set; } = true;
    public string? TrangThai { get; set; }
}

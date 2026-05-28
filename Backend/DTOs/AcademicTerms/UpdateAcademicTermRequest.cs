using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AcademicTerms;

public class UpdateAcademicTermRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Required(ErrorMessage = "Mã học kỳ không được để trống.")]
    [MaxLength(30, ErrorMessage = "Mã học kỳ không được vượt quá 30 ký tự.")]
    public string MaCodeHocKy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên học kỳ không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên học kỳ không được vượt quá 100 ký tự.")]
    public string TenHocKy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Năm học không được để trống.")]
    [MaxLength(20, ErrorMessage = "Năm học không được vượt quá 20 ký tự.")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Năm học phải có định dạng YYYY-YYYY (vd: 2025-2026).")]
    public string NamHoc { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "Thứ tự trong năm phải là 1 (Spring), 2 (Summer), hoặc 3 (Fall).")]
    public int ThuTuTrongNam { get; set; }

    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    public DateOnly? NgayKetThucBlock5 { get; set; }
    public bool DaKhoa { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số tín chỉ tối đa phải lớn hơn 0.")]
    public int? SoTinChiToiDa { get; set; }

    public DateOnly? HanRutMon { get; set; }
}

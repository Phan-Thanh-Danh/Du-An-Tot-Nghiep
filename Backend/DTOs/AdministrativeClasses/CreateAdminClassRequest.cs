using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AdministrativeClasses;

public class CreateAdminClassRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Required(ErrorMessage = "Mã lớp không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã lớp không được vượt quá 50 ký tự.")]
    public string MaCodeLop { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên lớp không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên lớp không được vượt quá 255 ký tự.")]
    public string TenLop { get; set; } = string.Empty;

    public int? MaGiaoVienChuNhiem { get; set; }

    [Range(1900, 2100, ErrorMessage = "Năm nhập học không hợp lệ.")]
    public int? NamNhapHoc { get; set; }
}

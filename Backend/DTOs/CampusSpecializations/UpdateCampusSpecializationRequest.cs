using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.CampusSpecializations;

public class UpdateCampusSpecializationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Chuyên ngành không hợp lệ.")]
    public int MaChuyenNganh { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Required(ErrorMessage = "Trạng thái không được để trống.")]
    [MaxLength(30, ErrorMessage = "Trạng thái không được vượt quá 30 ký tự.")]
    public string TrangThai { get; set; } = string.Empty;

    [Range(2000, int.MaxValue, ErrorMessage = "Năm bắt đầu phải từ 2000 trở lên.")]
    public int? NamBatDau { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Chỉ tiêu dự kiến phải lớn hơn hoặc bằng 0.")]
    public int? ChiTieuDuKien { get; set; }

    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

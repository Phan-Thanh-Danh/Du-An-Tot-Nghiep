using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingPrograms;

public class UpdateTrainingProgramRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Chuyên ngành không hợp lệ.")]
    public int MaChuyenNganh { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Khóa tuyển sinh không hợp lệ.")]
    public int MaKhoaTuyenSinh { get; set; }

    [Required(ErrorMessage = "Mã chương trình không được để trống.")]
    [MaxLength(100, ErrorMessage = "Mã chương trình không được vượt quá 100 ký tự.")]
    public string MaCodeChuongTrinh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên chương trình không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên chương trình không được vượt quá 255 ký tự.")]
    public string TenChuongTrinh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phiên bản không được để trống.")]
    [MaxLength(50, ErrorMessage = "Phiên bản không được vượt quá 50 ký tự.")]
    public string Version { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Số học kỳ phải lớn hơn 0.")]
    public int SoHocKy { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Thời gian đào tạo phải lớn hơn 0.")]
    public int ThoiGianDaoTaoThang { get; set; }

    public int? TongTinChiYeuCau { get; set; }
    public int? SoTinChiToiThieuMoiKy { get; set; }
    public int? SoTinChiToiDaMoiKy { get; set; }

    [Required(ErrorMessage = "Trạng thái không được để trống.")]
    [MaxLength(30, ErrorMessage = "Trạng thái không được vượt quá 30 ký tự.")]
    public string TrangThai { get; set; } = string.Empty;

    public string? MoTa { get; set; }
    public int? NguonChuongTrinhId { get; set; }
    public string? GhiChuThayDoi { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
    public bool ConHoatDong { get; set; } = true;
}

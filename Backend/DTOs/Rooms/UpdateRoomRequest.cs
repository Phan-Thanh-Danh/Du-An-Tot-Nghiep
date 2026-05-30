using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Rooms;

public class UpdateRoomRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Đơn vị không hợp lệ.")]
    public int MaDonVi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Tòa nhà không hợp lệ.")]
    public int MaToaNha { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Tầng không hợp lệ.")]
    public int MaTang { get; set; }

    [Required(ErrorMessage = "Mã phòng không được để trống.")]
    [MaxLength(50, ErrorMessage = "Mã phòng không được vượt quá 50 ký tự.")]
    public string MaCodePhong { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên phòng không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên phòng không được vượt quá 255 ký tự.")]
    public string TenPhong { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0.")]
    public int SucChua { get; set; }

    [Required(ErrorMessage = "Loại phòng không được để trống.")]
    [MaxLength(30, ErrorMessage = "Loại phòng không được vượt quá 30 ký tự.")]
    public string LoaiPhong { get; set; } = string.Empty;

    [Required(ErrorMessage = "Trạng thái phòng không được để trống.")]
    [MaxLength(20, ErrorMessage = "Trạng thái phòng không được vượt quá 20 ký tự.")]
    public string TrangThaiPhong { get; set; } = string.Empty;

    public string? GhiChu { get; set; }
}

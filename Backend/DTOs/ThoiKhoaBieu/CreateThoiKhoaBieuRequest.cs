using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.ThoiKhoaBieu;

public class CreateThoiKhoaBieuRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã khóa học phải lớn hơn 0.")]
    public int MaKhoaHoc { get; set; }

    [Range(1, 7, ErrorMessage = "Thứ trong tuần phải từ 1 đến 7.")]
    public int ThuTrongTuan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã ca học phải lớn hơn 0.")]
    public int MaCaHoc { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã phòng phải lớn hơn 0.")]
    public int MaPhong { get; set; }

    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }

    [MaxLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
    public string? TrangThai { get; set; }

    public int? MaLopDangThaoTac { get; set; }
}

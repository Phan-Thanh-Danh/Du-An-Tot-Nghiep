using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.ThoiKhoaBieu;

public class CheckScheduleConflictRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã khóa học phải lớn hơn 0.")]
    public int MaKhoaHoc { get; set; }

    [Range(1, 7, ErrorMessage = "Thứ trong tuần phải từ 1 đến 7.")]
    public int ThuTrongTuan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã ca học phải lớn hơn 0.")]
    public int MaCaHoc { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã phòng phải lớn hơn 0.")]
    public int MaPhong { get; set; }

    public int? ExcludeMaTkb { get; set; }
}

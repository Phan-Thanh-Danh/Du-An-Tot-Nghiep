using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Attendance;

public class BulkUpdateAttendanceItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã sinh viên phải lớn hơn 0.")]
    public int MaSinhVien { get; set; }

    [Required(ErrorMessage = "Trạng thái điểm danh không được để trống.")]
    public string TrangThai { get; set; } = string.Empty;
}

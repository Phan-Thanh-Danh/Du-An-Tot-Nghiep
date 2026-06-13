using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Attendance;

public class UpdateAttendanceRequest
{
    [Required(ErrorMessage = "Trạng thái điểm danh không được để trống.")]
    public string TrangThai { get; set; } = string.Empty;
}

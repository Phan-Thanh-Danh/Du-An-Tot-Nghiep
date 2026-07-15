using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Attendance;

public class StudentAttendanceQueryParameters
{
    public DateOnly? NgayTu { get; set; }
    public DateOnly? NgayDen { get; set; }
    public int? MaKhoaHoc { get; set; }
    public string? TrangThai { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 1000, ErrorMessage = "pageSize phải từ 1 đến 1000.")]
    public int PageSize { get; set; } = 20;
}

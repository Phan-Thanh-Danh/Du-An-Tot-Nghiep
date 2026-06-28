using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Notifications;

public class AdminNotificationQueryParameters
{
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
    public string? LoaiThongBao { get; set; }
    public string? MucDo { get; set; }
    public string? TrangThai { get; set; }
    public int? NguoiTao { get; set; }
    public string? Keyword { get; set; }
    public DateOnly? NgayTu { get; set; }
    public DateOnly? NgayDen { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

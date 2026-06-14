using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Notifications;

public class NotificationQueryParameters
{
    public bool? DaDoc { get; set; }
    public string? LoaiThongBao { get; set; }
    public string? MucDo { get; set; }
    public DateOnly? NgayTu { get; set; }
    public DateOnly? NgayDen { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}

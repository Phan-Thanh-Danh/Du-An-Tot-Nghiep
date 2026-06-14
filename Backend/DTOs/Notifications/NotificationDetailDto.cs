namespace Backend.DTOs.Notifications;

public class NotificationDetailDto : NotificationDto
{
    public string NoiDung { get; set; } = string.Empty;
    public string? NoiDungJson { get; set; }
    public string? NoiDungText { get; set; }
}

namespace Backend.DTOs.Notifications;

public class NotificationRecipientPreviewResultDto
{
    public int Count { get; set; }
    public List<NotificationRecipientPreviewDto> Recipients { get; set; } = [];
}

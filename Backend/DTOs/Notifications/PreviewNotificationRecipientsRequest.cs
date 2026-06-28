namespace Backend.DTOs.Notifications;

public class PreviewNotificationRecipientsRequest
{
    public string? PhamViGui { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public List<int> TargetIds { get; set; } = [];
    public List<string> RoleCodes { get; set; } = [];
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
}

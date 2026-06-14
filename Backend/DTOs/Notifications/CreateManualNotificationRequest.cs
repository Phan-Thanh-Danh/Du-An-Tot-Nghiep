namespace Backend.DTOs.Notifications;

public class CreateManualNotificationRequest
{
    public string TieuDe { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string? NoiDungJson { get; set; }
    public string? NoiDungText { get; set; }
    public string MucDo { get; set; } = "info";
    public string TargetType { get; set; } = string.Empty;
    public List<int> TargetIds { get; set; } = [];
}

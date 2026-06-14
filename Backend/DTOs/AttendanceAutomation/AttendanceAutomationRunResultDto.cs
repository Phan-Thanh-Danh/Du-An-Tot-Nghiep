namespace Backend.DTOs.AttendanceAutomation;

public class AttendanceAutomationRunResultDto
{
    public int AutoSubmitted { get; set; }
    public int AutoLocked { get; set; }
    public DateTime ProcessedAt { get; set; }
    public IReadOnlyList<AttendanceAutomationItemDto> Items { get; set; } = [];
}

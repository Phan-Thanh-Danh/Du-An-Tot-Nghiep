namespace Backend.DTOs.SmartTimetable;

public class TimetablePublishResultDto
{
    public bool Success { get; set; }
    public int BuoiHocDaTao { get; set; }
    public int BuoiHocLoi { get; set; }
    public List<string> ChiTietLoi { get; set; } = new();
}

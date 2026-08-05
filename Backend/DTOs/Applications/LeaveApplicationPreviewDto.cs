using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Applications;

public class LeaveApplicationPreviewRequestDto
{
    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }
}

public class LeaveApplicationPreviewResponseDto
{
    public int TotalSessions { get; set; }
    public List<LeaveApplicationSessionDto> Sessions { get; set; } = [];
}

public class LeaveApplicationSessionDto
{
    public DateOnly Date { get; set; }
    public string Weekday { get; set; } = string.Empty;
    public string Shift { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Lecturer { get; set; } = string.Empty;
}

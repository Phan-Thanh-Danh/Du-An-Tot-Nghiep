namespace Backend.DTOs.Attendance;

public class BulkUpdateAttendanceRequest
{
    public IReadOnlyList<BulkUpdateAttendanceItemRequest> Items { get; set; } = [];
}

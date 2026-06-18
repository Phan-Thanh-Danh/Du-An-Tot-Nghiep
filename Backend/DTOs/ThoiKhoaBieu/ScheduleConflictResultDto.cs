namespace Backend.DTOs.ThoiKhoaBieu;

public class ScheduleConflictResultDto
{
    public bool HasConflict => Conflicts.Count > 0;
    public IReadOnlyList<ScheduleConflictDto> Conflicts { get; set; } = [];
}

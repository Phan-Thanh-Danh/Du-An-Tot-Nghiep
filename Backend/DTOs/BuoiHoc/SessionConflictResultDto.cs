namespace Backend.DTOs.BuoiHoc;

public class SessionConflictResultDto
{
    public bool HasConflict => Conflicts.Count > 0;
    public IReadOnlyList<SessionConflictDto> Conflicts { get; set; } = [];
}

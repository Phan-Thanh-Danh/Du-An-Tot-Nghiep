namespace Backend.DTOs.BuoiHoc;

public class GenerateSessionsResultDto
{
    public int MaTkb { get; set; }
    public int TotalDates { get; set; }
    public int Created { get; set; }
    public int SkippedExisting { get; set; }
    public IReadOnlyList<BuoiHocDto> Sessions { get; set; } = [];
}

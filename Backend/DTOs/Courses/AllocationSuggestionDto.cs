namespace Backend.DTOs.Courses;

public class AllocationSuggestionDto
{
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int MucDoPhuHop { get; set; }
    public bool LaMonChinh { get; set; }
    public int SoLanDaDay { get; set; }
    public int CurrentWorkload { get; set; }
    public int ProjectedWorkload { get; set; }
    public double Score { get; set; }
    public bool IsOverloaded { get; set; }
}

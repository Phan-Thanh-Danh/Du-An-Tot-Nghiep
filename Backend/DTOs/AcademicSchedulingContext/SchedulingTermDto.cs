namespace Backend.DTOs.AcademicSchedulingContext;

public class SchedulingTermDto
{
    public int MaHocKy { get; set; }
    public string MaCodeHocKy { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public int? DaysUntilStart { get; set; }
}

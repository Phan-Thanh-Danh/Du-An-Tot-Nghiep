namespace Backend.DTOs.Applications;

public class StudentApplicationQueryParameters
{
    public string? TrangThai { get; set; }
    public string? LoaiDon { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

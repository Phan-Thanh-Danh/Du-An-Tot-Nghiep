namespace Backend.DTOs.SmartTimetable;

public class ConflictCheckBatchResultDto
{
    public List<ConflictCheckResultItem> Results { get; set; } = new();
}

public class ConflictCheckResultItem
{
    public int MaKhoaHoc { get; set; }
    public bool HasConflict { get; set; }
    public List<string> Conflicts { get; set; } = new();
    public int? ThuTrongTuan { get; set; }
    public int? MaCaHoc { get; set; }
    public int? MaPhong { get; set; }
}

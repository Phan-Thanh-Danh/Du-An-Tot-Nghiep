namespace Backend.Models;

public class ScheduleDraftItem
{
    public int MaDraftItem { get; set; }
    public int MaJob { get; set; }
    public int MaKhoaHoc { get; set; }
    public int? ThuTrongTuan { get; set; }
    public int? MaCaHoc { get; set; }
    public int? MaPhong { get; set; }
    public string TrangThai { get; set; } = "pending";
    public double? Score { get; set; }
    public string? CanhBaoJson { get; set; }
    public string? LoiJson { get; set; }
    public string? ScoreBreakdownJson { get; set; }
    public string? LyDoGoiYJson { get; set; }

    public ScheduleGenerationJob? Job { get; set; }
    public KhoaHoc? KhoaHoc { get; set; }
    public CaHoc? CaHoc { get; set; }
    public PhongHoc? Phong { get; set; }
}

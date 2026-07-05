namespace Backend.DTOs.SmartTimetable;

public class ScheduleDraftDto
{
    public int MaJob { get; set; }
    public Guid DraftId { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    public string TrangThai { get; set; } = "draft";
    public int? TongCourse { get; set; }
    public int? SoXepDuoc { get; set; }
    public int? SoKhongXepDuoc { get; set; }
    public double? Score { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayXuatBan { get; set; }
    public List<ScheduleDraftItemDto> Items { get; set; } = new();
}

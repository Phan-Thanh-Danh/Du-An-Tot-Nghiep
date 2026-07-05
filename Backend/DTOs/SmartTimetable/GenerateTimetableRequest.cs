namespace Backend.DTOs.SmartTimetable;

public class GenerateTimetableRequest
{
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public int? TongTheHe { get; set; } = 100;
    public double? TyLeCheo { get; set; } = 0.5;
    public int? DoTuoiThoToiDa { get; set; } = 10;
    public int? KichThuocQuanThe { get; set; } = 50;
    public List<int>? MaKhoaHocFilter { get; set; }
}

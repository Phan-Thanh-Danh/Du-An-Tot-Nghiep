namespace Backend.DTOs.SmartTimetable;

public class ConflictCheckBatchRequest
{
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public List<ConflictCheckItem> Items { get; set; } = new();
}

public class ConflictCheckItem
{
    public int MaKhoaHoc { get; set; }
    public int? ThuTrongTuan { get; set; }
    public int? MaCaHoc { get; set; }
    public int? MaPhong { get; set; }
}

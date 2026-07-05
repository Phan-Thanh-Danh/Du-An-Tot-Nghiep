namespace Backend.Models;

public class ScheduleGenerationJob
{
    public int MaJob { get; set; }
    public Guid DraftId { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    public int NguoiYeuCau { get; set; }
    public string TrangThai { get; set; } = "draft";
    public int? TongCourse { get; set; }
    public int? SoXepDuoc { get; set; }
    public int? SoKhongXepDuoc { get; set; }
    public double? Score { get; set; }
    public string? TomTatJson { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayXuatBan { get; set; }

    public DonVi? DonVi { get; set; }
    public HocKy? HocKy { get; set; }
    public NguoiDung? NguoiYeuCauNavigation { get; set; }
}

namespace Backend.DTOs.Notifications;

public class SystemNotificationRequest
{
    public string TieuDe { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string? NoiDungText { get; set; }
    public string? NoiDungJson { get; set; }
    public string LoaiThongBao { get; set; } = "system";
    public string MucDo { get; set; } = "info";
    public string? DoiTuongLienKet { get; set; }
    public int? MaDoiTuongLienKet { get; set; }
    public string? LoaiSuKien { get; set; }
    public string? DuongDan { get; set; }
    public int MaDonVi { get; set; }
    public int? NguoiTao { get; set; }
}

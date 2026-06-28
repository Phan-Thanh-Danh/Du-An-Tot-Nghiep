namespace Backend.DTOs.Notifications;

public class CreateManualNotificationRequest
{
    public string TieuDe { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string? TomTatNoiDung { get; set; }
    public string? NoiDungJson { get; set; }
    public string? NoiDungText { get; set; }
    public string MucDo { get; set; } = "info";
    public string LoaiThongBao { get; set; } = "manual";
    public string? PhamViGui { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public List<int> TargetIds { get; set; } = [];
    public List<string> RoleCodes { get; set; } = [];
    public int? MaDonVi { get; set; }
    public int? CampusId { get; set; }
    public string? LoaiDoiTuongLienKet { get; set; }
    public string? DoiTuongLienKet { get; set; }
    public int? MaDoiTuongLienKet { get; set; }
    public string? DuongDan { get; set; }
}

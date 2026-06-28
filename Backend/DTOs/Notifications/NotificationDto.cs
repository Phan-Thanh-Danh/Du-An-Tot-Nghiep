namespace Backend.DTOs.Notifications;

public class NotificationDto
{
    public int MaThongBao { get; set; }
    public Guid MaNhomThongBao { get; set; }
    public string? TieuDe { get; set; }
    public string? TomTat { get; set; }
    public string? TomTatNoiDung { get; set; }
    public string LoaiThongBao { get; set; } = string.Empty;
    public string MucDo { get; set; } = string.Empty;
    public string? DoiTuongLienKet { get; set; }
    public string? LoaiDoiTuongLienKet { get; set; }
    public int? MaDoiTuongLienKet { get; set; }
    public string? DuongDan { get; set; }
    public bool DaDoc { get; set; }
    public DateTime? DocLuc { get; set; }
    public bool DaAn { get; set; }
    public DateTime? AnLuc { get; set; }
    public DateTime NhanLuc { get; set; }
    public DateTime NgayTao { get; set; }
}

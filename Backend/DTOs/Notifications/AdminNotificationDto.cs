namespace Backend.DTOs.Notifications;

public class AdminNotificationDto
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
    public string PhamViGui { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public string? DuongDan { get; set; }
    public int RecipientCount { get; set; }
    public int ReadCount { get; set; }
    public int UnreadCount { get; set; }
    public int HiddenCount { get; set; }
    public int? NguoiTao { get; set; }
    public string? TenNguoiTao { get; set; }
    public DateTime? GuiLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

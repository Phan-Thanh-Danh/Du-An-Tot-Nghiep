namespace Backend.DTOs.Notifications;

public class AdminNotificationRecipientDto
{
    public int MaThongBaoNguoiNhan { get; set; }
    public int MaThongBao { get; set; }
    public int MaNguoiNhan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTroChinh { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
    public bool DaDoc { get; set; }
    public DateTime? DocLuc { get; set; }
    public bool DaAn { get; set; }
    public DateTime? AnLuc { get; set; }
    public DateTime NhanLuc { get; set; }
}

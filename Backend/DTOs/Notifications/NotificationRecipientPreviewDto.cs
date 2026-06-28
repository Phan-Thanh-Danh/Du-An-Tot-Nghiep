namespace Backend.DTOs.Notifications;

public class NotificationRecipientPreviewDto
{
    public int MaNguoiNhan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTroChinh { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string? TenDonVi { get; set; }
}

namespace Backend.Services;

public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string otp, CancellationToken cancellationToken = default);
    Task SendCampusSpecializationApprovedAsync(
        string toEmail,
        CampusSpecializationDecisionEmailData data,
        CancellationToken cancellationToken = default);

    Task SendCampusSpecializationRejectedAsync(
        string toEmail,
        CampusSpecializationDecisionEmailData data,
        CancellationToken cancellationToken = default);
}

public class CampusSpecializationDecisionEmailData
{
    public string TenNguoiNhan { get; set; } = string.Empty;
    public int MaDeXuat { get; set; }
    public string TenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public string TenCoSo { get; set; } = string.Empty;
    public string TrangThaiMoi { get; set; } = string.Empty;
    public int? NamBatDau { get; set; }
    public int? ChiTieuDuKien { get; set; }
    public string TenNguoiXuLy { get; set; } = string.Empty;
    public string ThoiGianXuLy { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}

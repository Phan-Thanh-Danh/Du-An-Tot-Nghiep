using System.Net;
using System.Net.Mail;
using Backend.Constants;
using Backend.Exceptions;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string otp, CancellationToken cancellationToken = default)
    {
        ValidateSettings();

        try
        {
            await SendHtmlEmailAsync(
                toEmail,
                "Mã OTP quên mật khẩu",
                BuildPasswordResetOtpBody(otp),
                cancellationToken);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException)
        {
            _logger.LogError(exception, "Failed to send password reset OTP email to {Email}", toEmail);
            throw new ApiException(StatusCodes.Status500InternalServerError, "Không thể gửi email OTP. Vui lòng thử lại sau.");
        }
    }

    public async Task SendCampusSpecializationApprovedAsync(
        string toEmail,
        CampusSpecializationDecisionEmailData data,
        CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        await SendHtmlEmailAsync(
            toEmail,
            "Thông báo duyệt đề xuất mở chuyên ngành",
            BuildCampusSpecializationApprovedBody(data),
            cancellationToken);
    }

    public async Task SendCampusSpecializationRejectedAsync(
        string toEmail,
        CampusSpecializationDecisionEmailData data,
        CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        await SendHtmlEmailAsync(
            toEmail,
            "Thông báo từ chối đề xuất mở chuyên ngành",
            BuildCampusSpecializationRejectedBody(data),
            cancellationToken);
    }

    private async Task SendHtmlEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host)
            || _settings.Port <= 0
            || string.IsNullOrWhiteSpace(_settings.UserName)
            || string.IsNullOrWhiteSpace(_settings.Password)
            || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình SMTP chưa đầy đủ.");
        }
    }

    private string BuildPasswordResetOtpBody(string otp)
    {
        var appName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(_settings.FromName)
            ? "LMS System"
            : _settings.FromName);
        var supportEmail = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(_settings.SupportEmail)
            ? _settings.FromEmail
            : _settings.SupportEmail);
        var encodedOtp = WebUtility.HtmlEncode(otp);

        return $$"""
               <!DOCTYPE html>
               <html lang="vi">
               <head>
                 <meta charset="UTF-8" />
                 <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                 <title>Mã xác thực đặt lại mật khẩu</title>
               </head>

               <body style="margin:0; padding:0; background-color:#f5f5f5; font-family:Arial, Helvetica, sans-serif; color:#222222;">
                 <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; padding:32px 0;">
                   <tr>
                     <td align="center">
                       <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border:1px solid #dddddd;">
                         <tr>
                           <td style="padding:24px 36px; border-bottom:3px solid #1f3a5f;">
                             <h1 style="margin:0; color:#1f3a5f; font-size:22px; font-weight:700;">
                               {{appName}}
                             </h1>
                             <p style="margin:6px 0 0; color:#666666; font-size:14px;">
                               Hệ thống quản lý học tập
                             </p>
                           </td>
                         </tr>

                         <tr>
                           <td style="padding:32px 36px;">
                             <h2 style="margin:0 0 18px; color:#222222; font-size:20px; font-weight:600;">
                               Xác nhận yêu cầu đặt lại mật khẩu
                             </h2>

                             <p style="margin:0 0 16px; color:#444444; font-size:15px; line-height:1.6;">
                               Xin chào,
                             </p>

                             <p style="margin:0 0 20px; color:#444444; font-size:15px; line-height:1.6;">
                               Hệ thống đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                               Vui lòng sử dụng mã xác thực bên dưới để tiếp tục.
                             </p>

                             <table width="100%" cellpadding="0" cellspacing="0" style="margin:28px 0;">
                               <tr>
                                 <td align="center">
                                   <table cellpadding="0" cellspacing="0" style="border:1px solid #c9d3df; background-color:#f8fafc;">
                                     <tr>
                                       <td style="padding:18px 32px;">
                                         <span style="font-size:30px; letter-spacing:6px; font-weight:700; color:#1f3a5f;">
                                           {{encodedOtp}}
                                         </span>
                                       </td>
                                     </tr>
                                   </table>
                                 </td>
                               </tr>
                             </table>

                             <p style="margin:0 0 16px; color:#444444; font-size:15px; line-height:1.6;">
                               Mã xác thực này có hiệu lực trong
                               <strong>{{OtpConstants.ExpirationMinutes}} phút</strong>.
                               Không chia sẻ mã này với bất kỳ ai.
                             </p>

                             <p style="margin:0 0 20px; color:#444444; font-size:15px; line-height:1.6;">
                               Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.
                               Mật khẩu hiện tại của bạn sẽ không bị thay đổi.
                             </p>

                             <div style="margin:26px 0; border-top:1px solid #e5e5e5;"></div>

                             <p style="margin:0; color:#444444; font-size:15px; line-height:1.6;">
                               Trân trọng,<br />
                               <strong>Ban quản trị {{appName}}</strong>
                             </p>
                           </td>
                         </tr>

                         <tr>
                           <td style="padding:20px 36px; background-color:#f7f7f7; border-top:1px solid #dddddd;">
                             <p style="margin:0 0 6px; color:#777777; font-size:13px; line-height:1.5;">
                               Đây là email được gửi tự động từ hệ thống. Vui lòng không trả lời email này.
                             </p>
                             <p style="margin:0; color:#777777; font-size:13px; line-height:1.5;">
                               Nếu cần hỗ trợ, vui lòng liên hệ: {{supportEmail}}
                             </p>
                           </td>
                         </tr>
                       </table>
                     </td>
                   </tr>
                 </table>
               </body>
               </html>
               """;
    }

    private string BuildCampusSpecializationApprovedBody(CampusSpecializationDecisionEmailData data)
    {
        var systemName = EncodeSystemName();
        var recipientName = EncodeText(data.TenNguoiNhan);
        var specializationName = EncodeText(data.TenChuyenNganh);
        var campusName = EncodeText(data.TenCoSo);
        var status = EncodeText(data.TrangThaiMoi);
        var proposalId = EncodeText(data.MaDeXuat.ToString());
        var majorName = EncodeText(data.TenNganh);
        var startYear = EncodeNullableNumber(data.NamBatDau);
        var expectedQuota = EncodeNullableNumber(data.ChiTieuDuKien);
        var approverName = EncodeText(data.TenNguoiXuLy);
        var approvedAt = EncodeText(data.ThoiGianXuLy);
        var note = EncodeText(string.IsNullOrWhiteSpace(data.GhiChu) ? "Không có ghi chú." : data.GhiChu);

        return $$"""
               <!DOCTYPE html>
               <html lang="vi">
               <head>
                 <meta charset="UTF-8" />
                 <title>Thông báo duyệt đề xuất mở chuyên ngành</title>
               </head>
               <body style="margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#1f2937;">
                 <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f8; padding:24px 0;">
                   <tr>
                     <td align="center">
                       <table width="640" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);">
                         <tr>
                           <td style="background-color:#16a34a; padding:24px 32px; color:#ffffff;">
                             <h1 style="margin:0; font-size:22px; font-weight:700;">Đề xuất mở chuyên ngành đã được duyệt</h1>
                             <p style="margin:8px 0 0; font-size:14px; opacity:0.95;">Thông báo từ {{systemName}}</p>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:32px;">
                             <p style="margin:0 0 16px; font-size:15px; line-height:1.6;">Kính gửi <strong>{{recipientName}}</strong>,</p>
                             <p style="margin:0 0 20px; font-size:15px; line-height:1.6;">
                               Đề xuất mở chuyên ngành <strong>{{specializationName}}</strong> tại
                               <strong>{{campusName}}</strong> đã được
                               <span style="color:#16a34a; font-weight:700;">DUYỆT</span>.
                             </p>
                             <table width="100%" cellpadding="0" cellspacing="0" style="margin:20px 0; border-radius:10px; background-color:#ecfdf5; border:1px solid #bbf7d0;">
                               <tr>
                                 <td style="padding:16px 20px;">
                                   <p style="margin:0; font-size:14px; color:#166534; font-weight:700;">Trạng thái mới: {{status}}</p>
                                 </td>
                               </tr>
                             </table>
                             <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse; margin-top:20px;">
                               <tr>
                                 <td colspan="2" style="padding:12px 0; font-size:16px; font-weight:700; color:#111827; border-bottom:1px solid #e5e7eb;">Thông tin đề xuất</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280; width:40%;">Mã đề xuất</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{proposalId}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Ngành đào tạo</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{majorName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Chuyên ngành</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{specializationName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Cơ sở đề xuất</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{campusName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Năm bắt đầu dự kiến</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{startYear}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Chỉ tiêu dự kiến</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{expectedQuota}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Người duyệt</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{approverName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Thời gian duyệt</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{approvedAt}}</td>
                               </tr>
                             </table>
                             <div style="margin-top:24px; padding:18px 20px; background-color:#f9fafb; border-left:4px solid #16a34a; border-radius:8px;">
                               <p style="margin:0 0 8px; font-size:14px; font-weight:700; color:#111827;">Ghi chú từ người duyệt</p>
                               <p style="margin:0; font-size:14px; line-height:1.6; color:#374151;">{{note}}</p>
                             </div>
                             <p style="margin:24px 0 0; font-size:15px; line-height:1.6;">
                               Cơ sở có thể tiếp tục các bước chuẩn bị vận hành như cấu hình chương trình học, môn học theo chuyên ngành, lớp hành chính, lớp học phần và kế hoạch đào tạo theo phạm vi được phê duyệt.
                             </p>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:20px 32px; background-color:#f9fafb; border-top:1px solid #e5e7eb;">
                             <p style="margin:0; font-size:13px; color:#6b7280; line-height:1.5;">Trân trọng,<br /><strong>{{systemName}}</strong></p>
                           </td>
                         </tr>
                       </table>
                     </td>
                   </tr>
                 </table>
               </body>
               </html>
               """;
    }

    private string BuildCampusSpecializationRejectedBody(CampusSpecializationDecisionEmailData data)
    {
        var systemName = EncodeSystemName();
        var recipientName = EncodeText(data.TenNguoiNhan);
        var specializationName = EncodeText(data.TenChuyenNganh);
        var campusName = EncodeText(data.TenCoSo);
        var status = EncodeText(data.TrangThaiMoi);
        var proposalId = EncodeText(data.MaDeXuat.ToString());
        var majorName = EncodeText(data.TenNganh);
        var startYear = EncodeNullableNumber(data.NamBatDau);
        var expectedQuota = EncodeNullableNumber(data.ChiTieuDuKien);
        var handlerName = EncodeText(data.TenNguoiXuLy);
        var handledAt = EncodeText(data.ThoiGianXuLy);
        var rejectReason = EncodeText(string.IsNullOrWhiteSpace(data.GhiChu) ? "Không có lý do từ chối." : data.GhiChu);

        return $$"""
               <!DOCTYPE html>
               <html lang="vi">
               <head>
                 <meta charset="UTF-8" />
                 <title>Thông báo từ chối đề xuất mở chuyên ngành</title>
               </head>
               <body style="margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#1f2937;">
                 <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f8; padding:24px 0;">
                   <tr>
                     <td align="center">
                       <table width="640" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);">
                         <tr>
                           <td style="background-color:#dc2626; padding:24px 32px; color:#ffffff;">
                             <h1 style="margin:0; font-size:22px; font-weight:700;">Đề xuất mở chuyên ngành chưa được duyệt</h1>
                             <p style="margin:8px 0 0; font-size:14px; opacity:0.95;">Thông báo từ {{systemName}}</p>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:32px;">
                             <p style="margin:0 0 16px; font-size:15px; line-height:1.6;">Kính gửi <strong>{{recipientName}}</strong>,</p>
                             <p style="margin:0 0 20px; font-size:15px; line-height:1.6;">
                               Đề xuất mở chuyên ngành <strong>{{specializationName}}</strong> tại
                               <strong>{{campusName}}</strong> đã
                               <span style="color:#dc2626; font-weight:700;">KHÔNG ĐƯỢC DUYỆT</span> tại thời điểm này.
                             </p>
                             <table width="100%" cellpadding="0" cellspacing="0" style="margin:20px 0; border-radius:10px; background-color:#fef2f2; border:1px solid #fecaca;">
                               <tr>
                                 <td style="padding:16px 20px;">
                                   <p style="margin:0; font-size:14px; color:#991b1b; font-weight:700;">Trạng thái mới: {{status}}</p>
                                 </td>
                               </tr>
                             </table>
                             <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse; margin-top:20px;">
                               <tr>
                                 <td colspan="2" style="padding:12px 0; font-size:16px; font-weight:700; color:#111827; border-bottom:1px solid #e5e7eb;">Thông tin đề xuất</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280; width:40%;">Mã đề xuất</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{proposalId}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Ngành đào tạo</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{majorName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Chuyên ngành</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{specializationName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Cơ sở đề xuất</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{campusName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Năm bắt đầu dự kiến</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{startYear}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Chỉ tiêu dự kiến</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{expectedQuota}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Người xử lý</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{handlerName}}</td>
                               </tr>
                               <tr>
                                 <td style="padding:12px 0; font-size:14px; color:#6b7280;">Thời gian xử lý</td>
                                 <td style="padding:12px 0; font-size:14px; font-weight:600;">{{handledAt}}</td>
                               </tr>
                             </table>
                             <div style="margin-top:24px; padding:18px 20px; background-color:#fef2f2; border-left:4px solid #dc2626; border-radius:8px;">
                               <p style="margin:0 0 8px; font-size:14px; font-weight:700; color:#991b1b;">Lý do từ chối</p>
                               <p style="margin:0; font-size:14px; line-height:1.6; color:#374151;">{{rejectReason}}</p>
                             </div>
                             <p style="margin:24px 0 0; font-size:15px; line-height:1.6;">
                               Đơn vị có thể rà soát lại các điều kiện cần thiết như năng lực giảng viên, phòng lab/cơ sở vật chất, nhu cầu tuyển sinh, doanh nghiệp OJT hoặc các yêu cầu học vụ liên quan trước khi gửi lại đề xuất mới.
                             </p>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:20px 32px; background-color:#f9fafb; border-top:1px solid #e5e7eb;">
                             <p style="margin:0; font-size:13px; color:#6b7280; line-height:1.5;">Trân trọng,<br /><strong>{{systemName}}</strong></p>
                           </td>
                         </tr>
                       </table>
                     </td>
                   </tr>
                 </table>
               </body>
               </html>
               """;
    }

    private string EncodeSystemName()
    {
        return EncodeText(string.IsNullOrWhiteSpace(_settings.FromName) ? "LMS System" : _settings.FromName);
    }

    private static string EncodeNullableNumber(int? value)
    {
        return EncodeText(value?.ToString() ?? "Chưa cập nhật");
    }

    private static string EncodeText(string? value)
    {
        return WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value);
    }
}

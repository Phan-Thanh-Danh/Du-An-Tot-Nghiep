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

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = "Mã OTP quên mật khẩu",
            Body = BuildPasswordResetOtpBody(otp),
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException)
        {
            _logger.LogError(exception, "Failed to send password reset OTP email to {Email}", toEmail);
            throw new ApiException(StatusCodes.Status500InternalServerError, "Không thể gửi email OTP. Vui lòng thử lại sau.");
        }
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
}

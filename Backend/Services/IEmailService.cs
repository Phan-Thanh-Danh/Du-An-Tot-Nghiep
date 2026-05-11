namespace Backend.Services;

public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string otp, CancellationToken cancellationToken = default);
}

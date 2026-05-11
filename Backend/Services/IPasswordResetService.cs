using Backend.DTOs;

namespace Backend.Services;

public interface IPasswordResetService
{
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}

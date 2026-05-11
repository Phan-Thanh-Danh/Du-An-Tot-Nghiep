using System.Net.Mail;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public PasswordResetService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Email không tồn tại.");
        }

        EnsureUserCanResetPassword(user);

        var oldOtps = await _context.PasswordResetOtps
            .Where(x => x.Email == email && !x.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var oldOtp in oldOtps)
        {
            oldOtp.IsUsed = true;
        }

        var otp = OtpHelper.GenerateSixDigitOtp();
        var passwordResetOtp = new PasswordResetOtp
        {
            Email = email,
            OtpCode = PasswordHelper.HashPassword(otp),
            ExpiredAt = DateTime.UtcNow.AddMinutes(OtpConstants.ExpirationMinutes),
            IsVerified = false,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PasswordResetOtps.AddAsync(passwordResetOtp, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetOtpAsync(email, otp, cancellationToken);
    }

    public async Task VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var otpRecord = await GetValidOtpAsync(request.Email, request.Otp, requireVerified: false, cancellationToken);

        otpRecord.IsVerified = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mật khẩu xác nhận không khớp.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mật khẩu mới phải có tối thiểu 8 ký tự.");
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Email không tồn tại.");
        }

        EnsureUserCanResetPassword(user);

        var otpRecord = await GetValidOtpAsync(email, request.Otp, requireVerified: true, cancellationToken);

        user.MatKhauHash = PasswordHelper.HashPassword(request.NewPassword);
        user.DangNhapLanDau = false;
        user.SoLanSaiMatKhau = 0;
        if (user.TrangThai == UserStatuses.DbFirstLogin)
        {
            user.TrangThai = UserStatuses.DbActive;
        }

        otpRecord.IsUsed = true;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PasswordResetOtp> GetValidOtpAsync(
        string email,
        string otp,
        bool requireVerified,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var otpRecord = await _context.PasswordResetOtps
            .Where(x => x.Email == normalizedEmail && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otpRecord is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "OTP không tồn tại hoặc đã được sử dụng.");
        }

        if (otpRecord.ExpiredAt <= DateTime.UtcNow)
        {
            otpRecord.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);
            throw new ApiException(StatusCodes.Status400BadRequest, "OTP đã hết hạn.");
        }

        if (!PasswordHelper.VerifyPassword(otp, otpRecord.OtpCode))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "OTP không chính xác.");
        }

        if (requireVerified && !otpRecord.IsVerified)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "OTP chưa được xác thực.");
        }

        return otpRecord;
    }

    private static void EnsureUserCanResetPassword(NguoiDung user)
    {
        if (user.TrangThai == UserStatuses.DbLocked)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản đang bị khóa, không thể đặt lại mật khẩu.");
        }
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Email không được để trống.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            _ = new MailAddress(normalizedEmail);
        }
        catch (FormatException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Email không hợp lệ.");
        }

        return normalizedEmail;
    }
}

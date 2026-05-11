using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Auth;

public class AuthService : IAuthService
{
    private const int MaxFailedPasswordAttempts = 5;
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthService(ApplicationDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Email hoặc mật khẩu không chính xác.");
        }

        var role = AuthRoles.FromDatabaseCode(user.VaiTroChinh);
        var status = UserStatuses.FromDatabaseStatus(user.TrangThai, user.DangNhapLanDau);

        if (status == UserStatuses.Locked)
        {
            await AddAuditLogAsync(user, "LOGIN_LOCKED", null, new { user.Email, Status = status });
            await _context.SaveChangesAsync();
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản của bạn đang bị khóa.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.MatKhauHash ?? string.Empty))
        {
            user.SoLanSaiMatKhau = Math.Min(user.SoLanSaiMatKhau + 1, MaxFailedPasswordAttempts);

            if (user.SoLanSaiMatKhau >= MaxFailedPasswordAttempts)
            {
                user.TrangThai = UserStatuses.DbLocked;
                user.DangNhapLanDau = false;
                await AddAuditLogAsync(user, "ACCOUNT_LOCKED", null, new { user.Email, Reason = "Too many failed login attempts" });
            }

            await AddAuditLogAsync(user, "LOGIN_FAILED", null, new { user.Email });
            await _context.SaveChangesAsync();

            throw new ApiException(StatusCodes.Status401Unauthorized, "Email hoặc mật khẩu không chính xác.");
        }

        user.SoLanSaiMatKhau = 0;
        user.LanDangNhapCuoi = DateTime.UtcNow;
        await AddAuditLogAsync(user, "LOGIN_SUCCESS", null, new { user.Email });

        status = UserStatuses.FromDatabaseStatus(user.TrangThai, user.DangNhapLanDau);
        var token = _jwtHelper.GenerateToken(user, role, status);
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            RequiresPasswordChange = status == UserStatuses.FirstLogin,
            User = ToAuthUserDto(user, role, status)
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.MaNguoiDung == userId);
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người dùng.");
        }

        if (!PasswordHelper.VerifyPassword(request.OldPassword, user.MatKhauHash ?? string.Empty))
        {
            await AddAuditLogAsync(user, "PASSWORD_CHANGE_FAILED", null, new { Reason = "Mật khẩu hiện tại không khớp" });
            await _context.SaveChangesAsync();
            throw new ApiException(StatusCodes.Status400BadRequest, "Mật khẩu hiện tại không chính xác.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mật khẩu xác nhận không khớp.");
        }

        var passwordStrengthError = PasswordHelper.GetPasswordStrengthError(request.NewPassword);
        if (passwordStrengthError is not null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, passwordStrengthError);
        }

        if (PasswordHelper.VerifyPassword(request.NewPassword, user.MatKhauHash ?? string.Empty))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mật khẩu mới không được trùng với mật khẩu hiện tại.");
        }

        user.MatKhauHash = PasswordHelper.HashPassword(request.NewPassword);
        user.DangNhapLanDau = false;
        user.TrangThai = UserStatuses.DbActive;
        user.SoLanSaiMatKhau = 0;

        await AddAuditLogAsync(user, "PASSWORD_CHANGED", null, new { user.Email });
        await _context.SaveChangesAsync();
    }

    private static AuthUserDto ToAuthUserDto(NguoiDung user, string role, string status)
    {
        return new AuthUserDto
        {
            UserId = user.MaNguoiDung,
            Email = user.Email,
            FullName = user.HoTen,
            Role = role,
            CampusId = user.MaDonVi,
            Status = status
        };
    }

    private async Task AddAuditLogAsync(NguoiDung user, string action, object? oldValue, object? newValue)
    {
        var auditLog = new NhatKyKiemToan
        {
            MaDonVi = user.MaDonVi,
            LoaiDoiTuong = nameof(NguoiDung),
            MaDoiTuong = user.MaNguoiDung,
            HanhDong = action,
            GiaTriCu = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            GiaTriMoi = newValue is null ? null : JsonSerializer.Serialize(newValue),
            NguoiThayDoi = user.MaNguoiDung,
            ThoiDiemThayDoi = DateTime.UtcNow
        };

        await _context.NhatKyKiemToans.AddAsync(auditLog);
    }
}

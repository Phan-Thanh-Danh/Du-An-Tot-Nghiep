using System.Security.Cryptography;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Auth;

public class AuthService : IAuthService
{
    private const int MaxFailedPasswordAttempts = 5;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly JwtHelper _jwtHelper;
    private readonly IAuditLogService _auditLogService;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        JwtHelper jwtHelper,
        IAuditLogService auditLogService)
    {
        _context = context;
        _configuration = configuration;
        _jwtHelper = jwtHelper;
        _auditLogService = auditLogService;
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
            await AddAuditLogAsync(
                user,
                "LOGIN",
                null,
                new { user.Email, Status = status },
                "Đăng nhập bị từ chối do tài khoản bị khóa.");
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản của bạn đang bị khóa.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.MatKhauHash ?? string.Empty))
        {
            var oldValue = new
            {
                user.Email,
                user.TrangThai,
                user.SoLanSaiMatKhau
            };

            user.SoLanSaiMatKhau = Math.Min(user.SoLanSaiMatKhau + 1, MaxFailedPasswordAttempts);

            if (user.SoLanSaiMatKhau >= MaxFailedPasswordAttempts)
            {
                user.TrangThai = UserStatuses.DbLocked;
                user.DangNhapLanDau = false;
            }

            await _context.SaveChangesAsync();

            if (user.TrangThai == UserStatuses.DbLocked)
            {
                await AddAuditLogAsync(
                    user,
                    "LOCK",
                    oldValue,
                    new { user.Email, user.TrangThai, user.SoLanSaiMatKhau },
                    "Tự động khóa tài khoản do đăng nhập sai quá số lần cho phép.");
            }

            await AddAuditLogAsync(
                user,
                "LOGIN_FAILED",
                null,
                new { user.Email },
                "Đăng nhập thất bại.");

            throw new ApiException(StatusCodes.Status401Unauthorized, "Email hoặc mật khẩu không chính xác.");
        }

        user.SoLanSaiMatKhau = 0;
        user.LanDangNhapCuoi = DateTime.UtcNow;

        status = UserStatuses.FromDatabaseStatus(user.TrangThai, user.DangNhapLanDau);
        var token = _jwtHelper.GenerateToken(user, role, status);
        var refreshToken = CreateRefreshToken(user.MaNguoiDung);
        await _context.TokenLamMois.AddAsync(refreshToken.Entity);
        await _context.SaveChangesAsync();

        await AddAuditLogAsync(
            user,
            "LOGIN",
            null,
            new { user.Email, Status = status },
            "Đăng nhập thành công.");

        return new LoginResponseDto
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.Entity.HetHanLuc,
            RequiresPasswordChange = status == UserStatuses.FirstLogin,
            User = ToAuthUserDto(user, role, status)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await GetStoredRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null || refreshToken.NguoiDung is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Refresh token không hợp lệ hoặc đã hết hạn.");
        }

        var user = refreshToken.NguoiDung;
        var role = AuthRoles.FromDatabaseCode(user.VaiTroChinh);
        var status = UserStatuses.FromDatabaseStatus(user.TrangThai, user.DangNhapLanDau);

        if (status == UserStatuses.Locked)
        {
            refreshToken.ThuHoiLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await AddAuditLogAsync(
                user,
                "REFRESH_TOKEN_REJECTED",
                null,
                new { user.Email, Status = status, refreshToken.MaTokenLamMoi },
                "Từ chối refresh token do tài khoản bị khóa.");
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản của bạn đang bị khóa.");
        }

        refreshToken.ThuHoiLuc = DateTime.UtcNow;
        var newRefreshToken = CreateRefreshToken(user.MaNguoiDung);
        await _context.TokenLamMois.AddAsync(newRefreshToken.Entity);

        var accessToken = _jwtHelper.GenerateToken(user, role, status);
        await _context.SaveChangesAsync();
        await AddAuditLogAsync(
            user,
            "REFRESH_TOKEN_ROTATED",
            null,
            new { user.Email, OldRefreshTokenId = refreshToken.MaTokenLamMoi, NewRefreshTokenId = newRefreshToken.Entity.MaTokenLamMoi },
            "Xoay vòng refresh token.");

        return new LoginResponseDto
        {
            AccessToken = accessToken.Token,
            ExpiresAt = accessToken.ExpiresAt,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiresAt = newRefreshToken.Entity.HetHanLuc,
            RequiresPasswordChange = status == UserStatuses.FirstLogin,
            User = ToAuthUserDto(user, role, status)
        };
    }

    public async Task LogoutAsync(RevokeTokenRequestDto request)
    {
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var refreshToken = await _context.TokenLamMois
            .Include(x => x.NguoiDung)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (refreshToken is not null && refreshToken.ThuHoiLuc is null)
        {
            refreshToken.ThuHoiLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (refreshToken.NguoiDung is not null)
            {
                await AddAuditLogAsync(
                    refreshToken.NguoiDung,
                    "LOGOUT",
                    null,
                    new { refreshToken.NguoiDung.Email, refreshToken.MaTokenLamMoi },
                    "Đăng xuất và thu hồi refresh token.");
            }
        }
    }

    public async Task RevokeTokenAsync(RevokeTokenRequestDto request)
    {
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var refreshToken = await _context.TokenLamMois
            .Include(x => x.NguoiDung)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy refresh token.");
        }

        if (refreshToken.ThuHoiLuc is not null)
        {
            return;
        }

        refreshToken.ThuHoiLuc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (refreshToken.NguoiDung is not null)
        {
            await AddAuditLogAsync(
                refreshToken.NguoiDung,
                "REVOKE_TOKEN",
                null,
                new { refreshToken.NguoiDung.Email, refreshToken.MaTokenLamMoi },
                "Quản trị thu hồi refresh token.");
        }
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
            await AddAuditLogAsync(
                user,
                "CHANGE_PASSWORD_FAILED",
                null,
                new { Reason = "Mật khẩu hiện tại không khớp" },
                "Đổi mật khẩu thất bại.");
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

        await _context.SaveChangesAsync();
        await AddAuditLogAsync(
            user,
            "CHANGE_PASSWORD",
            null,
            new { user.Email },
            "Người dùng đổi mật khẩu.");
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

    private async Task<TokenLamMoi?> GetStoredRefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashRefreshToken(refreshToken);

        return await _context.TokenLamMois
            .Include(x => x.NguoiDung)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.ThuHoiLuc == null &&
                x.HetHanLuc > DateTime.UtcNow);
    }

    private (string Token, TokenLamMoi Entity) CreateRefreshToken(int userId)
    {
        var token = GenerateRefreshToken();
        var entity = new TokenLamMoi
        {
            MaNguoiDung = userId,
            TokenHash = HashRefreshToken(token),
            HetHanLuc = DateTime.UtcNow.AddDays(GetRefreshTokenExpiresInDays()),
            NgayTao = DateTime.UtcNow
        };

        return (token, entity);
    }

    private int GetRefreshTokenExpiresInDays()
    {
        return int.TryParse(_configuration["JwtSettings:RefreshTokenExpiresInDays"], out var days) && days > 0
            ? days
            : 7;
    }

    private static string GenerateRefreshToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Refresh token không được để trống.");
        }

        var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hashBytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private async Task AddAuditLogAsync(
        NguoiDung user,
        string action,
        object? oldValue,
        object? newValue,
        string description)
    {
        await _auditLogService.LogAsync(
            "User",
            user.MaNguoiDung.ToString(),
            action,
            oldValue,
            newValue,
            user.MaNguoiDung,
            user.MaDonVi,
            description);
    }
}

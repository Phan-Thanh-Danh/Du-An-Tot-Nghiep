using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AccountService(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<AccountProfileResponse> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await GetUserAsync(userId, cancellationToken);
        return ToProfileResponse(user);
    }

    public async Task<AccountProfileResponse> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.Email is null && request.HoTen is null && request.SoDienThoai is null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Vui lòng cung cấp ít nhất một thông tin cần cập nhật."
            );
        }

        var user = await GetUserAsync(userId, cancellationToken);
        var oldValue = ToProfileResponse(user);

        if (request.Email is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Email không được để trống."
                );
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailExists = await _context.NguoiDungs.AnyAsync(
                x => x.Email.ToLower() == normalizedEmail && x.MaNguoiDung != userId,
                cancellationToken
            );

            if (emailExists)
            {
                throw new ApiException(
                    StatusCodes.Status409Conflict,
                    "Email đã được sử dụng bởi tài khoản khác."
                );
            }

            user.Email = normalizedEmail;
        }

        if (request.HoTen is not null)
        {
            if (string.IsNullOrWhiteSpace(request.HoTen))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Họ tên không được để trống."
                );
            }

            user.HoTen = request.HoTen.Trim();
        }

        if (request.SoDienThoai is not null)
        {
            user.SoDienThoai = string.IsNullOrWhiteSpace(request.SoDienThoai)
                ? null
                : request.SoDienThoai.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
        var newValue = ToProfileResponse(user);
        await _auditLogService.LogAsync(
            "User",
            user.MaNguoiDung.ToString(),
            "UPDATE_PROFILE",
            oldValue,
            newValue,
            user.MaNguoiDung,
            user.MaDonVi,
            "Người dùng cập nhật hồ sơ cá nhân.",
            cancellationToken
        );

        return ToProfileResponse(user);
    }

    public async Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Mật khẩu xác nhận không khớp."
            );
        }

        var passwordStrengthError = PasswordHelper.GetPasswordStrengthError(request.NewPassword);
        if (passwordStrengthError is not null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, passwordStrengthError);
        }

        var user = await GetUserAsync(userId, cancellationToken);

        if (
            !PasswordHelper.VerifyPassword(
                request.CurrentPassword,
                user.MatKhauHash ?? string.Empty
            )
        )
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Mật khẩu hiện tại không chính xác."
            );
        }

        if (PasswordHelper.VerifyPassword(request.NewPassword, user.MatKhauHash ?? string.Empty))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Mật khẩu mới không được trùng với mật khẩu hiện tại."
            );
        }

        var oldValue = new
        {
            user.MaNguoiDung,
            user.Email,
            user.TrangThai,
            user.DangNhapLanDau,
        };

        user.MatKhauHash = PasswordHelper.HashPassword(request.NewPassword);
        user.DangNhapLanDau = false;
        if (user.TrangThai == UserStatuses.DbFirstLogin)
        {
            user.TrangThai = UserStatuses.DbActive;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            "User",
            user.MaNguoiDung.ToString(),
            "CHANGE_PASSWORD",
            oldValue,
            new
            {
                user.MaNguoiDung,
                user.Email,
                user.TrangThai,
                user.DangNhapLanDau,
            },
            user.MaNguoiDung,
            user.MaDonVi,
            "Người dùng đổi mật khẩu tài khoản.",
            cancellationToken
        );
    }

    private async Task<NguoiDung> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(
            x => x.MaNguoiDung == userId,
            cancellationToken
        );
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người dùng.");
        }

        return user;
    }

    private static AccountProfileResponse ToProfileResponse(NguoiDung user)
    {
        return new AccountProfileResponse
        {
            Id = user.MaNguoiDung,
            Email = user.Email,
            HoTen = user.HoTen,
            SoDienThoai = user.SoDienThoai,
            VaiTroChinh = user.VaiTroChinh,
            TrangThai = user.TrangThai,
        };
    }
}

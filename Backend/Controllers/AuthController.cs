using System.Security.Claims;
using Backend.Constants;
using Backend.DTOs;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Services;
using Backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        var userIdClaim = User.FindFirstValue(CustomClaimTypes.UserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _passwordResetService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Mã OTP đã được gửi về email" });
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        await _passwordResetService.VerifyOtpAsync(request, cancellationToken);
        return Ok(new { message = "Xác thực OTP thành công" });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _passwordResetService.ResetPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }
}

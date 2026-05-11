using System.Security.Claims;
using Backend.Constants;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<AccountProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var profile = await _accountService.GetProfileAsync(GetCurrentUserId(), cancellationToken);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<AccountProfileResponse>> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _accountService.UpdateProfileAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(profile);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _accountService.ChangePasswordAsync(GetCurrentUserId(), request, cancellationToken);
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(CustomClaimTypes.UserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return userId;
    }
}

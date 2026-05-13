using Backend.DTOs.AdminUsers;
using Backend.DTOs.Common;
using Backend.Services.AdminUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminUserManagement")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<UserListItemDto>>>> GetUsers(
        [FromQuery] UserQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<UserListItemDto>>.Ok(users));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<UserDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<UserDetailDto>.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<UserDetailDto>>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = user.MaNguoiDung },
            ApiResponseDto<UserDetailDto>.Ok(user, "Tạo tài khoản thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<UserDetailDto>>> Update(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<UserDetailDto>.Ok(user, "Cập nhật tài khoản thành công"));
    }

    [HttpPatch("{id:int}/lock")]
    public async Task<ActionResult<ApiResponseDto>> Lock(int id, CancellationToken cancellationToken)
    {
        await _userService.LockAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Khóa tài khoản thành công"));
    }

    [HttpPatch("{id:int}/unlock")]
    public async Task<ActionResult<ApiResponseDto>> Unlock(int id, CancellationToken cancellationToken)
    {
        await _userService.UnlockAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Mở khóa tài khoản thành công"));
    }

    [HttpPatch("{id:int}/reset-password")]
    public async Task<ActionResult<ApiResponseDto>> ResetPassword(
        int id,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _userService.ResetPasswordAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto.Ok("Đặt lại mật khẩu thành công"));
    }
}

using Backend.DTOs.Common;
using Backend.DTOs.Rbac;
using Backend.Services.Rbac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/rbac")]
[Authorize(Policy = "RbacManagement")]
public class RbacController : ControllerBase
{
    private readonly IRbacService _rbacService;

    public RbacController(IRbacService rbacService)
    {
        _rbacService = rbacService;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<RoleDto>>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _rbacService.GetRolesAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<RoleDto>>.Ok(roles));
    }

    [HttpGet("roles/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RoleDto>>> GetRoleById(int id, CancellationToken cancellationToken)
    {
        var role = await _rbacService.GetRoleByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RoleDto>.Ok(role));
    }

    [HttpPost("roles")]
    public async Task<ActionResult<ApiResponseDto<RoleDto>>> CreateRole(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _rbacService.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetRoleById),
            new { id = role.MaVaiTro },
            ApiResponseDto<RoleDto>.Ok(role, "Tạo vai trò thành công"));
    }

    [HttpPut("roles/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RoleDto>>> UpdateRole(
        int id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _rbacService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<RoleDto>.Ok(role, "Cập nhật vai trò thành công"));
    }

    [HttpDelete("roles/{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteRole(int id, CancellationToken cancellationToken)
    {
        await _rbacService.DeleteRoleAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa vai trò thành công"));
    }

    [HttpGet("users/{userId:int}/roles")]
    public async Task<ActionResult<ApiResponseDto<UserRoleAssignmentDto>>> GetUserRoles(
        int userId,
        CancellationToken cancellationToken)
    {
        var result = await _rbacService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(ApiResponseDto<UserRoleAssignmentDto>.Ok(result));
    }

    [HttpPut("users/{userId:int}/roles")]
    public async Task<ActionResult<ApiResponseDto<UserRoleAssignmentDto>>> AssignUserRoles(
        int userId,
        AssignUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rbacService.AssignUserRolesAsync(userId, request, cancellationToken);
        return Ok(ApiResponseDto<UserRoleAssignmentDto>.Ok(result, "Gán vai trò thành công"));
    }
}

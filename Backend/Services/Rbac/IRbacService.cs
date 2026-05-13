using Backend.DTOs.Rbac;

namespace Backend.Services.Rbac;

public interface IRbacService
{
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleDto> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<UserRoleAssignmentDto> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserRoleAssignmentDto> AssignUserRolesAsync(int userId, AssignUserRolesRequest request, CancellationToken cancellationToken = default);
}

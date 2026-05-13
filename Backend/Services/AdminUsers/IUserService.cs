using Backend.DTOs.AdminUsers;
using Backend.DTOs.Common;

namespace Backend.Services.AdminUsers;

public interface IUserService
{
    Task<PagedResultDto<UserListItemDto>> GetUsersAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<UserDetailDto> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDetailDto> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task LockAsync(int userId, CancellationToken cancellationToken = default);
    Task UnlockAsync(int userId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);
}

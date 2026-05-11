using Backend.DTOs;

namespace Backend.Services;

public interface IAccountService
{
    Task<AccountProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<AccountProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

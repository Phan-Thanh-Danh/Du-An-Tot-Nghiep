using Backend.DTOs.AdminUsers;

namespace Backend.Services.AdminUsers;

public interface IUserBulkImportService
{
    Task<UserImportResultDto> ImportAsync(
        IFormFile file,
        bool dryRun,
        int? defaultMaDonVi,
        CancellationToken cancellationToken = default);
}

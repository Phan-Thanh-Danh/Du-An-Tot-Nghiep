using Backend.DTOs.AdministrativeClasses;
using Backend.DTOs.Common;

namespace Backend.Services.AdministrativeClasses;

public interface IAdministrativeClassService
{
    Task<PagedResultDto<AdminClassDto>> GetClassesAsync(AdminClassQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<AdminClassDto> GetByIdAsync(int classId, CancellationToken cancellationToken = default);
    Task<AdminClassDto> CreateAsync(CreateAdminClassRequest request, CancellationToken cancellationToken = default);
    Task<AdminClassDto> UpdateAsync(int classId, UpdateAdminClassRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int classId, CancellationToken cancellationToken = default);
}

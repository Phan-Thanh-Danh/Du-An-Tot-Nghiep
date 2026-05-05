using Backend.DTOs.Organizations;

namespace Backend.Services.Organizations;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationResponseDto>> GetAllAsync();
    Task<IReadOnlyList<OrganizationTreeDto>> GetTreeAsync();
    Task<OrganizationResponseDto> GetByIdAsync(int id);
    Task<OrganizationResponseDto> CreateAsync(OrganizationCreateDto dto, int currentUserId);
    Task<OrganizationResponseDto> UpdateAsync(int id, OrganizationUpdateDto dto, int currentUserId);
    Task SoftDeleteAsync(int id, int currentUserId);
    Task HardDeleteAsync(int id, int currentUserId);
    Task<OrganizationTreeDto> GetSubtreeAsync(int id);
    Task ValidateParentAsync(int? parentId, string organizationLevel);
    Task PreventCircularReferenceAsync(int id, int? newParentId);
}

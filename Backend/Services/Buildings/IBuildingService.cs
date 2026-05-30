using Backend.DTOs.Buildings;
using Backend.DTOs.Common;

namespace Backend.Services.Buildings;

public interface IBuildingService
{
    Task<PagedResultDto<BuildingDto>> GetBuildingsAsync(BuildingQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<BuildingDto> GetByIdAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<BuildingDto> CreateAsync(CreateBuildingRequest request, CancellationToken cancellationToken = default);
    Task<BuildingDto> UpdateAsync(int buildingId, UpdateBuildingRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int buildingId, CancellationToken cancellationToken = default);
}

using Backend.DTOs.Common;
using Backend.DTOs.Floors;

namespace Backend.Services.Floors;

public interface IFloorService
{
    Task<PagedResultDto<FloorDto>> GetFloorsAsync(FloorQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<PagedResultDto<FloorDto>> GetByBuildingAsync(int buildingId, FloorQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<FloorDto> GetByIdAsync(int floorId, CancellationToken cancellationToken = default);
    Task<FloorDto> CreateAsync(CreateFloorRequest request, CancellationToken cancellationToken = default);
    Task<FloorDto> UpdateAsync(int floorId, UpdateFloorRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int floorId, CancellationToken cancellationToken = default);
}

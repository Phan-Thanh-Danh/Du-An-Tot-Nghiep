using Backend.DTOs.Common;
using Backend.DTOs.Rooms;

namespace Backend.Services.Rooms;

public interface IRoomService
{
    Task<PagedResultDto<RoomListItemDto>> GetRoomsAsync(RoomQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<PagedResultDto<RoomListItemDto>> GetByFloorAsync(int floorId, RoomQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> UpdateAsync(int roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int roomId, CancellationToken cancellationToken = default);
}

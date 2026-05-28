using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Rooms;
using Backend.Services.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private const string ReaderRoles =
        AuthRoles.SuperAdmin + "," +
        AuthRoles.Admin + "," +
        AuthRoles.CampusAdmin + "," +
        AuthRoles.SubCampusAdmin + "," +
        AuthRoles.AcademicStaff;

    private const string ManagerRoles =
        AuthRoles.SuperAdmin + "," +
        AuthRoles.Admin + "," +
        AuthRoles.CampusAdmin + "," +
        AuthRoles.SubCampusAdmin;

    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<RoomListItemDto>>>> GetRooms(
        [FromQuery] RoomQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetRoomsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<RoomListItemDto>>.Ok(rooms));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<RoomDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RoomDetailDto>.Ok(room));
    }

    [HttpGet("/api/master-data/floors/{floorId:int}/rooms")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<RoomListItemDto>>>> GetByFloor(
        int floorId,
        [FromQuery] RoomQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetByFloorAsync(floorId, parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<RoomListItemDto>>.Ok(rooms));
    }

    [HttpPost]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<RoomDetailDto>>> Create(
        CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = room.MaPhong },
            ApiResponseDto<RoomDetailDto>.Ok(room, "Tạo phòng học thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<RoomDetailDto>>> Update(
        int id,
        UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<RoomDetailDto>.Ok(room, "Cập nhật phòng học thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa phòng học thành công"));
    }
}

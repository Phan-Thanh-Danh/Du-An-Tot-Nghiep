using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Floors;
using Backend.Services.Floors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
public class FloorsController : ControllerBase
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

    private readonly IFloorService _floorService;

    public FloorsController(IFloorService floorService)
    {
        _floorService = floorService;
    }

    [HttpGet("api/master-data/floors")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<FloorDto>>>> GetFloors(
        [FromQuery] FloorQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var floors = await _floorService.GetFloorsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<FloorDto>>.Ok(floors));
    }

    [HttpGet("api/master-data/buildings/{buildingId:int}/floors")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<FloorDto>>>> GetByBuilding(
        int buildingId,
        [FromQuery] FloorQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var floors = await _floorService.GetByBuildingAsync(buildingId, parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<FloorDto>>.Ok(floors));
    }

    [HttpGet("api/master-data/floors/{id:int}")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<FloorDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var floor = await _floorService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<FloorDto>.Ok(floor));
    }

    [HttpPost("api/master-data/floors")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<FloorDto>>> Create(
        CreateFloorRequest request,
        CancellationToken cancellationToken)
    {
        var floor = await _floorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = floor.MaTang },
            ApiResponseDto<FloorDto>.Ok(floor, "Tạo tầng thành công"));
    }

    [HttpPut("api/master-data/floors/{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<FloorDto>>> Update(
        int id,
        UpdateFloorRequest request,
        CancellationToken cancellationToken)
    {
        var floor = await _floorService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<FloorDto>.Ok(floor, "Cập nhật tầng thành công"));
    }

    [HttpDelete("api/master-data/floors/{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _floorService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa tầng thành công"));
    }
}

using Backend.Constants;
using Backend.DTOs.Buildings;
using Backend.DTOs.Common;
using Backend.Services.Buildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/buildings")]
[Authorize]
public class BuildingsController : ControllerBase
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

    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<BuildingDto>>>> GetBuildings(
        [FromQuery] BuildingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var buildings = await _buildingService.GetBuildingsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<BuildingDto>>.Ok(buildings));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<ActionResult<ApiResponseDto<BuildingDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var building = await _buildingService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<BuildingDto>.Ok(building));
    }

    [HttpPost]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<BuildingDto>>> Create(
        CreateBuildingRequest request,
        CancellationToken cancellationToken)
    {
        var building = await _buildingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = building.MaToaNha },
            ApiResponseDto<BuildingDto>.Ok(building, "Tạo tòa nhà thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto<BuildingDto>>> Update(
        int id,
        UpdateBuildingRequest request,
        CancellationToken cancellationToken)
    {
        var building = await _buildingService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<BuildingDto>.Ok(building, "Cập nhật tòa nhà thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _buildingService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa tòa nhà thành công"));
    }
}

using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Specializations;
using Backend.Services.Specializations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/specializations")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class ChuyenNganhController : ControllerBase
{
    private readonly IChuyenNganhService _specializationService;

    public ChuyenNganhController(IChuyenNganhService specializationService)
    {
        _specializationService = specializationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<SpecializationDto>>>> GetSpecializations(
        [FromQuery] SpecializationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var specializations = await _specializationService.GetSpecializationsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<SpecializationDto>>.Ok(specializations));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<SpecializationDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var specialization = await _specializationService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SpecializationDto>.Ok(specialization));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<SpecializationDto>>> Create(
        CreateSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var specialization = await _specializationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = specialization.MaChuyenNganh },
            ApiResponseDto<SpecializationDto>.Ok(specialization, "Tạo chuyên ngành thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<SpecializationDto>>> Update(
        int id,
        UpdateSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var specialization = await _specializationService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<SpecializationDto>.Ok(specialization, "Cập nhật chuyên ngành thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _specializationService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Khóa chuyên ngành thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<SpecializationDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var specialization = await _specializationService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SpecializationDto>.Ok(specialization, "Mở khóa chuyên ngành thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<SpecializationDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var specialization = await _specializationService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SpecializationDto>.Ok(specialization, "Khóa chuyên ngành thành công"));
    }
}

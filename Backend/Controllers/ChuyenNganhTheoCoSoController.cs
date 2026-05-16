using Backend.Constants;
using Backend.DTOs.CampusSpecializations;
using Backend.DTOs.Common;
using Backend.Services.CampusSpecializations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/campus-specializations")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class ChuyenNganhTheoCoSoController : ControllerBase
{
    private readonly IChuyenNganhTheoCoSoService _campusSpecializationService;

    public ChuyenNganhTheoCoSoController(IChuyenNganhTheoCoSoService campusSpecializationService)
    {
        _campusSpecializationService = campusSpecializationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CampusSpecializationDto>>>> GetCampusSpecializations(
        [FromQuery] CampusSpecializationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var campusSpecializations = await _campusSpecializationService.GetCampusSpecializationsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<CampusSpecializationDto>>.Ok(campusSpecializations));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Create(
        CreateCampusSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = campusSpecialization.MaChuyenNganhCoSo },
            ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Tạo chuyên ngành theo cơ sở thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Update(
        int id,
        UpdateCampusSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Cập nhật chuyên ngành theo cơ sở thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _campusSpecializationService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Khóa chuyên ngành theo cơ sở thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Kích hoạt chuyên ngành theo cơ sở thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Vô hiệu hóa chuyên ngành theo cơ sở thành công"));
    }

    [HttpPatch("{id:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Approve(
        int id,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Phê duyệt chuyên ngành theo cơ sở thành công"));
    }

    [HttpPatch("{id:int}/reject")]
    public async Task<ActionResult<ApiResponseDto<CampusSpecializationDto>>> Reject(
        int id,
        [FromBody] RejectCampusSpecializationRequest? request,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _campusSpecializationService.RejectAsync(
            id,
            request ?? new RejectCampusSpecializationRequest(),
            cancellationToken);
        return Ok(ApiResponseDto<CampusSpecializationDto>.Ok(campusSpecialization, "Từ chối chuyên ngành theo cơ sở thành công"));
    }
}

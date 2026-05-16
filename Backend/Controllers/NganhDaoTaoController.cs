using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Majors;
using Backend.Services.Majors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/majors")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class NganhDaoTaoController : ControllerBase
{
    private readonly INganhDaoTaoService _majorService;

    public NganhDaoTaoController(INganhDaoTaoService majorService)
    {
        _majorService = majorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<MajorDto>>>> GetMajors(
        [FromQuery] MajorQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var majors = await _majorService.GetMajorsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<MajorDto>>.Ok(majors));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<MajorDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var major = await _majorService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<MajorDto>.Ok(major));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<MajorDto>>> Create(
        CreateMajorRequest request,
        CancellationToken cancellationToken)
    {
        var major = await _majorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = major.MaNganh },
            ApiResponseDto<MajorDto>.Ok(major, "Tạo ngành đào tạo thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<MajorDto>>> Update(
        int id,
        UpdateMajorRequest request,
        CancellationToken cancellationToken)
    {
        var major = await _majorService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<MajorDto>.Ok(major, "Cập nhật ngành đào tạo thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _majorService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Khóa ngành đào tạo thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<MajorDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var major = await _majorService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<MajorDto>.Ok(major, "Mở khóa ngành đào tạo thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<MajorDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var major = await _majorService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<MajorDto>.Ok(major, "Khóa ngành đào tạo thành công"));
    }
}

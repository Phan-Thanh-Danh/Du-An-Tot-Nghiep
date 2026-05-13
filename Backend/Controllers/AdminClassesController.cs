using Backend.DTOs.AdministrativeClasses;
using Backend.DTOs.Common;
using Backend.Services.AdministrativeClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Policy = "AcademicOperations")]
public class AdminClassesController : ControllerBase
{
    private readonly IAdministrativeClassService _classService;

    public AdminClassesController(IAdministrativeClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AdminClassDto>>>> GetClasses(
        [FromQuery] AdminClassQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var classes = await _classService.GetClassesAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AdminClassDto>>.Ok(classes));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AdminClassDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var classEntity = await _classService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AdminClassDto>.Ok(classEntity));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<AdminClassDto>>> Create(
        CreateAdminClassRequest request,
        CancellationToken cancellationToken)
    {
        var classEntity = await _classService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = classEntity.MaLop },
            ApiResponseDto<AdminClassDto>.Ok(classEntity, "Tạo lớp hành chính thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AdminClassDto>>> Update(
        int id,
        UpdateAdminClassRequest request,
        CancellationToken cancellationToken)
    {
        var classEntity = await _classService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AdminClassDto>.Ok(classEntity, "Cập nhật lớp hành chính thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _classService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa lớp hành chính thành công"));
    }
}

using Backend.DTOs.CaHoc;
using Backend.DTOs.Common;
using Backend.Services.CaHoc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/ca-hoc")]
[Authorize(Policy = "AcademicOperations")]
public class CaHocController : ControllerBase
{
    private readonly ICaHocService _caHocService;

    public CaHocController(ICaHocService caHocService)
    {
        _caHocService = caHocService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CaHocDto>>>> Get(
        [FromQuery] CaHocQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var shifts = await _caHocService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<CaHocDto>>.Ok(shifts));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<CaHocDto>>>> GetActive(
        CancellationToken cancellationToken)
    {
        var shifts = await _caHocService.GetActiveAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<CaHocDto>>.Ok(shifts));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CaHocDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var shift = await _caHocService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CaHocDto>.Ok(shift));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CaHocDto>>> Create(
        CreateCaHocRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await _caHocService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = shift.MaCaHoc },
            ApiResponseDto<CaHocDto>.Ok(shift, "Tạo ca học thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CaHocDto>>> Update(
        int id,
        UpdateCaHocRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await _caHocService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CaHocDto>.Ok(shift, "Cập nhật ca học thành công"));
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<ActionResult<ApiResponseDto<CaHocDto>>> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        var shift = await _caHocService.ToggleActiveAsync(id, cancellationToken);
        var message = shift.ConHoatDong ? "Bật hoạt động ca học thành công" : "Tắt hoạt động ca học thành công";
        return Ok(ApiResponseDto<CaHocDto>.Ok(shift, message));
    }
}

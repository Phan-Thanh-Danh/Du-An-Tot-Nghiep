using Backend.DTOs.Common;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/thoi-khoa-bieu")]
[Authorize(Policy = "AcademicOperations")]
public class ThoiKhoaBieuController : ControllerBase
{
    private readonly IThoiKhoaBieuService _thoiKhoaBieuService;

    public ThoiKhoaBieuController(IThoiKhoaBieuService thoiKhoaBieuService)
    {
        _thoiKhoaBieuService = thoiKhoaBieuService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ThoiKhoaBieuDto>>>> Get(
        [FromQuery] ThoiKhoaBieuQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var schedules = await _thoiKhoaBieuService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ThoiKhoaBieuDto>>.Ok(schedules));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Create(
        CreateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = schedule.MaTkb },
            ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Tạo thời khóa biểu thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Update(
        int id,
        UpdateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Cập nhật thời khóa biểu thành công"));
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.CancelAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Hủy thời khóa biểu thành công"));
    }
}

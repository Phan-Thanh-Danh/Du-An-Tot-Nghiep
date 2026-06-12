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
    private readonly IScheduleConflictService _scheduleConflictService;

    public ThoiKhoaBieuController(
        IThoiKhoaBieuService thoiKhoaBieuService,
        IScheduleConflictService scheduleConflictService)
    {
        _thoiKhoaBieuService = thoiKhoaBieuService;
        _scheduleConflictService = scheduleConflictService;
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

    [HttpPost("check-xung-dot")]
    public async Task<ActionResult<ApiResponseDto<ScheduleConflictResultDto>>> CheckConflicts(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _scheduleConflictService.CheckConflictsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<ScheduleConflictResultDto>.Ok(
            result,
            "Kiểm tra xung đột thời khóa biểu thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Create(
        CreateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _thoiKhoaBieuService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = schedule.MaTkb },
                ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Tạo thời khóa biểu thành công"));
        }
        catch (ScheduleConflictException exception)
        {
            return ToConflictResponse(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Update(
        int id,
        UpdateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _thoiKhoaBieuService.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Cập nhật thời khóa biểu thành công"));
        }
        catch (ScheduleConflictException exception)
        {
            return ToConflictResponse(exception);
        }
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.CancelAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Hủy thời khóa biểu thành công"));
    }

    private ConflictObjectResult ToConflictResponse(ScheduleConflictException exception)
    {
        return Conflict(new ApiResponseDto<ScheduleConflictResultDto>
        {
            Success = false,
            Message = "Thời khóa biểu bị xung đột.",
            Data = exception.Result,
            Errors = exception.Result.Conflicts
                .Select(x => x.Message)
                .Distinct()
                .ToList()
        });
    }
}

using Backend.DTOs.BuoiHoc;
using Backend.DTOs.Common;
using Backend.Services.BuoiHoc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/buoi-hoc")]
[Authorize(Policy = "AcademicOperations")]
public class BuoiHocController : ControllerBase
{
    private readonly IBuoiHocService _buoiHocService;

    public BuoiHocController(IBuoiHocService buoiHocService)
    {
        _buoiHocService = buoiHocService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<BuoiHocDto>>>> Get(
        [FromQuery] BuoiHocQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var sessions = await _buoiHocService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<BuoiHocDto>>.Ok(sessions));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var session = await _buoiHocService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session));
    }

    [HttpPut("{id:int}/change-teacher")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> ChangeTeacher(
        int id,
        [FromBody] ChangeSessionTeacherRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _buoiHocService.ChangeTeacherAsync(id, request, cancellationToken);
            return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session, "Đổi giáo viên dạy thay thành công."));
        }
        catch (SessionConflictException exception)
        {
            return Conflict(ToConflictResponse(exception));
        }
    }

    [HttpPut("{id:int}/change-room")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> ChangeRoom(
        int id,
        [FromBody] ChangeSessionRoomRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _buoiHocService.ChangeRoomAsync(id, request, cancellationToken);
            return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session, "Đổi phòng học thành công."));
        }
        catch (SessionConflictException exception)
        {
            return Conflict(ToConflictResponse(exception));
        }
    }

    [HttpPut("{id:int}/change-shift")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> ChangeShift(
        int id,
        [FromBody] ChangeSessionShiftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _buoiHocService.ChangeShiftAsync(id, request, cancellationToken);
            return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session, "Đổi ca học thành công."));
        }
        catch (SessionConflictException exception)
        {
            return Conflict(ToConflictResponse(exception));
        }
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> Cancel(
        int id,
        [FromBody] CancelSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _buoiHocService.CancelAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session, "Hủy buổi học thành công."));
    }

    private static ApiResponseDto<SessionConflictResultDto> ToConflictResponse(
        SessionConflictException exception)
    {
        return new ApiResponseDto<SessionConflictResultDto>
        {
            Success = false,
            Message = exception.Message,
            Data = exception.Result,
            Errors = [exception.Message]
        };
    }
}

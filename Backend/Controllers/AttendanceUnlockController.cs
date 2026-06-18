using Backend.DTOs.AttendanceUnlock;
using Backend.DTOs.Common;
using Backend.Services.AttendanceUnlock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AttendanceUnlockController : ControllerBase
{
    private readonly IAttendanceUnlockService _attendanceUnlockService;

    public AttendanceUnlockController(IAttendanceUnlockService attendanceUnlockService)
    {
        _attendanceUnlockService = attendanceUnlockService;
    }

    [HttpPost("buoi-hoc/{id:int}/attendance/unlock-requests")]
    public async Task<ActionResult<ApiResponseDto<AttendanceUnlockRequestDto>>> Create(
        int id,
        [FromBody] CreateAttendanceUnlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.CreateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AttendanceUnlockRequestDto>.Ok(result, "Tạo yêu cầu mở khóa điểm danh thành công."));
    }

    [HttpGet("teacher/attendance/unlock-requests")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AttendanceUnlockRequestDto>>>> GetTeacherRequests(
        [FromQuery] AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.GetTeacherRequestsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AttendanceUnlockRequestDto>>.Ok(
            result,
            "Lấy danh sách yêu cầu mở khóa điểm danh của giáo viên thành công."));
    }

    [HttpGet("admin/attendance/unlock-requests")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AttendanceUnlockRequestDto>>>> GetAdminRequests(
        [FromQuery] AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.GetAdminRequestsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AttendanceUnlockRequestDto>>.Ok(
            result,
            "Lấy danh sách yêu cầu mở khóa điểm danh thành công."));
    }

    [HttpGet("admin/attendance/unlock-requests/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AttendanceUnlockRequestDto>>> GetAdminRequestById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.GetAdminRequestByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AttendanceUnlockRequestDto>.Ok(result, "Lấy chi tiết yêu cầu mở khóa điểm danh thành công."));
    }

    [HttpPost("admin/attendance/unlock-requests/{id:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<AttendanceUnlockRequestDto>>> Approve(
        int id,
        [FromBody] ApproveAttendanceUnlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.ApproveAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AttendanceUnlockRequestDto>.Ok(result, "Duyệt yêu cầu mở khóa điểm danh thành công."));
    }

    [HttpPost("admin/attendance/unlock-requests/{id:int}/reject")]
    public async Task<ActionResult<ApiResponseDto<AttendanceUnlockRequestDto>>> Reject(
        int id,
        [FromBody] RejectAttendanceUnlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceUnlockService.RejectAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AttendanceUnlockRequestDto>.Ok(result, "Từ chối yêu cầu mở khóa điểm danh thành công."));
    }
}

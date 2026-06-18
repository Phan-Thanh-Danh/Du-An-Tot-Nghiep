using Backend.DTOs.Attendance;
using Backend.DTOs.Common;
using Backend.Services.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("teacher/attendance/today")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<AttendanceSessionDto>>>> GetTeacherToday(
        CancellationToken cancellationToken)
    {
        var sessions = await _attendanceService.GetTeacherTodayAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<AttendanceSessionDto>>.Ok(
            sessions,
            "Lấy danh sách buổi học hôm nay thành công."));
    }

    [HttpPost("buoi-hoc/{id:int}/attendance/start")]
    public async Task<ActionResult<ApiResponseDto<AttendanceDetailDto>>> Start(
        int id,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.StartAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AttendanceDetailDto>.Ok(attendance, "Mở điểm danh thành công."));
    }

    [HttpGet("buoi-hoc/{id:int}/attendance")]
    public async Task<ActionResult<ApiResponseDto<AttendanceDetailDto>>> GetSessionAttendance(
        int id,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.GetSessionAttendanceAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AttendanceDetailDto>.Ok(attendance, "Lấy danh sách điểm danh thành công."));
    }

    [HttpPatch("buoi-hoc/{id:int}/attendance/{maSinhVien:int}")]
    public async Task<ActionResult<ApiResponseDto<AttendanceStudentDto>>> UpdateStudent(
        int id,
        int maSinhVien,
        [FromBody] UpdateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.UpdateStudentAsync(id, maSinhVien, request, cancellationToken);
        return Ok(ApiResponseDto<AttendanceStudentDto>.Ok(attendance, "Cập nhật điểm danh sinh viên thành công."));
    }

    [HttpPut("buoi-hoc/{id:int}/attendance/bulk")]
    public async Task<ActionResult<ApiResponseDto<AttendanceDetailDto>>> BulkUpdate(
        int id,
        [FromBody] BulkUpdateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.BulkUpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AttendanceDetailDto>.Ok(attendance, "Cập nhật điểm danh hàng loạt thành công."));
    }

    [HttpPost("buoi-hoc/{id:int}/attendance/submit")]
    public async Task<ActionResult<ApiResponseDto<AttendanceDetailDto>>> Submit(
        int id,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.SubmitAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AttendanceDetailDto>.Ok(attendance, "Gửi điểm danh thành công."));
    }

    [HttpGet("student/attendance")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentAttendanceDto>>>> GetStudentAttendance(
        [FromQuery] StudentAttendanceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.GetStudentAttendanceAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<StudentAttendanceDto>>.Ok(
            attendance,
            "Lấy điểm danh của sinh viên thành công."));
    }
}

using Backend.Constants;
using Backend.DTOs.AttendanceAutomation;
using Backend.DTOs.Common;
using Backend.Services.AttendanceAutomation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/attendance-automation")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff}")]
public class AttendanceAutomationController : ControllerBase
{
    private readonly IAttendanceAutomationService _attendanceAutomationService;

    public AttendanceAutomationController(IAttendanceAutomationService attendanceAutomationService)
    {
        _attendanceAutomationService = attendanceAutomationService;
    }

    [HttpPost("run-once")]
    public async Task<ActionResult<ApiResponseDto<AttendanceAutomationRunResultDto>>> RunOnce(
        CancellationToken cancellationToken)
    {
        var result = await _attendanceAutomationService.ProcessDueAttendanceAsync(cancellationToken: cancellationToken);
        return Ok(ApiResponseDto<AttendanceAutomationRunResultDto>.Ok(result, "Đã xử lý tự động điểm danh."));
    }
}

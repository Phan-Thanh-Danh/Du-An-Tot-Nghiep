using Backend.DTOs.AttendancePolicy;
using Backend.DTOs.Common;
using Backend.Services.AttendancePolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/attendance-policy")]
[Authorize(Policy = "AcademicOperations")]
public class AttendancePolicyController : ControllerBase
{
    private readonly IAttendancePolicyService _attendancePolicyService;

    public AttendancePolicyController(IAttendancePolicyService attendancePolicyService)
    {
        _attendancePolicyService = attendancePolicyService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<QuyDinhChuyenCanDto>>> GetCurrent(CancellationToken cancellationToken)
    {
        var policy = await _attendancePolicyService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponseDto<QuyDinhChuyenCanDto>.Ok(policy, "Lấy chính sách điểm danh hiện hành thành công."));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuyDinhChuyenCanDto>>>> GetHistory(CancellationToken cancellationToken)
    {
        var history = await _attendancePolicyService.GetHistoryAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<QuyDinhChuyenCanDto>>.Ok(history, "Lấy lịch sử thay đổi chính sách điểm danh thành công."));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponseDto<QuyDinhChuyenCanDto>>> Update(
        [FromBody] UpdateQuyDinhChuyenCanRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _attendancePolicyService.UpdateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<QuyDinhChuyenCanDto>.Ok(policy, "Cập nhật chính sách điểm danh thành công."));
    }
}

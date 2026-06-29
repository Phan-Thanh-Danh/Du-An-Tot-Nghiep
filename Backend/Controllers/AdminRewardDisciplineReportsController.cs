using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/reward-discipline/reports")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminRewardDisciplineReportsController : ControllerBase
{
    private readonly IRewardDisciplineReportService _reportService;

    public AdminRewardDisciplineReportsController(IRewardDisciplineReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponseDto<RewardDisciplineOverviewReportDto>>> GetOverview(
        [FromQuery] RewardDisciplineReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetOverviewAsync(query, cancellationToken);
        return Ok(ApiResponseDto<RewardDisciplineOverviewReportDto>.Ok(
            result,
            "Lấy báo cáo tổng quan khen thưởng/kỷ luật thành công."));
    }

    [HttpGet("rewards")]
    public async Task<ActionResult<ApiResponseDto<RewardReportDto>>> GetRewards(
        [FromQuery] RewardReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetRewardsAsync(query, cancellationToken);
        return Ok(ApiResponseDto<RewardReportDto>.Ok(
            result,
            "Lấy báo cáo khen thưởng thành công."));
    }

    [HttpGet("discipline")]
    public async Task<ActionResult<ApiResponseDto<DisciplineReportDto>>> GetDiscipline(
        [FromQuery] DisciplineReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetDisciplineAsync(query, cancellationToken);
        return Ok(ApiResponseDto<DisciplineReportDto>.Ok(
            result,
            "Lấy báo cáo kỷ luật thành công."));
    }

    [HttpGet("certificates")]
    public async Task<ActionResult<ApiResponseDto<CertificateReportDto>>> GetCertificates(
        [FromQuery] CertificateReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetCertificatesAsync(query, cancellationToken);
        return Ok(ApiResponseDto<CertificateReportDto>.Ok(
            result,
            "Lấy báo cáo bằng khen thành công."));
    }

    [HttpGet("appeals")]
    public async Task<ActionResult<ApiResponseDto<DisciplineAppealReportDto>>> GetAppeals(
        [FromQuery] DisciplineAppealReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetAppealsAsync(query, cancellationToken);
        return Ok(ApiResponseDto<DisciplineAppealReportDto>.Ok(
            result,
            "Lấy báo cáo khiếu nại kỷ luật thành công."));
    }

    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponseDto<RewardDisciplineTrendReportDto>>> GetTrends(
        [FromQuery] RewardDisciplineTrendQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetTrendsAsync(query, cancellationToken);
        return Ok(ApiResponseDto<RewardDisciplineTrendReportDto>.Ok(
            result,
            "Lấy báo cáo xu hướng khen thưởng/kỷ luật thành công."));
    }

    [HttpGet("top-students")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<TopStudentReportItemDto>>>> GetTopStudents(
        [FromQuery] TopStudentReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetTopStudentsAsync(query, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<TopStudentReportItemDto>>.Ok(
            result,
            "Lấy báo cáo sinh viên nổi bật/cần lưu ý thành công."));
    }
}

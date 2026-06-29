using Backend.Constants;
using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/applications/reports")]
[Authorize]
public class AdminApplicationReportsController : ControllerBase
{
    private readonly IApplicationReportService _reportService;

    public AdminApplicationReportsController(IApplicationReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponseDto<ApplicationReportOverviewDto>>> GetOverview(
        [FromQuery] ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<ApplicationReportOverviewDto>
        {
            Success = true,
            Message = "Lấy báo cáo tổng quan thành công",
            Data = result
        });
    }

    [HttpGet("by-type")]
    public async Task<ActionResult<ApiResponseDto<List<ApplicationByTypeReportDto>>>> GetByType(
        [FromQuery] ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetByTypeAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<List<ApplicationByTypeReportDto>>
        {
            Success = true,
            Message = "Lấy báo cáo theo loại đơn thành công",
            Data = result
        });
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponseDto<PendingApplicationReportDto>>> GetPending(
        [FromQuery] PendingApplicationReportQuery parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetPendingAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<PendingApplicationReportDto>
        {
            Success = true,
            Message = "Lấy báo cáo đơn chờ xử lý thành công",
            Data = result
        });
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponseDto<OverdueApplicationReportDto>>> GetOverdue(
        [FromQuery] OverdueApplicationReportQuery parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetOverdueAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<OverdueApplicationReportDto>
        {
            Success = true,
            Message = "Lấy báo cáo đơn quá hạn thành công",
            Data = result
        });
    }

    [HttpGet("processing-time")]
    public async Task<ActionResult<ApiResponseDto<ProcessingTimeReportDto>>> GetProcessingTime(
        [FromQuery] ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetProcessingTimeAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<ProcessingTimeReportDto>
        {
            Success = true,
            Message = "Lấy báo cáo thời gian xử lý thành công",
            Data = result
        });
    }

    [HttpGet("by-assignee")]
    public async Task<ActionResult<ApiResponseDto<List<ApplicationByAssigneeReportDto>>>> GetByAssignee(
        [FromQuery] ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetByAssigneeAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<List<ApplicationByAssigneeReportDto>>
        {
            Success = true,
            Message = "Lấy báo cáo theo người xử lý thành công",
            Data = result
        });
    }

    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponseDto<ApplicationTrendReportDto>>> GetTrends(
        [FromQuery] ApplicationTrendQuery parameters,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetTrendsAsync(parameters, cancellationToken);
        return Ok(new ApiResponseDto<ApplicationTrendReportDto>
        {
            Success = true,
            Message = "Lấy báo cáo xu hướng thành công",
            Data = result
        });
    }
}

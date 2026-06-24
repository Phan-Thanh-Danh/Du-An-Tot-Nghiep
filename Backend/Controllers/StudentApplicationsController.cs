using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/applications")]
[Authorize(Policy = "ApplicationStudent")]
public class StudentApplicationsController : ControllerBase
{
    private readonly IStudentApplicationService _studentApplicationService;

    public StudentApplicationsController(IStudentApplicationService studentApplicationService)
    {
        _studentApplicationService = studentApplicationService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Create(
        CreateStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.MaDonTu },
            ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Tạo đơn nháp thành công."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentApplicationListItemDto>>>> Get(
        [FromQuery] StudentApplicationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.GetOwnAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<StudentApplicationListItemDto>>.Ok(result, "Lấy danh sách đơn từ thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.GetOwnDetailAsync(id, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Lấy chi tiết đơn từ thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Update(
        int id,
        UpdateStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Cập nhật đơn từ thành công."));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Submit(
        int id,
        SubmitStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.SubmitAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Nộp đơn thành công."));
    }

    [HttpPost("{id:int}/resubmit")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Resubmit(
        int id,
        ResubmitStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.ResubmitAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Nộp lại đơn thành công."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Cancel(
        int id,
        CancelStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentApplicationService.CancelAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Hủy đơn thành công."));
    }
}

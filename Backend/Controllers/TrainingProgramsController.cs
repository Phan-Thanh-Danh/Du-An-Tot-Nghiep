using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.TrainingPrograms;
using Backend.Services.TrainingPrograms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/training-programs")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Chairman},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class TrainingProgramsController : ControllerBase
{
    private readonly ITrainingProgramService _trainingProgramService;

    public TrainingProgramsController(ITrainingProgramService trainingProgramService)
    {
        _trainingProgramService = trainingProgramService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TrainingProgramDto>>>> Get(
        [FromQuery] TrainingProgramQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var programs = await _trainingProgramService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<TrainingProgramDto>>.Ok(programs, "Lấy danh sách chương trình đào tạo thành công"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Lấy thông tin chương trình đào tạo thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Create(
        CreateTrainingProgramRequest request,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = program.MaChuongTrinh },
            ApiResponseDto<TrainingProgramDto>.Ok(program, "Tạo chương trình đào tạo thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Update(
        int id,
        UpdateTrainingProgramRequest request,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Cập nhật chương trình đào tạo thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _trainingProgramService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Vô hiệu hóa chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/submit")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Submit(
        int id,
        [FromBody] SubmitTrainingProgramRequest? request,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.SubmitAsync(id, request ?? new SubmitTrainingProgramRequest(), cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Gửi duyệt chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Kích hoạt chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Vô hiệu hóa chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Approve(
        int id,
        [FromBody] ApproveTrainingProgramRequest? request,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.ApproveAsync(id, request ?? new ApproveTrainingProgramRequest(), cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Duyệt chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/reject")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Reject(
        int id,
        RejectTrainingProgramRequest request,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.RejectAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Từ chối chương trình đào tạo thành công"));
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramDto>>> Archive(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _trainingProgramService.ArchiveAsync(id, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramDto>.Ok(program, "Lưu trữ chương trình đào tạo thành công"));
    }
}

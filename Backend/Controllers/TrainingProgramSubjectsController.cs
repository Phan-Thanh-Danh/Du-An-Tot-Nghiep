using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.TrainingProgramSubjects;
using Backend.Services.TrainingProgramSubjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/training-program-subjects")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Chairman},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class TrainingProgramSubjectsController : ControllerBase
{
    private readonly ITrainingProgramSubjectService _trainingProgramSubjectService;

    public TrainingProgramSubjectsController(ITrainingProgramSubjectService trainingProgramSubjectService)
    {
        _trainingProgramSubjectService = trainingProgramSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TrainingProgramSubjectDto>>>> Get(
        [FromQuery] TrainingProgramSubjectQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var subjects = await _trainingProgramSubjectService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<TrainingProgramSubjectDto>>.Ok(subjects, "Lấy danh sách môn học trong chương trình thành công"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramSubjectDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var subject = await _trainingProgramSubjectService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramSubjectDto>.Ok(subject, "Lấy thông tin môn học trong chương trình thành công"));
    }

    [HttpGet("by-program/{programId:int}")]
    public async Task<ActionResult<ApiResponseDto<List<TrainingProgramSubjectDto>>>> GetByProgram(
        int programId,
        CancellationToken cancellationToken)
    {
        var subjects = await _trainingProgramSubjectService.GetByProgramAsync(programId, cancellationToken);
        return Ok(ApiResponseDto<List<TrainingProgramSubjectDto>>.Ok(subjects, "Lấy danh sách môn học theo chương trình thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramSubjectDto>>> Create(
        CreateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _trainingProgramSubjectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = subject.MaChuongTrinhMonHoc },
            ApiResponseDto<TrainingProgramSubjectDto>.Ok(subject, "Thêm môn học vào chương trình thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramSubjectDto>>> Update(
        int id,
        UpdateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _trainingProgramSubjectService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<TrainingProgramSubjectDto>.Ok(subject, "Cập nhật môn học trong chương trình thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await _trainingProgramSubjectService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa môn học khỏi chương trình thành công"));
    }
}

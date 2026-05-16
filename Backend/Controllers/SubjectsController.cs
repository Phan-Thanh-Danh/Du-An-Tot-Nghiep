using Backend.DTOs.Common;
using Backend.DTOs.Subjects;
using Backend.Services.Subjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/subjects")]
[Authorize(Policy = "AcademicOperations")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<SubjectDto>>>> GetSubjects(
        [FromQuery] SubjectQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var subjects = await _subjectService.GetSubjectsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<SubjectDto>>.Ok(subjects));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<SubjectDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SubjectDto>.Ok(subject));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<SubjectDto>>> Create(
        CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = subject.MaMonHoc },
            ApiResponseDto<SubjectDto>.Ok(subject, "Tạo môn học thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<SubjectDto>>> Update(
        int id,
        UpdateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<SubjectDto>.Ok(subject, "Cập nhật môn học thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Khóa môn học thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<SubjectDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SubjectDto>.Ok(subject, "Mở khóa môn học thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<SubjectDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<SubjectDto>.Ok(subject, "Khóa môn học thành công"));
    }
}

using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.CourseSyllabuses;
using Backend.Services.CourseSyllabuses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/course-syllabuses")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Teacher}")]
public class CourseSyllabusesController : ControllerBase
{
    private readonly ICourseSyllabusService _courseSyllabusService;

    public CourseSyllabusesController(ICourseSyllabusService courseSyllabusService)
    {
        _courseSyllabusService = courseSyllabusService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CourseSyllabusDto>>>> GetCourseSyllabuses(
        [FromQuery] CourseSyllabusQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var syllabuses = await _courseSyllabusService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<CourseSyllabusDto>>.Ok(syllabuses));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Create(
        CreateCourseSyllabusRequest request,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = syllabus.MaSyllabus },
            ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Tạo đề cương môn học thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Update(
        int id,
        UpdateCourseSyllabusRequest request,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Cập nhật đề cương môn học thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _courseSyllabusService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Lưu trữ đề cương môn học thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Kích hoạt đề cương môn học thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Vô hiệu hóa đề cương môn học thành công"));
    }

    [HttpPatch("{id:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Approve(
        int id,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Duyệt đề cương môn học thành công"));
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<ActionResult<ApiResponseDto<CourseSyllabusDto>>> Archive(
        int id,
        CancellationToken cancellationToken)
    {
        var syllabus = await _courseSyllabusService.ArchiveAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CourseSyllabusDto>.Ok(syllabus, "Lưu trữ đề cương môn học thành công"));
    }
}

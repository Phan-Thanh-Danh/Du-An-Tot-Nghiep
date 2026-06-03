using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Courses;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Teacher}")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<KhoaHocDto>>>> GetCourses(
        [FromQuery] KhoaHocQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var courses = await _courseService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<KhoaHocDto>>.Ok(courses));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<KhoaHocDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var course = await _courseService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<KhoaHocDetailDto>.Ok(course));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<KhoaHocDto>>> Create(
        CreateKhoaHocRequest request,
        CancellationToken cancellationToken)
    {
        var course = await _courseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = course.MaKhoaHoc },
            ApiResponseDto<KhoaHocDto>.Ok(course, "Tạo khóa học thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<KhoaHocDto>>> Update(
        int id,
        UpdateKhoaHocRequest request,
        CancellationToken cancellationToken)
    {
        var course = await _courseService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<KhoaHocDto>.Ok(course, "Cập nhật khóa học thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Lưu trữ khóa học thành công"));
    }
}

using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Courses;
using Backend.DTOs.Courses.AssignmentSuggestions;
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
    private readonly ICourseAssignmentSuggestionService _suggestionService;

    public CoursesController(
        ICourseService courseService,
        ICourseAssignmentSuggestionService suggestionService)
    {
        _courseService = courseService;
        _suggestionService = suggestionService;
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

    [HttpPost("assignment-suggestions")]
    [Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
    public async Task<ActionResult<ApiResponseDto<CourseAssignmentSuggestionResultDto>>> GetAssignmentSuggestions(
        [FromBody] CourseAssignmentSuggestionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items["CurrentUser"] is not CurrentUserContext currentUser)
        {
            return Unauthorized();
        }

        var result = await _suggestionService.GetSuggestionsAsync(request, currentUser.CampusId, cancellationToken);
        return Ok(ApiResponseDto<CourseAssignmentSuggestionResultDto>.Ok(result));
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

    [HttpPost("bulk-assign")]
    public async Task<ActionResult<ApiResponseDto<BulkAssignCoursesResultDto>>> BulkAssign(
        BulkAssignCoursesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.BulkAssignAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BulkAssignCoursesResultDto>.Ok(result, $"Tạo thành công {result.CreatedCount}/{result.CreatedCount + result.SkippedCount} lớp"));
    }

    [HttpPost("{id:int}/clone")]
    public async Task<ActionResult<ApiResponseDto<KhoaHocDto>>> Clone(
        int id,
        CloneCourseRequest request,
        CancellationToken cancellationToken)
    {
        var course = await _courseService.CloneAsync(id, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = course.MaKhoaHoc },
            ApiResponseDto<KhoaHocDto>.Ok(course, "Nhân bản khóa học thành công"));
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

    [HttpPost("batch-archive")]
    public async Task<ActionResult<ApiResponseDto<BatchCourseActionResultDto>>> BatchArchive(
        BatchCourseActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.BatchArchiveAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BatchCourseActionResultDto>.Ok(result, $"Đã lưu trữ {result.Count} khóa học"));
    }

    [HttpPost("batch-publish")]
    public async Task<ActionResult<ApiResponseDto<BatchCourseActionResultDto>>> BatchPublish(
        BatchCourseActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.BatchPublishAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BatchCourseActionResultDto>.Ok(result, $"Đã xuất bản {result.Count} khóa học"));
    }
}

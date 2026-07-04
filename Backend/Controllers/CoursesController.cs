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

    [HttpPost("bulk-assign")]
    public async Task<ActionResult<ApiResponseDto<BulkAssignCoursesResultDto>>> BulkAssign(
        BulkAssignCoursesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.BulkAssignAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BulkAssignCoursesResultDto>.Ok(result, $"Tạo thành công {result.CreatedCount}/{result.CreatedCount + result.SkippedCount} lớp"));
    }

    [HttpPost("allocation-suggestions")]
    public async Task<ActionResult<ApiResponseDto<List<AllocationSuggestionDto>>>> GetAllocationSuggestions(
        AllocationSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        var suggestions = await _courseService.GetAllocationSuggestionsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<List<AllocationSuggestionDto>>.Ok(suggestions));
    }

    [HttpPost("allocation-preview")]
    public async Task<ActionResult<ApiResponseDto<AllocationPreviewDto>>> PreviewAllocation(
        BulkAssignCoursesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.PreviewAllocationAsync(request, cancellationToken);
        return Ok(ApiResponseDto<AllocationPreviewDto>.Ok(result));
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

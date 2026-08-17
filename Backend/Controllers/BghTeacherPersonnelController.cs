using Backend.Constants;
using Backend.DTOs.AdminUsers;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.TeacherPersonnel;
using Backend.Services.AdminUsers;
using Backend.Services.TeacherPersonnel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh/teacher-personnel")]
[Authorize(Roles = AuthRoles.SuperAdmin + "," + AuthRoles.Admin + "," + AuthRoles.Principal + "," + AuthRoles.AcademicStaff + ",sieu_quan_tri,quan_tri,hieu_truong,nhan_vien")]
public class BghTeacherPersonnelController : ControllerBase
{
    private readonly IUserBulkImportService _importService;
    private readonly ITeacherPersonnelService _teacherService;

    public BghTeacherPersonnelController(
        IUserBulkImportService importService,
        ITeacherPersonnelService teacherService)
    {
        _importService = importService;
        _teacherService = teacherService;
    }

    private CurrentUserContext GetCurrentUser()
    {
        return HttpContext.Items["CurrentUser"] as CurrentUserContext
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherPersonnelListDto>>>> GetTeachers(
        [FromQuery] TeacherPersonnelQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetTeachersAsync(GetCurrentUser(), query, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<TeacherPersonnelListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherPersonnelDetailDto>>> GetTeacherDetail(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetTeacherDetailAsync(GetCurrentUser(), id, cancellationToken);
        return Ok(ApiResponseDto<TeacherPersonnelDetailDto>.Ok(result));
    }

    [HttpGet("{id:int}/workload")]
    public async Task<ActionResult<ApiResponseDto<TeacherWorkloadSummaryDto>>> GetTeacherWorkload(
        int id,
        [FromQuery] int? semesterId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetTeacherWorkloadAsync(GetCurrentUser(), id, semesterId, cancellationToken);
        return Ok(ApiResponseDto<TeacherWorkloadSummaryDto>.Ok(result));
    }

    [HttpGet("{id:int}/session-logs")]
    public async Task<ActionResult<ApiResponseDto<TeacherSessionLogsSummaryDto>>> GetTeacherSessionLogs(
        int id,
        [FromQuery] int? semesterId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetTeacherSessionLogsAsync(GetCurrentUser(), id, semesterId, cancellationToken);
        return Ok(ApiResponseDto<TeacherSessionLogsSummaryDto>.Ok(result));
    }

    [HttpGet("{id:int}/evaluations")]
    public async Task<ActionResult<ApiResponseDto<TeacherEvaluationSummaryDto>>> GetTeacherEvaluations(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetTeacherEvaluationsAsync(GetCurrentUser(), id, cancellationToken);
        return Ok(ApiResponseDto<TeacherEvaluationSummaryDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TeacherPersonnelDetailDto>>> CreateTeacher(
        [FromBody] CreateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.CreateTeacherAsync(GetCurrentUser(), request, cancellationToken);
        return Ok(ApiResponseDto<TeacherPersonnelDetailDto>.Ok(result, "Tạo mới tài khoản giảng viên thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherPersonnelDetailDto>>> UpdateTeacher(
        int id,
        [FromBody] UpdateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.UpdateTeacherAsync(GetCurrentUser(), id, request, cancellationToken);
        return Ok(ApiResponseDto<TeacherPersonnelDetailDto>.Ok(result, "Cập nhật hồ sơ giảng viên thành công."));
    }

    [HttpPost("{id:int}/toggle-lock")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ToggleLock(
        int id,
        [FromBody] ToggleTeacherLockRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.ToggleLockTeacherAsync(GetCurrentUser(), id, request, cancellationToken);
        return Ok(ApiResponseDto<bool>.Ok(result, "Thay đổi trạng thái tài khoản thành công."));
    }

    [HttpGet("hierarchy-tree")]
    public async Task<ActionResult<ApiResponseDto<List<OrganizationHierarchyNodeDto>>>> GetHierarchyTree(
        CancellationToken cancellationToken = default)
    {
        var result = await _teacherService.GetHierarchyTreeAsync(GetCurrentUser(), cancellationToken);
        return Ok(ApiResponseDto<List<OrganizationHierarchyNodeDto>>.Ok(result));
    }

    [HttpPost("import-excel")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponseDto<UserImportResultDto>>> ImportTeachers(
        IFormFile file,
        [FromForm] bool dryRun = true,
        [FromForm] int? defaultMaDonVi = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.ImportAsync(
            file,
            dryRun,
            defaultMaDonVi,
            cancellationToken);

        var message = result.DaLuu
            ? $"Đã tạo mới {result.SoDongTaoMoi} và cập nhật {result.SoDongCapNhat} tài khoản."
            : result.SoDongLoi > 0
                ? "File còn lỗi; hệ thống chưa lưu tài khoản nào."
                : "Kiểm tra file thành công; chưa lưu dữ liệu vì đang ở chế độ dry-run.";
        return Ok(ApiResponseDto<UserImportResultDto>.Ok(result, message));
    }
}

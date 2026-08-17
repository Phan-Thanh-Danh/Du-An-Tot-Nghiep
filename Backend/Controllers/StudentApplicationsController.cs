using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/applications")]
[Authorize(Policy = AuthPolicies.ApplicationStudent)]
public class StudentApplicationsController : ControllerBase
{
    private readonly IStudentApplicationService _studentApplicationService;
    private readonly ApplicationDbContext _db;

    public StudentApplicationsController(IStudentApplicationService studentApplicationService, ApplicationDbContext db)
    {
        _studentApplicationService = studentApplicationService;
        _db = db;
    }

    private async Task EnsureHasPermissionAsync(string permissionCode, CancellationToken ct)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var roleCode = currentUser?.Role ?? "hoc_sinh";

        if (roleCode == "SuperAdmin" || roleCode == "sieu_quan_tri" || roleCode == "Admin" || roleCode == "quan_tri")
            return;

        var hasPerm = await _db.VaiTroQuyenHans
            .AsNoTracking()
            .AnyAsync(vp => vp.VaiTro != null &&
                           (vp.VaiTro.MaCodeVaiTro == roleCode || vp.VaiTro.MaCodeVaiTro == "hoc_sinh") &&
                           vp.QuyenHan != null && vp.QuyenHan.MaCode == permissionCode, ct);

        if (!hasPerm)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, $"Vai trò của bạn chưa được cấp quyền '{permissionCode}' để thực hiện hành động này.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Create(
        CreateStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
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
        await EnsureHasPermissionAsync("requests.read", cancellationToken);
        var result = await _studentApplicationService.GetOwnAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<StudentApplicationListItemDto>>.Ok(result, "Lấy danh sách đơn từ thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.read", cancellationToken);
        var result = await _studentApplicationService.GetOwnDetailAsync(id, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Lấy chi tiết đơn từ thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Update(
        int id,
        UpdateStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
        var result = await _studentApplicationService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Cập nhật đơn từ thành công."));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Submit(
        int id,
        SubmitStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
        var result = await _studentApplicationService.SubmitAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Nộp đơn thành công."));
    }

    [HttpPost("{id:int}/resubmit")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Resubmit(
        int id,
        ResubmitStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
        var result = await _studentApplicationService.ResubmitAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Nộp lại đơn thành công."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<StudentApplicationDetailDto>>> Cancel(
        int id,
        CancelStudentApplicationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
        var result = await _studentApplicationService.CancelAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<StudentApplicationDetailDto>.Ok(result, "Hủy đơn thành công."));
    }

    [HttpPost("leave-preview")]
    public async Task<ActionResult<ApiResponseDto<LeaveApplicationPreviewResponseDto>>> PreviewLeaveApplication(
        LeaveApplicationPreviewRequestDto request,
        CancellationToken cancellationToken)
    {
        await EnsureHasPermissionAsync("requests.create", cancellationToken);
        var result = await _studentApplicationService.PreviewLeaveApplicationAsync(request, cancellationToken);
        return Ok(ApiResponseDto<LeaveApplicationPreviewResponseDto>.Ok(result, "Lấy thông tin buổi học thành công."));
    }
}

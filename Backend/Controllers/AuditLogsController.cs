using Backend.Constants;
using Backend.DTOs.Audit;
using Backend.DTOs.Common;
using Backend.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = ReaderRoles)]
public class AuditLogsController : ControllerBase
{
    private const string ReaderRoles =
        AuthRoles.SuperAdmin + "," +
        AuthRoles.Admin + "," +
        AuthRoles.CampusAdmin;

    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AuditLogListItemDto>>>> Get(
        [FromQuery] AuditLogQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AuditLogListItemDto>>.Ok(
            result,
            "Lấy danh sách audit log thành công"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AuditLogDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AuditLogDetailDto>.Ok(
            result,
            "Lấy chi tiết audit log thành công"));
    }
}

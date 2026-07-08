using Backend.Constants;
using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/applications/workflow")]
[Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.AcademicStaff}")]
public class AdminWorkflowController : ControllerBase
{
    private readonly IApplicationWorkflowService _workflowService;

    public AdminWorkflowController(IApplicationWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<WorkflowConfigDto>>>> GetWorkflows(CancellationToken cancellationToken)
    {
        var workflows = await _workflowService.GetWorkflowsAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<WorkflowConfigDto>>.Ok(workflows, "Lấy cấu hình quy trình thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<WorkflowConfigDto>>> UpdateWorkflow(
        int id,
        UpdateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _workflowService.UpdateWorkflowAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<WorkflowConfigDto>.Ok(updated, "Cập nhật quy trình thành công."));
    }
}

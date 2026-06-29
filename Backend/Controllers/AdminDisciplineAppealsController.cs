using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/discipline-appeals")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.AcademicStaff}")]
public class AdminDisciplineAppealsController : ControllerBase
{
    private readonly IDisciplineAppealService _service;

    public AdminDisciplineAppealsController(IDisciplineAppealService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DisciplineAppealListItemDto>>> GetAppeals(
        [FromQuery] DisciplineAppealQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDisciplineAppealsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DisciplineAppealDetailDto>> GetAppealDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDisciplineAppealDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<DisciplineAppealDetailDto>> ResolveAppeal(
        int id,
        [FromBody] ResolveDisciplineAppealRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ResolveDisciplineAppealAsync(id, request, cancellationToken);
        return Ok(result);
    }
}

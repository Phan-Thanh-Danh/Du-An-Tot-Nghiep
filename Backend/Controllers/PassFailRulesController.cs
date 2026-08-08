using Backend.DTOs.Auth;
using Backend.DTOs.PassFailRules;
using Backend.Services.PassFailRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/pass-fail-rules")]
[Authorize]
public class PassFailRulesController : ControllerBase
{
    private readonly IPassFailRuleService _service;

    public PassFailRulesController(IPassFailRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "AcademicOperations")]
    [ProducesResponseType(typeof(PassFailRuleListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int? maHocKy,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetListAsync(maHocKy, search, pageIndex, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    [ProducesResponseType(typeof(PassFailRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetAsync(id, ct);
        if (result is null)
        {
            return NotFound(new { message = "Không tìm thấy cấu hình điểm của môn học." });
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AcademicOperations")]
    [ProducesResponseType(typeof(PassFailRuleDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UpsertPassFailRuleRequest request, CancellationToken ct = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var result = await _service.CreateAsync(request, currentUser?.UserId, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    [ProducesResponseType(typeof(PassFailRuleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertPassFailRuleRequest request, CancellationToken ct = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var result = await _service.UpdateAsync(id, request, currentUser?.UserId, ct);
        return Ok(result);
    }
}

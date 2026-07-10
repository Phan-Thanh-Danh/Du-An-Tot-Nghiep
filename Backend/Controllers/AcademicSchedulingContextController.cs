using Backend.DTOs.Common;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Services.AcademicSchedulingContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/academic-scheduling/context")]
[Authorize(Policy = "AcademicOperations")]
public class AcademicSchedulingContextController : ControllerBase
{
    private readonly IAcademicSchedulingContextService _contextService;

    public AcademicSchedulingContextController(IAcademicSchedulingContextService contextService)
    {
        _contextService = contextService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<AcademicSchedulingContextDto>>> GetContext(CancellationToken cancellationToken)
    {
        var campusIdClaim = User.FindFirst("CampusId")?.Value;
        if (string.IsNullOrEmpty(campusIdClaim) || !int.TryParse(campusIdClaim, out var campusId))
        {
            return BadRequest(ApiResponseDto.Fail("Không xác định được cơ sở của người dùng."));
        }

        var context = await _contextService.GetContextAsync(campusId, cancellationToken);
        return Ok(ApiResponseDto<AcademicSchedulingContextDto>.Ok(context));
    }
}

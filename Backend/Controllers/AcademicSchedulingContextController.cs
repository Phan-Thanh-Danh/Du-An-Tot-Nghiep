using Backend.Constants;
using Backend.DTOs.Auth;
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
    public async Task<ActionResult<ApiResponseDto<AcademicSchedulingContextDto>>> GetContext(
        [FromQuery(Name = "campusId")] int? campusId,
        CancellationToken cancellationToken)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userCampusId = currentUser?.CampusId ?? 0;
        // Query and header values never broaden a campus-scoped role's claim.
        var isGlobal = currentUser?.Role == AuthRoles.SuperAdmin;

        int requestedCampusId = 0;
        if (campusId.HasValue && campusId.Value > 0)
        {
            requestedCampusId = campusId.Value;
        }
        else if (int.TryParse(Request.Query["campusId"].FirstOrDefault(), out var qCampusId) && qCampusId > 0)
        {
            requestedCampusId = qCampusId;
        }
        else if (int.TryParse(Request.Headers["X-Campus-Id"].FirstOrDefault(), out var hCampusId) && hCampusId > 0)
        {
            requestedCampusId = hCampusId;
        }

        if (requestedCampusId > 0 && !isGlobal && requestedCampusId != userCampusId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseDto.Fail("Bạn không có quyền truy cập dữ liệu của cơ sở khác."));
        }

        int effectiveCampusId = (isGlobal && requestedCampusId > 0) ? requestedCampusId : (userCampusId > 0 ? userCampusId : requestedCampusId);

        if (effectiveCampusId <= 0)
        {
            return BadRequest(ApiResponseDto.Fail("Không xác định được cơ sở của người dùng."));
        }

        var context = await _contextService.GetContextAsync(effectiveCampusId, cancellationToken);
        return Ok(ApiResponseDto<AcademicSchedulingContextDto>.Ok(context));
    }
}

using Backend.Constants;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/rewards")]
[Authorize(Roles = $"{AuthRoles.SuperAdmin},{AuthRoles.Admin},{AuthRoles.CampusAdmin}")]
public class AdminRewardCertificatesController : ControllerBase
{
    private readonly ICertificateGenerationService _certificateGenerationService;

    public AdminRewardCertificatesController(ICertificateGenerationService certificateGenerationService)
    {
        _certificateGenerationService = certificateGenerationService;
    }

    [HttpGet("{rewardId:int}/certificate/download")]
    public async Task<IActionResult> Download(int rewardId, CancellationToken cancellationToken)
    {
        var result = await _certificateGenerationService.DownloadAsync(rewardId, cancellationToken);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, no-store";
        return File(result.Content, result.ContentType, result.FileName);
    }
}

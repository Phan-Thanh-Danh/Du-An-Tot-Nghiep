using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/rewards")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentRewardsController : ControllerBase
{
    private readonly IStudentRewardService _studentRewardService;

    public StudentRewardsController(IStudentRewardService studentRewardService)
    {
        _studentRewardService = studentRewardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentRewardListItemDto>>>> GetMyRewards(
        [FromQuery] StudentRewardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _studentRewardService.GetMyRewardsAsync(userId, parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<StudentRewardListItemDto>>.Ok(
            result,
            "Lấy danh sách khen thưởng thành công."));
    }

    [HttpGet("{rewardId:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentRewardDetailDto>>> GetMyRewardById(
        int rewardId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _studentRewardService.GetMyRewardByIdAsync(userId, rewardId, cancellationToken);
        return Ok(ApiResponseDto<StudentRewardDetailDto>.Ok(
            result,
            "Lấy chi tiết khen thưởng thành công."));
    }

    [HttpGet("{rewardId:int}/certificate/download")]
    public async Task<IActionResult> DownloadCertificate(
        int rewardId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _studentRewardService.DownloadMyCertificateAsync(userId, rewardId, cancellationToken);

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, no-store";

        return File(result.Content, result.ContentType, result.FileName);
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
        {
            return currentUser.UserId;
        }

        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

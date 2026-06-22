using System.Security.Claims;
using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.QuizAttempts;
using Backend.Services.QuizAttempts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/quiz-attempts")]
[Authorize(Roles = AuthRoles.Student)]
public class QuizAttemptsController : ControllerBase
{
    private readonly IQuizAttemptService _quizAttemptService;

    public QuizAttemptsController(IQuizAttemptService quizAttemptService)
    {
        _quizAttemptService = quizAttemptService;
    }

    [HttpGet("{quizId:int}/availability")]
    public async Task<ActionResult<ApiResponseDto<QuizAvailabilityDto>>> GetAvailability(int quizId, CancellationToken ct)
    {
        var result = await _quizAttemptService.GetAvailabilityAsync(quizId, GetCurrentUserId(), ct);
        return Ok(ApiResponseDto<QuizAvailabilityDto>.Ok(result));
    }

    [HttpPost("{quizId:int}/start")]
    public async Task<ActionResult<ApiResponseDto<StartQuizAttemptResponse>>> Start(int quizId, CancellationToken ct)
    {
        var result = await _quizAttemptService.StartAsync(quizId, GetCurrentUserId(), ct);
        return Ok(ApiResponseDto<StartQuizAttemptResponse>.Ok(result, "Bắt đầu lượt làm quiz thành công"));
    }

    [HttpPut("sessions/{attemptId:int}/autosave")]
    public async Task<ActionResult<ApiResponseDto>> AutoSave(int attemptId, SaveQuizAnswersRequest request, CancellationToken ct)
    {
        await _quizAttemptService.SaveAnswersAsync(attemptId, request, GetCurrentUserId(), ct);
        return Ok(ApiResponseDto.Ok("Đã lưu tạm câu trả lời"));
    }

    [HttpPost("sessions/{attemptId:int}/submit")]
    public async Task<ActionResult<ApiResponseDto<QuizAttemptResultDto>>> Submit(int attemptId, SubmitQuizAttemptRequest request, CancellationToken ct)
    {
        var result = await _quizAttemptService.SubmitAsync(attemptId, request, GetCurrentUserId(), ct);
        return Ok(ApiResponseDto<QuizAttemptResultDto>.Ok(result, "Nộp bài quiz thành công"));
    }

    [HttpGet("{quizId:int}/history")]
    public async Task<ActionResult<ApiResponseDto<QuizAttemptHistoryDto>>> GetHistory(int quizId, CancellationToken ct)
    {
        var result = await _quizAttemptService.GetHistoryAsync(quizId, GetCurrentUserId(), ct);
        return Ok(ApiResponseDto<QuizAttemptHistoryDto>.Ok(result));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
        {
            return currentUser.UserId;
        }

        var claim = User.FindFirstValue(CustomClaimTypes.UserId);
        return claim != null ? int.Parse(claim) : 0;
    }
}

using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.QuizManagement;
using Backend.Services.QuizManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/exam/de-kiem-tra")]
[Authorize(Roles = AuthRoles.HoiDongQuanLyNoiDung)]
public class QuizManagementController : ControllerBase
{
    private readonly IQuizManagementService _quizService;

    public QuizManagementController(IQuizManagementService quizService)
    {
        _quizService = quizService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.Items["CurrentUser"];
        if (userIdClaim is Backend.Models.NguoiDung user)
            return user.MaNguoiDung;

        var claim = User.FindFirst(CustomClaimTypes.UserId);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<QuizDto>>>> GetQuizzes([FromQuery] QuizFilterDto filter, CancellationToken ct)
    {
        var result = await _quizService.GetQuizzesAsync(filter, ct);
        return Ok(ApiResponseDto<PagedResultDto<QuizDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<QuizDetailDto>>> GetQuizById(int id, CancellationToken ct)
    {
        var result = await _quizService.GetQuizByIdAsync(id, ct);
        return Ok(ApiResponseDto<QuizDetailDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<QuizDetailDto>>> CreateQuiz(CreateQuizRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _quizService.CreateQuizAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetQuizById), new { id = result.MaDeKiemTra }, ApiResponseDto<QuizDetailDto>.Ok(result, "Tạo Đề kiểm tra thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<QuizDetailDto>>> UpdateQuiz(int id, UpdateQuizRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _quizService.UpdateQuizAsync(id, request, userId, ct);
        return Ok(ApiResponseDto<QuizDetailDto>.Ok(result, "Cập nhật Đề kiểm tra thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteQuiz(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.DeleteQuizAsync(id, userId, ct);
        return Ok(ApiResponseDto.Ok("Xóa Đề kiểm tra thành công"));
    }

    [HttpGet("{id:int}/cau-hoi")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuizQuestionDto>>>> GetQuizQuestions(int id, CancellationToken ct)
    {
        var result = await _quizService.GetQuizQuestionsAsync(id, ct);
        return Ok(ApiResponseDto<IReadOnlyList<QuizQuestionDto>>.Ok(result));
    }

    [HttpPost("{id:int}/cau-hoi")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuizQuestionDto>>>> AssignQuestions(int id, AssignQuizQuestionsRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _quizService.AssignQuestionsAsync(id, request, userId, ct);
        return Ok(ApiResponseDto<IReadOnlyList<QuizQuestionDto>>.Ok(result, "Gán câu hỏi thành công"));
    }

    [HttpPut("{id:int}/cau-hoi")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuizQuestionDto>>>> ReplaceQuestions(int id, AssignQuizQuestionsRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _quizService.ReplaceQuestionsAsync(id, request, userId, ct);
        return Ok(ApiResponseDto<IReadOnlyList<QuizQuestionDto>>.Ok(result, "Cập nhật toàn bộ câu hỏi thành công"));
    }

    [HttpPut("{id:int}/cau-hoi/{maCauHoi:int}")]
    public async Task<ActionResult<ApiResponseDto<QuizQuestionDto>>> UpdateQuestion(int id, int maCauHoi, UpdateQuizQuestionRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _quizService.UpdateQuestionAsync(id, maCauHoi, request, userId, ct);
        return Ok(ApiResponseDto<QuizQuestionDto>.Ok(result, "Cập nhật điểm/thứ tự câu hỏi thành công"));
    }

    [HttpDelete("{id:int}/cau-hoi/{maCauHoi:int}")]
    public async Task<ActionResult<ApiResponseDto>> RemoveQuestion(int id, int maCauHoi, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.RemoveQuestionAsync(id, maCauHoi, userId, ct);
        return Ok(ApiResponseDto.Ok("Xóa câu hỏi khỏi Đề kiểm tra thành công"));
    }

    [HttpPut("{id:int}/cau-hoi/reorder")]
    public async Task<ActionResult<ApiResponseDto>> ReorderQuestions(int id, ReorderQuizQuestionsRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.ReorderQuestionsAsync(id, request, userId, ct);
        return Ok(ApiResponseDto.Ok("Sắp xếp câu hỏi thành công"));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<ApiResponseDto>> PublishQuiz(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.PublishQuizAsync(id, userId, ct);
        return Ok(ApiResponseDto.Ok("Xuất bản Đề kiểm tra thành công"));
    }

    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<ApiResponseDto>> UnpublishQuiz(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.UnpublishQuizAsync(id, userId, ct);
        return Ok(ApiResponseDto.Ok("Chuyển Đề kiểm tra về trạng thái nháp thành công"));
    }

    [HttpPost("{id:int}/open")]
    public async Task<ActionResult<ApiResponseDto>> OpenQuiz(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.OpenQuizAsync(id, userId, ct);
        return Ok(ApiResponseDto.Ok("Mở quiz thành công"));
    }

    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<ApiResponseDto>> CloseQuiz(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _quizService.CloseQuizAsync(id, userId, ct);
        return Ok(ApiResponseDto.Ok("Đóng quiz thành công"));
    }
}

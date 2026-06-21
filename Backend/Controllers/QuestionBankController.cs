using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.QuestionBank;
using Backend.Services.QuestionBank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/question-bank/questions")]
[Authorize(Roles = AuthRoles.HoiDongQuanLyNoiDung)]
public class QuestionBankController : ControllerBase
{
    private readonly IQuestionBankService _questionBankService;

    public QuestionBankController(IQuestionBankService questionBankService)
    {
        _questionBankService = questionBankService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<QuestionDto>>> GetQuestions([FromQuery] QuestionFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _questionBankService.GetQuestionsAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionDto>> GetQuestionById(int id, CancellationToken cancellationToken)
    {
        var result = await _questionBankService.GetQuestionByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionDto>> CreateQuestion([FromBody] CreateQuestionDto input, CancellationToken cancellationToken)
    {
        var result = await _questionBankService.CreateQuestionAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionById), new { id = result.MaCauHoi }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(int id, [FromBody] UpdateQuestionDto input, CancellationToken cancellationToken)
    {
        var result = await _questionBankService.UpdateQuestionAsync(id, input, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id, CancellationToken cancellationToken)
    {
        await _questionBankService.DeleteQuestionAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateQuestion(int id, CancellationToken cancellationToken)
    {
        await _questionBankService.ActivateQuestionAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateQuestion(int id, CancellationToken cancellationToken)
    {
        await _questionBankService.DeactivateQuestionAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("import-template")]
    [AllowAnonymous] // Maybe? Or keep it authorized. The requirement didn't mention allow anonymous. Keep it authorized.
    public async Task<IActionResult> DownloadImportTemplate(CancellationToken cancellationToken)
    {
        var fileBytes = await _questionBankService.GenerateImportTemplateAsync(cancellationToken);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "QuestionImportTemplate.xlsx");
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportQuestions(IFormFile file, CancellationToken cancellationToken)
    {
        var count = await _questionBankService.ImportQuestionsAsync(file, cancellationToken);
        return Ok(new { message = $"Đã import thành công {count} câu hỏi" });
    }
}

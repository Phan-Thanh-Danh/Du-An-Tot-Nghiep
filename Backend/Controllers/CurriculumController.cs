using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Curriculum;
using Backend.Services.Curriculum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/curriculum")]
[Authorize]
public class CurriculumController : ControllerBase
{
    private readonly ICurriculumService _service;

    public CurriculumController(ICurriculumService service)
    {
        _service = service;
    }

    private CurrentUserContext CurrentUser =>
        (CurrentUserContext)HttpContext.Items["CurrentUser"]!;

    // ═════════════════════════════════════════════════════════
    //  CHAPTERS
    // ═════════════════════════════════════════════════════════

    [HttpGet("subjects/{subjectId}/chapters")]
    public async Task<IActionResult> GetChapters(int subjectId, CancellationToken ct)
    {
        var result = await _service.GetChaptersBySubjectAsync(subjectId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("chapters/{id}")]
    public async Task<IActionResult> GetChapter(int id, CancellationToken ct)
    {
        var result = await _service.GetChapterByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("chapters")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> CreateChapter([FromBody] ChuongCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateChapterAsync(dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return StatusCode(StatusCodes.Status201Created, new { success = true, data = result });
    }

    [HttpPut("chapters/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> UpdateChapter(int id, [FromBody] ChuongUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateChapterAsync(id, dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("chapters/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> DeleteChapter(int id, CancellationToken ct)
    {
        await _service.DeleteChapterAsync(id, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, message = "Xóa chương thành công." });
    }

    [HttpPut("subjects/{subjectId}/chapters/reorder")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> ReorderChapters(int subjectId, [FromBody] ReorderRequestDto dto, CancellationToken ct)
    {
        await _service.ReorderChaptersAsync(subjectId, dto, ct);
        return Ok(new { success = true, message = "Sắp xếp thứ tự chương thành công." });
    }

    // ═════════════════════════════════════════════════════════
    //  LESSONS
    // ═════════════════════════════════════════════════════════

    [HttpGet("chapters/{chapterId}/lessons")]
    public async Task<IActionResult> GetLessons(int chapterId, CancellationToken ct)
    {
        var result = await _service.GetLessonsByChapterAsync(chapterId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("lessons/{id}")]
    public async Task<IActionResult> GetLesson(int id, CancellationToken ct)
    {
        var result = await _service.GetLessonByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("lessons")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> CreateLesson([FromBody] BaiHocCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateLessonAsync(dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return StatusCode(StatusCodes.Status201Created, new { success = true, data = result });
    }

    [HttpPut("lessons/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] BaiHocUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateLessonAsync(id, dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("lessons/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> DeleteLesson(int id, CancellationToken ct)
    {
        await _service.DeleteLessonAsync(id, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, message = "Xóa bài học thành công." });
    }

    [HttpPut("chapters/{chapterId}/lessons/reorder")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> ReorderLessons(int chapterId, [FromBody] ReorderRequestDto dto, CancellationToken ct)
    {
        await _service.ReorderLessonsAsync(chapterId, dto, ct);
        return Ok(new { success = true, message = "Sắp xếp thứ tự bài học thành công." });
    }

    // ═════════════════════════════════════════════════════════
    //  CONTENT BLOCKS
    // ═════════════════════════════════════════════════════════

    [HttpGet("lessons/{lessonId}/content")]
    public async Task<IActionResult> GetContent(int lessonId, CancellationToken ct)
    {
        var result = await _service.GetContentByLessonAsync(lessonId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("content/{id}")]
    public async Task<IActionResult> GetContentById(int id, CancellationToken ct)
    {
        var result = await _service.GetContentByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("content")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> CreateContent([FromBody] BaiHocNoiDungCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateContentAsync(dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return StatusCode(StatusCodes.Status201Created, new { success = true, data = result });
    }

    [HttpPut("content/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> UpdateContent(int id, [FromBody] BaiHocNoiDungUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateContentAsync(id, dto, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("content/{id}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> DeleteContent(int id, CancellationToken ct)
    {
        await _service.DeleteContentAsync(id, CurrentUser.UserId, CurrentUser.CampusId, ct);
        return Ok(new { success = true, message = "Xóa nội dung bài học thành công." });
    }

    [HttpPut("lessons/{lessonId}/content/reorder")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<IActionResult> ReorderContent(int lessonId, [FromBody] ReorderRequestDto dto, CancellationToken ct)
    {
        await _service.ReorderContentAsync(lessonId, dto, ct);
        return Ok(new { success = true, message = "Sắp xếp thứ tự nội dung thành công." });
    }
}

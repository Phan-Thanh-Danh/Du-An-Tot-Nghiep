using Backend.DTOs.TeachingPreferences;
using Backend.Services.TeachingPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/teacher/teaching-preferences")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class TeacherTeachingPreferencesController : ControllerBase
{
    private readonly ITeachingPreferenceService _service;

    public TeacherTeachingPreferencesController(ITeachingPreferenceService service)
    {
        _service = service;
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is Backend.DTOs.Auth.CurrentUserContext user)
            return user.UserId;
        throw new UnauthorizedAccessException("User context is missing.");
    }

    [HttpGet("context")]
    public async Task<IActionResult> GetContext()
    {
        var result = await _service.GetContextAsync(GetCurrentUserId());
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{maHocKy}")]
    public async Task<IActionResult> GetForm(int maHocKy)
    {
        var result = await _service.GetTeacherFormAsync(GetCurrentUserId(), maHocKy);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{maHocKy}")]
    public async Task<IActionResult> SaveDraft(int maHocKy, [FromBody] UpdateTeachingPreferenceDto dto)
    {
        var result = await _service.SaveDraftAsync(GetCurrentUserId(), maHocKy, dto);
        return Ok(new { success = true, data = result, message = "Lưu nháp thành công." });
    }

    [HttpPost("{maHocKy}/submit")]
    public async Task<IActionResult> Submit(int maHocKy, [FromBody] SubmitTeachingPreferenceDto dto)
    {
        var result = await _service.SubmitAsync(GetCurrentUserId(), maHocKy, dto);
        return Ok(new { success = true, data = result, message = "Đã gửi nguyện vọng thành công." });
    }
}

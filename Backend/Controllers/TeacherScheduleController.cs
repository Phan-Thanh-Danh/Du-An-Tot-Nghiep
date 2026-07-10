using Backend.DTOs.Common;
using Backend.DTOs.Auth;
using Backend.DTOs.TeacherSchedule;
using Backend.Services.TeacherSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher/schedule")]
[Authorize]
public class TeacherScheduleController : ControllerBase
{
    private readonly ITeacherScheduleService _scheduleService;

    public TeacherScheduleController(ITeacherScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    private int GetCurrentTeacherId()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null || currentUser.Role != "Teacher")
        {
            throw new UnauthorizedAccessException("Người dùng hiện tại không phải là Giảng viên hoặc chưa đăng nhập.");
        }
        return currentUser.UserId;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponseDto<TeacherScheduleSummaryDto>>> GetSummary()
    {
        var teacherId = GetCurrentTeacherId();
        var summary = await _scheduleService.GetSummaryAsync(teacherId);
        return Ok(ApiResponseDto<TeacherScheduleSummaryDto>.Ok(summary));
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponseDto<List<TeacherScheduleItemDto>>>> GetTodaySchedule()
    {
        var teacherId = GetCurrentTeacherId();
        var schedule = await _scheduleService.GetTodayScheduleAsync(teacherId);
        return Ok(ApiResponseDto<List<TeacherScheduleItemDto>>.Ok(schedule));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherScheduleItemDto>>>> GetSchedule([FromQuery] TeacherScheduleQueryDto query)
    {
        var teacherId = GetCurrentTeacherId();
        var pagedResult = await _scheduleService.GetScheduleAsync(teacherId, query);
        return Ok(ApiResponseDto<PagedResultDto<TeacherScheduleItemDto>>.Ok(pagedResult));
    }

    [HttpGet("terms")]
    public async Task<ActionResult<ApiResponseDto<List<TeacherScheduleTermDto>>>> GetTerms()
    {
        var teacherId = GetCurrentTeacherId();
        var terms = await _scheduleService.GetTermsAsync(teacherId);
        return Ok(ApiResponseDto<List<TeacherScheduleTermDto>>.Ok(terms));
    }
}

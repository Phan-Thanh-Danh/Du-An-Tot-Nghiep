using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentSchedule;
using Backend.Services.StudentSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/schedule")]
[Authorize]
public class StudentScheduleController : ControllerBase
{
    private readonly IStudentScheduleService _scheduleService;
    private readonly Data.ApplicationDbContext _db;

    public StudentScheduleController(IStudentScheduleService scheduleService, Data.ApplicationDbContext db)
    {
        _scheduleService = scheduleService;
        _db = db;
    }

    private async Task<(int studentId, int classId)?> GetCurrentUserAndClassAsync()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null) return null;

        var user = await _db.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUser.UserId);

        if (user == null || user.MaLop == null)
            return null; // The student is not in a class or doesn't exist

        return (user.MaNguoiDung, user.MaLop.Value);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponseDto<StudentScheduleSummaryDto>>> GetSummary()
    {
        var userContext = await GetCurrentUserAndClassAsync();
        if (userContext == null)
            return Ok(ApiResponseDto<StudentScheduleSummaryDto>.Ok(new StudentScheduleSummaryDto(), "Sinh viên chưa được xếp lớp."));

        var summary = await _scheduleService.GetScheduleSummaryAsync(userContext.Value.studentId, userContext.Value.classId);
        return Ok(ApiResponseDto<StudentScheduleSummaryDto>.Ok(summary));
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponseDto<List<StudentScheduleItemDto>>>> GetToday()
    {
        var userContext = await GetCurrentUserAndClassAsync();
        if (userContext == null)
            return Ok(ApiResponseDto<List<StudentScheduleItemDto>>.Ok(new List<StudentScheduleItemDto>(), "Sinh viên chưa được xếp lớp."));

        var today = await _scheduleService.GetTodayScheduleAsync(userContext.Value.studentId, userContext.Value.classId);
        return Ok(ApiResponseDto<List<StudentScheduleItemDto>>.Ok(today));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentScheduleItemDto>>>> GetSchedule([FromQuery] StudentScheduleQueryDto query)
    {
        var userContext = await GetCurrentUserAndClassAsync();
        if (userContext == null)
            return Ok(ApiResponseDto<PagedResultDto<StudentScheduleItemDto>>.Ok(new PagedResultDto<StudentScheduleItemDto> { Items = new List<StudentScheduleItemDto>(), TotalItems = 0, PageIndex = 1, PageSize = 10 }, "Sinh viên chưa được xếp lớp."));

        if (query.NgayTu > query.NgayDen)
            return BadRequest(ApiResponseDto.Fail("Ngày bắt đầu không thể lớn hơn ngày kết thúc"));

        var result = await _scheduleService.GetScheduleAsync(userContext.Value.studentId, userContext.Value.classId, query);
        return Ok(ApiResponseDto<PagedResultDto<StudentScheduleItemDto>>.Ok(result));
    }

    [HttpGet("terms")]
    public async Task<ActionResult<ApiResponseDto<List<StudentScheduleTermDto>>>> GetTerms()
    {
        var userContext = await GetCurrentUserAndClassAsync();
        if (userContext == null)
            return Ok(ApiResponseDto<List<StudentScheduleTermDto>>.Ok(new List<StudentScheduleTermDto>(), "Sinh viên chưa được xếp lớp."));

        var terms = await _scheduleService.GetScheduleTermsAsync(userContext.Value.studentId, userContext.Value.classId);
        return Ok(ApiResponseDto<List<StudentScheduleTermDto>>.Ok(terms));
    }
}

using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/discipline-records")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentDisciplineRecordsController : ControllerBase
{
    private readonly IDisciplineRecordService _recordService;
    private readonly IDisciplineAppealService _appealService;

    public StudentDisciplineRecordsController(
        IDisciplineRecordService recordService,
        IDisciplineAppealService appealService)
    {
        _recordService = recordService;
        _appealService = appealService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<StudentDisciplineRecordListItemDto>>> GetMyRecords(
        [FromQuery] StudentDisciplineRecordQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _recordService.GetStudentDisciplineRecordsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDisciplineRecordDetailDto>> GetMyRecordDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _recordService.GetStudentDisciplineRecordDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/appeals")]
    public async Task<ActionResult<DisciplineAppealListItemDto>> CreateAppeal(
        int id,
        [FromBody] CreateDisciplineAppealRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appealService.CreateDisciplineAppealAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("appeals/{appealId}")]
    public async Task<ActionResult<DisciplineAppealDetailDto>> GetMyAppealDetail(
        int appealId,
        CancellationToken cancellationToken)
    {
        // GetDisciplineAppealDetailAsync handles role-based access to ensure student only sees their own
        var result = await _appealService.GetDisciplineAppealDetailAsync(appealId, cancellationToken);
        return Ok(result);
    }
}

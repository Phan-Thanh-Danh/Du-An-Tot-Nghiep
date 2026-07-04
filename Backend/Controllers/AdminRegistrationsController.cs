using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Registrations;
using Backend.Services.Registrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin},{AuthRoles.SubCampusAdmin},{AuthRoles.AcademicStaff}")]
public class AdminRegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public AdminRegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpGet("registration-periods")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<RegistrationPeriodDto>>>> GetPeriods(CancellationToken cancellationToken)
    {
        var periods = await _registrationService.GetPeriodsAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<RegistrationPeriodDto>>.Ok(periods));
    }

    [HttpGet("registration-periods/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RegistrationPeriodDto>>> GetPeriod(int id, CancellationToken cancellationToken)
    {
        var period = await _registrationService.GetPeriodAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RegistrationPeriodDto>.Ok(period));
    }

    [HttpPost("registration-periods")]
    public async Task<ActionResult<ApiResponseDto<RegistrationPeriodDto>>> CreatePeriod(
        UpsertRegistrationPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var period = await _registrationService.CreatePeriodAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPeriod), new { id = period.Id }, ApiResponseDto<RegistrationPeriodDto>.Ok(period, "Tạo đợt đăng ký thành công"));
    }

    [HttpPut("registration-periods/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RegistrationPeriodDto>>> UpdatePeriod(
        int id,
        UpsertRegistrationPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var period = await _registrationService.UpdatePeriodAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<RegistrationPeriodDto>.Ok(period, "Cập nhật đợt đăng ký thành công"));
    }

    [HttpPost("registration-periods/{id:int}/open")]
    public async Task<ActionResult<ApiResponseDto<RegistrationPeriodDto>>> OpenPeriod(int id, CancellationToken cancellationToken)
    {
        var period = await _registrationService.OpenPeriodAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RegistrationPeriodDto>.Ok(period, "Đã mở đợt đăng ký"));
    }

    [HttpPost("registration-periods/{id:int}/close")]
    public async Task<ActionResult<ApiResponseDto<RegistrationPeriodDto>>> ClosePeriod(int id, CancellationToken cancellationToken)
    {
        var period = await _registrationService.ClosePeriodAsync(id, cancellationToken);
        return Ok(ApiResponseDto<RegistrationPeriodDto>.Ok(period, "Đã đóng đợt đăng ký"));
    }

    [HttpDelete("registration-periods/{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteDraftPeriod(int id, CancellationToken cancellationToken)
    {
        await _registrationService.DeleteDraftPeriodAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Đã xóa đợt đăng ký nháp"));
    }

    [HttpGet("registration-periods/{id:int}/registrations")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>>> GetPeriodRegistrations(
        int id,
        CancellationToken cancellationToken)
    {
        var registrations = await _registrationService.GetPeriodRegistrationsAsync(id, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>.Ok(registrations));
    }

    [HttpGet("registrations")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<AdminRegistrationResultDto>>>> GetRegistrationResults(CancellationToken cancellationToken)
    {
        var registrations = await _registrationService.GetRegistrationResultsAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<AdminRegistrationResultDto>>.Ok(registrations));
    }

    [HttpGet("course-sections/capacity")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>>> GetCourseSectionCapacity(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var sections = await _registrationService.GetCourseSectionsAsync(status, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>.Ok(sections));
    }

    [HttpPut("course-sections/{id:int}/capacity")]
    public async Task<ActionResult<ApiResponseDto<CourseSectionRegistrationDto>>> UpdateCapacity(
        int id,
        UpdateCourseSectionCapacityRequest request,
        CancellationToken cancellationToken)
    {
        var section = await _registrationService.UpdateCapacityAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CourseSectionRegistrationDto>.Ok(section, "Cập nhật sức chứa thành công"));
    }

    [HttpPost("course-sections/{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<CourseSectionRegistrationDto>>> CancelSection(
        int id,
        CourseSectionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var section = await _registrationService.CancelSectionAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CourseSectionRegistrationDto>.Ok(section, "Đã hủy lớp học phần"));
    }

    [HttpPost("course-sections/{id:int}/reopen")]
    public async Task<ActionResult<ApiResponseDto<CourseSectionRegistrationDto>>> ReopenSection(
        int id,
        CourseSectionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var section = await _registrationService.ReopenSectionAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CourseSectionRegistrationDto>.Ok(section, "Đã mở lại lớp học phần"));
    }
}

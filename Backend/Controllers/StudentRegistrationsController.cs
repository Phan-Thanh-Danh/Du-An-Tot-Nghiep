using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Registrations;
using Backend.Services.Registrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/registrations")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentRegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public StudentRegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>>> GetAvailable(CancellationToken cancellationToken)
    {
        var sections = await _registrationService.GetAvailableSectionsForStudentAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<CourseSectionRegistrationDto>>.Ok(sections));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<StudentRegistrationDto>>>> GetMine(CancellationToken cancellationToken)
    {
        var registrations = await _registrationService.GetStudentRegistrationsAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<StudentRegistrationDto>>.Ok(registrations));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<StudentRegistrationDto>>> Enroll(
        StudentEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var registration = await _registrationService.EnrollAsync(request, cancellationToken);
        return Ok(ApiResponseDto<StudentRegistrationDto>.Ok(registration, "Đăng ký học phần thành công"));
    }

    [HttpPost("{id:int}/withdraw")]
    public async Task<ActionResult<ApiResponseDto<StudentRegistrationDto>>> Withdraw(int id, CancellationToken cancellationToken)
    {
        var registration = await _registrationService.WithdrawAsync(id, cancellationToken);
        return Ok(ApiResponseDto<StudentRegistrationDto>.Ok(registration, "Hủy đăng ký học phần thành công"));
    }
}

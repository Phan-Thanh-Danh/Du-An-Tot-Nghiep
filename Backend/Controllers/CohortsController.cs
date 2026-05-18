using Backend.DTOs.Cohorts;
using Backend.DTOs.Common;
using Backend.Services.Cohorts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/cohorts")]
[Authorize(Policy = "AcademicOperations")]
public class CohortsController : ControllerBase
{
    private readonly ICohortService _cohortService;

    public CohortsController(ICohortService cohortService)
    {
        _cohortService = cohortService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CohortDto>>>> Get(
        [FromQuery] CohortQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var cohorts = await _cohortService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<CohortDto>>.Ok(cohorts, "Lấy danh sách khóa tuyển sinh thành công"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CohortDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var cohort = await _cohortService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CohortDto>.Ok(cohort, "Lấy thông tin khóa tuyển sinh thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CohortDto>>> Create(
        CreateCohortRequest request,
        CancellationToken cancellationToken)
    {
        var cohort = await _cohortService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = cohort.MaKhoaTuyenSinh },
            ApiResponseDto<CohortDto>.Ok(cohort, "Tạo khóa tuyển sinh thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CohortDto>>> Update(
        int id,
        UpdateCohortRequest request,
        CancellationToken cancellationToken)
    {
        var cohort = await _cohortService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CohortDto>.Ok(cohort, "Cập nhật khóa tuyển sinh thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _cohortService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Vô hiệu hóa khóa tuyển sinh thành công"));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponseDto<CohortDto>>> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        var cohort = await _cohortService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CohortDto>.Ok(cohort, "Kích hoạt khóa tuyển sinh thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<CohortDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var cohort = await _cohortService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CohortDto>.Ok(cohort, "Vô hiệu hóa khóa tuyển sinh thành công"));
    }
}

using Backend.DTOs.BuoiHoc;
using Backend.DTOs.Common;
using Backend.Services.BuoiHoc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/buoi-hoc")]
[Authorize(Policy = "AcademicOperations")]
public class BuoiHocController : ControllerBase
{
    private readonly IBuoiHocService _buoiHocService;

    public BuoiHocController(IBuoiHocService buoiHocService)
    {
        _buoiHocService = buoiHocService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<BuoiHocDto>>>> Get(
        [FromQuery] BuoiHocQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var sessions = await _buoiHocService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<BuoiHocDto>>.Ok(sessions));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<BuoiHocDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var session = await _buoiHocService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<BuoiHocDetailDto>.Ok(session));
    }
}

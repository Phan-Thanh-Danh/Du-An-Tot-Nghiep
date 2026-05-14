using Backend.DTOs.AcademicTerms;
using Backend.DTOs.Common;
using Backend.Services.AcademicTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/academic-terms")]
[Authorize(Policy = "AcademicOperations")]
public class AcademicTermsController : ControllerBase
{
    private readonly IAcademicTermService _termService;

    public AcademicTermsController(IAcademicTermService termService)
    {
        _termService = termService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<AcademicTermDto>>>> GetTerms(
        [FromQuery] AcademicTermQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var terms = await _termService.GetTermsAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<AcademicTermDto>>.Ok(terms));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AcademicTermDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var term = await _termService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AcademicTermDto>.Ok(term));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<AcademicTermDto>>> Create(
        CreateAcademicTermRequest request,
        CancellationToken cancellationToken)
    {
        var term = await _termService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = term.MaHocKy },
            ApiResponseDto<AcademicTermDto>.Ok(term, "Tạo học kỳ thành công"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<AcademicTermDto>>> Update(
        int id,
        UpdateAcademicTermRequest request,
        CancellationToken cancellationToken)
    {
        var term = await _termService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<AcademicTermDto>.Ok(term, "Cập nhật học kỳ thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _termService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa học kỳ thành công"));
    }

    [HttpPatch("{id:int}/lock")]
    public async Task<ActionResult<ApiResponseDto<AcademicTermDto>>> Lock(
        int id,
        CancellationToken cancellationToken)
    {
        var term = await _termService.LockAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AcademicTermDto>.Ok(term, "Khóa học kỳ thành công"));
    }

    [HttpPatch("{id:int}/unlock")]
    public async Task<ActionResult<ApiResponseDto<AcademicTermDto>>> Unlock(
        int id,
        CancellationToken cancellationToken)
    {
        var term = await _termService.UnlockAsync(id, cancellationToken);
        return Ok(ApiResponseDto<AcademicTermDto>.Ok(term, "Mở khóa học kỳ thành công"));
    }
}

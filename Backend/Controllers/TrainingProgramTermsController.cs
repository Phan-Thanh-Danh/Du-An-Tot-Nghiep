using Backend.DTOs.Common;
using Backend.DTOs.TrainingProgramTerms;
using Backend.Services.AcademicTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/training-program-terms")]
[Authorize(Policy = "AcademicOperations")]
public class TrainingProgramTermsController : ControllerBase
{
    private readonly ITrainingProgramTermService _service;

    public TrainingProgramTermsController(ITrainingProgramTermService service)
    {
        _service = service;
    }

    [HttpGet("by-program/{programId:int}")]
    public async Task<ActionResult<ApiResponseDto<List<TrainingProgramTermDto>>>> GetByProgram(
        int programId,
        CancellationToken cancellationToken)
    {
        var items = await _service.GetByProgramAsync(programId, cancellationToken);
        return Ok(ApiResponseDto<List<TrainingProgramTermDto>>.Ok(items));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TrainingProgramTermDto>>> Create(
        CreateTrainingProgramTermRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetByProgram),
            new { programId = item.MaChuongTrinh },
            ApiResponseDto<TrainingProgramTermDto>.Ok(item, "Thêm học kỳ vào chương trình thành công"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto.Ok("Xóa học kỳ khỏi chương trình thành công"));
    }
}

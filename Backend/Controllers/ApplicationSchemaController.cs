using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationSchemaController : ControllerBase
{
    private readonly IApplicationSchemaService _schemaService;

    public ApplicationSchemaController(IApplicationSchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    [HttpGet("schema/types")]
    public ActionResult<ApiResponseDto<IReadOnlyList<ApplicationTypeDto>>> GetTypes()
    {
        var result = _schemaService.GetTypes();
        return Ok(ApiResponseDto<IReadOnlyList<ApplicationTypeDto>>.Ok(result, "Lấy danh sách loại đơn thành công."));
    }

    [HttpGet("schema/statuses")]
    public ActionResult<ApiResponseDto<IReadOnlyList<ApplicationStatusDto>>> GetStatuses()
    {
        var result = _schemaService.GetStatuses();
        return Ok(ApiResponseDto<IReadOnlyList<ApplicationStatusDto>>.Ok(result, "Lấy danh sách trạng thái đơn thành công."));
    }

    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<ApplicationTemplateDto>>>> GetTemplates(
        CancellationToken cancellationToken)
    {
        var result = await _schemaService.GetActiveTemplatesAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<ApplicationTemplateDto>>.Ok(result, "Lấy danh sách mẫu đơn thành công."));
    }

    [HttpGet("templates/{loaiDon}")]
    public async Task<ActionResult<ApiResponseDto<ApplicationTemplateDto>>> GetTemplateByType(
        string loaiDon,
        CancellationToken cancellationToken)
    {
        var result = await _schemaService.GetActiveTemplateByTypeAsync(loaiDon, cancellationToken);
        return Ok(ApiResponseDto<ApplicationTemplateDto>.Ok(result, "Lấy mẫu đơn thành công."));
    }
}

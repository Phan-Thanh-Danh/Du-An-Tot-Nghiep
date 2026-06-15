using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Finance.ProgramTuitionConfigs;
using Backend.Services.Finance.ProgramTuitionConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/finance/program-tuition-configs")]
[Authorize]
public class ProgramTuitionConfigsController : ControllerBase
{
    private readonly IProgramTuitionConfigService _service;

    public ProgramTuitionConfigsController(IProgramTuitionConfigService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigReaders)]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProgramTuitionConfigListItemDto>>>> Get(
        [FromQuery] ProgramTuitionConfigQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var configs = await _service.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ProgramTuitionConfigListItemDto>>.Ok(
            configs,
            "Lấy danh sách cấu hình học phí chương trình thành công"));
    }

    [HttpGet("options")]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigReaders)]
    public async Task<ActionResult<ApiResponseDto<ProgramTuitionConfigOptionsDto>>> GetOptions(
        CancellationToken cancellationToken)
    {
        var options = await _service.GetOptionsAsync(cancellationToken);
        return Ok(ApiResponseDto<ProgramTuitionConfigOptionsDto>.Ok(
            options,
            "Lấy dữ liệu chọn cấu hình học phí chương trình thành công"));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigReaders)]
    public async Task<ActionResult<ApiResponseDto<ProgramTuitionConfigDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var config = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ProgramTuitionConfigDetailDto>.Ok(
            config,
            "Lấy cấu hình học phí chương trình thành công"));
    }

    [HttpPost]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigManagers)]
    public async Task<ActionResult<ApiResponseDto<ProgramTuitionConfigDetailDto>>> Create(
        CreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = config.Id },
            ApiResponseDto<ProgramTuitionConfigDetailDto>.Ok(
                config,
                "Tạo cấu hình học phí chương trình thành công"));
    }

    [HttpPost("bulk")]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigManagers)]
    public async Task<ActionResult<ApiResponseDto<BulkProgramTuitionConfigResultDto>>> BulkCreate(
        BulkCreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.BulkCreateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BulkProgramTuitionConfigResultDto>.Ok(
            result,
            "Tạo hàng loạt cấu hình học phí chương trình thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigManagers)]
    public async Task<ActionResult<ApiResponseDto<ProgramTuitionConfigDetailDto>>> Update(
        int id,
        UpdateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<ProgramTuitionConfigDetailDto>.Ok(
            config,
            "Cập nhật cấu hình học phí chương trình thành công"));
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.TuitionConfigManagers)]
    public async Task<ActionResult<ApiResponseDto<ProgramTuitionConfigDetailDto>>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var config = await _service.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ProgramTuitionConfigDetailDto>.Ok(
            config,
            "Vô hiệu hóa cấu hình học phí chương trình thành công"));
    }
}

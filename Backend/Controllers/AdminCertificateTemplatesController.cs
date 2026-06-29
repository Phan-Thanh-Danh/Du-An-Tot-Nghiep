using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Services.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/certificate-templates")]
[Authorize(Roles = AuthRoles.SuperAdmin)]
public class AdminCertificateTemplatesController : ControllerBase
{
    private readonly ICertificateTemplateService _certificateTemplateService;

    public AdminCertificateTemplatesController(ICertificateTemplateService certificateTemplateService)
    {
        _certificateTemplateService = certificateTemplateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CertificateTemplateDto>>>> Get(
        [FromQuery] CertificateTemplateQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<CertificateTemplateDto>>.Ok(
            result,
            "Lấy danh sách mẫu bằng khen thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CertificateTemplateDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CertificateTemplateDto>.Ok(
            result,
            "Lấy chi tiết mẫu bằng khen thành công."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CertificateTemplateDto>>> Create(
        CreateCertificateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.MaMauBangKhen },
            ApiResponseDto<CertificateTemplateDto>.Ok(
                result,
                "Tạo mẫu bằng khen thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CertificateTemplateDto>>> Update(
        int id,
        UpdateCertificateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CertificateTemplateDto>.Ok(
            result,
            "Cập nhật mẫu bằng khen thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<CertificateTemplateDto>>> Disable(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.DisableAsync(id, cancellationToken);
        return Ok(ApiResponseDto<CertificateTemplateDto>.Ok(
            result,
            "Vô hiệu hóa mẫu bằng khen thành công."));
    }

    [HttpPost("{id:int}/preview")]
    public async Task<ActionResult<ApiResponseDto<CertificateTemplatePreviewDto>>> Preview(
        int id,
        CertificateTemplatePreviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificateTemplateService.PreviewAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<CertificateTemplatePreviewDto>.Ok(
            result,
            "Tạo preview mẫu bằng khen thành công."));
    }
}

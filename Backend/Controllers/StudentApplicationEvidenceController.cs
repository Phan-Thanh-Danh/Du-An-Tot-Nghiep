using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/applications/{applicationId:int}/attachments")]
[Authorize(Policy = "ApplicationStudent")]
public class StudentApplicationEvidenceController : ControllerBase
{
    private readonly IApplicationEvidenceService _evidenceService;

    public StudentApplicationEvidenceController(IApplicationEvidenceService evidenceService)
    {
        _evidenceService = evidenceService;
    }

    [HttpPost]
    [RequestSizeLimit(27L * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 27L * 1024 * 1024)]
    public async Task<ActionResult<ApiResponseDto<ApplicationEvidenceUploadResponseDto>>> Upload(
        int applicationId,
        [FromForm] List<IFormFile> files,
        [FromForm] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _evidenceService.UploadAsync(applicationId, files, rowVersion, cancellationToken);
        return CreatedAtAction(
            nameof(Download),
            new { applicationId, attachmentId = result.UploadedFiles.FirstOrDefault()?.MaTep ?? 0 },
            ApiResponseDto<ApplicationEvidenceUploadResponseDto>.Ok(result, "Tải minh chứng thành công."));
    }

    [HttpGet("{attachmentId:int}/download")]
    public async Task<IActionResult> Download(
        int applicationId,
        int attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _evidenceService.DownloadAsync(applicationId, attachmentId, cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(result.Content, result.ContentType, result.FileName, enableRangeProcessing: false);
    }

    [HttpDelete("{attachmentId:int}")]
    public async Task<ActionResult<ApiResponseDto<ApplicationEvidenceDeleteResponseDto>>> Delete(
        int applicationId,
        int attachmentId,
        [FromBody] DeleteApplicationEvidenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _evidenceService.DeleteAsync(applicationId, attachmentId, request, cancellationToken);
        return Ok(ApiResponseDto<ApplicationEvidenceDeleteResponseDto>.Ok(result, "Xóa minh chứng thành công."));
    }
}

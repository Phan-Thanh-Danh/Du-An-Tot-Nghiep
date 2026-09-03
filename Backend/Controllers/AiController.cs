using System.Threading;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IOllamaService _ollamaService;
    private readonly IBghAiAnalyticsService _bghAiAnalyticsService;

    public AiController(IOllamaService ollamaService, IBghAiAnalyticsService bghAiAnalyticsService)
    {
        _ollamaService = ollamaService;
        _bghAiAnalyticsService = bghAiAnalyticsService;
    }

    [HttpGet("health")]
    public async Task<ActionResult<ApiResponseDto<AiHealthResponse>>> GetHealth(CancellationToken cancellationToken)
    {
        var health = await _ollamaService.CheckHealthAsync(cancellationToken);
        return Ok(ApiResponseDto<AiHealthResponse>.Ok(health));
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponseDto<AiChatResponse>>> Chat(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var response = await _ollamaService.ChatAsync(request, user, cancellationToken);
        return Ok(ApiResponseDto<AiChatResponse>.Ok(response));
    }

    [HttpPost("embedding-test")]
    [Authorize(Roles = AuthRoles.SuperAdmin + "," + AuthRoles.Admin)]
    public async Task<ActionResult<ApiResponseDto<AiEmbeddingTestResponse>>> TestEmbedding(
        [FromBody] AiEmbeddingTestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ollamaService.TestEmbeddingAsync(request.Text, cancellationToken);
        return Ok(ApiResponseDto<AiEmbeddingTestResponse>.Ok(result));
    }

    [HttpGet("dashboard-insight")]
    public async Task<ActionResult<ApiResponseDto<AiDashboardInsightDto>>> GetDashboardInsight(
        [FromQuery] bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var insight = await _ollamaService.GetDashboardInsightAsync(user, forceRefresh, cancellationToken);
        return Ok(ApiResponseDto<AiDashboardInsightDto>.Ok(insight));
    }

    // ── AI Action: Tạo Quiz & nạp vào CSDL ──────────────────────────────────
    [HttpPost("actions/generate-quiz")]
    public async Task<ActionResult<ApiResponseDto<AiGenerateQuizResponse>>> GenerateQuiz(
        [FromBody] AiGenerateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var result = await _ollamaService.GenerateQuizAsync(request, user, cancellationToken);
        return Ok(ApiResponseDto<AiGenerateQuizResponse>.Ok(result));
    }

    // ── BGH AI Analytics & Orchestrator ─────────────────────────────────────
    [HttpPost("bgh/report")]
    public async Task<ActionResult<ApiResponseDto<BghAiReportResponse>>> GenerateBghReport(
        [FromBody] BghAiReportRequest request,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext ?? new CurrentUserContext();
        var report = await _bghAiAnalyticsService.GenerateBghAiReportAsync(request, user, cancellationToken);
        return Ok(ApiResponseDto<BghAiReportResponse>.Ok(report));
    }

    [HttpGet("analytics/gpa")]
    public async Task<ActionResult<ApiResponseDto<GpaAnalyticsContextDto>>> GetGpaAnalytics(
        [FromQuery] int semesterId = 0,
        [FromQuery] int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetGpaAnalyticsContextAsync(user?.CampusId ?? 1, semesterId, departmentId, cancellationToken);
        return Ok(ApiResponseDto<GpaAnalyticsContextDto>.Ok(data));
    }

    [HttpGet("analytics/at-risk")]
    public async Task<ActionResult<ApiResponseDto<AtRiskAnalyticsContextDto>>> GetAtRiskAnalytics(
        [FromQuery] int semesterId = 0,
        [FromQuery] int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetAtRiskAnalyticsContextAsync(user?.CampusId ?? 1, semesterId, departmentId, cancellationToken);
        return Ok(ApiResponseDto<AtRiskAnalyticsContextDto>.Ok(data));
    }

    [HttpGet("analytics/pass-fail")]
    public async Task<ActionResult<ApiResponseDto<PassFailAnalyticsContextDto>>> GetPassFailAnalytics(
        [FromQuery] int semesterId = 0,
        [FromQuery] int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetPassFailAnalyticsContextAsync(user?.CampusId ?? 1, semesterId, departmentId, cancellationToken);
        return Ok(ApiResponseDto<PassFailAnalyticsContextDto>.Ok(data));
    }

    [HttpGet("analytics/teacher-eval")]
    public async Task<ActionResult<ApiResponseDto<TeacherEvaluationContextDto>>> GetTeacherEvaluationAnalytics(
        [FromQuery] int semesterId = 0,
        [FromQuery] int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetTeacherEvaluationContextAsync(user?.CampusId ?? 1, semesterId, departmentId, cancellationToken);
        return Ok(ApiResponseDto<TeacherEvaluationContextDto>.Ok(data));
    }

    [HttpGet("analytics/awards")]
    public async Task<ActionResult<ApiResponseDto<AwardsAnalyticsContextDto>>> GetAwardsAnalytics(
        [FromQuery] int? semesterId = null,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetAwardsAnalyticsContextAsync(user?.CampusId ?? 1, semesterId, cancellationToken);
        return Ok(ApiResponseDto<AwardsAnalyticsContextDto>.Ok(data));
    }

    [HttpGet("analytics/facilities")]
    public async Task<ActionResult<ApiResponseDto<FacilitiesAnalyticsContextDto>>> GetFacilitiesAnalytics(
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var data = await _bghAiAnalyticsService.GetFacilitiesAnalyticsContextAsync(user?.CampusId ?? 1, cancellationToken);
        return Ok(ApiResponseDto<FacilitiesAnalyticsContextDto>.Ok(data));
    }

    [HttpPost("certificate-templates/ai-edit")]
    public async Task<ActionResult<ApiResponseDto<AiCertificateTemplateEditResponse>>> EditCertificateTemplate(
        [FromBody] AiCertificateTemplateEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var result = await _bghAiAnalyticsService.EditCertificateTemplateWithAiAsync(request, user ?? new CurrentUserContext(), cancellationToken);
        return Ok(ApiResponseDto<AiCertificateTemplateEditResponse>.Ok(result));
    }

    [HttpGet("actions/download-quiz-doc")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadQuizDoc([FromQuery] int maDeKiemTra, CancellationToken cancellationToken)
    {
        var docBytes = await _ollamaService.ExportQuizDocAsync(maDeKiemTra, cancellationToken);
        if (docBytes == null || docBytes.Length == 0)
        {
            return NotFound("Không tìm thấy dữ liệu đề thi hoặc đề chưa có câu hỏi.");
        }

        var fileName = $"De_Tu_Luyen_On_Tap_{maDeKiemTra}.doc";
        return File(docBytes, "application/msword", fileName);
    }
}

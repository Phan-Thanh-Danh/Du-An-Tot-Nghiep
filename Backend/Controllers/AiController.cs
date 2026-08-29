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

    public AiController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
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
}

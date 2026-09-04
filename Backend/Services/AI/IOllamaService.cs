using System.Threading;
using System.Threading.Tasks;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;

namespace Backend.Services.AI;

public interface IOllamaService
{
    // Internal task completion: no chat shortcuts, academic context or side effects.
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, object schema, string mode = "fast", int maxTokens = 2048, CancellationToken cancellationToken = default);
    Task<AiHealthResponse> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<AiChatResponse> ChatAsync(AiChatRequest request, CurrentUserContext? userContext, CancellationToken cancellationToken = default);
    Task<AiEmbeddingTestResponse> TestEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
    Task<AiDashboardInsightDto> GetDashboardInsightAsync(CurrentUserContext? userContext, bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<AiGenerateQuizResponse> GenerateQuizAsync(AiGenerateQuizRequest request, CurrentUserContext? userContext, CancellationToken cancellationToken = default);
    Task<byte[]?> ExportQuizDocAsync(int maDeKiemTra, CancellationToken cancellationToken = default);
}

using System.Threading;
using System.Threading.Tasks;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;

namespace Backend.Services.AI;

public interface IOllamaService
{
    Task<AiHealthResponse> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<AiChatResponse> ChatAsync(AiChatRequest request, CurrentUserContext? userContext, CancellationToken cancellationToken = default);
    Task<AiEmbeddingTestResponse> TestEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}

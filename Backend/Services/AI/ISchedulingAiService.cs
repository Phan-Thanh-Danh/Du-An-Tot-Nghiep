using System.Threading;
using System.Threading.Tasks;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;

namespace Backend.Services.AI;

public interface ISchedulingAiService
{
    Task<AiSchedulingInterpretResponse> InterpretIntentAsync(
        AiSchedulingInterpretRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default);

    Task<AiExplainDraftResponse> ExplainDraftAsync(
        AiExplainDraftRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default);

    Task<AiExplainReadinessResponse> ExplainReadinessAsync(
        AiExplainReadinessRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default);
}

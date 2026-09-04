using System.Threading;
using System.Threading.Tasks;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;

namespace Backend.Services.AI;

public class ResolvedAcademicContext
{
    public bool HasAcademicData { get; set; }
    public string Intent { get; set; } = string.Empty;
    public string GroundingContext { get; set; } = string.Empty;
    public string? DirectAnswer { get; set; }
    public AiChatActionDto? SuggestedAction { get; set; }
}

public interface IAiAcademicQueryResolver
{
    Task<ResolvedAcademicContext> ResolveAcademicContextAsync(
        string message,
        CurrentUserContext? userContext,
        CancellationToken cancellationToken = default);
}

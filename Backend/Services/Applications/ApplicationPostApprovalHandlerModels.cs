using System.Text.Json;
using Backend.Models;

namespace Backend.Services.Applications;

public sealed class ApplicationPostApprovalOutcome
{
    public required string Outcome { get; init; }
    public required string Handler { get; init; }
    public required string PublicNote { get; init; }
    public string? InternalCode { get; init; }
    public JsonElement Data { get; init; }
}

public interface IApplicationPostApprovalHandler
{
    bool CanHandle(string applicationType);
    Task<ApplicationPostApprovalOutcome> ProcessAsync(
        DonTu application,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);
}

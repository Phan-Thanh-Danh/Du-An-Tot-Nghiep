using Backend.Models;

namespace Backend.Services.Applications;

public sealed class ApplicationSubmissionRuleContext
{
    public DonTu Application { get; init; } = null!;
    public NguoiDung Student { get; init; } = null!;
    public ApplicationFormDataValidationResult FormData { get; init; } = null!;
}

public interface IApplicationSubmissionRule
{
    string SupportedType { get; }
    string? BuildDuplicateLockKey(ApplicationSubmissionRuleContext context);
    Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default);
}

using Backend.Constants;
using Backend.Exceptions;

namespace Backend.Services.Applications;

public class ApplicationProcessingStateMachine : IApplicationProcessingStateMachine
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [ApplicationProcessingStatuses.Pending] =
            [
                ApplicationProcessingStatuses.Recorded,
                ApplicationProcessingStatuses.Succeeded,
                ApplicationProcessingStatuses.Failed,
                ApplicationProcessingStatuses.ManualRequired
            ],
            [ApplicationProcessingStatuses.ManualRequired] =
            [
                ApplicationProcessingStatuses.Recorded,
                ApplicationProcessingStatuses.Succeeded,
                ApplicationProcessingStatuses.Failed
            ],
            [ApplicationProcessingStatuses.Failed] =
            [
                ApplicationProcessingStatuses.Recorded,
                ApplicationProcessingStatuses.Succeeded,
                ApplicationProcessingStatuses.ManualRequired
            ]
        };

    private static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ApplicationProcessingStatuses.Recorded,
        ApplicationProcessingStatuses.Succeeded
    };

    public bool CanTransition(string currentStatus, string targetStatus)
    {
        return ApplicationProcessingStatuses.All.Contains(currentStatus) &&
               ApplicationProcessingStatuses.All.Contains(targetStatus) &&
               Transitions.TryGetValue(currentStatus, out var targets) &&
               targets.Contains(targetStatus, StringComparer.OrdinalIgnoreCase);
    }

    public void EnsureTransitionAllowed(string currentStatus, string targetStatus)
    {
        if (!CanTransition(currentStatus, targetStatus))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Trạng thái xử lý nghiệp vụ hiện tại không cho phép cập nhật kết quả.");
        }
    }

    public bool IsTerminal(string status)
    {
        return TerminalStatuses.Contains(status);
    }
}

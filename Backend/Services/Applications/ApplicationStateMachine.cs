using Backend.Constants;
using Backend.Exceptions;

namespace Backend.Services.Applications;

public class ApplicationStateMachine : IApplicationStateMachine
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [ApplicationStatuses.Draft] =
            [
                ApplicationStatuses.Submitted,
                ApplicationStatuses.Cancelled
            ],
            [ApplicationStatuses.Submitted] =
            [
                ApplicationStatuses.InReview,
                ApplicationStatuses.Cancelled
            ],
            [ApplicationStatuses.InReview] =
            [
                ApplicationStatuses.NeedSupplement,
                ApplicationStatuses.Approved,
                ApplicationStatuses.Rejected
            ],
            [ApplicationStatuses.NeedSupplement] =
            [
                ApplicationStatuses.InReview,
                ApplicationStatuses.Cancelled
            ],
            [ApplicationStatuses.Approved] = [],
            [ApplicationStatuses.Rejected] = [],
            [ApplicationStatuses.Cancelled] = []
        };

    private static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ApplicationStatuses.Approved,
        ApplicationStatuses.Rejected,
        ApplicationStatuses.Cancelled
    };

    public bool CanTransition(string currentStatus, string targetStatus)
    {
        if (!ApplicationStatuses.All.Contains(currentStatus) || !ApplicationStatuses.All.Contains(targetStatus))
        {
            return false;
        }

        return Transitions.TryGetValue(currentStatus, out var targets) &&
               targets.Contains(targetStatus, StringComparer.OrdinalIgnoreCase);
    }

    public void EnsureTransitionAllowed(string currentStatus, string targetStatus)
    {
        if (!CanTransition(currentStatus, targetStatus))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không thể chuyển trạng thái đơn từ từ '{currentStatus}' sang '{targetStatus}'.");
        }
    }

    public bool IsTerminal(string status)
    {
        return TerminalStatuses.Contains(status);
    }

    public IReadOnlyCollection<string> GetAllowedTargets(string status)
    {
        if (!ApplicationStatuses.All.Contains(status))
        {
            return [];
        }

        return Transitions.TryGetValue(status, out var targets) ? targets : [];
    }
}

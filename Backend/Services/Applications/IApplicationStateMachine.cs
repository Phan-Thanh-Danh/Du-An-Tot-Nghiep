namespace Backend.Services.Applications;

public interface IApplicationStateMachine
{
    bool CanTransition(string currentStatus, string targetStatus);
    void EnsureTransitionAllowed(string currentStatus, string targetStatus);
    bool IsTerminal(string status);
    IReadOnlyCollection<string> GetAllowedTargets(string status);
}

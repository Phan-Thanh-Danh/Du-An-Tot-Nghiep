namespace Backend.Services.Applications;

public interface IApplicationProcessingStateMachine
{
    bool CanTransition(string currentStatus, string targetStatus);
    void EnsureTransitionAllowed(string currentStatus, string targetStatus);
    bool IsTerminal(string status);
}

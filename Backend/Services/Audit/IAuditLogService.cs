namespace Backend.Services.Audit;

public interface IAuditLogService
{
    Task AddAsync(
        int campusId,
        string entityName,
        int entityId,
        string action,
        int actorUserId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default);
}

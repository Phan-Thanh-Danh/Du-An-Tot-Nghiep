using Backend.DTOs.Audit;
using Backend.DTOs.Common;

namespace Backend.Services.Audit;

public interface IAuditLogService
{
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        object? oldValue,
        object? newValue,
        int? changedBy,
        int? maDonVi,
        string? description,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        int campusId,
        string entityName,
        int entityId,
        string action,
        int actorUserId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AuditLogListItemDto>> GetAsync(
        AuditLogQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AuditLogDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}

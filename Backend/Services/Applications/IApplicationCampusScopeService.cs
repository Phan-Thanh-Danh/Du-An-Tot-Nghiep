using Backend.DTOs.Auth;
using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationCampusScopeService
{
    Task<ApplicationActorContext> GetCurrentActorAsync(CancellationToken cancellationToken = default);
    Task<bool> CanAccessCampusAsync(ApplicationActorContext actor, int campusId, CancellationToken cancellationToken = default);
    Task EnsureCampusFilterAllowedAsync(ApplicationActorContext actor, int? campusId, CancellationToken cancellationToken = default);
    IQueryable<DonTu> ApplyApplicationScope(IQueryable<DonTu> query, ApplicationActorContext actor);
    IQueryable<NguoiDung> ApplyUserScope(IQueryable<NguoiDung> query, ApplicationActorContext actor);
    Task<NguoiDung> GetAssignableUserAsync(ApplicationActorContext actor, int userId, CancellationToken cancellationToken = default);
}

public sealed class ApplicationActorContext
{
    public required CurrentUserContext Claims { get; init; }
    public required NguoiDung User { get; init; }
    public required string Role { get; init; }
    public required int CampusId { get; init; }
    public bool IsGlobal { get; init; }
    public IReadOnlySet<int>? AllowedCampusIds { get; init; }
}

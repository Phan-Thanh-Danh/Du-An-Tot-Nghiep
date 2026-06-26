using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationCampusScopeService : IApplicationCampusScopeService
{
    private static readonly string[] QueueReadRoles =
    [
        AuthRoles.SuperAdmin,
        AuthRoles.Admin,
        AuthRoles.CampusAdmin,
        AuthRoles.SubCampusAdmin,
        AuthRoles.AcademicStaff,
        AuthRoles.Principal
    ];

    private static readonly string[] AssignableRoles =
    [
        AuthRoles.SuperAdmin,
        AuthRoles.Admin,
        AuthRoles.CampusAdmin,
        AuthRoles.SubCampusAdmin,
        AuthRoles.AcademicStaff
    ];

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationCampusScopeService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApplicationActorContext> GetCurrentActorAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        var user = await _context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == currentUser.UserId, cancellationToken);
        if (user is null || user.TrangThai != UserStatuses.DbActive)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản không có quyền truy cập hàng đợi đơn từ.");
        }

        var role = AuthRoles.FromDatabaseCode(user.VaiTroChinh);
        if (!QueueReadRoles.Contains(role))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Tài khoản không có quyền truy cập hàng đợi đơn từ.");
        }

        var isGlobal = role is AuthRoles.SuperAdmin or AuthRoles.Admin;
        IReadOnlySet<int>? allowedCampusIds = null;
        if (!isGlobal)
        {
            allowedCampusIds = role == AuthRoles.CampusAdmin
                ? await GetCampusWithDescendantsAsync(user.MaDonVi, cancellationToken)
                : new HashSet<int> { user.MaDonVi };
        }

        return new ApplicationActorContext
        {
            Claims = currentUser,
            User = user,
            Role = role,
            CampusId = user.MaDonVi,
            IsGlobal = isGlobal,
            AllowedCampusIds = allowedCampusIds
        };
    }

    public async Task<bool> CanAccessCampusAsync(
        ApplicationActorContext actor,
        int campusId,
        CancellationToken cancellationToken = default)
    {
        if (actor.IsGlobal)
        {
            return true;
        }

        if (actor.AllowedCampusIds is not null)
        {
            return actor.AllowedCampusIds.Contains(campusId);
        }

        return await Task.FromResult(false);
    }

    public async Task EnsureCampusFilterAllowedAsync(
        ApplicationActorContext actor,
        int? campusId,
        CancellationToken cancellationToken = default)
    {
        if (campusId.HasValue && !await CanAccessCampusAsync(actor, campusId.Value, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập dữ liệu cơ sở này.");
        }
    }

    public IQueryable<DonTu> ApplyApplicationScope(IQueryable<DonTu> query, ApplicationActorContext actor)
    {
        return actor.IsGlobal || actor.AllowedCampusIds is null
            ? query
            : query.Where(x => actor.AllowedCampusIds.Contains(x.MaDonVi));
    }

    public IQueryable<NguoiDung> ApplyUserScope(IQueryable<NguoiDung> query, ApplicationActorContext actor)
    {
        return actor.IsGlobal || actor.AllowedCampusIds is null
            ? query
            : query.Where(x => actor.AllowedCampusIds.Contains(x.MaDonVi));
    }

    public async Task<NguoiDung> GetAssignableUserAsync(
        ApplicationActorContext actor,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await ApplyUserScope(_context.NguoiDungs.AsNoTracking(), actor)
            .FirstOrDefaultAsync(x => x.MaNguoiDung == userId, cancellationToken);
        if (user is null ||
            user.TrangThai != UserStatuses.DbActive ||
            !AssignableRoles.Contains(AuthRoles.FromDatabaseCode(user.VaiTroChinh)))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người xử lý phù hợp.");
        }

        return user;
    }

    private async Task<IReadOnlySet<int>> GetCampusWithDescendantsAsync(
        int rootCampusId,
        CancellationToken cancellationToken)
    {
        var units = await _context.DonVis.AsNoTracking()
            .Select(x => new { x.MaDonVi, x.MaDonViCha })
            .ToListAsync(cancellationToken);
        var byParent = units
            .Where(x => x.MaDonViCha.HasValue)
            .GroupBy(x => x.MaDonViCha!.Value)
            .ToDictionary(x => x.Key, x => x.Select(item => item.MaDonVi).ToList());

        var result = new HashSet<int> { rootCampusId };
        var queue = new Queue<int>();
        queue.Enqueue(rootCampusId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!byParent.TryGetValue(current, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (result.Add(child))
                {
                    queue.Enqueue(child);
                }
            }
        }

        return result;
    }
}

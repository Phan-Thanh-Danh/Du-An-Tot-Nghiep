using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Rbac;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Rbac;

public class RbacService : IRbacService
{
    private readonly IRbacRepository _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RbacService(
        IRbacRepository repository,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryRoles()
            .AsNoTracking()
            .OrderBy(x => x.MaVaiTro)
            .Select(x => ToRoleDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleDto> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await GetExistingRoleAsync(roleId, cancellationToken);
        return ToRoleDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageRoleCatalog(currentUser);

        var roleCode = NormalizeRoleCode(request.MaCodeVaiTro);
        var roleName = NormalizeRoleName(request.TenVaiTro);
        await ValidateRoleCodeAsync(roleCode, null, cancellationToken);

        var role = new VaiTro
        {
            MaVaiTro = await _repository.GetNextRoleIdAsync(cancellationToken),
            MaCodeVaiTro = roleCode,
            TenVaiTro = roleName
        };

        await _repository.AddRoleAsync(role, cancellationToken);
        await _auditLogService.AddAsync(
            currentUser.CampusId,
            nameof(VaiTro),
            role.MaVaiTro,
            "ROLE_CREATED",
            currentUser.UserId,
            null,
            ToRoleDto(role),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return ToRoleDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(
        int roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageRoleCatalog(currentUser);

        var role = await GetExistingRoleAsync(roleId, cancellationToken);
        var oldValue = ToRoleDto(role);
        var roleCode = NormalizeRoleCode(request.MaCodeVaiTro);
        var roleName = NormalizeRoleName(request.TenVaiTro);
        await ValidateRoleCodeAsync(roleCode, roleId, cancellationToken);

        if (!role.MaCodeVaiTro.Equals(roleCode, StringComparison.OrdinalIgnoreCase) &&
            await IsRoleInUseAsync(role, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể đổi mã vai trò đang được gán cho người dùng.");
        }

        role.MaCodeVaiTro = roleCode;
        role.TenVaiTro = roleName;

        await _auditLogService.AddAsync(
            currentUser.CampusId,
            nameof(VaiTro),
            role.MaVaiTro,
            "ROLE_UPDATED",
            currentUser.UserId,
            oldValue,
            ToRoleDto(role),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return ToRoleDto(role);
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageRoleCatalog(currentUser);

        var role = await GetExistingRoleAsync(roleId, cancellationToken);
        if (await IsRoleInUseAsync(role, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể xóa vai trò đang được gán cho người dùng.");
        }

        var oldValue = ToRoleDto(role);
        _repository.RemoveRole(role);

        await _auditLogService.AddAsync(
            currentUser.CampusId,
            nameof(VaiTro),
            role.MaVaiTro,
            "ROLE_DELETED",
            currentUser.UserId,
            oldValue,
            null,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserRoleAssignmentDto> GetUserRolesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        return await ToUserRoleAssignmentDtoAsync(user, cancellationToken);
    }

    public async Task<UserRoleAssignmentDto> AssignUserRolesAsync(
        int userId,
        AssignUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        var oldValue = await ToUserRoleAssignmentDtoAsync(user, cancellationToken);
        var requestedRoleIds = request.MaVaiTroBoSung
            .Append(request.MaVaiTroChinh)
            .Distinct()
            .ToList();

        var roles = await _repository.QueryRoles()
            .Where(x => requestedRoleIds.Contains(x.MaVaiTro))
            .ToListAsync(cancellationToken);

        if (roles.Count != requestedRoleIds.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách vai trò không hợp lệ.");
        }

        var primaryRole = roles.First(x => x.MaVaiTro == request.MaVaiTroChinh);
        foreach (var role in roles)
        {
            ValidateAssignableRole(currentUser, role);
        }

        var existingAssignments = await _repository.GetUserRoleAssignmentsAsync(userId, cancellationToken);
        if (existingAssignments.Count > 0)
        {
            _repository.RemoveRoleAssignments(existingAssignments);
        }

        foreach (var role in roles)
        {
            await _repository.AddRoleAssignmentAsync(new PhanQuyenNguoiDung
            {
                MaNguoiDung = user.MaNguoiDung,
                MaVaiTro = role.MaVaiTro,
                NgayGan = DateTime.UtcNow
            }, cancellationToken);
        }

        user.VaiTroChinh = primaryRole.MaCodeVaiTro;
        var newValue = await ToUserRoleAssignmentDtoAsync(user, cancellationToken, roles);

        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(PhanQuyenNguoiDung),
            user.MaNguoiDung,
            "USER_ROLES_ASSIGNED",
            currentUser.UserId,
            oldValue,
            newValue,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await ToUserRoleAssignmentDtoAsync(user, cancellationToken);
    }

    private async Task<VaiTro> GetExistingRoleAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy vai trò.");
        }

        return role;
    }

    private async Task<NguoiDung> GetManagedUserAsync(
        int userId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người dùng.");
        }

        if (!await CanManageOrganizationAsync(currentUser, user.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gán vai trò cho người dùng thuộc đơn vị này.");
        }

        return user;
    }

    private async Task ValidateRoleCodeAsync(
        string roleCode,
        int? excludedRoleId,
        CancellationToken cancellationToken)
    {
        if (!AuthRoles.IsKnownDatabaseCode(roleCode))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mã vai trò không thuộc danh sách vai trò hệ thống được hỗ trợ.");
        }

        var existingRole = await _repository.GetRoleByCodeAsync(roleCode, cancellationToken);
        if (existingRole is not null && (!excludedRoleId.HasValue || existingRole.MaVaiTro != excludedRoleId.Value))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã vai trò đã tồn tại.");
        }
    }

    private async Task<bool> IsRoleInUseAsync(VaiTro role, CancellationToken cancellationToken)
    {
        return await _repository.QueryRoleAssignments()
                   .AnyAsync(x => x.MaVaiTro == role.MaVaiTro, cancellationToken) ||
               await _repository.QueryUsers()
                   .AnyAsync(x => x.VaiTroChinh == role.MaCodeVaiTro, cancellationToken);
    }

    private static void ValidateAssignableRole(CurrentUserContext currentUser, VaiTro role)
    {
        if (!AuthRoles.IsKnownDatabaseCode(role.MaCodeVaiTro))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vai trò này chưa được hỗ trợ bởi hệ thống phân quyền API.");
        }

        if (currentUser.Role != AuthRoles.SuperAdmin &&
            role.MaCodeVaiTro.Equals(AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được gán vai trò SuperAdmin.");
        }
    }

    private void EnsureCanManageRoleCatalog(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý danh mục vai trò.");
        }
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _repository.QueryOrganizations()
                .AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            var organizations = await _repository.QueryOrganizations()
                .AsNoTracking()
                .Select(x => new { x.MaDonVi, x.MaDonViCha })
                .ToListAsync(cancellationToken);

            var allowedIds = new HashSet<int> { currentUser.CampusId };
            var queue = new Queue<int>();
            queue.Enqueue(currentUser.CampusId);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                foreach (var child in organizations.Where(x => x.MaDonViCha == parentId))
                {
                    if (allowedIds.Add(child.MaDonVi))
                    {
                        queue.Enqueue(child.MaDonVi);
                    }
                }
            }

            return allowedIds;
        }

        return new HashSet<int> { currentUser.CampusId };
    }

    private async Task<bool> CanManageOrganizationAsync(
        CurrentUserContext currentUser,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        return allowedOrganizationIds.Contains(organizationId);
    }

    private async Task<UserRoleAssignmentDto> ToUserRoleAssignmentDtoAsync(
        NguoiDung user,
        CancellationToken cancellationToken,
        IReadOnlyList<VaiTro>? assignedRolesOverride = null)
    {
        var organizationName = await _repository.QueryOrganizations()
            .AsNoTracking()
            .Where(x => x.MaDonVi == user.MaDonVi)
            .Select(x => x.TenDonVi)
            .FirstOrDefaultAsync(cancellationToken);
        var primaryRole = await _repository.GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);

        if (primaryRole is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vai trò chính của người dùng không hợp lệ.");
        }

        var assignedRoles = assignedRolesOverride ?? await (
            from assignment in _repository.QueryRoleAssignments().AsNoTracking()
            join role in _repository.QueryRoles().AsNoTracking()
                on assignment.MaVaiTro equals role.MaVaiTro
            where assignment.MaNguoiDung == user.MaNguoiDung
            orderby role.MaVaiTro
            select role)
            .ToListAsync(cancellationToken);

        return new UserRoleAssignmentDto
        {
            MaNguoiDung = user.MaNguoiDung,
            HoTen = user.HoTen,
            Email = user.Email,
            MaDonVi = user.MaDonVi,
            TenDonVi = organizationName ?? string.Empty,
            VaiTroChinh = ToRoleDto(primaryRole),
            VaiTroDuocGan = assignedRoles
                .OrderBy(x => x.MaVaiTro)
                .Select(ToRoleDto)
                .ToList()
        };
    }

    private static RoleDto ToRoleDto(VaiTro role)
    {
        return new RoleDto
        {
            MaVaiTro = role.MaVaiTro,
            MaCodeVaiTro = role.MaCodeVaiTro,
            TenVaiTro = role.TenVaiTro
        };
    }

    private static string NormalizeRoleCode(string roleCode)
    {
        return roleCode.Trim().ToLowerInvariant();
    }

    private static string NormalizeRoleName(string roleName)
    {
        var normalizedRoleName = roleName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên vai trò không được để trống.");
        }

        return normalizedRoleName;
    }
}

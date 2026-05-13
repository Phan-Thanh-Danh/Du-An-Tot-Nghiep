using Backend.Constants;
using Backend.DTOs.AdminUsers;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AdminUsers;

public class UserService : IUserService
{
    private static readonly HashSet<string> ManageableRoleCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        AuthRoles.ToDatabaseCode(AuthRoles.Admin),
        AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin),
        AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
        AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
        AuthRoles.ToDatabaseCode(AuthRoles.Student)
    };

    private readonly IUserRepository _repository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IUserRepository repository,
        IPasswordHasherService passwordHasher,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<UserListItemDto>> GetUsersAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var query =
            from user in _repository.QueryUsers().AsNoTracking()
            join organization in _repository.QueryOrganizations().AsNoTracking()
                on user.MaDonVi equals organization.MaDonVi
            join role in _repository.QueryRoles().AsNoTracking()
                on user.VaiTroChinh equals role.MaCodeVaiTro
            where allowedOrganizationIds.Contains(user.MaDonVi)
            select new
            {
                User = user,
                OrganizationName = organization.TenDonVi,
                RoleName = role.TenVaiTro,
                RoleCode = role.MaCodeVaiTro
            };

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.User.HoTen.ToLower().Contains(keyword) ||
                x.User.Email.ToLower().Contains(keyword) ||
                (x.User.SoDienThoai != null && x.User.SoDienThoai.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Role))
        {
            var roleCode = NormalizeRoleCode(parameters.Role);
            query = query.Where(x => x.RoleCode == roleCode);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.User.TrangThai == status);
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem dữ liệu của đơn vị này.");
            }

            query = query.Where(x => x.User.MaDonVi == parameters.MaDonVi.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.User.NgayTao)
            .ThenBy(x => x.User.HoTen)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new UserListItemDto
            {
                MaNguoiDung = x.User.MaNguoiDung,
                HoTen = x.User.HoTen,
                Email = x.User.Email,
                SoDienThoai = x.User.SoDienThoai,
                TenVaiTro = x.RoleName,
                TenDonVi = x.OrganizationName,
                TrangThai = UserStatuses.FromDatabaseStatus(x.User.TrangThai, x.User.DangNhapLanDau),
                NgayTao = x.User.NgayTao
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserListItemDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<UserDetailDto> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        return await ToDetailDtoAsync(user, cancellationToken);
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _repository.EmailExistsAsync(normalizedEmail, cancellationToken: cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Email đã được sử dụng bởi tài khoản khác.");
        }

        var role = await ValidateRoleAsync(request.MaVaiTro, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var classId = await ValidateClassAsync(role.MaCodeVaiTro, request.MaLopHanhChinh, organization.MaDonVi, cancellationToken);
        var passwordStrengthError = _passwordHasher.GetPasswordStrengthError(request.MatKhau);
        if (passwordStrengthError is not null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, passwordStrengthError);
        }

        var user = new NguoiDung
        {
            MaDonVi = organization.MaDonVi,
            Email = normalizedEmail,
            HoTen = NormalizeRequiredText(request.HoTen, "Họ tên"),
            VaiTroChinh = role.MaCodeVaiTro,
            MaLop = classId,
            SoDienThoai = NormalizeOptionalText(request.SoDienThoai),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = _passwordHasher.HashPassword(request.MatKhau),
            NgayTao = DateTime.UtcNow,
            SoLanSaiMatKhau = 0,
            DangNhapLanDau = true
        };

        await _repository.AddAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _repository.AddRoleAssignmentAsync(new PhanQuyenNguoiDung
        {
            MaNguoiDung = user.MaNguoiDung,
            MaVaiTro = role.MaVaiTro,
            NgayGan = DateTime.UtcNow
        }, cancellationToken);

        var snapshot = await CreateAuditSnapshotAsync(user, role, cancellationToken);
        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(NguoiDung),
            user.MaNguoiDung,
            "USER_CREATED",
            currentUser.UserId,
            null,
            snapshot,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await ToDetailDtoAsync(user, cancellationToken);
    }

    public async Task<UserDetailDto> UpdateAsync(
        int userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        var oldRole = await GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(user, oldRole, cancellationToken);
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _repository.EmailExistsAsync(normalizedEmail, userId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Email đã được sử dụng bởi tài khoản khác.");
        }

        var newRole = await ValidateRoleAsync(request.MaVaiTro, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var classId = await ValidateClassAsync(newRole.MaCodeVaiTro, request.MaLopHanhChinh, organization.MaDonVi, cancellationToken);

        user.Email = normalizedEmail;
        user.HoTen = NormalizeRequiredText(request.HoTen, "Họ tên");
        user.SoDienThoai = NormalizeOptionalText(request.SoDienThoai);
        user.MaDonVi = organization.MaDonVi;
        user.MaLop = classId;
        user.VaiTroChinh = newRole.MaCodeVaiTro;

        await ReplaceRoleAssignmentAsync(user.MaNguoiDung, newRole.MaVaiTro, cancellationToken);

        var newValue = await CreateAuditSnapshotAsync(user, newRole, cancellationToken);
        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(NguoiDung),
            user.MaNguoiDung,
            "USER_UPDATED",
            currentUser.UserId,
            oldValue,
            newValue,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await ToDetailDtoAsync(user, cancellationToken);
    }

    public async Task LockAsync(int userId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (currentUser.UserId == userId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể tự khóa tài khoản đang đăng nhập.");
        }

        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        var role = await GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);

        user.TrangThai = UserStatuses.DbLocked;
        user.DangNhapLanDau = false;

        var newValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);
        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(NguoiDung),
            user.MaNguoiDung,
            "USER_LOCKED",
            currentUser.UserId,
            oldValue,
            newValue,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAsync(int userId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        var role = await GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);

        user.TrangThai = UserStatuses.DbActive;
        user.SoLanSaiMatKhau = 0;

        var newValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);
        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(NguoiDung),
            user.MaNguoiDung,
            "USER_UNLOCKED",
            currentUser.UserId,
            oldValue,
            newValue,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(
        int userId,
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var user = await GetManagedUserAsync(userId, currentUser, cancellationToken);
        var role = await GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);
        var passwordStrengthError = _passwordHasher.GetPasswordStrengthError(request.NewPassword);
        if (passwordStrengthError is not null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, passwordStrengthError);
        }

        user.MatKhauHash = _passwordHasher.HashPassword(request.NewPassword);
        user.SoLanSaiMatKhau = 0;
        user.DangNhapLanDau = true;
        if (user.TrangThai != UserStatuses.DbLocked)
        {
            user.TrangThai = UserStatuses.DbFirstLogin;
        }

        var newValue = await CreateAuditSnapshotAsync(user, role, cancellationToken);
        await _auditLogService.AddAsync(
            user.MaDonVi,
            nameof(NguoiDung),
            user.MaNguoiDung,
            "USER_PASSWORD_RESET",
            currentUser.UserId,
            oldValue,
            newValue,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<NguoiDung> GetManagedUserAsync(
        int userId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người dùng.");
        }

        if (!await CanManageOrganizationAsync(currentUser, user.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý người dùng thuộc đơn vị này.");
        }

        return user;
    }

    private async Task<VaiTro> ValidateRoleAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vai trò không tồn tại.");
        }

        if (!ManageableRoleCodes.Contains(role.MaCodeVaiTro))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vai trò này không được phép gán trong màn quản trị tài khoản.");
        }

        return role;
    }

    private async Task<DonVi> ValidateOrganizationAsync(
        int organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetOrganizationByIdAsync(organizationId, cancellationToken);
        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc không hoạt động.");
        }

        if (!await CanManageOrganizationAsync(currentUser, organizationId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý người dùng thuộc đơn vị này.");
        }

        return organization;
    }

    private async Task<int?> ValidateClassAsync(
        string roleCode,
        int? classId,
        int organizationId,
        CancellationToken cancellationToken)
    {
        if (!roleCode.Equals(AuthRoles.ToDatabaseCode(AuthRoles.Student), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!classId.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học sinh phải được gán lớp hành chính.");
        }

        var classEntity = await _repository.GetClassByIdAsync(classId.Value, cancellationToken);
        if (classEntity is null || !classEntity.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lớp hành chính không tồn tại hoặc không hoạt động.");
        }

        if (classEntity.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lớp hành chính không thuộc đơn vị đã chọn.");
        }

        return classEntity.MaLop;
    }

    private async Task ReplaceRoleAssignmentAsync(
        int userId,
        int roleId,
        CancellationToken cancellationToken)
    {
        var existingAssignments = await _repository.GetRoleAssignmentsAsync(userId, cancellationToken);
        if (existingAssignments.Count > 0)
        {
            _repository.RemoveRoleAssignments(existingAssignments);
        }

        await _repository.AddRoleAssignmentAsync(new PhanQuyenNguoiDung
        {
            MaNguoiDung = userId,
            MaVaiTro = roleId,
            NgayGan = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task<UserDetailDto> ToDetailDtoAsync(NguoiDung user, CancellationToken cancellationToken)
    {
        var role = await GetRoleByCodeAsync(user.VaiTroChinh, cancellationToken);
        var organization = await _repository.QueryOrganizations()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == user.MaDonVi, cancellationToken);
        var classEntity = user.MaLop.HasValue
            ? await _repository.QueryClasses()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLop == user.MaLop.Value, cancellationToken)
            : null;

        return new UserDetailDto
        {
            MaNguoiDung = user.MaNguoiDung,
            HoTen = user.HoTen,
            Email = user.Email,
            SoDienThoai = user.SoDienThoai,
            MaVaiTro = role.MaVaiTro,
            MaCodeVaiTro = role.MaCodeVaiTro,
            TenVaiTro = role.TenVaiTro,
            MaDonVi = user.MaDonVi,
            TenDonVi = organization?.TenDonVi ?? string.Empty,
            MaLopHanhChinh = user.MaLop,
            TenLopHanhChinh = classEntity?.TenLop,
            TrangThai = UserStatuses.FromDatabaseStatus(user.TrangThai, user.DangNhapLanDau),
            NgayTao = user.NgayTao,
            LanDangNhapCuoi = user.LanDangNhapCuoi
        };
    }

    private async Task<VaiTro> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken)
    {
        var role = await _repository.QueryRoles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaCodeVaiTro == roleCode, cancellationToken);

        if (role is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vai trò của người dùng không hợp lệ.");
        }

        return role;
    }

    private async Task<object> CreateAuditSnapshotAsync(
        NguoiDung user,
        VaiTro role,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.QueryOrganizations()
            .AsNoTracking()
            .Where(x => x.MaDonVi == user.MaDonVi)
            .Select(x => new { x.MaDonVi, x.TenDonVi })
            .FirstOrDefaultAsync(cancellationToken);
        var classEntity = user.MaLop.HasValue
            ? await _repository.QueryClasses()
                .AsNoTracking()
                .Where(x => x.MaLop == user.MaLop.Value)
                .Select(x => new { x.MaLop, x.TenLop })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new
        {
            user.MaNguoiDung,
            user.HoTen,
            user.Email,
            user.SoDienThoai,
            user.MaDonVi,
            TenDonVi = organization?.TenDonVi,
            MaVaiTro = role.MaVaiTro,
            role.MaCodeVaiTro,
            role.TenVaiTro,
            MaLopHanhChinh = user.MaLop,
            TenLopHanhChinh = classEntity?.TenLop,
            user.TrangThai,
            user.DangNhapLanDau,
            user.SoLanSaiMatKhau
        };
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

        // Admin thông thường chỉ quản lý user trong chính đơn vị đang đăng nhập.
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

    private static string NormalizeEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Email không được để trống.");
        }

        return normalizedEmail;
    }

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalizedValue;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRoleCode(string role)
    {
        return AuthRoles.ToDatabaseCode(role.Trim());
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim() switch
        {
            UserStatuses.Active => UserStatuses.DbActive,
            UserStatuses.Locked => UserStatuses.DbLocked,
            UserStatuses.FirstLogin => UserStatuses.DbFirstLogin,
            var value => value
        };
    }
}

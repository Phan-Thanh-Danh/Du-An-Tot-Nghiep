using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AdministrativeClasses;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AdministrativeClasses;

public class AdministrativeClassService : IAdministrativeClassService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdministrativeClassService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<AdminClassDto>> GetClassesAsync(
        AdminClassQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var query = CreateClassQuery()
            .Where(x => allowedOrganizationIds.Contains(x.Class.MaDonVi));

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Class.MaCodeLop.ToLower().Contains(keyword) ||
                x.Class.TenLop.ToLower().Contains(keyword));
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem lớp của đơn vị này.");
            }

            query = query.Where(x => x.Class.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.Class.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Class.TenLop)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x.Class, x.Organization, x.Teacher))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminClassDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminClassDto> GetByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await CreateClassQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Class.MaLop == classId, cancellationToken);

        if (result is null || !allowedOrganizationIds.Contains(result.Class.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy lớp hành chính.");
        }

        return ToDto(result.Class, result.Organization, result.Teacher);
    }

    public async Task<AdminClassDto> CreateAsync(
        CreateAdminClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var classCode = NormalizeRequiredText(request.MaCodeLop, "Mã lớp");
        var className = NormalizeRequiredText(request.TenLop, "Tên lớp");

        await ValidateClassCodeAsync(classCode, null, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var teacher = await ValidateTeacherAsync(request.MaGiaoVienChuNhiem, organization.MaDonVi, cancellationToken);

        var classEntity = new LopHanhChinh
        {
            MaDonVi = organization.MaDonVi,
            MaCodeLop = classCode,
            TenLop = className,
            MaGiaoVienChuNhiem = teacher?.MaNguoiDung,
            NamNhapHoc = request.NamNhapHoc,
            ConHoatDong = true
        };

        _context.LopHanhChinhs.Add(classEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(classEntity, organization, teacher);
    }

    public async Task<AdminClassDto> UpdateAsync(
        int classId,
        UpdateAdminClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var classEntity = await GetManagedClassAsync(classId, currentUser, cancellationToken);
        var classCode = NormalizeRequiredText(request.MaCodeLop, "Mã lớp");
        var className = NormalizeRequiredText(request.TenLop, "Tên lớp");

        await ValidateClassCodeAsync(classCode, classId, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var teacher = await ValidateTeacherAsync(request.MaGiaoVienChuNhiem, organization.MaDonVi, cancellationToken);

        classEntity.MaDonVi = organization.MaDonVi;
        classEntity.MaCodeLop = classCode;
        classEntity.TenLop = className;
        classEntity.MaGiaoVienChuNhiem = teacher?.MaNguoiDung;
        classEntity.NamNhapHoc = request.NamNhapHoc;
        classEntity.ConHoatDong = request.ConHoatDong;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(classEntity, organization, teacher);
    }

    public async Task DeleteAsync(int classId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var classEntity = await GetManagedClassAsync(classId, currentUser, cancellationToken);
        classEntity.ConHoatDong = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ClassQueryResult> CreateClassQuery()
    {
        return
            from classEntity in _context.LopHanhChinhs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on classEntity.MaDonVi equals organization.MaDonVi
            join teacher in _context.NguoiDungs.AsNoTracking()
                on classEntity.MaGiaoVienChuNhiem equals teacher.MaNguoiDung into teacherJoin
            from teacher in teacherJoin.DefaultIfEmpty()
            select new ClassQueryResult(classEntity, organization, teacher);
    }

    private async Task<LopHanhChinh> GetManagedClassAsync(
        int classId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var classEntity = await _context.LopHanhChinhs.FirstOrDefaultAsync(x => x.MaLop == classId, cancellationToken);
        if (classEntity is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy lớp hành chính.");
        }

        if (!await CanManageOrganizationAsync(currentUser, classEntity.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý lớp của đơn vị này.");
        }

        return classEntity;
    }

    private async Task ValidateClassCodeAsync(
        string classCode,
        int? excludedClassId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.LopHanhChinhs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeLop == classCode &&
                (!excludedClassId.HasValue || x.MaLop != excludedClassId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã lớp đã tồn tại.");
        }
    }

    private async Task<DonVi> ValidateOrganizationAsync(
        int organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);
        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc không hoạt động.");
        }

        if (!await CanManageOrganizationAsync(currentUser, organizationId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý lớp của đơn vị này.");
        }

        return organization;
    }

    private async Task<NguoiDung?> ValidateTeacherAsync(
        int? teacherId,
        int organizationId,
        CancellationToken cancellationToken)
    {
        if (!teacherId.HasValue)
        {
            return null;
        }

        var teacher = await _context.NguoiDungs.FirstOrDefaultAsync(x => x.MaNguoiDung == teacherId.Value, cancellationToken);
        if (teacher is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên chủ nhiệm không tồn tại.");
        }

        if (!teacher.VaiTroChinh.Equals(AuthRoles.ToDatabaseCode(AuthRoles.Teacher), StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên chủ nhiệm phải có vai trò giáo viên.");
        }

        if (teacher.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giáo viên chủ nhiệm phải thuộc cùng đơn vị với lớp.");
        }

        return teacher;
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
            return await _context.DonVis
                .AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            var organizations = await _context.DonVis
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

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalizedValue;
    }

    private static AdminClassDto ToDto(LopHanhChinh classEntity, DonVi organization, NguoiDung? teacher)
    {
        return new AdminClassDto
        {
            MaLop = classEntity.MaLop,
            MaDonVi = classEntity.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaCodeLop = classEntity.MaCodeLop,
            TenLop = classEntity.TenLop,
            MaGiaoVienChuNhiem = classEntity.MaGiaoVienChuNhiem,
            TenGiaoVienChuNhiem = teacher?.HoTen,
            NamNhapHoc = classEntity.NamNhapHoc,
            ConHoatDong = classEntity.ConHoatDong
        };
    }

    private sealed record ClassQueryResult(LopHanhChinh Class, DonVi Organization, NguoiDung? Teacher);
}

using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Buildings;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Buildings;

public class BuildingService : IBuildingService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BuildingService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<BuildingDto>> GetBuildingsAsync(
        BuildingQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        var pageIndex = parameters.PageNumber ?? parameters.PageIndex;

        var query = _context.ToaNhas
            .AsNoTracking()
            .Where(x => allowedOrganizationIdList.Contains(x.MaDonVi));

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.MaCodeToaNha.ToLower().Contains(keyword) ||
                x.TenToaNha.ToLower().Contains(keyword));
        }

        if (parameters.MaDonVi.HasValue)
        {
            EnsureOrganizationInScope(allowedOrganizationIds, parameters.MaDonVi.Value, "Bạn không có quyền xem tòa nhà của đơn vị này.");
            query = query.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await (
            from building in query
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            orderby organization.TenDonVi, building.MaCodeToaNha
            select new BuildingDto
            {
                MaToaNha = building.MaToaNha,
                MaDonVi = building.MaDonVi,
                TenDonVi = organization.TenDonVi,
                MaCodeToaNha = building.MaCodeToaNha,
                TenToaNha = building.TenToaNha,
                DiaChi = building.DiaChi,
                SoTang = building.SoTang,
                ConHoatDong = building.ConHoatDong,
                NgayTao = building.NgayTao,
                NgayCapNhat = building.NgayCapNhat
            })
            .Skip((pageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BuildingDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<BuildingDto> GetByIdAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await (
            from building in _context.ToaNhas.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            where building.MaToaNha == buildingId
            select new BuildingQueryResult(building, organization))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || !allowedOrganizationIds.Contains(result.Building.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tòa nhà.");
        }

        return ToDto(result.Building, result.Organization);
    }

    public async Task<BuildingDto> CreateAsync(CreateBuildingRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var buildingCode = NormalizeRequiredText(request.MaCodeToaNha, "Mã tòa nhà").ToUpperInvariant();
        var buildingName = NormalizeRequiredText(request.TenToaNha, "Tên tòa nhà");

        await ValidateBuildingCodeAsync(organization.MaDonVi, buildingCode, null, cancellationToken);

        var building = new ToaNha
        {
            MaDonVi = organization.MaDonVi,
            MaCodeToaNha = buildingCode,
            TenToaNha = buildingName,
            DiaChi = NormalizeOptionalText(request.DiaChi),
            SoTang = request.SoTang,
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        _context.ToaNhas.Add(building);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(building, organization);
    }

    public async Task<BuildingDto> UpdateAsync(int buildingId, UpdateBuildingRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var building = await GetManagedBuildingAsync(buildingId, currentUser, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var buildingCode = NormalizeRequiredText(request.MaCodeToaNha, "Mã tòa nhà").ToUpperInvariant();
        var buildingName = NormalizeRequiredText(request.TenToaNha, "Tên tòa nhà");

        await ValidateBuildingCodeAsync(organization.MaDonVi, buildingCode, buildingId, cancellationToken);

        building.MaDonVi = organization.MaDonVi;
        building.MaCodeToaNha = buildingCode;
        building.TenToaNha = buildingName;
        building.DiaChi = NormalizeOptionalText(request.DiaChi);
        building.SoTang = request.SoTang;
        building.ConHoatDong = request.ConHoatDong;
        building.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(building, organization);
    }

    public async Task DeleteAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var building = await GetManagedBuildingAsync(buildingId, currentUser, cancellationToken);
        var hasActiveFloors = await _context.Tangs.AnyAsync(x => x.MaToaNha == buildingId && x.ConHoatDong, cancellationToken);
        var hasRooms = await _context.PhongHocs.AnyAsync(x => x.MaToaNha == buildingId && x.TrangThaiPhong != "ngung_hoat_dong", cancellationToken);

        if (hasActiveFloors || hasRooms)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Tòa nhà đang có tầng hoặc phòng học hoạt động.");
        }

        building.ConHoatDong = false;
        building.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<BuildingQueryResult> CreateBuildingQuery()
    {
        return
            from building in _context.ToaNhas.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            select new BuildingQueryResult(building, organization);
    }

    private async Task<ToaNha> GetManagedBuildingAsync(int buildingId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var building = await _context.ToaNhas.FirstOrDefaultAsync(x => x.MaToaNha == buildingId, cancellationToken);
        if (building is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tòa nhà.");
        }

        if (!await CanManageOrganizationAsync(currentUser, building.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý tòa nhà của đơn vị này.");
        }

        return building;
    }

    private async Task<DonVi> ValidateOrganizationAsync(int organizationId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);
        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc không hoạt động.");
        }

        if (!await CanManageOrganizationAsync(currentUser, organizationId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý tòa nhà của đơn vị này.");
        }

        return organization;
    }

    private async Task ValidateBuildingCodeAsync(int organizationId, string buildingCode, int? excludedBuildingId, CancellationToken cancellationToken)
    {
        var exists = await _context.ToaNhas
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaDonVi == organizationId &&
                x.MaCodeToaNha == buildingCode &&
                (!excludedBuildingId.HasValue || x.MaToaNha != excludedBuildingId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã tòa nhà đã tồn tại trong đơn vị này.");
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

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _context.DonVis.AsNoTracking().Select(x => x.MaDonVi).ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            var organizations = await _context.DonVis.AsNoTracking().Select(x => new { x.MaDonVi, x.MaDonViCha }).ToListAsync(cancellationToken);
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

    private async Task<bool> CanManageOrganizationAsync(CurrentUserContext currentUser, int organizationId, CancellationToken cancellationToken)
    {
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        return allowedOrganizationIds.Contains(organizationId);
    }

    private static void EnsureOrganizationInScope(HashSet<int> allowedOrganizationIds, int organizationId, string message)
    {
        if (!allowedOrganizationIds.Contains(organizationId))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, message);
        }
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

    private static BuildingDto ToDto(ToaNha building, DonVi organization)
    {
        return new BuildingDto
        {
            MaToaNha = building.MaToaNha,
            MaDonVi = building.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaCodeToaNha = building.MaCodeToaNha,
            TenToaNha = building.TenToaNha,
            DiaChi = building.DiaChi,
            SoTang = building.SoTang,
            ConHoatDong = building.ConHoatDong,
            NgayTao = building.NgayTao,
            NgayCapNhat = building.NgayCapNhat
        };
    }

    private sealed record BuildingQueryResult(ToaNha Building, DonVi Organization);
}

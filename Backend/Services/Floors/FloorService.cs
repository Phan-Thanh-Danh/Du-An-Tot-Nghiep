using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Floors;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Floors;

public class FloorService : IFloorService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FloorService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<FloorDto>> GetFloorsAsync(
        FloorQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        var pageIndex = parameters.PageNumber ?? parameters.PageIndex;

        var queryBase =
            from floor in _context.Tangs.AsNoTracking()
            join building in _context.ToaNhas.AsNoTracking()
                on floor.MaToaNha equals building.MaToaNha
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            where allowedOrganizationIdList.Contains(building.MaDonVi)
            select new { Floor = floor, Building = building, Organization = organization };

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            queryBase = queryBase.Where(x =>
                x.Floor.TenTang.ToLower().Contains(keyword) ||
                x.Building.MaCodeToaNha.ToLower().Contains(keyword) ||
                x.Building.TenToaNha.ToLower().Contains(keyword));
        }

        if (parameters.MaDonVi.HasValue)
        {
            EnsureOrganizationInScope(allowedOrganizationIds, parameters.MaDonVi.Value, "Bạn không có quyền xem tầng của đơn vị này.");
            queryBase = queryBase.Where(x => x.Building.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.MaToaNha.HasValue)
        {
            queryBase = queryBase.Where(x => x.Floor.MaToaNha == parameters.MaToaNha.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            queryBase = queryBase.Where(x => x.Floor.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await queryBase.CountAsync(cancellationToken);
        var items = await queryBase
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Building.MaCodeToaNha)
            .ThenBy(x => x.Floor.ThuTuTang)
            .Skip((pageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new FloorDto
            {
                MaTang = x.Floor.MaTang,
                MaToaNha = x.Floor.MaToaNha,
                MaCodeToaNha = x.Building.MaCodeToaNha,
                TenToaNha = x.Building.TenToaNha,
                MaDonVi = x.Building.MaDonVi,
                TenDonVi = x.Organization.TenDonVi,
                TenTang = x.Floor.TenTang,
                ThuTuTang = x.Floor.ThuTuTang,
                MoTa = x.Floor.MoTa,
                ConHoatDong = x.Floor.ConHoatDong
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FloorDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PagedResultDto<FloorDto>> GetByBuildingAsync(
        int buildingId,
        FloorQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters.MaToaNha = buildingId;
        return await GetFloorsAsync(parameters, cancellationToken);
    }

    public async Task<FloorDto> GetByIdAsync(int floorId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await (
            from floor in _context.Tangs.AsNoTracking()
            join building in _context.ToaNhas.AsNoTracking()
                on floor.MaToaNha equals building.MaToaNha
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            where floor.MaTang == floorId
            select new FloorQueryResult(floor, building, organization))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || !allowedOrganizationIds.Contains(result.Building.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tầng.");
        }

        return ToDto(result.Floor, result.Building, result.Organization);
    }

    public async Task<FloorDto> CreateAsync(CreateFloorRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var building = await ValidateBuildingAsync(request.MaToaNha, currentUser, cancellationToken);
        var floorName = NormalizeRequiredText(request.TenTang, "Tên tầng");

        await ValidateFloorOrderAsync(building.MaToaNha, request.ThuTuTang, null, cancellationToken);

        var floor = new Tang
        {
            MaToaNha = building.MaToaNha,
            TenTang = floorName,
            ThuTuTang = request.ThuTuTang,
            MoTa = NormalizeOptionalText(request.MoTa),
            ConHoatDong = true
        };

        _context.Tangs.Add(floor);
        await _context.SaveChangesAsync(cancellationToken);

        var organization = await _context.DonVis.AsNoTracking().FirstAsync(x => x.MaDonVi == building.MaDonVi, cancellationToken);
        return ToDto(floor, building, organization);
    }

    public async Task<FloorDto> UpdateAsync(int floorId, UpdateFloorRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var floor = await GetManagedFloorAsync(floorId, currentUser, cancellationToken);
        var building = await ValidateBuildingAsync(request.MaToaNha, currentUser, cancellationToken);
        var floorName = NormalizeRequiredText(request.TenTang, "Tên tầng");

        await ValidateFloorOrderAsync(building.MaToaNha, request.ThuTuTang, floorId, cancellationToken);

        floor.MaToaNha = building.MaToaNha;
        floor.TenTang = floorName;
        floor.ThuTuTang = request.ThuTuTang;
        floor.MoTa = NormalizeOptionalText(request.MoTa);
        floor.ConHoatDong = request.ConHoatDong;

        await _context.SaveChangesAsync(cancellationToken);

        var organization = await _context.DonVis.AsNoTracking().FirstAsync(x => x.MaDonVi == building.MaDonVi, cancellationToken);
        return ToDto(floor, building, organization);
    }

    public async Task DeleteAsync(int floorId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var floor = await GetManagedFloorAsync(floorId, currentUser, cancellationToken);
        var hasRooms = await _context.PhongHocs.AnyAsync(x =>
            x.MaTang == floorId && x.TrangThaiPhong != "ngung_hoat_dong",
            cancellationToken);

        if (hasRooms)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Tầng đang có phòng học hoạt động.");
        }

        floor.ConHoatDong = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResultDto<FloorDto>> ToPagedResultAsync(
        IQueryable<FloorQueryResult> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Building.MaCodeToaNha)
            .ThenBy(x => x.Floor.ThuTuTang)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToDto(x.Floor, x.Building, x.Organization))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FloorDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<FloorQueryResult> CreateFloorQuery()
    {
        return
            from floor in _context.Tangs.AsNoTracking()
            join building in _context.ToaNhas.AsNoTracking()
                on floor.MaToaNha equals building.MaToaNha
            join organization in _context.DonVis.AsNoTracking()
                on building.MaDonVi equals organization.MaDonVi
            select new FloorQueryResult(floor, building, organization);
    }

    private async Task<Tang> GetManagedFloorAsync(int floorId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var floor = await _context.Tangs.FirstOrDefaultAsync(x => x.MaTang == floorId, cancellationToken);
        if (floor is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy tầng.");
        }

        var building = await _context.ToaNhas.AsNoTracking().FirstOrDefaultAsync(x => x.MaToaNha == floor.MaToaNha, cancellationToken);
        if (building is null || !await CanManageOrganizationAsync(currentUser, building.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý tầng của tòa nhà này.");
        }

        return floor;
    }

    private async Task<ToaNha> ValidateBuildingAsync(int buildingId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var building = await _context.ToaNhas.FirstOrDefaultAsync(x => x.MaToaNha == buildingId, cancellationToken);
        if (building is null || !building.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tòa nhà không tồn tại hoặc không hoạt động.");
        }

        if (!await CanManageOrganizationAsync(currentUser, building.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý tầng của tòa nhà này.");
        }

        return building;
    }

    private async Task ValidateFloorOrderAsync(int buildingId, int floorOrder, int? excludedFloorId, CancellationToken cancellationToken)
    {
        var exists = await _context.Tangs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaToaNha == buildingId &&
                x.ThuTuTang == floorOrder &&
                (!excludedFloorId.HasValue || x.MaTang != excludedFloorId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Thứ tự tầng đã tồn tại trong tòa nhà này.");
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

    private static FloorDto ToDto(Tang floor, ToaNha building, DonVi organization)
    {
        return new FloorDto
        {
            MaTang = floor.MaTang,
            MaToaNha = floor.MaToaNha,
            MaCodeToaNha = building.MaCodeToaNha,
            TenToaNha = building.TenToaNha,
            MaDonVi = building.MaDonVi,
            TenDonVi = organization.TenDonVi,
            TenTang = floor.TenTang,
            ThuTuTang = floor.ThuTuTang,
            MoTa = floor.MoTa,
            ConHoatDong = floor.ConHoatDong
        };
    }

    private sealed record FloorQueryResult(Tang Floor, ToaNha Building, DonVi Organization);
}

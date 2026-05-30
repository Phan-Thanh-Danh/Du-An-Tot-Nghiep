using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Rooms;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Rooms;

public class RoomService : IRoomService
{
    private const string ActiveStatus = "hoat_dong";
    private const string MaintenanceStatus = "bao_tri";
    private const string InactiveStatus = "ngung_hoat_dong";

    private static readonly HashSet<string> AllowedRoomStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ActiveStatus,
        MaintenanceStatus,
        InactiveStatus
    };

    private static readonly HashSet<string> AllowedRoomTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ly_thuyet",
        "phong_thi_nghiem",
        "thuc_hanh",
        "lab",
        "hoi_truong",
        "truc_tuyen",
        "khac"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoomService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<RoomListItemDto>> GetRoomsAsync(
        RoomQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        var pageIndex = parameters.PageNumber ?? parameters.PageIndex;
        var buildingId = parameters.MaToaNha ?? parameters.BuildingId;
        var floorId = parameters.MaTang ?? parameters.FloorId;

        var queryBase =
            from room in _context.PhongHocs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on room.MaDonVi equals organization.MaDonVi
            join building in _context.ToaNhas.AsNoTracking()
                on room.MaToaNha equals building.MaToaNha
            join floor in _context.Tangs.AsNoTracking()
                on room.MaTang equals floor.MaTang
            where allowedOrganizationIdList.Contains(room.MaDonVi)
            select new { Room = room, Organization = organization, Building = building, Floor = floor };

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            queryBase = queryBase.Where(x =>
                x.Room.MaCodePhong.ToLower().Contains(keyword) ||
                x.Room.TenPhong.ToLower().Contains(keyword) ||
                x.Building.MaCodeToaNha.ToLower().Contains(keyword) ||
                x.Building.TenToaNha.ToLower().Contains(keyword) ||
                x.Floor.TenTang.ToLower().Contains(keyword));
        }

        if (parameters.MaDonVi.HasValue)
        {
            EnsureOrganizationInScope(allowedOrganizationIds, parameters.MaDonVi.Value, "Bạn không có quyền xem phòng học của đơn vị này.");
            queryBase = queryBase.Where(x => x.Room.MaDonVi == parameters.MaDonVi.Value);
        }

        if (buildingId.HasValue)
        {
            queryBase = queryBase.Where(x => x.Room.MaToaNha == buildingId.Value);
        }

        if (floorId.HasValue)
        {
            queryBase = queryBase.Where(x => x.Room.MaTang == floorId.Value);
        }

        var roomTypeFilter = parameters.LoaiPhong ?? parameters.RoomType;
        if (!string.IsNullOrWhiteSpace(roomTypeFilter))
        {
            var roomType = NormalizeRoomType(roomTypeFilter);
            queryBase = queryBase.Where(x => x.Room.LoaiPhong == roomType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThaiPhong))
        {
            var roomStatus = NormalizeRoomStatus(parameters.TrangThaiPhong);
            queryBase = queryBase.Where(x => x.Room.TrangThaiPhong == roomStatus);
        }

        if (parameters.IsActive.HasValue)
        {
            queryBase = parameters.IsActive.Value
                ? queryBase.Where(x => x.Room.TrangThaiPhong != InactiveStatus)
                : queryBase.Where(x => x.Room.TrangThaiPhong == InactiveStatus);
        }

        var totalItems = await queryBase.CountAsync(cancellationToken);
        var items = await queryBase
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Building.MaCodeToaNha)
            .ThenBy(x => x.Floor.ThuTuTang)
            .ThenBy(x => x.Room.MaCodePhong)
            .Skip((pageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new RoomListItemDto
            {
                MaPhong = x.Room.MaPhong,
                MaDonVi = x.Room.MaDonVi,
                TenDonVi = x.Organization.TenDonVi,
                MaToaNha = x.Building.MaToaNha,
                MaCodeToaNha = x.Building.MaCodeToaNha,
                TenToaNha = x.Building.TenToaNha,
                MaTang = x.Floor.MaTang,
                TenTang = x.Floor.TenTang,
                ThuTuTang = x.Floor.ThuTuTang,
                MaCodePhong = x.Room.MaCodePhong,
                TenPhong = x.Room.TenPhong,
                SucChua = x.Room.SucChua,
                LoaiPhong = x.Room.LoaiPhong,
                TrangThaiPhong = x.Room.TrangThaiPhong
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<RoomListItemDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<RoomDetailDto> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await (
            from room in _context.PhongHocs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on room.MaDonVi equals organization.MaDonVi
            join building in _context.ToaNhas.AsNoTracking()
                on room.MaToaNha equals building.MaToaNha
            join floor in _context.Tangs.AsNoTracking()
                on room.MaTang equals floor.MaTang
            where room.MaPhong == roomId
            select new RoomQueryResult(room, organization, building, floor))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || !allowedOrganizationIds.Contains(result.Room.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy phòng học.");
        }

        return ToDetailDto(result.Room, result.Organization, result.Building, result.Floor);
    }

    public async Task<PagedResultDto<RoomListItemDto>> GetByFloorAsync(
        int floorId,
        RoomQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        parameters.MaTang = floorId;
        return await GetRoomsAsync(parameters, cancellationToken);
    }

    public async Task<RoomDetailDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var roomCode = NormalizeRequiredText(request.MaCodePhong, "Mã phòng").ToUpperInvariant();
        var roomName = NormalizeRequiredText(request.TenPhong, "Tên phòng");
        var roomType = NormalizeRoomType(request.LoaiPhong);

        ValidateCapacity(request.SucChua);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var building = await ValidateBuildingAsync(request.MaToaNha, organization.MaDonVi, currentUser, cancellationToken);
        var floor = await ValidateFloorAsync(request.MaTang, building.MaToaNha, cancellationToken);
        await ValidateRoomCodeAsync(organization.MaDonVi, roomCode, null, cancellationToken);

        var room = new PhongHoc
        {
            MaDonVi = organization.MaDonVi,
            MaToaNha = building.MaToaNha,
            MaTang = floor.MaTang,
            MaCodePhong = roomCode,
            TenPhong = roomName,
            SucChua = request.SucChua,
            LoaiPhong = roomType,
            TrangThaiPhong = ActiveStatus,
            GhiChu = NormalizeOptionalText(request.GhiChu)
        };

        _context.PhongHocs.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDetailDto(room, organization, building, floor);
    }

    public async Task<RoomDetailDto> UpdateAsync(int roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var room = await GetManagedRoomAsync(roomId, currentUser, cancellationToken);
        var roomCode = NormalizeRequiredText(request.MaCodePhong, "Mã phòng").ToUpperInvariant();
        var roomName = NormalizeRequiredText(request.TenPhong, "Tên phòng");
        var roomType = NormalizeRoomType(request.LoaiPhong);
        var roomStatus = NormalizeRoomStatus(request.TrangThaiPhong);

        ValidateCapacity(request.SucChua);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var building = await ValidateBuildingAsync(request.MaToaNha, organization.MaDonVi, currentUser, cancellationToken);
        var floor = await ValidateFloorAsync(request.MaTang, building.MaToaNha, cancellationToken);
        await ValidateRoomCodeAsync(organization.MaDonVi, roomCode, roomId, cancellationToken);

        room.MaDonVi = organization.MaDonVi;
        room.MaToaNha = building.MaToaNha;
        room.MaTang = floor.MaTang;
        room.MaCodePhong = roomCode;
        room.TenPhong = roomName;
        room.SucChua = request.SucChua;
        room.LoaiPhong = roomType;
        room.TrangThaiPhong = roomStatus;
        room.GhiChu = NormalizeOptionalText(request.GhiChu);

        await _context.SaveChangesAsync(cancellationToken);
        return ToDetailDto(room, organization, building, floor);
    }

    public async Task DeleteAsync(int roomId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var room = await GetManagedRoomAsync(roomId, currentUser, cancellationToken);
        room.TrangThaiPhong = InactiveStatus;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<RoomQueryResult> CreateRoomQuery()
    {
        return
            from room in _context.PhongHocs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on room.MaDonVi equals organization.MaDonVi
            join building in _context.ToaNhas.AsNoTracking()
                on room.MaToaNha equals building.MaToaNha
            join floor in _context.Tangs.AsNoTracking()
                on room.MaTang equals floor.MaTang
            select new RoomQueryResult(room, organization, building, floor);
    }

    private async Task<PhongHoc> GetManagedRoomAsync(int roomId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var room = await _context.PhongHocs.FirstOrDefaultAsync(x => x.MaPhong == roomId, cancellationToken);
        if (room is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy phòng học.");
        }

        if (!await CanManageOrganizationAsync(currentUser, room.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý phòng học của đơn vị này.");
        }

        return room;
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý phòng học của đơn vị này.");
        }

        return organization;
    }

    private async Task<ToaNha> ValidateBuildingAsync(
        int buildingId,
        int organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var building = await _context.ToaNhas.FirstOrDefaultAsync(x => x.MaToaNha == buildingId, cancellationToken);
        if (building is null || !building.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tòa nhà không tồn tại hoặc không hoạt động.");
        }

        if (building.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tòa nhà không thuộc đơn vị đã chọn.");
        }

        if (!await CanManageOrganizationAsync(currentUser, building.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý phòng học của tòa nhà này.");
        }

        return building;
    }

    private async Task<Tang> ValidateFloorAsync(int floorId, int buildingId, CancellationToken cancellationToken)
    {
        var floor = await _context.Tangs.FirstOrDefaultAsync(x => x.MaTang == floorId, cancellationToken);
        if (floor is null || !floor.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tầng không tồn tại hoặc không hoạt động.");
        }

        if (floor.MaToaNha != buildingId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tầng không thuộc tòa nhà đã chọn.");
        }

        return floor;
    }

    private async Task ValidateRoomCodeAsync(int organizationId, string roomCode, int? excludedRoomId, CancellationToken cancellationToken)
    {
        var exists = await _context.PhongHocs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaDonVi == organizationId &&
                x.MaCodePhong == roomCode &&
                (!excludedRoomId.HasValue || x.MaPhong != excludedRoomId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã phòng đã tồn tại trong đơn vị này.");
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

    private static string NormalizeRoomType(string value)
    {
        var roomType = NormalizeRequiredText(value, "Loại phòng").ToLowerInvariant();
        if (!AllowedRoomTypes.Contains(roomType))
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "Loại phòng không hợp lệ. Giá trị hợp lệ: ly_thuyet, phong_thi_nghiem, thuc_hanh, lab, hoi_truong, truc_tuyen, khac.");
        }

        return roomType;
    }

    private static string NormalizeRoomStatus(string value)
    {
        var roomStatus = NormalizeRequiredText(value, "Trạng thái phòng").ToLowerInvariant();
        if (!AllowedRoomStatuses.Contains(roomStatus))
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "Trạng thái phòng không hợp lệ. Giá trị hợp lệ: hoat_dong, bao_tri, ngung_hoat_dong.");
        }

        return roomStatus;
    }

    private static void ValidateCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Sức chứa phải lớn hơn 0.");
        }
    }

    private static RoomListItemDto ToListItemDto(PhongHoc room, DonVi organization, ToaNha building, Tang floor)
    {
        return new RoomListItemDto
        {
            MaPhong = room.MaPhong,
            MaDonVi = room.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaToaNha = building.MaToaNha,
            MaCodeToaNha = building.MaCodeToaNha,
            TenToaNha = building.TenToaNha,
            MaTang = floor.MaTang,
            TenTang = floor.TenTang,
            ThuTuTang = floor.ThuTuTang,
            MaCodePhong = room.MaCodePhong,
            TenPhong = room.TenPhong,
            SucChua = room.SucChua,
            LoaiPhong = room.LoaiPhong,
            TrangThaiPhong = room.TrangThaiPhong
        };
    }

    private static RoomDetailDto ToDetailDto(PhongHoc room, DonVi organization, ToaNha building, Tang floor)
    {
        return new RoomDetailDto
        {
            MaPhong = room.MaPhong,
            MaDonVi = room.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaToaNha = building.MaToaNha,
            MaCodeToaNha = building.MaCodeToaNha,
            TenToaNha = building.TenToaNha,
            MaTang = floor.MaTang,
            TenTang = floor.TenTang,
            ThuTuTang = floor.ThuTuTang,
            MaCodePhong = room.MaCodePhong,
            TenPhong = room.TenPhong,
            SucChua = room.SucChua,
            LoaiPhong = room.LoaiPhong,
            TrangThaiPhong = room.TrangThaiPhong,
            GhiChu = room.GhiChu
        };
    }

    private sealed record RoomQueryResult(PhongHoc Room, DonVi Organization, ToaNha Building, Tang Floor);
}

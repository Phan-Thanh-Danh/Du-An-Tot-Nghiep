using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Organizations;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Organizations;

public class OrganizationService : IOrganizationService
{
    private const string ApiRoot = "Root";
    private const string ApiCampus = "Campus";
    private const string ApiSubCampus = "SubCampus";
    private const string DbRoot = "root";
    private const string DbCampus = "co_so";
    private const string DbSubCampus = "co_so_con";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<OrganizationResponseDto>> GetAllAsync()
    {
        var organizations = await GetScopedActiveOrganizationsAsync();

        return organizations
            .OrderBy(x => x.MaDonViCha.HasValue)
            .ThenBy(x => x.MaDonViCha)
            .ThenBy(x => x.TenDonVi)
            .Select(ToResponseDto)
            .ToList();
    }

    public async Task<IReadOnlyList<OrganizationTreeDto>> GetTreeAsync()
    {
        var organizations = await GetScopedActiveOrganizationsAsync();
        return BuildTree(organizations);
    }

    public async Task<OrganizationResponseDto> GetByIdAsync(int id)
    {
        var organization = (await GetScopedActiveOrganizationsAsync())
            .FirstOrDefault(x => x.MaDonVi == id);

        if (organization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Organization was not found.");
        }

        return ToResponseDto(organization);
    }

    public async Task<OrganizationResponseDto> CreateAsync(OrganizationCreateDto dto, int currentUserId)
    {
        _ = currentUserId;

        var name = NormalizeName(dto.Name);
        var level = ToDatabaseLevel(dto.OrganizationLevel);

        await ValidateParentAsync(dto.ParentId, level);

        var organization = new DonVi
        {
            MaDonViCha = dto.ParentId,
            TenDonVi = name,
            CapDonVi = level,
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        _context.DonVis.Add(organization);
        await _context.SaveChangesAsync();

        return ToResponseDto(organization);
    }

    public async Task<OrganizationResponseDto> UpdateAsync(int id, OrganizationUpdateDto dto, int currentUserId)
    {
        _ = currentUserId;

        var organization = await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == id);
        if (organization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Organization was not found.");
        }

        var name = NormalizeName(dto.Name);
        var level = ToDatabaseLevel(dto.OrganizationLevel);

        await ValidateParentAsync(dto.ParentId, level);
        await PreventCircularReferenceAsync(id, dto.ParentId);

        organization.MaDonViCha = dto.ParentId;
        organization.TenDonVi = name;
        organization.CapDonVi = level;
        organization.ConHoatDong = dto.IsActive;
        organization.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToResponseDto(organization);
    }

    public async Task SoftDeleteAsync(int id, int currentUserId)
    {
        _ = currentUserId;

        var organization = await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == id);
        if (organization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Organization was not found.");
        }

        organization.ConHoatDong = false;
        organization.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id, int currentUserId)
    {
        _ = currentUserId;

        var organization = await _context.DonVis.FirstOrDefaultAsync(x => x.MaDonVi == id);
        if (organization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Organization was not found.");
        }

        var blockers = await GetHardDeleteBlockersAsync(id);
        if (blockers.Count > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Cannot hard delete this organization because related data exists: {string.Join(", ", blockers)}.");
        }

        _context.DonVis.Remove(organization);
        await _context.SaveChangesAsync();
    }

    public async Task<OrganizationTreeDto> GetSubtreeAsync(int id)
    {
        var organizations = await GetScopedActiveOrganizationsAsync();
        var root = organizations.FirstOrDefault(x => x.MaDonVi == id);

        if (root is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Organization was not found.");
        }

        return BuildNode(root, organizations);
    }

    public async Task ValidateParentAsync(int? parentId, string organizationLevel)
    {
        var level = ToDatabaseLevel(organizationLevel);

        if (level == DbRoot)
        {
            if (parentId.HasValue)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Root organization cannot have a parent.");
            }

            return;
        }

        if (!parentId.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Campus and SubCampus organizations must have a parent.");
        }

        var parent = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == parentId.Value && x.ConHoatDong);

        if (parent is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Parent organization is invalid or inactive.");
        }

        if (level == DbCampus && parent.CapDonVi != DbRoot)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Campus organization must have a Root parent.");
        }

        if (level == DbSubCampus && parent.CapDonVi != DbCampus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "SubCampus organization must have a Campus parent.");
        }
    }

    public async Task PreventCircularReferenceAsync(int id, int? newParentId)
    {
        if (!newParentId.HasValue)
        {
            return;
        }

        if (id == newParentId.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Organization cannot be its own parent.");
        }

        var organizations = await _context.DonVis
            .AsNoTracking()
            .Select(x => new { x.MaDonVi, x.MaDonViCha })
            .ToListAsync();

        var parentLookup = organizations.ToDictionary(x => x.MaDonVi, x => x.MaDonViCha);
        var visited = new HashSet<int>();
        var currentParentId = newParentId;

        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == id)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Circular organization hierarchy is not allowed.");
            }

            if (!visited.Add(currentParentId.Value) ||
                !parentLookup.TryGetValue(currentParentId.Value, out var nextParentId))
            {
                return;
            }

            currentParentId = nextParentId;
        }
    }

    private async Task<List<DonVi>> GetScopedActiveOrganizationsAsync()
    {
        var currentUser = GetCurrentUser();
        var organizations = await _context.DonVis
            .AsNoTracking()
            .Where(x => x.ConHoatDong)
            .ToListAsync();

        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return organizations;
        }

        var allowedIds = currentUser.Role == AuthRoles.CampusAdmin
            ? GetDescendantIds(organizations, currentUser.CampusId)
            : new HashSet<int> { currentUser.CampusId };

        return organizations
            .Where(x => allowedIds.Contains(x.MaDonVi))
            .ToList();
    }

    private async Task<List<string>> GetHardDeleteBlockersAsync(int organizationId)
    {
        var blockers = new List<string>();

        await AddBlockerIfExistsAsync(blockers, "child organizations", () => _context.DonVis.AnyAsync(x => x.MaDonViCha == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(AnhChupPhanTich), () => _context.AnhChupPhanTichs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(BaoCaoSuDungPhong), () => _context.BaoCaoSuDungPhongs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(CauHinhKhenThuong), () => _context.CauHinhKhenThuongs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(DatPhong), () => _context.DatPhongs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(DiemDanh), () => _context.DiemDanhs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(DiemSo), () => _context.DiemSos.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(GiaiDoanDangKy), () => _context.GiaiDoanDangKys.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(HoaDon), () => _context.HoaDons.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(HocKy), () => _context.HocKys.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(HoSoKyLuat), () => _context.HoSoKyLuats.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(KhoaHoc), () => _context.KhoaHocs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(LopHanhChinh), () => _context.LopHanhChinhs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(LopHocPhan), () => _context.LopHocPhans.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(NguoiDung), () => _context.NguoiDungs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(NhatKyKiemToan), () => _context.NhatKyKiemToans.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(NhatKyThongBao), () => _context.NhatKyThongBaos.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(PhongHoc), () => _context.PhongHocs.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(ThoiKhoaBieu), () => _context.ThoiKhoaBieus.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(ThongBao), () => _context.ThongBaos.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(ThongBaoHenGio), () => _context.ThongBaoHenGios.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(XuatBaoCao), () => _context.XuatBaoCaos.AnyAsync(x => x.MaDonVi == organizationId));
        await AddBlockerIfExistsAsync(blockers, nameof(YeuCauHoanPhi), () => _context.YeuCauHoanPhis.AnyAsync(x => x.MaDonVi == organizationId));

        return blockers;
    }

    private static async Task AddBlockerIfExistsAsync(List<string> blockers, string blockerName, Func<Task<bool>> hasRelatedData)
    {
        if (await hasRelatedData())
        {
            blockers.Add(blockerName);
        }
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid authentication token.");
        }

        return currentUser;
    }

    private static HashSet<int> GetDescendantIds(IReadOnlyCollection<DonVi> organizations, int rootId)
    {
        var allowedIds = new HashSet<int> { rootId };
        var queue = new Queue<int>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            foreach (var child in organizations.Where(x => x.MaDonViCha == currentId))
            {
                if (allowedIds.Add(child.MaDonVi))
                {
                    queue.Enqueue(child.MaDonVi);
                }
            }
        }

        return allowedIds;
    }

    private static List<OrganizationTreeDto> BuildTree(IReadOnlyCollection<DonVi> organizations)
    {
        var organizationIds = organizations.Select(x => x.MaDonVi).ToHashSet();

        return organizations
            .Where(x => !x.MaDonViCha.HasValue || !organizationIds.Contains(x.MaDonViCha.Value))
            .OrderBy(x => x.TenDonVi)
            .Select(x => BuildNode(x, organizations))
            .ToList();
    }

    private static OrganizationTreeDto BuildNode(DonVi organization, IReadOnlyCollection<DonVi> organizations)
    {
        var node = ToTreeDto(organization);
        node.Children = organizations
            .Where(x => x.MaDonViCha == organization.MaDonVi)
            .OrderBy(x => x.TenDonVi)
            .Select(x => BuildNode(x, organizations))
            .ToList();

        return node;
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Organization name is required.");
        }

        if (normalizedName.Length > 255)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Organization name cannot exceed 255 characters.");
        }

        return normalizedName;
    }

    private static string ToDatabaseLevel(string organizationLevel)
    {
        return organizationLevel.Trim() switch
        {
            var value when value.Equals(ApiRoot, StringComparison.OrdinalIgnoreCase) => DbRoot,
            var value when value.Equals(ApiCampus, StringComparison.OrdinalIgnoreCase) => DbCampus,
            var value when value.Equals(ApiSubCampus, StringComparison.OrdinalIgnoreCase) => DbSubCampus,
            var value when value.Equals(DbRoot, StringComparison.OrdinalIgnoreCase) => DbRoot,
            var value when value.Equals(DbCampus, StringComparison.OrdinalIgnoreCase) => DbCampus,
            var value when value.Equals(DbSubCampus, StringComparison.OrdinalIgnoreCase) => DbSubCampus,
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "OrganizationLevel must be Root, Campus, or SubCampus.")
        };
    }

    private static string ToApiLevel(string databaseLevel)
    {
        return databaseLevel switch
        {
            DbRoot => ApiRoot,
            DbCampus => ApiCampus,
            DbSubCampus => ApiSubCampus,
            _ => databaseLevel
        };
    }

    private static OrganizationResponseDto ToResponseDto(DonVi organization)
    {
        return new OrganizationResponseDto
        {
            Id = organization.MaDonVi,
            ParentId = organization.MaDonViCha,
            Name = organization.TenDonVi,
            OrganizationLevel = ToApiLevel(organization.CapDonVi),
            IsActive = organization.ConHoatDong,
            CreatedAt = organization.NgayTao,
            UpdatedAt = organization.NgayCapNhat
        };
    }

    private static OrganizationTreeDto ToTreeDto(DonVi organization)
    {
        return new OrganizationTreeDto
        {
            Id = organization.MaDonVi,
            ParentId = organization.MaDonViCha,
            Name = organization.TenDonVi,
            OrganizationLevel = ToApiLevel(organization.CapDonVi),
            IsActive = organization.ConHoatDong,
            CreatedAt = organization.NgayTao,
            UpdatedAt = organization.NgayCapNhat
        };
    }
}

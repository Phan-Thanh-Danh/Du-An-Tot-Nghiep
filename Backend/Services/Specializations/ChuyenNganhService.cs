using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Specializations;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Specializations;

public class ChuyenNganhService : IChuyenNganhService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChuyenNganhService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<SpecializationDto>> GetSpecializationsAsync(
        SpecializationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = CreateSpecializationQuery();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Specialization.MaCodeChuyenNganh.ToLower().Contains(keyword) ||
                x.Specialization.TenChuyenNganh.ToLower().Contains(keyword) ||
                x.Major.TenNganh.ToLower().Contains(keyword));
        }

        if (parameters.MaNganh.HasValue)
        {
            query = query.Where(x => x.Specialization.MaNganh == parameters.MaNganh.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.Specialization.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Major.TenNganh)
            .ThenBy(x => x.Specialization.MaCodeChuyenNganh)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x.Specialization, x.Major))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SpecializationDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<SpecializationDto> GetByIdAsync(int specializationId, CancellationToken cancellationToken = default)
    {
        var result = await CreateSpecializationQuery()
            .FirstOrDefaultAsync(x => x.Specialization.MaChuyenNganh == specializationId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chuyên ngành.");
        }

        return ToDto(result.Specialization, result.Major);
    }

    public async Task<SpecializationDto> CreateAsync(
        CreateSpecializationRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var major = await ValidateMajorAsync(request.MaNganh, cancellationToken);
        var specializationCode = NormalizeCode(request.MaCodeChuyenNganh, "Mã chuyên ngành");
        var specializationName = NormalizeRequiredText(request.TenChuyenNganh, "Tên chuyên ngành");
        await ValidateSpecializationCodeAsync(specializationCode, null, cancellationToken);

        var specialization = new ChuyenNganh
        {
            MaNganh = major.MaNganh,
            MaCodeChuyenNganh = specializationCode,
            TenChuyenNganh = specializationName,
            MoTa = NormalizeOptionalText(request.MoTa),
            ConHoatDong = true
        };

        _context.ChuyenNganhs.Add(specialization);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(specialization, major);
    }

    public async Task<SpecializationDto> UpdateAsync(
        int specializationId,
        UpdateSpecializationRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var specialization = await GetManagedSpecializationAsync(specializationId, cancellationToken);
        var major = await ValidateMajorAsync(request.MaNganh, cancellationToken);
        var specializationCode = NormalizeCode(request.MaCodeChuyenNganh, "Mã chuyên ngành");
        var specializationName = NormalizeRequiredText(request.TenChuyenNganh, "Tên chuyên ngành");
        await ValidateSpecializationCodeAsync(specializationCode, specializationId, cancellationToken);

        specialization.MaNganh = major.MaNganh;
        specialization.MaCodeChuyenNganh = specializationCode;
        specialization.TenChuyenNganh = specializationName;
        specialization.MoTa = NormalizeOptionalText(request.MoTa);
        specialization.ConHoatDong = request.ConHoatDong;
        specialization.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(specialization, major);
    }

    public async Task DeleteAsync(int specializationId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var specialization = await GetManagedSpecializationAsync(specializationId, cancellationToken);
        specialization.ConHoatDong = false;
        specialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SpecializationDto> ActivateAsync(int specializationId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var specialization = await GetManagedSpecializationAsync(specializationId, cancellationToken);
        var major = await ValidateMajorAsync(specialization.MaNganh, cancellationToken);
        specialization.ConHoatDong = true;
        specialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(specialization, major);
    }

    public async Task<SpecializationDto> DeactivateAsync(int specializationId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var specialization = await GetManagedSpecializationAsync(specializationId, cancellationToken);
        var major = await _context.NganhDaoTaos
            .AsNoTracking()
            .FirstAsync(x => x.MaNganh == specialization.MaNganh, cancellationToken);

        specialization.ConHoatDong = false;
        specialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(specialization, major);
    }

    private IQueryable<SpecializationQueryResult> CreateSpecializationQuery()
    {
        return
            from specialization in _context.ChuyenNganhs.AsNoTracking()
            join major in _context.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            select new SpecializationQueryResult
            {
                Specialization = specialization,
                Major = major
            };
    }

    private async Task<ChuyenNganh> GetManagedSpecializationAsync(
        int specializationId,
        CancellationToken cancellationToken)
    {
        var specialization = await _context.ChuyenNganhs
            .FirstOrDefaultAsync(x => x.MaChuyenNganh == specializationId, cancellationToken);
        if (specialization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chuyên ngành.");
        }

        return specialization;
    }

    private async Task<NganhDaoTao> ValidateMajorAsync(int majorId, CancellationToken cancellationToken)
    {
        var major = await _context.NganhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNganh == majorId, cancellationToken);

        if (major is null || !major.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngành đào tạo không tồn tại hoặc không hoạt động.");
        }

        return major;
    }

    private async Task ValidateSpecializationCodeAsync(
        string specializationCode,
        int? excludedSpecializationId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ChuyenNganhs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeChuyenNganh == specializationCode &&
                (!excludedSpecializationId.HasValue || x.MaChuyenNganh != excludedSpecializationId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã chuyên ngành đã tồn tại.");
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

    private void EnsureSuperAdmin()
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý chuyên ngành chuẩn.");
        }
    }

    private static string NormalizeCode(string value, string fieldName)
    {
        return NormalizeRequiredText(value, fieldName).ToUpperInvariant();
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

    private static SpecializationDto ToDto(ChuyenNganh specialization, NganhDaoTao major)
    {
        return new SpecializationDto
        {
            MaChuyenNganh = specialization.MaChuyenNganh,
            MaNganh = specialization.MaNganh,
            MaCodeNganh = major.MaCodeNganh,
            TenNganh = major.TenNganh,
            MaCodeChuyenNganh = specialization.MaCodeChuyenNganh,
            TenChuyenNganh = specialization.TenChuyenNganh,
            MoTa = specialization.MoTa,
            ConHoatDong = specialization.ConHoatDong,
            NgayTao = specialization.NgayTao,
            NgayCapNhat = specialization.NgayCapNhat
        };
    }

    private sealed class SpecializationQueryResult
    {
        public ChuyenNganh Specialization { get; init; } = null!;
        public NganhDaoTao Major { get; init; } = null!;
    }
}

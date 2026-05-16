using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Majors;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Majors;

public class NganhDaoTaoService : INganhDaoTaoService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NganhDaoTaoService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<MajorDto>> GetMajorsAsync(
        MajorQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.NganhDaoTaos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.MaCodeNganh.ToLower().Contains(keyword) ||
                x.TenNganh.ToLower().Contains(keyword));
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.MaCodeNganh)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<MajorDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<MajorDto> GetByIdAsync(int majorId, CancellationToken cancellationToken = default)
    {
        var major = await _context.NganhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNganh == majorId, cancellationToken);

        if (major is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy ngành đào tạo.");
        }

        return ToDto(major);
    }

    public async Task<MajorDto> CreateAsync(CreateMajorRequest request, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var majorCode = NormalizeCode(request.MaCodeNganh, "Mã ngành");
        var majorName = NormalizeRequiredText(request.TenNganh, "Tên ngành");
        await ValidateMajorCodeAsync(majorCode, null, cancellationToken);

        var major = new NganhDaoTao
        {
            MaCodeNganh = majorCode,
            TenNganh = majorName,
            MoTa = NormalizeOptionalText(request.MoTa),
            ConHoatDong = true
        };

        _context.NganhDaoTaos.Add(major);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(major);
    }

    public async Task<MajorDto> UpdateAsync(
        int majorId,
        UpdateMajorRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var major = await GetManagedMajorAsync(majorId, cancellationToken);
        var majorCode = NormalizeCode(request.MaCodeNganh, "Mã ngành");
        var majorName = NormalizeRequiredText(request.TenNganh, "Tên ngành");
        await ValidateMajorCodeAsync(majorCode, majorId, cancellationToken);

        major.MaCodeNganh = majorCode;
        major.TenNganh = majorName;
        major.MoTa = NormalizeOptionalText(request.MoTa);
        major.ConHoatDong = request.ConHoatDong;
        major.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(major);
    }

    public async Task DeleteAsync(int majorId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var major = await GetManagedMajorAsync(majorId, cancellationToken);
        major.ConHoatDong = false;
        major.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<MajorDto> ActivateAsync(int majorId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var major = await GetManagedMajorAsync(majorId, cancellationToken);
        major.ConHoatDong = true;
        major.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(major);
    }

    public async Task<MajorDto> DeactivateAsync(int majorId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var major = await GetManagedMajorAsync(majorId, cancellationToken);
        major.ConHoatDong = false;
        major.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(major);
    }

    private async Task<NganhDaoTao> GetManagedMajorAsync(int majorId, CancellationToken cancellationToken)
    {
        var major = await _context.NganhDaoTaos.FirstOrDefaultAsync(x => x.MaNganh == majorId, cancellationToken);
        if (major is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy ngành đào tạo.");
        }

        return major;
    }

    private async Task ValidateMajorCodeAsync(
        string majorCode,
        int? excludedMajorId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.NganhDaoTaos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeNganh == majorCode &&
                (!excludedMajorId.HasValue || x.MaNganh != excludedMajorId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã ngành đã tồn tại.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý ngành đào tạo.");
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

    private static MajorDto ToDto(NganhDaoTao major)
    {
        return new MajorDto
        {
            MaNganh = major.MaNganh,
            MaCodeNganh = major.MaCodeNganh,
            TenNganh = major.TenNganh,
            MoTa = major.MoTa,
            ConHoatDong = major.ConHoatDong,
            NgayTao = major.NgayTao,
            NgayCapNhat = major.NgayCapNhat
        };
    }
}

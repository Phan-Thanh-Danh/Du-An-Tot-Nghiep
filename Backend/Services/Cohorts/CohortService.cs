using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Cohorts;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Cohorts;

public class CohortService : ICohortService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CohortService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<CohortDto>> GetAsync(
        CohortQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.KhoaTuyenSinhs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.MaCodeKhoa.ToLower().Contains(keyword) ||
                x.TenKhoa.ToLower().Contains(keyword));
        }

        if (parameters.NamBatDau.HasValue)
        {
            query = query.Where(x => x.NamBatDau == parameters.NamBatDau.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.NamBatDau)
            .ThenBy(x => x.MaCodeKhoa)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CohortDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CohortDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cohort = await _context.KhoaTuyenSinhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaTuyenSinh == id, cancellationToken);

        if (cohort is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa tuyển sinh.");
        }

        return ToDto(cohort);
    }

    public async Task<CohortDto> CreateAsync(CreateCohortRequest request, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var cohortCode = NormalizeCode(request.MaCodeKhoa);
        var cohortName = NormalizeRequiredText(request.TenKhoa, "Tên khóa tuyển sinh");
        ValidateYearRange(request.NamBatDau, request.NamKetThucDuKien);
        await EnsureCodeUniqueAsync(cohortCode, null, cancellationToken);

        var cohort = new KhoaTuyenSinh
        {
            MaCodeKhoa = cohortCode,
            TenKhoa = cohortName,
            NamBatDau = request.NamBatDau,
            NamKetThucDuKien = request.NamKetThucDuKien,
            MoTa = NormalizeOptionalText(request.MoTa),
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        _context.KhoaTuyenSinhs.Add(cohort);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(cohort);
    }

    public async Task<CohortDto> UpdateAsync(
        int id,
        UpdateCohortRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var cohort = await GetManagedCohortAsync(id, cancellationToken);
        var cohortCode = NormalizeCode(request.MaCodeKhoa);
        var cohortName = NormalizeRequiredText(request.TenKhoa, "Tên khóa tuyển sinh");
        ValidateYearRange(request.NamBatDau, request.NamKetThucDuKien);
        await EnsureCodeUniqueAsync(cohortCode, id, cancellationToken);

        cohort.MaCodeKhoa = cohortCode;
        cohort.TenKhoa = cohortName;
        cohort.NamBatDau = request.NamBatDau;
        cohort.NamKetThucDuKien = request.NamKetThucDuKien;
        cohort.MoTa = NormalizeOptionalText(request.MoTa);
        cohort.ConHoatDong = request.ConHoatDong;
        cohort.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(cohort);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var cohort = await GetManagedCohortAsync(id, cancellationToken);
        cohort.ConHoatDong = false;
        cohort.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CohortDto> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var cohort = await GetManagedCohortAsync(id, cancellationToken);
        cohort.ConHoatDong = true;
        cohort.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(cohort);
    }

    public async Task<CohortDto> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var cohort = await GetManagedCohortAsync(id, cancellationToken);
        cohort.ConHoatDong = false;
        cohort.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(cohort);
    }

    private async Task<KhoaTuyenSinh> GetManagedCohortAsync(int id, CancellationToken cancellationToken)
    {
        var cohort = await _context.KhoaTuyenSinhs.FirstOrDefaultAsync(x => x.MaKhoaTuyenSinh == id, cancellationToken);
        if (cohort is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa tuyển sinh.");
        }

        return cohort;
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý khóa tuyển sinh.");
        }
    }

    private static string NormalizeCode(string value)
    {
        return NormalizeRequiredText(value, "Mã khóa tuyển sinh").ToUpperInvariant();
    }

    private static void ValidateYearRange(int startYear, int? expectedEndYear)
    {
        if (startYear < 2000)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Năm bắt đầu phải lớn hơn hoặc bằng 2000.");
        }

        if (expectedEndYear.HasValue && expectedEndYear.Value < startYear)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Năm kết thúc dự kiến phải lớn hơn hoặc bằng năm bắt đầu.");
        }
    }

    private async Task EnsureCodeUniqueAsync(
        string cohortCode,
        int? excludedCohortId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.KhoaTuyenSinhs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeKhoa == cohortCode &&
                (!excludedCohortId.HasValue || x.MaKhoaTuyenSinh != excludedCohortId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã khóa tuyển sinh đã tồn tại.");
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

    private static CohortDto ToDto(KhoaTuyenSinh cohort)
    {
        return new CohortDto
        {
            MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh,
            MaCodeKhoa = cohort.MaCodeKhoa,
            TenKhoa = cohort.TenKhoa,
            NamBatDau = cohort.NamBatDau,
            NamKetThucDuKien = cohort.NamKetThucDuKien,
            MoTa = cohort.MoTa,
            ConHoatDong = cohort.ConHoatDong,
            NgayTao = cohort.NgayTao,
            NgayCapNhat = cohort.NgayCapNhat
        };
    }
}

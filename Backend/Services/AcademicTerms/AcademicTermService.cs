using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicTerms;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AcademicTerms;

public class AcademicTermService : IAcademicTermService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AcademicTermService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<AcademicTermDto>> GetTermsAsync(
        AcademicTermQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();

        var termQuery = _context.HocKys
            .AsNoTracking()
            .Where(x => allowedOrganizationIdList.Contains(x.MaDonVi));

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            termQuery = termQuery.Where(x =>
                x.MaCodeHocKy.ToLower().Contains(keyword) ||
                x.TenHocKy.ToLower().Contains(keyword));
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem học kỳ của đơn vị này.");
            }

            termQuery = termQuery.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.DaKhoa.HasValue)
        {
            termQuery = termQuery.Where(x => x.DaKhoa == parameters.DaKhoa.Value);
        }

        var query =
            from term in termQuery
            join organization in _context.DonVis.AsNoTracking()
                on term.MaDonVi equals organization.MaDonVi
            select new { Term = term, Organization = organization };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenByDescending(x => x.Term.NgayBatDau)
            .ThenBy(x => x.Term.MaCodeHocKy)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new AcademicTermDto
            {
                MaHocKy = x.Term.MaHocKy,
                MaDonVi = x.Term.MaDonVi,
                TenDonVi = x.Organization.TenDonVi,
                MaCodeHocKy = x.Term.MaCodeHocKy,
                TenHocKy = x.Term.TenHocKy,
                NgayBatDau = x.Term.NgayBatDau,
                NgayKetThuc = x.Term.NgayKetThuc,
                DaKhoa = x.Term.DaKhoa,
                SoTinChiToiDa = x.Term.SoTinChiToiDa,
                HanRutMon = x.Term.HanRutMon
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AcademicTermDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AcademicTermDto> GetByIdAsync(int termId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        var result = await (
            from term in _context.HocKys.AsNoTracking()
            where term.MaHocKy == termId && allowedOrganizationIdList.Contains(term.MaDonVi)
            join organization in _context.DonVis.AsNoTracking()
                on term.MaDonVi equals organization.MaDonVi
            select new AcademicTermDto
            {
                MaHocKy = term.MaHocKy,
                MaDonVi = term.MaDonVi,
                TenDonVi = organization.TenDonVi,
                MaCodeHocKy = term.MaCodeHocKy,
                TenHocKy = term.TenHocKy,
                NgayBatDau = term.NgayBatDau,
                NgayKetThuc = term.NgayKetThuc,
                DaKhoa = term.DaKhoa,
                SoTinChiToiDa = term.SoTinChiToiDa,
                HanRutMon = term.HanRutMon
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy học kỳ.");
        }

        return result;
    }

    public async Task<AcademicTermDto> CreateAsync(
        CreateAcademicTermRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var termCode = NormalizeRequiredText(request.MaCodeHocKy, "Mã học kỳ");
        var termName = NormalizeRequiredText(request.TenHocKy, "Tên học kỳ");

        ValidateTermData(request.NgayBatDau, request.NgayKetThuc, request.HanRutMon, request.SoTinChiToiDa);

        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        await ValidateTermCodeAsync(termCode, organization.MaDonVi, null, cancellationToken);

        var term = new HocKy
        {
            MaDonVi = organization.MaDonVi,
            MaCodeHocKy = termCode,
            TenHocKy = termName,
            NgayBatDau = request.NgayBatDau,
            NgayKetThuc = request.NgayKetThuc,
            DaKhoa = false,
            SoTinChiToiDa = request.SoTinChiToiDa,
            HanRutMon = request.HanRutMon
        };

        _context.HocKys.Add(term);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(term, organization);
    }

    public async Task<AcademicTermDto> UpdateAsync(
        int termId,
        UpdateAcademicTermRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var term = await GetManagedTermAsync(termId, currentUser, cancellationToken);
        if (term.DaKhoa)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ đã khóa, không thể cập nhật.");
        }

        var termCode = NormalizeRequiredText(request.MaCodeHocKy, "Mã học kỳ");
        var termName = NormalizeRequiredText(request.TenHocKy, "Tên học kỳ");

        ValidateTermData(request.NgayBatDau, request.NgayKetThuc, request.HanRutMon, request.SoTinChiToiDa);

        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        await ValidateTermCodeAsync(termCode, organization.MaDonVi, termId, cancellationToken);

        term.MaDonVi = organization.MaDonVi;
        term.MaCodeHocKy = termCode;
        term.TenHocKy = termName;
        term.NgayBatDau = request.NgayBatDau;
        term.NgayKetThuc = request.NgayKetThuc;
        term.DaKhoa = request.DaKhoa;
        term.SoTinChiToiDa = request.SoTinChiToiDa;
        term.HanRutMon = request.HanRutMon;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(term, organization);
    }

    public async Task DeleteAsync(int termId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var term = await GetManagedTermAsync(termId, currentUser, cancellationToken);

        if (!term.DaKhoa)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Vui lòng khóa học kỳ trước khi xóa.");
        }

        var blockers = await GetDeleteBlockersAsync(termId, cancellationToken);
        if (blockers.Count > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không thể xóa học kỳ vì còn dữ liệu liên quan: {string.Join(", ", blockers)}.");
        }

        _context.HocKys.Remove(term);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AcademicTermDto> LockAsync(int termId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var term = await GetManagedTermAsync(termId, currentUser, cancellationToken);

        term.DaKhoa = true;
        await _context.SaveChangesAsync(cancellationToken);

        var organization = await GetOrganizationForDtoAsync(term.MaDonVi, cancellationToken);
        return ToDto(term, organization);
    }

    public async Task<AcademicTermDto> UnlockAsync(int termId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role is not AuthRoles.SuperAdmin and not AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền mở khóa học kỳ.");
        }

        var term = await GetManagedTermAsync(termId, currentUser, cancellationToken);

        term.DaKhoa = false;
        await _context.SaveChangesAsync(cancellationToken);

        var organization = await GetOrganizationForDtoAsync(term.MaDonVi, cancellationToken);
        return ToDto(term, organization);
    }

    private async Task<HocKy> GetManagedTermAsync(
        int termId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var term = await _context.HocKys.FirstOrDefaultAsync(x => x.MaHocKy == termId, cancellationToken);
        if (term is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy học kỳ.");
        }

        if (!await CanManageOrganizationAsync(currentUser, term.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý học kỳ của đơn vị này.");
        }

        return term;
    }

    private async Task ValidateTermCodeAsync(
        string termCode,
        int organizationId,
        int? excludedTermId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.HocKys
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaDonVi == organizationId &&
                x.MaCodeHocKy == termCode &&
                (!excludedTermId.HasValue || x.MaHocKy != excludedTermId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã học kỳ đã tồn tại trong đơn vị này.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý học kỳ của đơn vị này.");
        }

        return organization;
    }

    private async Task<DonVi> GetOrganizationForDtoAsync(int organizationId, CancellationToken cancellationToken)
    {
        return await _context.DonVis
            .AsNoTracking()
            .FirstAsync(x => x.MaDonVi == organizationId, cancellationToken);
    }

    private async Task<List<string>> GetDeleteBlockersAsync(int termId, CancellationToken cancellationToken)
    {
        var blockers = new List<string>();

        await AddBlockerIfExistsAsync(blockers, nameof(LopHocPhan), () => _context.LopHocPhans.AnyAsync(x => x.MaHocKy == termId, cancellationToken));
        await AddBlockerIfExistsAsync(blockers, nameof(GiaiDoanDangKy), () => _context.GiaiDoanDangKys.AnyAsync(x => x.MaHocKy == termId, cancellationToken));
        await AddBlockerIfExistsAsync(blockers, nameof(DiemSo), () => _context.DiemSos.AnyAsync(x => x.MaHocKy == termId, cancellationToken));
        await AddBlockerIfExistsAsync(blockers, nameof(ThoiKhoaBieu), () => _context.ThoiKhoaBieus.AnyAsync(x => x.MaLopHocPhan.HasValue && x.LopHocPhan != null && x.LopHocPhan.MaHocKy == termId, cancellationToken));
        await AddBlockerIfExistsAsync(blockers, nameof(HoaDon), () => _context.HoaDons.AnyAsync(x => x.MaHocKy == termId, cancellationToken));

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

    private static void ValidateTermData(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? withdrawalDeadline,
        int? maxCredits)
    {
        if (startDate == default || endDate == default)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu và ngày kết thúc không hợp lệ.");
        }

        if (startDate >= endDate)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
        }

        if (withdrawalDeadline.HasValue &&
            (withdrawalDeadline.Value < startDate || withdrawalDeadline.Value > endDate))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hạn rút môn phải nằm trong thời gian học kỳ.");
        }

        if (maxCredits.HasValue && maxCredits.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ tối đa phải lớn hơn 0.");
        }
    }

    private static AcademicTermDto ToDto(HocKy term, DonVi organization)
    {
        return new AcademicTermDto
        {
            MaHocKy = term.MaHocKy,
            MaDonVi = term.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaCodeHocKy = term.MaCodeHocKy,
            TenHocKy = term.TenHocKy,
            NgayBatDau = term.NgayBatDau,
            NgayKetThuc = term.NgayKetThuc,
            DaKhoa = term.DaKhoa,
            SoTinChiToiDa = term.SoTinChiToiDa,
            HanRutMon = term.HanRutMon
        };
    }
}

using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.CampusSpecializations;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.CampusSpecializations;

public class ChuyenNganhTheoCoSoService : IChuyenNganhTheoCoSoService
{
    private const string DraftStatus = "draft";
    private const string PendingApprovalStatus = "pending_approval";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string InactiveStatus = "inactive";
    private const string RejectedStatus = "rejected";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        DraftStatus,
        PendingApprovalStatus,
        ApprovedStatus,
        ActiveStatus,
        InactiveStatus,
        RejectedStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly ILogger<ChuyenNganhTheoCoSoService> _logger;

    public ChuyenNganhTheoCoSoService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        ILogger<ChuyenNganhTheoCoSoService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PagedResultDto<CampusSpecializationDto>> GetCampusSpecializationsAsync(
        CampusSpecializationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();

        var query = CreateCampusSpecializationQuery()
            .Where(x => allowedOrganizationIdList.Contains(x.CampusSpecialization.MaDonVi));

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Specialization.MaCodeChuyenNganh.ToLower().Contains(keyword) ||
                x.Specialization.TenChuyenNganh.ToLower().Contains(keyword) ||
                x.Major.TenNganh.ToLower().Contains(keyword) ||
                x.Organization.TenDonVi.ToLower().Contains(keyword));
        }

        if (parameters.MaNganh.HasValue)
        {
            query = query.Where(x => x.Specialization.MaNganh == parameters.MaNganh.Value);
        }

        if (parameters.MaChuyenNganh.HasValue)
        {
            query = query.Where(x => x.CampusSpecialization.MaChuyenNganh == parameters.MaChuyenNganh.Value);
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem chuyên ngành tại cơ sở này.");
            }

            query = query.Where(x => x.CampusSpecialization.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.CampusSpecialization.TrangThai == status);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.CampusSpecialization.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Major.TenNganh)
            .ThenBy(x => x.Specialization.TenChuyenNganh)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x.CampusSpecialization, x.Specialization, x.Major, x.Organization))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CampusSpecializationDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CampusSpecializationDto> GetByIdAsync(
        int campusSpecializationId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await CreateCampusSpecializationQuery()
            .FirstOrDefaultAsync(x => x.CampusSpecialization.MaChuyenNganhCoSo == campusSpecializationId, cancellationToken);

        if (result is null || !allowedOrganizationIds.Contains(result.CampusSpecialization.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chuyên ngành theo cơ sở.");
        }

        return ToDto(result.CampusSpecialization, result.Specialization, result.Major, result.Organization);
    }

    public async Task<CampusSpecializationDto> CreateAsync(
        CreateCampusSpecializationRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var status = NormalizeStatus(request.TrangThai);

        if (currentUser.Role == AuthRoles.CampusAdmin && status != PendingApprovalStatus)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "CampusAdmin chỉ được tạo đề xuất mở chuyên ngành với trạng thái chờ duyệt.");
        }

        if (currentUser.Role is not AuthRoles.SuperAdmin and not AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền tạo chuyên ngành theo cơ sở.");
        }

        var specialization = await ValidateSpecializationAsync(request.MaChuyenNganh, cancellationToken);
        var major = await GetMajorForDtoAsync(specialization.MaNganh, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        await ValidateUniqueAssignmentAsync(request.MaChuyenNganh, request.MaDonVi, null, cancellationToken);
        ValidateCampusSpecializationData(request.NamBatDau, request.ChiTieuDuKien);

        var campusSpecialization = new ChuyenNganhTheoCoSo
        {
            MaChuyenNganh = specialization.MaChuyenNganh,
            MaDonVi = organization.MaDonVi,
            TrangThai = status,
            NamBatDau = request.NamBatDau,
            ChiTieuDuKien = request.ChiTieuDuKien,
            GhiChu = NormalizeOptionalText(request.GhiChu),
            ConHoatDong = status is not InactiveStatus and not RejectedStatus
        };

        _context.ChuyenNganhTheoCoSos.Add(campusSpecialization);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(campusSpecialization, specialization, major, organization);
    }

    public async Task<CampusSpecializationDto> UpdateAsync(
        int campusSpecializationId,
        UpdateCampusSpecializationRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        var status = NormalizeStatus(request.TrangThai);
        var specialization = await ValidateSpecializationAsync(request.MaChuyenNganh, cancellationToken);
        var currentUser = GetCurrentUser();
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        await ValidateUniqueAssignmentAsync(request.MaChuyenNganh, request.MaDonVi, campusSpecializationId, cancellationToken);
        ValidateCampusSpecializationData(request.NamBatDau, request.ChiTieuDuKien);
        var major = await GetMajorForDtoAsync(specialization.MaNganh, cancellationToken);

        campusSpecialization.MaChuyenNganh = specialization.MaChuyenNganh;
        campusSpecialization.MaDonVi = organization.MaDonVi;
        campusSpecialization.TrangThai = status;
        campusSpecialization.NamBatDau = request.NamBatDau;
        campusSpecialization.ChiTieuDuKien = request.ChiTieuDuKien;
        campusSpecialization.GhiChu = NormalizeOptionalText(request.GhiChu);
        campusSpecialization.ConHoatDong = request.ConHoatDong;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(campusSpecialization, specialization, major, organization);
    }

    public async Task DeleteAsync(int campusSpecializationId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        campusSpecialization.ConHoatDong = false;
        campusSpecialization.TrangThai = InactiveStatus;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CampusSpecializationDto> ActivateAsync(
        int campusSpecializationId,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        campusSpecialization.TrangThai = ActiveStatus;
        campusSpecialization.ConHoatDong = true;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(campusSpecializationId, cancellationToken);
    }

    public async Task<CampusSpecializationDto> DeactivateAsync(
        int campusSpecializationId,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        campusSpecialization.TrangThai = InactiveStatus;
        campusSpecialization.ConHoatDong = false;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(campusSpecializationId, cancellationToken);
    }

    public async Task<CampusSpecializationDto> ApproveAsync(
        int campusSpecializationId,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        var currentUser = GetCurrentUser();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        campusSpecialization.TrangThai = ApprovedStatus;
        campusSpecialization.ConHoatDong = true;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await SendDecisionEmailAsync(campusSpecializationId, currentUser, isApproved: true, campusSpecialization.GhiChu, cancellationToken);
        return await GetByIdAsync(campusSpecializationId, cancellationToken);
    }

    public async Task<CampusSpecializationDto> RejectAsync(
        int campusSpecializationId,
        RejectCampusSpecializationRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        var currentUser = GetCurrentUser();

        var campusSpecialization = await GetManagedCampusSpecializationAsync(campusSpecializationId, cancellationToken);
        campusSpecialization.TrangThai = RejectedStatus;
        campusSpecialization.ConHoatDong = false;
        campusSpecialization.GhiChu = NormalizeOptionalText(request.GhiChu) ?? campusSpecialization.GhiChu;
        campusSpecialization.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await SendDecisionEmailAsync(campusSpecializationId, currentUser, isApproved: false, campusSpecialization.GhiChu, cancellationToken);
        return await GetByIdAsync(campusSpecializationId, cancellationToken);
    }

    private IQueryable<CampusSpecializationQueryResult> CreateCampusSpecializationQuery()
    {
        return
            from campusSpecialization in _context.ChuyenNganhTheoCoSos.AsNoTracking()
            join specialization in _context.ChuyenNganhs.AsNoTracking()
                on campusSpecialization.MaChuyenNganh equals specialization.MaChuyenNganh
            join major in _context.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            join organization in _context.DonVis.AsNoTracking()
                on campusSpecialization.MaDonVi equals organization.MaDonVi
            select new CampusSpecializationQueryResult
            {
                CampusSpecialization = campusSpecialization,
                Specialization = specialization,
                Major = major,
                Organization = organization
            };
    }

    private async Task<ChuyenNganhTheoCoSo> GetManagedCampusSpecializationAsync(
        int campusSpecializationId,
        CancellationToken cancellationToken)
    {
        var campusSpecialization = await _context.ChuyenNganhTheoCoSos
            .FirstOrDefaultAsync(x => x.MaChuyenNganhCoSo == campusSpecializationId, cancellationToken);
        if (campusSpecialization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chuyên ngành theo cơ sở.");
        }

        return campusSpecialization;
    }

    private async Task<ChuyenNganh> ValidateSpecializationAsync(
        int specializationId,
        CancellationToken cancellationToken)
    {
        var specialization = await _context.ChuyenNganhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaChuyenNganh == specializationId, cancellationToken);

        if (specialization is null || !specialization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chuyên ngành không tồn tại hoặc không hoạt động.");
        }

        return specialization;
    }

    private async Task<DonVi> ValidateOrganizationAsync(
        int organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);

        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc không hoạt động.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, organizationId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền mở chuyên ngành ở cơ sở này.");
        }

        return organization;
    }

    private async Task<NganhDaoTao> GetMajorForDtoAsync(int majorId, CancellationToken cancellationToken)
    {
        return await _context.NganhDaoTaos
            .AsNoTracking()
            .FirstAsync(x => x.MaNganh == majorId, cancellationToken);
    }

    private async Task ValidateUniqueAssignmentAsync(
        int specializationId,
        int organizationId,
        int? excludedCampusSpecializationId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ChuyenNganhTheoCoSos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaChuyenNganh == specializationId &&
                x.MaDonVi == organizationId &&
                (!excludedCampusSpecializationId.HasValue || x.MaChuyenNganhCoSo != excludedCampusSpecializationId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chuyên ngành này đã được mở tại cơ sở đã chọn.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý chuyên ngành theo cơ sở.");
        }
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

    private async Task<bool> CanAccessOrganizationAsync(
        CurrentUserContext currentUser,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        return allowedOrganizationIds.Contains(organizationId);
    }

    private async Task SendDecisionEmailAsync(
        int campusSpecializationId,
        CurrentUserContext currentUser,
        bool isApproved,
        string? note,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await CreateCampusSpecializationQuery()
                .FirstOrDefaultAsync(x => x.CampusSpecialization.MaChuyenNganhCoSo == campusSpecializationId, cancellationToken);
            if (result is null)
            {
                return;
            }

            var handlerName = await _context.NguoiDungs
                .AsNoTracking()
                .Where(x => x.MaNguoiDung == currentUser.UserId)
                .Select(x => x.HoTen)
                .FirstOrDefaultAsync(cancellationToken);

            var recipients = await _context.NguoiDungs
                .AsNoTracking()
                .Where(x =>
                    x.MaDonVi == result.CampusSpecialization.MaDonVi &&
                    x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin) &&
                    x.TrangThai != UserStatuses.DbLocked)
                .Select(x => new { x.Email, x.HoTen })
                .ToListAsync(cancellationToken);

            if (recipients.Count == 0)
            {
                _logger.LogInformation(
                    "No CampusAdmin recipient found for campus specialization proposal {ProposalId}.",
                    campusSpecializationId);
                return;
            }

            foreach (var recipient in recipients)
            {
                var data = new CampusSpecializationDecisionEmailData
                {
                    TenNguoiNhan = recipient.HoTen,
                    MaDeXuat = result.CampusSpecialization.MaChuyenNganhCoSo,
                    TenNganh = result.Major.TenNganh,
                    TenChuyenNganh = result.Specialization.TenChuyenNganh,
                    TenCoSo = result.Organization.TenDonVi,
                    TrangThaiMoi = result.CampusSpecialization.TrangThai,
                    NamBatDau = result.CampusSpecialization.NamBatDau,
                    ChiTieuDuKien = result.CampusSpecialization.ChiTieuDuKien,
                    TenNguoiXuLy = string.IsNullOrWhiteSpace(handlerName) ? currentUser.Email : handlerName,
                    ThoiGianXuLy = FormatVietnamTime(DateTime.UtcNow),
                    GhiChu = note
                };

                if (isApproved)
                {
                    await _emailService.SendCampusSpecializationApprovedAsync(recipient.Email, data, cancellationToken);
                }
                else
                {
                    await _emailService.SendCampusSpecializationRejectedAsync(recipient.Email, data, cancellationToken);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Could not send campus specialization decision email for proposal {ProposalId}.",
                campusSpecializationId);
        }
    }

    private static string NormalizeStatus(string value)
    {
        var status = NormalizeRequiredText(value, "Trạng thái").ToLowerInvariant();
        if (!ValidStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái chuyên ngành theo cơ sở không hợp lệ.");
        }

        return status;
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

    private static void ValidateCampusSpecializationData(int? startYear, int? expectedQuota)
    {
        if (startYear.HasValue && startYear.Value < 2000)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Năm bắt đầu phải từ 2000 trở lên.");
        }

        if (expectedQuota.HasValue && expectedQuota.Value < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ tiêu dự kiến phải lớn hơn hoặc bằng 0.");
        }
    }

    private static string FormatVietnamTime(DateTime utcNow)
    {
        return utcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private static CampusSpecializationDto ToDto(
        ChuyenNganhTheoCoSo campusSpecialization,
        ChuyenNganh specialization,
        NganhDaoTao major,
        DonVi organization)
    {
        return new CampusSpecializationDto
        {
            MaChuyenNganhCoSo = campusSpecialization.MaChuyenNganhCoSo,
            MaChuyenNganh = campusSpecialization.MaChuyenNganh,
            MaCodeChuyenNganh = specialization.MaCodeChuyenNganh,
            TenChuyenNganh = specialization.TenChuyenNganh,
            MaNganh = specialization.MaNganh,
            TenNganh = major.TenNganh,
            MaDonVi = campusSpecialization.MaDonVi,
            TenDonVi = organization.TenDonVi,
            TrangThai = campusSpecialization.TrangThai,
            NamBatDau = campusSpecialization.NamBatDau,
            ChiTieuDuKien = campusSpecialization.ChiTieuDuKien,
            GhiChu = campusSpecialization.GhiChu,
            ConHoatDong = campusSpecialization.ConHoatDong
        };
    }

    private sealed class CampusSpecializationQueryResult
    {
        public ChuyenNganhTheoCoSo CampusSpecialization { get; init; } = null!;
        public ChuyenNganh Specialization { get; init; } = null!;
        public NganhDaoTao Major { get; init; } = null!;
        public DonVi Organization { get; init; } = null!;
    }
}

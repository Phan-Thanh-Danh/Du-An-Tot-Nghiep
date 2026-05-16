using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.CourseSyllabuses;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.CourseSyllabuses;

public class CourseSyllabusService : ICourseSyllabusService
{
    private const string DraftStatus = "draft";
    private const string PendingApprovalStatus = "pending_approval";
    private const string ApprovedStatus = "approved";
    private const string ActiveStatus = "active";
    private const string InactiveStatus = "inactive";
    private const string ArchivedStatus = "archived";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        DraftStatus,
        PendingApprovalStatus,
        ApprovedStatus,
        ActiveStatus,
        InactiveStatus,
        ArchivedStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CourseSyllabusService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<CourseSyllabusDto>> GetAsync(
        CourseSyllabusQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();

        var query = CreateSyllabusQuery()
            .Where(x => x.Syllabus.MaDonVi == null || allowedOrganizationIdList.Contains(x.Syllabus.MaDonVi.Value));

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Syllabus.TenSyllabus.ToLower().Contains(keyword) ||
                x.Subject.MaCodeMonHoc.ToLower().Contains(keyword) ||
                x.Subject.TenMonHoc.ToLower().Contains(keyword) ||
                x.Specialization.MaCodeChuyenNganh.ToLower().Contains(keyword) ||
                x.Specialization.TenChuyenNganh.ToLower().Contains(keyword) ||
                x.Major.TenNganh.ToLower().Contains(keyword) ||
                (x.Organization != null && x.Organization.TenDonVi.ToLower().Contains(keyword)));
        }

        if (parameters.MaMonHoc.HasValue)
        {
            query = query.Where(x => x.Syllabus.MaMonHoc == parameters.MaMonHoc.Value);
        }

        if (parameters.MaChuyenNganh.HasValue)
        {
            query = query.Where(x => x.Syllabus.MaChuyenNganh == parameters.MaChuyenNganh.Value);
        }

        if (parameters.MaNganh.HasValue)
        {
            query = query.Where(x => x.Specialization.MaNganh == parameters.MaNganh.Value);
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đề cương của đơn vị này.");
            }

            query = query.Where(x => x.Syllabus.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.Syllabus.TrangThai == status);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.Syllabus.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Subject.MaCodeMonHoc)
            .ThenBy(x => x.Specialization.TenChuyenNganh)
            .ThenBy(x => x.Organization == null ? string.Empty : x.Organization.TenDonVi)
            .ThenBy(x => x.Syllabus.Version)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x.Syllabus, x.Subject, x.Specialization, x.Major, x.Organization))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CourseSyllabusDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CourseSyllabusDto> GetByIdAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await CreateSyllabusQuery()
            .FirstOrDefaultAsync(x => x.Syllabus.MaSyllabus == syllabusId, cancellationToken);

        if (result is null ||
            (result.Syllabus.MaDonVi.HasValue && !allowedOrganizationIds.Contains(result.Syllabus.MaDonVi.Value)))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đề cương môn học.");
        }

        return ToDto(result.Syllabus, result.Subject, result.Specialization, result.Major, result.Organization);
    }

    public async Task<CourseSyllabusDto> CreateAsync(
        CreateCourseSyllabusRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var status = NormalizeStatus(string.IsNullOrWhiteSpace(request.TrangThai) ? DraftStatus : request.TrangThai);

        if (request.MaDonVi is null && currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được tạo đề cương chuẩn toàn hệ thống.");
        }

        if (request.MaDonVi.HasValue && currentUser.Role is not AuthRoles.SuperAdmin and not AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền tạo đề cương theo cơ sở.");
        }

        var subject = await ValidateSubjectAsync(request.MaMonHoc, cancellationToken);
        var specialization = await ValidateSpecializationAsync(request.MaChuyenNganh, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        await ValidateSpecializationOpenedAtOrganizationAsync(request.MaChuyenNganh, request.MaDonVi, cancellationToken);

        var syllabusName = NormalizeRequiredText(request.TenSyllabus, "Tên đề cương");
        var version = NormalizeRequiredText(request.Version, "Phiên bản");
        ValidateExpectedTerm(request.HocKyDuKien);
        await ValidateUniqueSyllabusAsync(
            request.MaMonHoc,
            request.MaChuyenNganh,
            request.MaDonVi,
            version,
            null,
            cancellationToken);

        var syllabus = new CourseSyllabus
        {
            MaMonHoc = subject.MaMonHoc,
            MaChuyenNganh = specialization.MaChuyenNganh,
            MaDonVi = organization?.MaDonVi,
            TenSyllabus = syllabusName,
            Version = version,
            HocKyDuKien = request.HocKyDuKien,
            BatBuoc = request.BatBuoc,
            TrangThai = status,
            ConHoatDong = status is not InactiveStatus and not ArchivedStatus
        };

        _context.CourseSyllabuses.Add(syllabus);
        await _context.SaveChangesAsync(cancellationToken);

        var major = await GetMajorAsync(specialization.MaNganh, cancellationToken);
        return ToDto(syllabus, subject, specialization, major, organization);
    }

    public async Task<CourseSyllabusDto> UpdateAsync(
        int syllabusId,
        UpdateCourseSyllabusRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var syllabus = await GetManagedSyllabusAsync(syllabusId, cancellationToken);
        var subject = await ValidateSubjectAsync(request.MaMonHoc, cancellationToken);
        var specialization = await ValidateSpecializationAsync(request.MaChuyenNganh, cancellationToken);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, GetCurrentUser(), cancellationToken);
        await ValidateSpecializationOpenedAtOrganizationAsync(request.MaChuyenNganh, request.MaDonVi, cancellationToken);

        var syllabusName = NormalizeRequiredText(request.TenSyllabus, "Tên đề cương");
        var version = NormalizeRequiredText(request.Version, "Phiên bản");
        var status = NormalizeStatus(request.TrangThai);
        ValidateExpectedTerm(request.HocKyDuKien);
        await ValidateUniqueSyllabusAsync(
            request.MaMonHoc,
            request.MaChuyenNganh,
            request.MaDonVi,
            version,
            syllabusId,
            cancellationToken);

        syllabus.MaMonHoc = subject.MaMonHoc;
        syllabus.MaChuyenNganh = specialization.MaChuyenNganh;
        syllabus.MaDonVi = organization?.MaDonVi;
        syllabus.TenSyllabus = syllabusName;
        syllabus.Version = version;
        syllabus.HocKyDuKien = request.HocKyDuKien;
        syllabus.BatBuoc = request.BatBuoc;
        syllabus.TrangThai = status;
        syllabus.ConHoatDong = request.ConHoatDong;
        syllabus.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var major = await GetMajorAsync(specialization.MaNganh, cancellationToken);
        return ToDto(syllabus, subject, specialization, major, organization);
    }

    public async Task DeleteAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var syllabus = await GetManagedSyllabusAsync(syllabusId, cancellationToken);
        syllabus.ConHoatDong = false;
        syllabus.TrangThai = ArchivedStatus;
        syllabus.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseSyllabusDto> ActivateAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        return await ChangeStatusAsync(syllabusId, ActiveStatus, true, cancellationToken);
    }

    public async Task<CourseSyllabusDto> DeactivateAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        return await ChangeStatusAsync(syllabusId, InactiveStatus, false, cancellationToken);
    }

    public async Task<CourseSyllabusDto> ApproveAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        return await ChangeStatusAsync(syllabusId, ApprovedStatus, true, cancellationToken);
    }

    public async Task<CourseSyllabusDto> ArchiveAsync(int syllabusId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        return await ChangeStatusAsync(syllabusId, ArchivedStatus, false, cancellationToken);
    }

    private async Task<CourseSyllabusDto> ChangeStatusAsync(
        int syllabusId,
        string status,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var syllabus = await GetManagedSyllabusAsync(syllabusId, cancellationToken);
        syllabus.TrangThai = status;
        syllabus.ConHoatDong = isActive;
        syllabus.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(syllabusId, cancellationToken);
    }

    private IQueryable<CourseSyllabusQueryResult> CreateSyllabusQuery()
    {
        return
            from syllabus in _context.CourseSyllabuses.AsNoTracking()
            join subject in _context.DanhMucMonHocs.AsNoTracking()
                on syllabus.MaMonHoc equals subject.MaMonHoc
            join specialization in _context.ChuyenNganhs.AsNoTracking()
                on syllabus.MaChuyenNganh equals specialization.MaChuyenNganh
            join major in _context.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            join organization in _context.DonVis.AsNoTracking()
                on syllabus.MaDonVi equals organization.MaDonVi into organizationJoin
            from organization in organizationJoin.DefaultIfEmpty()
            select new CourseSyllabusQueryResult
            {
                Syllabus = syllabus,
                Subject = subject,
                Specialization = specialization,
                Major = major,
                Organization = organization
            };
    }

    private async Task<CourseSyllabus> GetManagedSyllabusAsync(int syllabusId, CancellationToken cancellationToken)
    {
        var syllabus = await _context.CourseSyllabuses.FirstOrDefaultAsync(x => x.MaSyllabus == syllabusId, cancellationToken);
        if (syllabus is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đề cương môn học.");
        }

        return syllabus;
    }

    private async Task<DanhMucMonHoc> ValidateSubjectAsync(int subjectId, CancellationToken cancellationToken)
    {
        var subject = await _context.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMonHoc == subjectId, cancellationToken);

        if (subject is null || !subject.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Môn học không tồn tại hoặc không hoạt động.");
        }

        return subject;
    }

    private async Task<ChuyenNganh> ValidateSpecializationAsync(int specializationId, CancellationToken cancellationToken)
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

    private async Task<DonVi?> ValidateOrganizationAsync(
        int? organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (!organizationId.HasValue)
        {
            return null;
        }

        var organization = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == organizationId.Value, cancellationToken);
        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc không hoạt động.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, organizationId.Value, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý đề cương của đơn vị này.");
        }

        return organization;
    }

    private async Task<NganhDaoTao> GetMajorAsync(int majorId, CancellationToken cancellationToken)
    {
        return await _context.NganhDaoTaos
            .AsNoTracking()
            .FirstAsync(x => x.MaNganh == majorId, cancellationToken);
    }

    private async Task ValidateSpecializationOpenedAtOrganizationAsync(
        int specializationId,
        int? organizationId,
        CancellationToken cancellationToken)
    {
        if (!organizationId.HasValue)
        {
            return;
        }

        var isOpened = await _context.ChuyenNganhTheoCoSos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaChuyenNganh == specializationId &&
                x.MaDonVi == organizationId.Value &&
                x.ConHoatDong &&
                (x.TrangThai == "approved" || x.TrangThai == "active"),
                cancellationToken);

        if (!isOpened)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chuyên ngành chưa được duyệt hoặc kích hoạt tại cơ sở này.");
        }
    }

    private async Task ValidateUniqueSyllabusAsync(
        int subjectId,
        int specializationId,
        int? organizationId,
        string version,
        int? excludedSyllabusId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.CourseSyllabuses
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaMonHoc == subjectId &&
                x.MaChuyenNganh == specializationId &&
                x.MaDonVi == organizationId &&
                x.Version == version &&
                (!excludedSyllabusId.HasValue || x.MaSyllabus != excludedSyllabusId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đề cương môn học đã tồn tại cho môn học, chuyên ngành, cơ sở và phiên bản này.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý đề cương môn học.");
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

    private static string NormalizeStatus(string value)
    {
        var status = NormalizeRequiredText(value, "Trạng thái").ToLowerInvariant();
        if (!ValidStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái đề cương môn học không hợp lệ.");
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

    private static void ValidateExpectedTerm(int? expectedTerm)
    {
        if (expectedTerm.HasValue && expectedTerm.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ dự kiến phải lớn hơn 0.");
        }
    }

    private static CourseSyllabusDto ToDto(
        CourseSyllabus syllabus,
        DanhMucMonHoc subject,
        ChuyenNganh specialization,
        NganhDaoTao major,
        DonVi? organization)
    {
        return new CourseSyllabusDto
        {
            MaSyllabus = syllabus.MaSyllabus,
            MaMonHoc = syllabus.MaMonHoc,
            MaCodeMonHoc = subject.MaCodeMonHoc,
            TenMonHoc = subject.TenMonHoc,
            SoTinChi = subject.SoTinChi,
            MaChuyenNganh = syllabus.MaChuyenNganh,
            MaCodeChuyenNganh = specialization.MaCodeChuyenNganh,
            TenChuyenNganh = specialization.TenChuyenNganh,
            MaNganh = specialization.MaNganh,
            TenNganh = major.TenNganh,
            MaDonVi = syllabus.MaDonVi,
            TenDonVi = organization?.TenDonVi,
            TenSyllabus = syllabus.TenSyllabus,
            Version = syllabus.Version,
            HocKyDuKien = syllabus.HocKyDuKien,
            BatBuoc = syllabus.BatBuoc,
            TrangThai = syllabus.TrangThai,
            ConHoatDong = syllabus.ConHoatDong,
            NgayTao = syllabus.NgayTao,
            NgayCapNhat = syllabus.NgayCapNhat
        };
    }

    private sealed class CourseSyllabusQueryResult
    {
        public CourseSyllabus Syllabus { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public ChuyenNganh Specialization { get; init; } = null!;
        public NganhDaoTao Major { get; init; } = null!;
        public DonVi? Organization { get; init; }
    }
}

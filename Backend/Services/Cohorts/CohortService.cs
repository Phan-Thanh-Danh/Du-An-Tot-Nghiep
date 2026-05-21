using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Cohorts;
using Backend.DTOs.Common;
using Backend.DTOs.TrainingPrograms;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Cohorts;

public class CohortService : ICohortService
{
    private const string DraftStatus = "draft";
    private const string PendingApprovalStatus = "pending_approval";
    private const string ApprovedStatus = "approved";
    private const string RejectedStatus = "rejected";
    private const string ActiveStatus = "active";
    private const string InactiveStatus = "inactive";
    private const string ArchivedStatus = "archived";

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

    public async Task<TrainingProgramSetupDto> GetTrainingProgramSetupAsync(
        int cohortId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var cohort = await _context.KhoaTuyenSinhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaTuyenSinh == cohortId, cancellationToken);

        if (cohort is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa tuyển sinh.");
        }

        HashSet<int> allowedOrganizationIds = IsGlobalReader(currentUser)
            ? []
            : await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var specializations = await ApplySpecializationScope(CreateActiveSpecializationQuery(), currentUser, allowedOrganizationIds)
            .OrderBy(x => x.Major.TenNganh)
            .ThenBy(x => x.Specialization.TenChuyenNganh)
            .ToListAsync(cancellationToken);

        var specializationIds = specializations
            .Select(x => x.Specialization.MaChuyenNganh)
            .ToList();

        List<ChuongTrinhDaoTao> programs = specializationIds.Count == 0
            ? []
            : await _context.ChuongTrinhDaoTaos
                .AsNoTracking()
                .Include(x => x.KhoaTuyenSinh)
                .Where(x =>
                    specializationIds.Contains(x.MaChuyenNganh) &&
                    x.ConHoatDong &&
                    (x.MaKhoaTuyenSinh == cohortId ||
                        (x.MaKhoaTuyenSinh != cohortId &&
                            (x.TrangThai == ActiveStatus || x.TrangThai == ApprovedStatus))))
                .ToListAsync(cancellationToken);

        var currentProgramsBySpecialization = programs
            .Where(x => x.MaKhoaTuyenSinh == cohortId)
            .GroupBy(x => x.MaChuyenNganh)
            .ToDictionary(x => x.Key, x => x.ToList());

        var sourceProgramsBySpecialization = programs
            .Where(x =>
                x.MaKhoaTuyenSinh != cohortId &&
                IsSuggestedSourceStatus(x.TrangThai))
            .GroupBy(x => x.MaChuyenNganh)
            .ToDictionary(x => x.Key, x => x.ToList());

        var items = new List<TrainingProgramSetupItemDto>();
        foreach (var specialization in specializations)
        {
            var specializationId = specialization.Specialization.MaChuyenNganh;
            currentProgramsBySpecialization.TryGetValue(specializationId, out var currentPrograms);
            sourceProgramsBySpecialization.TryGetValue(specializationId, out var sourcePrograms);

            var currentProgram = PickCurrentProgram(currentPrograms ?? []);
            var suggestedSourceProgram = currentProgram is null
                ? PickSuggestedSourceProgram(sourcePrograms ?? [], cohort)
                : null;
            var canClone = currentProgram is null && suggestedSourceProgram is not null;

            items.Add(new TrainingProgramSetupItemDto
            {
                MaNganh = specialization.Major.MaNganh,
                MaCodeNganh = specialization.Major.MaCodeNganh,
                TenNganh = specialization.Major.TenNganh,
                MaChuyenNganh = specialization.Specialization.MaChuyenNganh,
                MaCodeChuyenNganh = specialization.Specialization.MaCodeChuyenNganh,
                TenChuyenNganh = specialization.Specialization.TenChuyenNganh,
                DaCoChuongTrinh = currentProgram is not null,
                ChuongTrinhHienTai = currentProgram is null ? null : ToSetupProgramDto(currentProgram),
                ChuongTrinhNguonDeXuat = suggestedSourceProgram is null ? null : ToSetupProgramDto(suggestedSourceProgram),
                CoTheClone = canClone,
                GhiChu = GetSetupNote(currentProgram, suggestedSourceProgram)
            });
        }

        var programCount = items.Count(x => x.DaCoChuongTrinh);

        return new TrainingProgramSetupDto
        {
            MaKhoaTuyenSinh = cohort.MaKhoaTuyenSinh,
            MaCodeKhoa = cohort.MaCodeKhoa,
            TenKhoa = cohort.TenKhoa,
            NamBatDau = cohort.NamBatDau,
            NamKetThucDuKien = cohort.NamKetThucDuKien,
            TongSoChuyenNganh = items.Count,
            SoChuyenNganhDaCoChuongTrinh = programCount,
            SoChuyenNganhChuaCoChuongTrinh = items.Count - programCount,
            Items = items
        };
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

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (IsGlobalReader(currentUser))
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

    private IQueryable<TrainingProgramSetupSpecialization> CreateActiveSpecializationQuery()
    {
        return
            from specialization in _context.ChuyenNganhs.AsNoTracking()
            join major in _context.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            where specialization.ConHoatDong && major.ConHoatDong
            select new TrainingProgramSetupSpecialization
            {
                Specialization = specialization,
                Major = major
            };
    }

    private IQueryable<TrainingProgramSetupSpecialization> ApplySpecializationScope(
        IQueryable<TrainingProgramSetupSpecialization> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        if (IsGlobalReader(currentUser))
        {
            return query;
        }

        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        return query.Where(x => _context.ChuyenNganhTheoCoSos
            .AsNoTracking()
            .Any(campusSpecialization =>
                campusSpecialization.MaChuyenNganh == x.Specialization.MaChuyenNganh &&
                campusSpecialization.ConHoatDong &&
                (campusSpecialization.TrangThai == ApprovedStatus || campusSpecialization.TrangThai == ActiveStatus) &&
                allowedOrganizationIdList.Contains(campusSpecialization.MaDonVi)));
    }

    private void EnsureSuperAdmin()
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý khóa tuyển sinh.");
        }
    }

    private static bool IsGlobalReader(CurrentUserContext currentUser)
    {
        return currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Chairman or AuthRoles.Admin;
    }

    private static ChuongTrinhDaoTao? PickCurrentProgram(IEnumerable<ChuongTrinhDaoTao> programs)
    {
        return programs
            .OrderBy(x => GetProgramStatusPriority(x.TrangThai))
            .ThenByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaChuongTrinh)
            .FirstOrDefault();
    }

    private static ChuongTrinhDaoTao? PickSuggestedSourceProgram(
        IEnumerable<ChuongTrinhDaoTao> programs,
        KhoaTuyenSinh cohort)
    {
        var sourcePrograms = programs
            .Where(x => IsSuggestedSourceStatus(x.TrangThai))
            .ToList();

        var previousCohortPrograms = sourcePrograms
            .Where(x => x.KhoaTuyenSinh is not null && x.KhoaTuyenSinh.NamBatDau < cohort.NamBatDau)
            .ToList();

        if (previousCohortPrograms.Count > 0)
        {
            return previousCohortPrograms
                .OrderBy(x => GetSourceStatusPriority(x.TrangThai))
                .ThenByDescending(x => x.KhoaTuyenSinh!.NamBatDau)
                .ThenByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaChuongTrinh)
                .FirstOrDefault();
        }

        return sourcePrograms
            .OrderBy(x => GetSourceStatusPriority(x.TrangThai))
            .ThenByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaChuongTrinh)
            .FirstOrDefault();
    }

    private static TrainingProgramSetupProgramDto ToSetupProgramDto(ChuongTrinhDaoTao program)
    {
        var cohort = program.KhoaTuyenSinh;
        return new TrainingProgramSetupProgramDto
        {
            MaChuongTrinh = program.MaChuongTrinh,
            MaCodeChuongTrinh = program.MaCodeChuongTrinh,
            TenChuongTrinh = program.TenChuongTrinh,
            Version = program.Version,
            TrangThai = program.TrangThai,
            MaKhoaTuyenSinh = program.MaKhoaTuyenSinh,
            MaCodeKhoa = cohort?.MaCodeKhoa ?? string.Empty,
            TenKhoa = cohort?.TenKhoa ?? string.Empty,
            SoHocKy = program.SoHocKy,
            ThoiGianDaoTaoThang = program.ThoiGianDaoTaoThang,
            TongTinChiYeuCau = program.TongTinChiYeuCau,
            NgayHieuLuc = program.NgayHieuLuc,
            NgayHetHieuLuc = program.NgayHetHieuLuc
        };
    }

    private static string GetSetupNote(ChuongTrinhDaoTao? currentProgram, ChuongTrinhDaoTao? suggestedSourceProgram)
    {
        if (currentProgram is not null)
        {
            return "Khóa tuyển sinh này đã có chương trình đào tạo cho chuyên ngành.";
        }

        return suggestedSourceProgram is not null
            ? "Có thể clone từ chương trình khóa trước."
            : "Chưa có chương trình nguồn để clone. Cần tạo mới.";
    }

    private static int GetProgramStatusPriority(string? status)
    {
        return NormalizeStatusForSort(status) switch
        {
            ActiveStatus => 1,
            ApprovedStatus => 2,
            PendingApprovalStatus => 3,
            DraftStatus => 4,
            RejectedStatus => 5,
            InactiveStatus => 6,
            ArchivedStatus => 7,
            _ => 99
        };
    }

    private static int GetSourceStatusPriority(string? status)
    {
        return NormalizeStatusForSort(status) switch
        {
            ActiveStatus => 1,
            ApprovedStatus => 2,
            _ => 99
        };
    }

    private static bool IsSuggestedSourceStatus(string? status)
    {
        return NormalizeStatusForSort(status) is ActiveStatus or ApprovedStatus;
    }

    private static string NormalizeStatusForSort(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? string.Empty : status.Trim().ToLowerInvariant();
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

    private sealed class TrainingProgramSetupSpecialization
    {
        public ChuyenNganh Specialization { get; init; } = null!;
        public NganhDaoTao Major { get; init; } = null!;
    }
}

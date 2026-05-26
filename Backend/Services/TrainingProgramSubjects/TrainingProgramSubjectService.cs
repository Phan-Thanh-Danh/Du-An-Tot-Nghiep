using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.TrainingProgramSubjects;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.TrainingProgramSubjects;

public class TrainingProgramSubjectService : ITrainingProgramSubjectService
{
    private const string DraftStatus = "draft";
    private const string RejectedStatus = "rejected";
    private const string ActiveStatus = "active";
    private const string ApprovedStatus = "approved";

    private const string RequiredSubjectType = "bat_buoc";
    private const string ElectiveSubjectType = "tu_chon";
    private const string AlternativeSubjectType = "thay_the";

    private static readonly HashSet<string> ValidSubjectTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        RequiredSubjectType,
        ElectiveSubjectType,
        AlternativeSubjectType
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TrainingProgramSubjectService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<TrainingProgramSubjectDto>> GetAsync(
        TrainingProgramSubjectQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = IsGlobalReader(currentUser)
            ? []
            : await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyReadScope(CreateSubjectQuery(), currentUser, allowedOrganizationIds);

        if (parameters.MaChuongTrinh.HasValue)
        {
            query = query.Where(x => x.MaChuongTrinh == parameters.MaChuongTrinh.Value);
        }

        if (parameters.MaMonHoc.HasValue)
        {
            query = query.Where(x => x.MaMonHoc == parameters.MaMonHoc.Value);
        }

        if (parameters.HocKyDuKien.HasValue)
        {
            query = query.Where(x => x.HocKyDuKien == parameters.HocKyDuKien.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiMonHoc))
        {
            var subjectType = NormalizeSubjectType(parameters.LoaiMonHoc);
            query = query.Where(x => x.LoaiMonHoc == subjectType);
        }

        if (parameters.BatBuoc.HasValue)
        {
            query = query.Where(x => x.BatBuoc == parameters.BatBuoc.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.DanhMucMonHoc != null &&
                (x.DanhMucMonHoc.MaCodeMonHoc.ToLower().Contains(keyword) ||
                    x.DanhMucMonHoc.TenMonHoc.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await ApplyDefaultOrder(query)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TrainingProgramSubjectDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<TrainingProgramSubjectDto>> GetByProgramAsync(
        int programId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = IsGlobalReader(currentUser)
            ? []
            : await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var program = await ApplyProgramReadScope(_context.ChuongTrinhDaoTaos.AsNoTracking(), currentUser, allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == programId, cancellationToken);

        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        var subjects = await ApplyDefaultOrder(CreateSubjectQuery()
                .Where(x => x.MaChuongTrinh == programId && x.ConHoatDong))
            .ToListAsync(cancellationToken);

        return subjects.Select(ToDto).ToList();
    }

    public async Task<TrainingProgramSubjectDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = IsGlobalReader(currentUser)
            ? []
            : await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var subject = await ApplyReadScope(CreateSubjectQuery(), currentUser, allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.MaChuongTrinhMonHoc == id, cancellationToken);

        if (subject is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học trong chương trình.");
        }

        return ToDto(subject);
    }

    public async Task<TrainingProgramSubjectDto> CreateAsync(
        CreateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var program = await ValidateEditableProgramAsync(request.MaChuongTrinh, cancellationToken);
        var subject = await ValidateSubjectAsync(request.MaMonHoc, cancellationToken);
        var subjectType = NormalizeSubjectType(request.LoaiMonHoc);
        ValidateTerm(request.HocKyDuKien, program.SoHocKy);
        ValidateCredits(request.SoTinChi);
        await EnsureUniqueProgramSubjectAsync(program.MaChuongTrinh, subject.MaMonHoc, null, cancellationToken);

        var programSubject = new MonHocTrongChuongTrinh
        {
            MaChuongTrinh = program.MaChuongTrinh,
            MaMonHoc = subject.MaMonHoc,
            HocKyDuKien = request.HocKyDuKien,
            SoTinChi = request.SoTinChi,
            LoaiMonHoc = subjectType,
            BatBuoc = NormalizeRequiredFlag(subjectType, request.BatBuoc),
            ThuTu = request.ThuTu ?? 0,
            GhiChu = NormalizeOptionalText(request.GhiChu),
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        _context.MonHocTrongChuongTrinhs.Add(programSubject);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(programSubject.MaChuongTrinhMonHoc, cancellationToken);
    }

    public async Task<TrainingProgramSubjectDto> UpdateAsync(
        int id,
        UpdateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var programSubject = await GetManagedProgramSubjectAsync(id, cancellationToken);
        var program = await ValidateEditableProgramAsync(programSubject.MaChuongTrinh, cancellationToken);
        var subjectType = NormalizeSubjectType(request.LoaiMonHoc);
        ValidateTerm(request.HocKyDuKien, program.SoHocKy);
        ValidateCredits(request.SoTinChi);

        programSubject.HocKyDuKien = request.HocKyDuKien;
        programSubject.SoTinChi = request.SoTinChi;
        programSubject.LoaiMonHoc = subjectType;
        programSubject.BatBuoc = NormalizeRequiredFlag(subjectType, request.BatBuoc);
        programSubject.ThuTu = request.ThuTu;
        programSubject.GhiChu = NormalizeOptionalText(request.GhiChu);
        programSubject.ConHoatDong = request.ConHoatDong;
        programSubject.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var programSubject = await GetManagedProgramSubjectAsync(id, cancellationToken);
        await ValidateEditableProgramAsync(programSubject.MaChuongTrinh, cancellationToken);

        programSubject.ConHoatDong = false;
        programSubject.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<MonHocTrongChuongTrinh> CreateSubjectQuery()
    {
        return _context.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Include(x => x.ChuongTrinhDaoTao)
            .Include(x => x.DanhMucMonHoc);
    }

    private IQueryable<MonHocTrongChuongTrinh> ApplyReadScope(
        IQueryable<MonHocTrongChuongTrinh> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        if (IsGlobalReader(currentUser))
        {
            return query;
        }

        return query.Where(x =>
            x.ChuongTrinhDaoTao != null &&
            ApplyProgramReadScope(_context.ChuongTrinhDaoTaos.AsNoTracking(), currentUser, allowedOrganizationIds)
                .Any(program => program.MaChuongTrinh == x.MaChuongTrinh));
    }

    private IQueryable<ChuongTrinhDaoTao> ApplyProgramReadScope(
        IQueryable<ChuongTrinhDaoTao> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        if (IsGlobalReader(currentUser))
        {
            return query;
        }

        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        return query.Where(program =>
            program.TrangThai == ActiveStatus &&
            program.ConHoatDong &&
            _context.ChuyenNganhTheoCoSos
                .AsNoTracking()
                .Any(campusSpecialization =>
                    campusSpecialization.MaChuyenNganh == program.MaChuyenNganh &&
                    campusSpecialization.ConHoatDong &&
                    (campusSpecialization.TrangThai == ApprovedStatus || campusSpecialization.TrangThai == ActiveStatus) &&
                    allowedOrganizationIdList.Contains(campusSpecialization.MaDonVi)));
    }

    private static IOrderedQueryable<MonHocTrongChuongTrinh> ApplyDefaultOrder(
        IQueryable<MonHocTrongChuongTrinh> query)
    {
        return query
            .OrderBy(x => x.HocKyDuKien)
            .ThenBy(x => x.ThuTu)
            .ThenBy(x => x.DanhMucMonHoc == null ? string.Empty : x.DanhMucMonHoc.TenMonHoc);
    }

    private async Task<MonHocTrongChuongTrinh> GetManagedProgramSubjectAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var programSubject = await _context.MonHocTrongChuongTrinhs
            .FirstOrDefaultAsync(x => x.MaChuongTrinhMonHoc == id, cancellationToken);

        if (programSubject is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học trong chương trình.");
        }

        return programSubject;
    }

    private async Task<ChuongTrinhDaoTao> ValidateEditableProgramAsync(
        int programId,
        CancellationToken cancellationToken)
    {
        var program = await _context.ChuongTrinhDaoTaos
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == programId, cancellationToken);

        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        if (!program.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình đào tạo không hoạt động.");
        }

        if (program.TrangThai is not DraftStatus and not RejectedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được chỉnh môn học khi chương trình đào tạo ở trạng thái draft hoặc rejected.");
        }

        return program;
    }

    private async Task<DanhMucMonHoc> ValidateSubjectAsync(
        int subjectId,
        CancellationToken cancellationToken)
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

    private async Task EnsureUniqueProgramSubjectAsync(
        int programId,
        int subjectId,
        int? excludedProgramSubjectId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaChuongTrinh == programId &&
                x.MaMonHoc == subjectId &&
                (!excludedProgramSubjectId.HasValue || x.MaChuongTrinhMonHoc != excludedProgramSubjectId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Môn học đã tồn tại trong chương trình đào tạo này.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý môn học trong chương trình đào tạo.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
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

    private static bool IsGlobalReader(CurrentUserContext currentUser)
    {
        return currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Chairman;
    }

    private static string NormalizeSubjectType(string value)
    {
        var subjectType = NormalizeRequiredText(value, "Loại môn học").ToLowerInvariant();
        if (!ValidSubjectTypes.Contains(subjectType))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại môn học phải là bat_buoc, tu_chon hoặc thay_the.");
        }

        return subjectType;
    }

    private static bool NormalizeRequiredFlag(string subjectType, bool? requestedValue)
    {
        return subjectType == RequiredSubjectType || (requestedValue ?? false);
    }

    private static void ValidateTerm(int expectedTerm, int semesterCount)
    {
        if (expectedTerm <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ dự kiến phải lớn hơn 0.");
        }

        if (expectedTerm > semesterCount)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ dự kiến không được lớn hơn số học kỳ của chương trình đào tạo.");
        }
    }


    private static void ValidateCredits(int credits)
    {
        if (credits <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ phải lớn hơn 0.");
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

    private static TrainingProgramSubjectDto ToDto(MonHocTrongChuongTrinh programSubject)
    {
        var program = programSubject.ChuongTrinhDaoTao;
        var subject = programSubject.DanhMucMonHoc;

        return new TrainingProgramSubjectDto
        {
            MaChuongTrinhMonHoc = programSubject.MaChuongTrinhMonHoc,
            MaChuongTrinh = programSubject.MaChuongTrinh,
            MaCodeChuongTrinh = program?.MaCodeChuongTrinh ?? string.Empty,
            TenChuongTrinh = program?.TenChuongTrinh ?? string.Empty,
            MaMonHoc = programSubject.MaMonHoc,
            MaCodeMonHoc = subject?.MaCodeMonHoc ?? string.Empty,
            TenMonHoc = subject?.TenMonHoc ?? string.Empty,
            HocKyDuKien = programSubject.HocKyDuKien,
            SoTinChi = programSubject.SoTinChi,
            LoaiMonHoc = programSubject.LoaiMonHoc,
            BatBuoc = programSubject.BatBuoc,
            ThuTu = programSubject.ThuTu,
            GhiChu = programSubject.GhiChu,
            ConHoatDong = programSubject.ConHoatDong,
            NgayTao = programSubject.NgayTao,
            NgayCapNhat = programSubject.NgayCapNhat
        };
    }
}

using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Subjects;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Subjects;

public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubjectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<SubjectDto>> GetSubjectsAsync(
        SubjectQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DanhMucMonHocs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.MaCodeMonHoc.ToLower().Contains(keyword) ||
                x.TenMonHoc.ToLower().Contains(keyword));
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.MaCodeMonHoc)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new SubjectDto
            {
                MaMonHoc = x.MaMonHoc,
                MaCodeMonHoc = x.MaCodeMonHoc,
                TenMonHoc = x.TenMonHoc,
                SoTinChi = x.SoTinChi,
                ConHoatDong = x.ConHoatDong
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SubjectDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<SubjectDto> GetByIdAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        var subject = await _context.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMonHoc == subjectId, cancellationToken);

        if (subject is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học.");
        }

        return ToDto(subject);
    }

    public async Task<SubjectDto> CreateAsync(
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var subjectCode = NormalizeSubjectCode(request.MaCodeMonHoc);
        var subjectName = NormalizeRequiredText(request.TenMonHoc, "Tên môn học");
        ValidateCredits(request.SoTinChi);
        await ValidateSubjectCodeAsync(subjectCode, null, cancellationToken);

        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = subjectCode,
            TenMonHoc = subjectName,
            SoTinChi = request.SoTinChi,
            ConHoatDong = true
        };

        _context.DanhMucMonHocs.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(subject);
    }

    public async Task<SubjectDto> UpdateAsync(
        int subjectId,
        UpdateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var subject = await GetManagedSubjectAsync(subjectId, cancellationToken);
        var subjectCode = NormalizeSubjectCode(request.MaCodeMonHoc);
        var subjectName = NormalizeRequiredText(request.TenMonHoc, "Tên môn học");
        ValidateCredits(request.SoTinChi);
        await ValidateSubjectCodeAsync(subjectCode, subjectId, cancellationToken);

        subject.MaCodeMonHoc = subjectCode;
        subject.TenMonHoc = subjectName;
        subject.SoTinChi = request.SoTinChi;
        subject.ConHoatDong = request.ConHoatDong;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(subject);
    }

    public async Task DeleteAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var subject = await GetManagedSubjectAsync(subjectId, cancellationToken);
        subject.ConHoatDong = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubjectDto> ActivateAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var subject = await GetManagedSubjectAsync(subjectId, cancellationToken);
        subject.ConHoatDong = true;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(subject);
    }

    public async Task<SubjectDto> DeactivateAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var subject = await GetManagedSubjectAsync(subjectId, cancellationToken);
        subject.ConHoatDong = false;
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(subject);
    }

    private async Task<DanhMucMonHoc> GetManagedSubjectAsync(int subjectId, CancellationToken cancellationToken)
    {
        var subject = await _context.DanhMucMonHocs.FirstOrDefaultAsync(x => x.MaMonHoc == subjectId, cancellationToken);
        if (subject is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học.");
        }

        return subject;
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý danh mục môn học.");
        }
    }

    private async Task ValidateSubjectCodeAsync(
        string subjectCode,
        int? excludedSubjectId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.DanhMucMonHocs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeMonHoc == subjectCode &&
                (!excludedSubjectId.HasValue || x.MaMonHoc != excludedSubjectId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã môn học đã tồn tại.");
        }
    }

    private async Task<bool> HasRelatedDataAsync(int subjectId, CancellationToken cancellationToken)
    {
        return
            await _context.LopHocPhans.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken) ||
            await _context.MonHocTienQuyets.AnyAsync(x => x.MaMonHoc == subjectId || x.MaMonTienQuyet == subjectId, cancellationToken) ||
            await _context.KhoaHocs.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken) ||
            await _context.CauHinhDiemMonHocs.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken) ||
            await _context.DiemSos.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken) ||
            await _context.CauHois.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken) ||
            await _context.DeKiemTras.AnyAsync(x => x.MaMonHoc == subjectId, cancellationToken);
    }

    private static string NormalizeSubjectCode(string value)
    {
        return NormalizeRequiredText(value, "Mã môn học").ToUpperInvariant();
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

    private static void ValidateCredits(int credits)
    {
        if (credits <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ phải lớn hơn 0.");
        }
    }

    private static SubjectDto ToDto(DanhMucMonHoc subject)
    {
        return new SubjectDto
        {
            MaMonHoc = subject.MaMonHoc,
            MaCodeMonHoc = subject.MaCodeMonHoc,
            TenMonHoc = subject.TenMonHoc,
            SoTinChi = subject.SoTinChi,
            ConHoatDong = subject.ConHoatDong
        };
    }
}

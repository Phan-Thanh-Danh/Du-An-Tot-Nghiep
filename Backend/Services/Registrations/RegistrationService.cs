using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Registrations;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Registrations;

public class RegistrationService : IRegistrationService
{
    private const string PeriodDraft = "nhap";
    private const string PeriodOpen = "dang_mo";
    private const string PeriodClosed = "da_dong";
    private const string SectionOpen = "mo";
    private const string SectionClosed = "dong";
    private const string SectionPendingCancel = "cho_huy";
    private const string SectionCancelled = "da_huy";
    private const string EnrollmentActive = "da_dang_ky";
    private const string EnrollmentWaitlist = "danh_sach_cho";
    private const string EnrollmentWithdrawn = "da_rut";
    private const string EnrollmentSectionCancelled = "lop_bi_huy";

    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public RegistrationService(
        ApplicationDbContext db,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<RegistrationPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);

        var periods = await PeriodQuery(currentUser)
            .OrderByDescending(p => p.BatDauLuc)
            .ToListAsync(cancellationToken);

        return await MapPeriodsAsync(periods, cancellationToken);
    }

    public async Task<RegistrationPeriodDto> GetPeriodAsync(int id, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var period = await GetManagedPeriodAsync(id, currentUser, cancellationToken);
        return (await MapPeriodsAsync([period], cancellationToken))[0];
    }

    public async Task<RegistrationPeriodDto> CreatePeriodAsync(UpsertRegistrationPeriodRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        ValidatePeriodWindow(request.OpenDate, request.CloseDate);

        var organizationId = request.MaDonVi ?? currentUser.CampusId;
        EnsureCanManageOrganization(currentUser, organizationId);

        var term = await _db.HocKys
            .FirstOrDefaultAsync(t => t.MaHocKy == request.MaHocKy && t.MaDonVi == organizationId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại trong cơ sở hiện tại.");

        var period = new GiaiDoanDangKy
        {
            MaDonVi = organizationId,
            MaHocKy = term.MaHocKy,
            BatDauLuc = request.OpenDate,
            KetThucLuc = request.CloseDate,
            SoTinChiToiDa = request.MaxCredits,
            TrangThai = PeriodDraft,
        };

        _db.GiaiDoanDangKys.Add(period);
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("GiaiDoanDangKy", period.MaGiaiDoanDk, "create", null, period, currentUser, cancellationToken);

        return (await MapPeriodsAsync([period], cancellationToken))[0];
    }

    public async Task<RegistrationPeriodDto> UpdatePeriodAsync(int id, UpsertRegistrationPeriodRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        ValidatePeriodWindow(request.OpenDate, request.CloseDate);

        var period = await GetManagedPeriodAsync(id, currentUser, cancellationToken);
        if (period.TrangThai == PeriodClosed)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Không thể sửa đợt đăng ký đã đóng.");
        }

        var organizationId = request.MaDonVi ?? period.MaDonVi;
        EnsureCanManageOrganization(currentUser, organizationId);
        var termExists = await _db.HocKys.AnyAsync(t => t.MaHocKy == request.MaHocKy && t.MaDonVi == organizationId, cancellationToken);
        if (!termExists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại trong cơ sở hiện tại.");
        }

        var oldValue = new { period.MaDonVi, period.MaHocKy, period.BatDauLuc, period.KetThucLuc, period.SoTinChiToiDa, period.TrangThai };
        period.MaDonVi = organizationId;
        period.MaHocKy = request.MaHocKy;
        period.BatDauLuc = request.OpenDate;
        period.KetThucLuc = request.CloseDate;
        period.SoTinChiToiDa = request.MaxCredits;

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("GiaiDoanDangKy", period.MaGiaiDoanDk, "update", oldValue, period, currentUser, cancellationToken);

        return (await MapPeriodsAsync([period], cancellationToken))[0];
    }

    public async Task<RegistrationPeriodDto> OpenPeriodAsync(int id, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var period = await GetManagedPeriodAsync(id, currentUser, cancellationToken);
        if (period.TrangThai == PeriodOpen)
        {
            return (await MapPeriodsAsync([period], cancellationToken))[0];
        }

        period.TrangThai = PeriodOpen;
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("GiaiDoanDangKy", period.MaGiaiDoanDk, "open", null, period, currentUser, cancellationToken);
        return (await MapPeriodsAsync([period], cancellationToken))[0];
    }

    public async Task<RegistrationPeriodDto> ClosePeriodAsync(int id, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var period = await GetManagedPeriodAsync(id, currentUser, cancellationToken);
        if (period.TrangThai == PeriodClosed)
        {
            return (await MapPeriodsAsync([period], cancellationToken))[0];
        }

        period.TrangThai = PeriodClosed;
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("GiaiDoanDangKy", period.MaGiaiDoanDk, "close", null, period, currentUser, cancellationToken);
        return (await MapPeriodsAsync([period], cancellationToken))[0];
    }

    public async Task DeleteDraftPeriodAsync(int id, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var period = await GetManagedPeriodAsync(id, currentUser, cancellationToken);
        if (period.TrangThai != PeriodDraft)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được xóa đợt đăng ký ở trạng thái nháp.");
        }

        _db.GiaiDoanDangKys.Remove(period);
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("GiaiDoanDangKy", id, "delete", period, null, currentUser, cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSectionRegistrationDto>> GetCourseSectionsAsync(string? status, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);

        var query = SectionQuery(currentUser);
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            var dbStatus = ToSectionDbStatus(status);
            query = query.Where(s => s.TrangThai == dbStatus);
        }

        var sections = await query.OrderBy(s => s.MaHocKy).ThenBy(s => s.MaCodeLopHocPhan).ToListAsync(cancellationToken);
        return await MapSectionsAsync(sections, null, cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSectionRegistrationDto>> GetPeriodRegistrationsAsync(int periodId, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var period = await GetManagedPeriodAsync(periodId, currentUser, cancellationToken);

        var sections = await SectionQuery(currentUser)
            .Where(s => s.MaDonVi == period.MaDonVi && s.MaHocKy == period.MaHocKy)
            .OrderBy(s => s.MaCodeLopHocPhan)
            .ToListAsync(cancellationToken);

        return await MapSectionsAsync(sections, null, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminRegistrationResultDto>> GetRegistrationResultsAsync(CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);

        var query = _db.DangKyHocPhans
            .Include(r => r.HocSinh)
            .Include(r => r.LopHocPhan)
                .ThenInclude(s => s!.MonHoc)
            .AsQueryable();
        if (!IsGlobalStaff(currentUser))
        {
            query = query.Where(r => r.LopHocPhan != null && r.LopHocPhan.MaDonVi == currentUser.CampusId);
        }

        var registrations = await query
            .OrderByDescending(r => r.NgayTao)
            .Take(500)
            .ToListAsync(cancellationToken);

        return registrations.Select(registration => new AdminRegistrationResultDto
        {
            Id = registration.MaDangKy,
            MaHocSinh = registration.MaHocSinh,
            StudentCode = registration.HocSinh?.MaNguoiDung.ToString() ?? registration.MaHocSinh.ToString(),
            StudentName = registration.HocSinh?.HoTen ?? string.Empty,
            MaLopHocPhan = registration.MaLopHocPhan,
            Group = registration.LopHocPhan?.MaCodeLopHocPhan ?? registration.MaLopHocPhan.ToString(),
            Course = registration.LopHocPhan?.MonHoc?.TenMonHoc ?? string.Empty,
            Credits = registration.LopHocPhan?.MonHoc?.SoTinChi ?? 0,
            Status = FromEnrollmentDbStatus(registration.TrangThai),
            WaitlistPosition = registration.ViTriCho,
            RegisteredAt = registration.NgayTao,
        }).ToList();
    }

    public async Task<CourseSectionRegistrationDto> UpdateCapacityAsync(int sectionId, UpdateCourseSectionCapacityRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var section = await GetManagedSectionAsync(sectionId, currentUser, cancellationToken);
        var activeCount = await CountActiveAsync(section.MaLopHocPhan, cancellationToken);
        if (request.Capacity < activeCount)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Sức chứa không thể nhỏ hơn số sinh viên đã đăng ký.");
        }

        var oldValue = new { section.SucChua, section.SoDaDangKy };
        section.SucChua = request.Capacity;
        await PromoteWaitlistAsync(section, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("LopHocPhan", section.MaLopHocPhan, "capacity.update", oldValue, section, currentUser, cancellationToken);

        return (await MapSectionsAsync([section], null, cancellationToken))[0];
    }

    public async Task<CourseSectionRegistrationDto> CancelSectionAsync(int sectionId, CourseSectionStatusRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var section = await GetManagedSectionAsync(sectionId, currentUser, cancellationToken);
        var oldValue = new { section.TrangThai, section.SoDaDangKy };

        var registrations = await _db.DangKyHocPhans
            .Where(r => r.MaLopHocPhan == section.MaLopHocPhan && (r.TrangThai == EnrollmentActive || r.TrangThai == EnrollmentWaitlist))
            .ToListAsync(cancellationToken);
        foreach (var registration in registrations)
        {
            registration.TrangThai = EnrollmentSectionCancelled;
            registration.ViTriCho = null;
        }

        section.TrangThai = SectionCancelled;
        section.SoDaDangKy = 0;
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("LopHocPhan", section.MaLopHocPhan, "cancel", oldValue, new { section, request.Reason }, currentUser, cancellationToken);

        return (await MapSectionsAsync([section], null, cancellationToken))[0];
    }

    public async Task<CourseSectionRegistrationDto> ReopenSectionAsync(int sectionId, CourseSectionStatusRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStaff(currentUser);
        var section = await GetManagedSectionAsync(sectionId, currentUser, cancellationToken);
        var oldValue = new { section.TrangThai };
        section.TrangThai = SectionOpen;
        section.SoDaDangKy = await CountActiveAsync(section.MaLopHocPhan, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("LopHocPhan", section.MaLopHocPhan, "reopen", oldValue, new { section, request.Reason }, currentUser, cancellationToken);

        return (await MapSectionsAsync([section], null, cancellationToken))[0];
    }

    public async Task<IReadOnlyList<CourseSectionRegistrationDto>> GetAvailableSectionsForStudentAsync(CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStudent(currentUser);
        var activePeriod = await GetOpenPeriodForStudentAsync(currentUser, cancellationToken);
        if (activePeriod is null)
        {
            return [];
        }

        var sections = await _db.LopHocPhans
            .Include(s => s.MonHoc)
            .Include(s => s.HocKy)
            .Where(s =>
                s.MaDonVi == currentUser.CampusId &&
                s.MaHocKy == activePeriod.MaHocKy &&
                s.TrangThai != SectionCancelled)
            .OrderBy(s => s.MaCodeLopHocPhan)
            .ToListAsync(cancellationToken);

        return await MapSectionsAsync(sections, currentUser.UserId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentRegistrationDto>> GetStudentRegistrationsAsync(CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStudent(currentUser);

        var registrations = await _db.DangKyHocPhans
            .Include(r => r.LopHocPhan)
                .ThenInclude(s => s!.MonHoc)
            .Include(r => r.LopHocPhan)
                .ThenInclude(s => s!.HocKy)
            .Where(r => r.MaHocSinh == currentUser.UserId && r.TrangThai != EnrollmentWithdrawn)
            .OrderByDescending(r => r.NgayTao)
            .ToListAsync(cancellationToken);

        return await MapStudentRegistrationsAsync(registrations, cancellationToken);
    }

    public async Task<StudentRegistrationDto> EnrollAsync(StudentEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStudent(currentUser);
        var section = await _db.LopHocPhans
            .Include(s => s.MonHoc)
            .Include(s => s.HocKy)
            .FirstOrDefaultAsync(s => s.MaLopHocPhan == request.MaLopHocPhan, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy lớp học phần.");

        EnsureStudentCanAccessSection(currentUser, section);
        var period = await GetOpenPeriodForStudentAsync(currentUser, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status409Conflict, "Không có đợt đăng ký đang mở.");
        if (period.MaHocKy != section.MaHocKy)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp học phần không thuộc đợt đăng ký đang mở.");
        }
        if (section.TrangThai == SectionCancelled)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp học phần đã hủy.");
        }
        if (section.TrangThai == SectionClosed)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp học phần đã đóng.");
        }

        await EnsureNoDuplicateSubjectAsync(currentUser.UserId, section, cancellationToken);
        await EnsureNoScheduleConflictAsync(currentUser.UserId, section, cancellationToken);
        await EnsureCreditLimitAsync(currentUser.UserId, section, period, cancellationToken);

        var existing = await _db.DangKyHocPhans
            .FirstOrDefaultAsync(r => r.MaHocSinh == currentUser.UserId && r.MaLopHocPhan == section.MaLopHocPhan, cancellationToken);

        var activeCount = await CountActiveAsync(section.MaLopHocPhan, cancellationToken);
        var isWaitlist = activeCount >= section.SucChua;
        var waitlistCount = await CountWaitlistAsync(section.MaLopHocPhan, cancellationToken);
        if (isWaitlist && section.QuotaVangToiDa.HasValue && waitlistCount >= section.QuotaVangToiDa.Value)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp đã hết chỗ và hàng chờ đã đầy.");
        }

        var registration = existing ?? new DangKyHocPhan
        {
            MaHocSinh = currentUser.UserId,
            MaLopHocPhan = section.MaLopHocPhan,
            NgayTao = DateTime.UtcNow,
            KiemTraTienQuyet = false,
            DaKiemTraTienQuyet = true,
        };

        registration.TrangThai = isWaitlist ? EnrollmentWaitlist : EnrollmentActive;
        registration.ViTriCho = isWaitlist ? waitlistCount + 1 : null;
        registration.LaHocLai = false;
        if (existing is null)
        {
            _db.DangKyHocPhans.Add(registration);
        }

        if (!isWaitlist)
        {
            section.SoDaDangKy = activeCount + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("DangKyHocPhan", registration.MaDangKy, "enroll", null, registration, currentUser, cancellationToken);
        return (await MapStudentRegistrationsAsync([registration], cancellationToken))[0];
    }

    public async Task<StudentRegistrationDto> WithdrawAsync(int registrationId, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureStudent(currentUser);
        var registration = await _db.DangKyHocPhans
            .Include(r => r.LopHocPhan)
                .ThenInclude(s => s!.MonHoc)
            .Include(r => r.LopHocPhan)
                .ThenInclude(s => s!.HocKy)
            .FirstOrDefaultAsync(r => r.MaDangKy == registrationId && r.MaHocSinh == currentUser.UserId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đăng ký.");

        var section = registration.LopHocPhan ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy lớp học phần.");
        var period = await GetOpenPeriodForStudentAsync(currentUser, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status409Conflict, "Không thể hủy khi đợt đăng ký đã đóng.");
        if (period.MaHocKy != section.MaHocKy)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Không thể hủy đăng ký ngoài đợt đang mở.");
        }

        var wasActive = registration.TrangThai == EnrollmentActive;
        if (registration.TrangThai != EnrollmentActive && registration.TrangThai != EnrollmentWaitlist)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đăng ký hiện tại không thể hủy.");
        }

        registration.TrangThai = EnrollmentWithdrawn;
        registration.ViTriCho = null;
        if (wasActive)
        {
            section.SoDaDangKy = Math.Max(0, section.SoDaDangKy - 1);
            await PromoteWaitlistAsync(section, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await LogAsync("DangKyHocPhan", registration.MaDangKy, "withdraw", null, registration, currentUser, cancellationToken);
        return (await MapStudentRegistrationsAsync([registration], cancellationToken))[0];
    }

    private IQueryable<GiaiDoanDangKy> PeriodQuery(CurrentUserContext currentUser)
    {
        var query = _db.GiaiDoanDangKys.Include(p => p.HocKy).AsQueryable();
        if (!IsGlobalStaff(currentUser))
        {
            query = query.Where(p => p.MaDonVi == currentUser.CampusId);
        }

        return query;
    }

    private IQueryable<LopHocPhan> SectionQuery(CurrentUserContext currentUser)
    {
        var query = _db.LopHocPhans
            .Include(s => s.MonHoc)
            .Include(s => s.HocKy)
            .AsQueryable();
        if (!IsGlobalStaff(currentUser))
        {
            query = query.Where(s => s.MaDonVi == currentUser.CampusId);
        }

        return query;
    }

    private async Task<GiaiDoanDangKy> GetManagedPeriodAsync(int id, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var period = await PeriodQuery(currentUser)
            .FirstOrDefaultAsync(p => p.MaGiaiDoanDk == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt đăng ký.");
        return period;
    }

    private async Task<LopHocPhan> GetManagedSectionAsync(int id, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var section = await SectionQuery(currentUser)
            .FirstOrDefaultAsync(s => s.MaLopHocPhan == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy lớp học phần.");
        return section;
    }

    private async Task<GiaiDoanDangKy?> GetOpenPeriodForStudentAsync(CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await _db.GiaiDoanDangKys
            .Where(p =>
                p.MaDonVi == currentUser.CampusId &&
                p.TrangThai == PeriodOpen &&
                p.BatDauLuc <= now &&
                p.KetThucLuc >= now)
            .OrderByDescending(p => p.BatDauLuc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RegistrationPeriodDto>> MapPeriodsAsync(IReadOnlyList<GiaiDoanDangKy> periods, CancellationToken cancellationToken)
    {
        if (periods.Count == 0)
        {
            return [];
        }

        var keys = periods.Select(p => new { p.MaDonVi, p.MaHocKy }).Distinct().ToList();
        var result = new List<RegistrationPeriodDto>(periods.Count);
        foreach (var period in periods)
        {
            var classCount = await _db.LopHocPhans.CountAsync(s => s.MaDonVi == period.MaDonVi && s.MaHocKy == period.MaHocKy, cancellationToken);
            var studentCount = await _db.DangKyHocPhans
                .Where(r => r.LopHocPhan != null &&
                    r.LopHocPhan.MaDonVi == period.MaDonVi &&
                    r.LopHocPhan.MaHocKy == period.MaHocKy &&
                    (r.TrangThai == EnrollmentActive || r.TrangThai == EnrollmentWaitlist))
                .Select(r => r.MaHocSinh)
                .Distinct()
                .CountAsync(cancellationToken);

            result.Add(new RegistrationPeriodDto
            {
                Id = period.MaGiaiDoanDk,
                MaDonVi = period.MaDonVi,
                MaHocKy = period.MaHocKy,
                Name = $"Đợt đăng ký - {period.HocKy?.TenHocKy ?? period.MaHocKy.ToString()}",
                Semester = period.HocKy?.TenHocKy ?? period.MaHocKy.ToString(),
                OpenDate = period.BatDauLuc,
                CloseDate = period.KetThucLuc,
                WithdrawDeadline = period.HocKy?.HanRutMon,
                MaxCredits = period.SoTinChiToiDa,
                Status = FromPeriodDbStatus(period.TrangThai),
                StudentCount = studentCount,
                ClassCount = classCount,
            });
        }

        return result;
    }

    private async Task<IReadOnlyList<CourseSectionRegistrationDto>> MapSectionsAsync(
        IReadOnlyList<LopHocPhan> sections,
        int? studentId,
        CancellationToken cancellationToken)
    {
        if (sections.Count == 0)
        {
            return [];
        }

        var sectionIds = sections.Select(s => s.MaLopHocPhan).ToList();
        var courses = await _db.KhoaHocs
            .Include(c => c.GiaoVien)
            .Where(c => c.MaLopHocPhan.HasValue && sectionIds.Contains(c.MaLopHocPhan.Value))
            .ToListAsync(cancellationToken);
        var courseBySection = courses
            .GroupBy(c => c.MaLopHocPhan!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var counts = await _db.DangKyHocPhans
            .Where(r => sectionIds.Contains(r.MaLopHocPhan))
            .GroupBy(r => r.MaLopHocPhan)
            .Select(g => new
            {
                SectionId = g.Key,
                Active = g.Count(r => r.TrangThai == EnrollmentActive),
                Waitlist = g.Count(r => r.TrangThai == EnrollmentWaitlist),
            })
            .ToDictionaryAsync(x => x.SectionId, x => x, cancellationToken);

        var studentRegistrations = studentId.HasValue
            ? await _db.DangKyHocPhans
                .Where(r => r.MaHocSinh == studentId.Value && sectionIds.Contains(r.MaLopHocPhan))
                .ToDictionaryAsync(r => r.MaLopHocPhan, r => r, cancellationToken)
            : [];

        var schedules = await BuildScheduleMapAsync(courses.Select(c => c.MaKhoaHoc).ToList(), cancellationToken);
        return sections.Select(section =>
        {
            courseBySection.TryGetValue(section.MaLopHocPhan, out var course);
            counts.TryGetValue(section.MaLopHocPhan, out var count);
            studentRegistrations.TryGetValue(section.MaLopHocPhan, out var registration);
            var enrolled = count?.Active ?? section.SoDaDangKy;
            var waitlist = count?.Waitlist ?? 0;

            return new CourseSectionRegistrationDto
            {
                Id = section.MaLopHocPhan,
                Code = section.MaCodeLopHocPhan,
                MaDonVi = section.MaDonVi,
                MaHocKy = section.MaHocKy,
                MaMonHoc = section.MaMonHoc,
                MaKhoaHoc = course?.MaKhoaHoc,
                Subject = section.MonHoc?.TenMonHoc ?? course?.TieuDe ?? section.MaMonHoc.ToString(),
                SubjectCode = section.MonHoc?.MaCodeMonHoc ?? string.Empty,
                Credits = section.MonHoc?.SoTinChi ?? 0,
                Semester = section.HocKy?.TenHocKy ?? section.MaHocKy.ToString(),
                Teacher = course?.GiaoVien?.HoTen ?? "Chưa phân công",
                Schedule = course is not null && schedules.TryGetValue(course.MaKhoaHoc, out var schedule) ? schedule : "Chưa có lịch",
                Room = string.Empty,
                Capacity = section.SucChua,
                Enrolled = enrolled,
                Waitlist = waitlist,
                MinEnroll = section.SoDangKyToiThieu ?? 0,
                Status = FromSectionDbStatus(section.TrangThai, enrolled, section.SucChua),
                RegistrationStatus = registration is null ? string.Empty : FromEnrollmentDbStatus(registration.TrangThai),
                RegistrationId = registration?.MaDangKy,
                WaitlistPosition = registration?.ViTriCho,
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<StudentRegistrationDto>> MapStudentRegistrationsAsync(
        IReadOnlyList<DangKyHocPhan> registrations,
        CancellationToken cancellationToken)
    {
        if (registrations.Count == 0)
        {
            return [];
        }

        var sectionIds = registrations.Select(r => r.MaLopHocPhan).Distinct().ToList();
        var courses = await _db.KhoaHocs
            .Include(c => c.GiaoVien)
            .Where(c => c.MaLopHocPhan.HasValue && sectionIds.Contains(c.MaLopHocPhan.Value))
            .ToListAsync(cancellationToken);
        var courseBySection = courses.GroupBy(c => c.MaLopHocPhan!.Value).ToDictionary(g => g.Key, g => g.First());
        var schedules = await BuildScheduleMapAsync(courses.Select(c => c.MaKhoaHoc).ToList(), cancellationToken);

        return registrations.Select(registration =>
        {
            var section = registration.LopHocPhan;
            courseBySection.TryGetValue(registration.MaLopHocPhan, out var course);
            return new StudentRegistrationDto
            {
                Id = registration.MaDangKy,
                MaLopHocPhan = registration.MaLopHocPhan,
                Code = section?.MaCodeLopHocPhan ?? registration.MaLopHocPhan.ToString(),
                Subject = section?.MonHoc?.TenMonHoc ?? course?.TieuDe ?? string.Empty,
                Credits = section?.MonHoc?.SoTinChi ?? 0,
                Semester = section?.HocKy?.TenHocKy ?? string.Empty,
                Teacher = course?.GiaoVien?.HoTen ?? "Chưa phân công",
                Schedule = course is not null && schedules.TryGetValue(course.MaKhoaHoc, out var schedule) ? schedule : "Chưa có lịch",
                Status = FromEnrollmentDbStatus(registration.TrangThai),
                WaitlistPosition = registration.ViTriCho,
                CreatedAt = registration.NgayTao,
            };
        }).ToList();
    }

    private async Task<Dictionary<int, string>> BuildScheduleMapAsync(IReadOnlyList<int> courseIds, CancellationToken cancellationToken)
    {
        if (courseIds.Count == 0)
        {
            return [];
        }

        var rows = await _db.ThoiKhoaBieus
            .Include(t => t.CaHoc)
            .Include(t => t.Phong)
            .Where(t => courseIds.Contains(t.MaKhoaHoc) && t.TrangThai != "da_huy")
            .OrderBy(t => t.ThuTrongTuan)
            .ThenBy(t => t.CaHoc != null ? t.CaHoc.ThuTu : 0)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(t => t.MaKhoaHoc)
            .ToDictionary(
                g => g.Key,
                g => string.Join("; ", g.Select(t =>
                    $"T{t.ThuTrongTuan}, {FormatShift(t.CaHoc)}{(t.Phong is null ? string.Empty : $" | {t.Phong.MaCodePhong}")}")));
    }

    private static string FormatShift(Backend.Models.CaHoc? shift)
    {
        return shift is null
            ? "Ca chưa rõ"
            : $"{shift.GioBatDau:HH\\:mm}-{shift.GioKetThuc:HH\\:mm}";
    }

    private async Task<int> CountActiveAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await _db.DangKyHocPhans.CountAsync(r => r.MaLopHocPhan == sectionId && r.TrangThai == EnrollmentActive, cancellationToken);
    }

    private async Task<int> CountWaitlistAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await _db.DangKyHocPhans.CountAsync(r => r.MaLopHocPhan == sectionId && r.TrangThai == EnrollmentWaitlist, cancellationToken);
    }

    private async Task PromoteWaitlistAsync(LopHocPhan section, CancellationToken cancellationToken)
    {
        var activeCount = await CountActiveAsync(section.MaLopHocPhan, cancellationToken);
        var seats = Math.Max(0, section.SucChua - activeCount);
        if (seats == 0)
        {
            section.SoDaDangKy = activeCount;
            return;
        }

        var waitlisted = await _db.DangKyHocPhans
            .Where(r => r.MaLopHocPhan == section.MaLopHocPhan && r.TrangThai == EnrollmentWaitlist)
            .OrderBy(r => r.ViTriCho ?? int.MaxValue)
            .ThenBy(r => r.NgayTao)
            .Take(seats)
            .ToListAsync(cancellationToken);

        foreach (var registration in waitlisted)
        {
            registration.TrangThai = EnrollmentActive;
            registration.ViTriCho = null;
        }

        var remaining = await _db.DangKyHocPhans
            .Where(r => r.MaLopHocPhan == section.MaLopHocPhan && r.TrangThai == EnrollmentWaitlist)
            .OrderBy(r => r.ViTriCho ?? int.MaxValue)
            .ThenBy(r => r.NgayTao)
            .ToListAsync(cancellationToken);
        for (var i = 0; i < remaining.Count; i++)
        {
            remaining[i].ViTriCho = i + 1;
        }

        section.SoDaDangKy = activeCount + waitlisted.Count;
    }

    private async Task EnsureNoDuplicateSubjectAsync(int studentId, LopHocPhan section, CancellationToken cancellationToken)
    {
        var hasDuplicate = await _db.DangKyHocPhans
            .AnyAsync(r =>
                r.MaHocSinh == studentId &&
                r.LopHocPhan != null &&
                r.LopHocPhan.MaHocKy == section.MaHocKy &&
                r.LopHocPhan.MaMonHoc == section.MaMonHoc &&
                r.MaLopHocPhan != section.MaLopHocPhan &&
                (r.TrangThai == EnrollmentActive || r.TrangThai == EnrollmentWaitlist),
                cancellationToken);
        if (hasDuplicate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Sinh viên đã đăng ký môn này trong học kỳ hiện tại.");
        }
    }

    private async Task EnsureCreditLimitAsync(int studentId, LopHocPhan section, GiaiDoanDangKy period, CancellationToken cancellationToken)
    {
        var currentCredits = await _db.DangKyHocPhans
            .Where(r =>
                r.MaHocSinh == studentId &&
                r.LopHocPhan != null &&
                r.LopHocPhan.MaHocKy == section.MaHocKy &&
                r.TrangThai == EnrollmentActive)
            .Select(r => r.LopHocPhan!.MonHoc != null ? r.LopHocPhan.MonHoc.SoTinChi : 0)
            .SumAsync(cancellationToken);
        var newCredits = section.MonHoc?.SoTinChi ?? await _db.DanhMucMonHocs
            .Where(s => s.MaMonHoc == section.MaMonHoc)
            .Select(s => s.SoTinChi)
            .FirstOrDefaultAsync(cancellationToken);
        if (currentCredits + newCredits > period.SoTinChiToiDa)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Vượt quá số tín chỉ tối đa của đợt đăng ký.");
        }
    }

    private async Task EnsureNoScheduleConflictAsync(int studentId, LopHocPhan targetSection, CancellationToken cancellationToken)
    {
        var targetCourseIds = await _db.KhoaHocs
            .Where(c => c.MaLopHocPhan == targetSection.MaLopHocPhan)
            .Select(c => c.MaKhoaHoc)
            .ToListAsync(cancellationToken);
        if (targetCourseIds.Count == 0)
        {
            return;
        }

        var targetSlots = await _db.ThoiKhoaBieus
            .Where(t => targetCourseIds.Contains(t.MaKhoaHoc) && t.TrangThai != "da_huy")
            .Select(t => new { t.ThuTrongTuan, t.MaCaHoc })
            .ToListAsync(cancellationToken);
        if (targetSlots.Count == 0)
        {
            return;
        }

        var enrolledCourseIds = await _db.DangKyHocPhans
            .Where(r =>
                r.MaHocSinh == studentId &&
                r.TrangThai == EnrollmentActive &&
                r.LopHocPhan != null &&
                r.LopHocPhan.MaHocKy == targetSection.MaHocKy)
            .Join(_db.KhoaHocs,
                r => r.MaLopHocPhan,
                c => c.MaLopHocPhan,
                (_, c) => c.MaKhoaHoc)
            .ToListAsync(cancellationToken);
        if (enrolledCourseIds.Count == 0)
        {
            return;
        }

        var hasConflict = await _db.ThoiKhoaBieus
            .Where(t => enrolledCourseIds.Contains(t.MaKhoaHoc) && t.TrangThai != "da_huy")
            .AnyAsync(t => targetSlots.Any(slot => slot.ThuTrongTuan == t.ThuTrongTuan && slot.MaCaHoc == t.MaCaHoc), cancellationToken);
        if (hasConflict)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp học phần bị trùng lịch với đăng ký hiện tại.");
        }
    }

    private static void ValidatePeriodWindow(DateTime openDate, DateTime closeDate)
    {
        if (closeDate <= openDate)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày đóng phải sau ngày mở đăng ký.");
        }
    }

    private static void EnsureStudentCanAccessSection(CurrentUserContext currentUser, LopHocPhan section)
    {
        if (section.MaDonVi != currentUser.CampusId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Không thể đăng ký lớp học phần ngoài cơ sở.");
        }
    }

    private CurrentUserContext GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Không xác định được người dùng hiện tại.");
    }

    private static void EnsureStaff(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.Admin or AuthRoles.SuperAdmin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền quản lý đăng ký học phần.");
        }
    }

    private static void EnsureStudent(CurrentUserContext currentUser)
    {
        if (currentUser.Role != AuthRoles.Student)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ sinh viên được thao tác đăng ký học phần.");
        }
    }

    private static bool IsGlobalStaff(CurrentUserContext currentUser)
    {
        return currentUser.Role is AuthRoles.Admin or AuthRoles.SuperAdmin;
    }

    private static void EnsureCanManageOrganization(CurrentUserContext currentUser, int organizationId)
    {
        if (!IsGlobalStaff(currentUser) && currentUser.CampusId != organizationId)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Không thể thao tác ngoài phạm vi cơ sở.");
        }
    }

    private async Task LogAsync(
        string entityType,
        int entityId,
        string action,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        await _auditLogService.LogAsync(
            entityType,
            entityId.ToString(),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            currentUser.CampusId,
            $"P7 registration workflow: {action}",
            cancellationToken);
    }

    private static string FromPeriodDbStatus(string status)
    {
        return status switch
        {
            PeriodOpen => "open",
            PeriodClosed => "closed",
            _ => "draft",
        };
    }

    private static string FromSectionDbStatus(string status, int enrolled, int capacity)
    {
        return status switch
        {
            SectionCancelled => "cancelled",
            SectionPendingCancel => "pending_cancel",
            SectionClosed => "closed",
            SectionOpen when enrolled >= capacity => "full",
            _ => "open",
        };
    }

    private static string ToSectionDbStatus(string status)
    {
        return status switch
        {
            "cancelled" => SectionCancelled,
            "pending_cancel" => SectionPendingCancel,
            "closed" => SectionClosed,
            _ => SectionOpen,
        };
    }

    private static string FromEnrollmentDbStatus(string status)
    {
        return status switch
        {
            EnrollmentActive => "Enrolled",
            EnrollmentWaitlist => "Waitlist",
            EnrollmentWithdrawn => "Dropped",
            EnrollmentSectionCancelled => "Cancelled",
            _ => status,
        };
    }
}

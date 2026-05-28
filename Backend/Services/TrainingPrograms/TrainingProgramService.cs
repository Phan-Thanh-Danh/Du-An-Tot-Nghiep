using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.TrainingPrograms;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.TrainingPrograms;

public class TrainingProgramService : ITrainingProgramService
{
    private const string DraftStatus = "draft";
    private const string PendingApprovalStatus = "pending_approval";
    private const string ApprovedStatus = "approved";
    private const string RejectedStatus = "rejected";
    private const string ActiveStatus = "active";
    private const string InactiveStatus = "inactive";
    private const string ArchivedStatus = "archived";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        DraftStatus,
        PendingApprovalStatus,
        ApprovedStatus,
        RejectedStatus,
        ActiveStatus,
        InactiveStatus,
        ArchivedStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TrainingProgramService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<TrainingProgramDto>> GetAsync(
        TrainingProgramQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyReadScope(CreateTrainingProgramQuery(), currentUser, allowedOrganizationIds);

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.MaCodeChuongTrinh.ToLower().Contains(keyword) ||
                x.TenChuongTrinh.ToLower().Contains(keyword) ||
                x.Version.ToLower().Contains(keyword));
        }

        if (parameters.MaChuyenNganh.HasValue)
        {
            query = query.Where(x => x.MaChuyenNganh == parameters.MaChuyenNganh.Value);
        }

        if (parameters.MaNganh.HasValue)
        {
            query = query.Where(x => x.ChuyenNganh != null && x.ChuyenNganh.MaNganh == parameters.MaNganh.Value);
        }

        if (parameters.MaKhoaTuyenSinh.HasValue)
        {
            query = query.Where(x => x.MaKhoaTuyenSinh == parameters.MaKhoaTuyenSinh.Value);
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (!IsGlobalReader(currentUser))
            {
                EnsureOrganizationInScope(allowedOrganizationIds, parameters.MaDonVi.Value, "Bạn không có quyền xem chương trình đào tạo của đơn vị này.");
            }

            query = FilterByOpenedSpecializationAtOrganization(query, parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.TrangThai == status);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.NgayTao)
            .ThenBy(x => x.TenChuongTrinh)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TrainingProgramDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<TrainingProgramDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var program = await ApplyReadScope(CreateTrainingProgramQuery(), currentUser, allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == id, cancellationToken);

        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        return ToDto(program);
    }

    public async Task<TrainingProgramDto> CreateAsync(
        CreateTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var programCode = NormalizeCode(request.MaCodeChuongTrinh);
        var programName = NormalizeRequiredText(request.TenChuongTrinh, "Tên chương trình");
        var version = NormalizeRequiredText(request.Version, "Phiên bản");
        var status = NormalizeStatus(request.TrangThai, useDefaultDraft: true);
        if (status != DraftStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình mới phải ở trạng thái draft trước khi gửi duyệt.");
        }

        var specialization = await ValidateTrainingProgramRequestAsync(
            null,
            request.MaChuyenNganh,
            request.MaKhoaTuyenSinh,
            programCode,
            version,
            request.SoHocKy,
            request.ThoiGianDaoTaoThang,
            request.TongTinChiYeuCau,
            request.SoTinChiToiThieuMoiKy,
            request.SoTinChiToiDaMoiKy,
            status,
            request.NguonChuongTrinhId,
            request.GhiChuThayDoi,
            request.NgayHieuLuc,
            request.NgayHetHieuLuc,
            cancellationToken);

        var program = new ChuongTrinhDaoTao
        {
            MaChuyenNganh = specialization.MaChuyenNganh,
            MaKhoaTuyenSinh = request.MaKhoaTuyenSinh,
            MaCodeChuongTrinh = programCode,
            TenChuongTrinh = programName,
            Version = version,
            SoHocKy = request.SoHocKy,
            ThoiGianDaoTaoThang = request.ThoiGianDaoTaoThang,
            TongTinChiYeuCau = request.TongTinChiYeuCau,
            SoTinChiToiThieuMoiKy = request.SoTinChiToiThieuMoiKy,
            SoTinChiToiDaMoiKy = request.SoTinChiToiDaMoiKy,
            TrangThai = status,
            MoTa = NormalizeOptionalText(request.MoTa),
            NguonChuongTrinhId = request.NguonChuongTrinhId,
            GhiChuThayDoi = NormalizeOptionalText(request.GhiChuThayDoi),
            NgayHieuLuc = request.NgayHieuLuc,
            NgayHetHieuLuc = request.NgayHetHieuLuc,
            ConHoatDong = status is not InactiveStatus and not ArchivedStatus,
            NgayTao = DateTime.UtcNow
        };

        _context.ChuongTrinhDaoTaos.Add(program);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(program.MaChuongTrinh, cancellationToken);
    }

    public async Task<TrainingProgramDto> CloneAsync(
        int sourceProgramId,
        CloneTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var sourceProgram = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == sourceProgramId, cancellationToken);

        if (sourceProgram is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình nguồn.");
        }

        var sourceStatus = sourceProgram.TrangThai.Trim().ToLowerInvariant();
        if (!sourceProgram.ConHoatDong || sourceStatus == ArchivedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình nguồn không khả dụng để clone.");
        }

        if (sourceStatus is not ApprovedStatus and not ActiveStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được clone từ chương trình đã được duyệt hoặc đang active.");
        }

        await ValidateTargetCohortAsync(request.MaKhoaTuyenSinhMoi, cancellationToken);

        if (sourceProgram.MaKhoaTuyenSinh == request.MaKhoaTuyenSinhMoi)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể clone chương trình sang cùng khóa tuyển sinh.");
        }

        var programCode = NormalizeCode(request.MaCodeChuongTrinh);
        var programName = NormalizeRequiredText(request.TenChuongTrinh, "Tên chương trình");
        var version = NormalizeRequiredText(request.Version, "Phiên bản");
        ValidateTrainingProgramData(
            sourceProgram.SoHocKy,
            sourceProgram.ThoiGianDaoTaoThang,
            sourceProgram.TongTinChiYeuCau,
            sourceProgram.SoTinChiToiThieuMoiKy,
            sourceProgram.SoTinChiToiDaMoiKy,
            request.NgayHieuLuc,
            request.NgayHetHieuLuc);

        await EnsureCodeUniqueAsync(programCode, null, cancellationToken);
        await EnsureProgramUniqueAsync(
            sourceProgram.MaChuyenNganh,
            request.MaKhoaTuyenSinhMoi,
            version,
            null,
            cancellationToken);

        var changeNote = NormalizeOptionalText(request.GhiChuThayDoi)
            ?? $"Clone từ chương trình {sourceProgram.MaCodeChuongTrinh}";

        var program = new ChuongTrinhDaoTao
        {
            MaChuyenNganh = sourceProgram.MaChuyenNganh,
            MaKhoaTuyenSinh = request.MaKhoaTuyenSinhMoi,
            MaCodeChuongTrinh = programCode,
            TenChuongTrinh = programName,
            Version = version,
            SoHocKy = sourceProgram.SoHocKy,
            ThoiGianDaoTaoThang = sourceProgram.ThoiGianDaoTaoThang,
            TongTinChiYeuCau = sourceProgram.TongTinChiYeuCau,
            SoTinChiToiThieuMoiKy = sourceProgram.SoTinChiToiThieuMoiKy,
            SoTinChiToiDaMoiKy = sourceProgram.SoTinChiToiDaMoiKy,
            TrangThai = DraftStatus,
            MoTa = sourceProgram.MoTa,
            NguonChuongTrinhId = sourceProgram.MaChuongTrinh,
            GhiChuThayDoi = changeNote,
            NgayHieuLuc = request.NgayHieuLuc,
            NgayHetHieuLuc = request.NgayHetHieuLuc,
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = null,
            NguoiGuiDuyetId = null,
            ThoiGianGuiDuyet = null,
            NguoiDuyetId = null,
            ThoiGianDuyet = null,
            GhiChuDuyet = null,
            NguoiTuChoiId = null,
            ThoiGianTuChoi = null,
            LyDoTuChoi = null
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.ChuongTrinhDaoTaos.Add(program);
        await _context.SaveChangesAsync(cancellationToken);

        var sourceSubjects = await _context.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == sourceProgram.MaChuongTrinh && x.ConHoatDong)
            .OrderBy(x => x.HocKyDuKien)
            .ThenBy(x => x.ThuTu)
            .ThenBy(x => x.MaChuongTrinhMonHoc)
            .ToListAsync(cancellationToken);

        var distinctSubjects = sourceSubjects
            .GroupBy(x => x.MaMonHoc)
            .Select(g => g.First())
            .ToList();

        foreach (var sourceSubject in distinctSubjects)
        {
            _context.MonHocTrongChuongTrinhs.Add(new MonHocTrongChuongTrinh
            {
                MaChuongTrinh = program.MaChuongTrinh,
                MaMonHoc = sourceSubject.MaMonHoc,
                HocKyDuKien = sourceSubject.HocKyDuKien,
                SoTinChi = sourceSubject.SoTinChi,
                LoaiMonHoc = sourceSubject.LoaiMonHoc,
                BatBuoc = sourceSubject.BatBuoc,
                ThuTu = sourceSubject.ThuTu,
                GhiChu = sourceSubject.GhiChu,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
        }

        var sourceOldToNewIdMap = await CloneCourseSyllabusesAsync(
            sourceProgram.MaChuongTrinh,
            program.MaChuongTrinh,
            distinctSubjects,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetByIdAsync(program.MaChuongTrinh, cancellationToken);
    }

    public async Task<TrainingProgramDto> UpdateAsync(
        int id,
        UpdateTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        if (program.TrangThai is not DraftStatus and not RejectedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được cập nhật chương trình ở trạng thái draft hoặc rejected.");
        }

        var programCode = NormalizeCode(request.MaCodeChuongTrinh);
        var programName = NormalizeRequiredText(request.TenChuongTrinh, "Tên chương trình");
        var version = NormalizeRequiredText(request.Version, "Phiên bản");
        var status = NormalizeStatus(request.TrangThai);
        if (status is not DraftStatus and not RejectedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PUT chỉ được giữ chương trình ở trạng thái draft hoặc rejected. Dùng endpoint riêng để gửi duyệt, duyệt hoặc kích hoạt.");
        }

        var specialization = await ValidateTrainingProgramRequestAsync(
            id,
            request.MaChuyenNganh,
            request.MaKhoaTuyenSinh,
            programCode,
            version,
            request.SoHocKy,
            request.ThoiGianDaoTaoThang,
            request.TongTinChiYeuCau,
            request.SoTinChiToiThieuMoiKy,
            request.SoTinChiToiDaMoiKy,
            status,
            request.NguonChuongTrinhId,
            request.GhiChuThayDoi,
            request.NgayHieuLuc,
            request.NgayHetHieuLuc,
            cancellationToken);

        program.MaChuyenNganh = specialization.MaChuyenNganh;
        program.MaKhoaTuyenSinh = request.MaKhoaTuyenSinh;
        program.MaCodeChuongTrinh = programCode;
        program.TenChuongTrinh = programName;
        program.Version = version;
        program.SoHocKy = request.SoHocKy;
        program.ThoiGianDaoTaoThang = request.ThoiGianDaoTaoThang;
        program.TongTinChiYeuCau = request.TongTinChiYeuCau;
        program.SoTinChiToiThieuMoiKy = request.SoTinChiToiThieuMoiKy;
        program.SoTinChiToiDaMoiKy = request.SoTinChiToiDaMoiKy;
        program.TrangThai = status;
        program.MoTa = NormalizeOptionalText(request.MoTa);
        program.NguonChuongTrinhId = request.NguonChuongTrinhId;
        program.GhiChuThayDoi = NormalizeOptionalText(request.GhiChuThayDoi);
        program.NgayHieuLuc = request.NgayHieuLuc;
        program.NgayHetHieuLuc = request.NgayHetHieuLuc;
        program.ConHoatDong = status is InactiveStatus or ArchivedStatus ? false : request.ConHoatDong;
        program.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        program.ConHoatDong = false;
        program.TrangThai = InactiveStatus;
        program.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrainingProgramDto> SubmitAsync(
        int id,
        SubmitTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureSuperAdmin();
        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);

        if (program.TrangThai is not DraftStatus and not RejectedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình draft hoặc rejected mới được gửi duyệt.");
        }

        ValidateSubmittableProgram(program);

        program.TrangThai = PendingApprovalStatus;
        program.NguoiGuiDuyetId = currentUser.UserId;
        program.ThoiGianGuiDuyet = DateTime.UtcNow;
        program.NguoiTuChoiId = null;
        program.ThoiGianTuChoi = null;
        program.LyDoTuChoi = null;
        program.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrainingProgramDto> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureChairman();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        if (program.TrangThai is not ApprovedStatus and not InactiveStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình đã duyệt hoặc đang inactive mới được kích hoạt.");
        }

        await EnsureNoActiveProgramConflictAsync(program.MaChuyenNganh, program.MaKhoaTuyenSinh, id, cancellationToken);

        program.TrangThai = ActiveStatus;
        program.ConHoatDong = true;
        program.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrainingProgramDto> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureChairman();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        if (program.TrangThai != ActiveStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình active mới được vô hiệu hóa.");
        }

        program.TrangThai = InactiveStatus;
        program.ConHoatDong = false;
        program.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrainingProgramDto> ApproveAsync(
        int id,
        ApproveTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureChairman();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        if (program.TrangThai != PendingApprovalStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình đang chờ duyệt mới được duyệt.");
        }

        program.TrangThai = ApprovedStatus;
        program.NguoiDuyetId = currentUser.UserId;
        program.ThoiGianDuyet = DateTime.UtcNow;
        program.GhiChuDuyet = NormalizeOptionalText(request.GhiChuDuyet);
        program.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrainingProgramDto> RejectAsync(
        int id,
        RejectTrainingProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureChairman();
        var rejectReason = NormalizeRequiredText(request.LyDoTuChoi, "Lý do từ chối");
        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);

        if (program.TrangThai != PendingApprovalStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình đang chờ duyệt mới được từ chối.");
        }

        program.TrangThai = RejectedStatus;
        program.NguoiTuChoiId = currentUser.UserId;
        program.ThoiGianTuChoi = DateTime.UtcNow;
        program.LyDoTuChoi = rejectReason;
        program.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrainingProgramDto> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureChairman();

        var program = await GetManagedTrainingProgramAsync(id, cancellationToken);
        if (program.TrangThai == ArchivedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình đã được lưu trữ.");
        }

        if (program.TrangThai is not ApprovedStatus and not ActiveStatus and not InactiveStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ chương trình approved, active hoặc inactive mới được lưu trữ.");
        }

        // TODO: chặn lưu trữ khi đã có lớp active sau khi bảng lớp gắn khóa tới chương trình đào tạo.
        program.TrangThai = ArchivedStatus;
        program.ConHoatDong = false;
        program.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<ChuongTrinhDaoTao> CreateTrainingProgramQuery()
    {
        return _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Include(x => x.ChuyenNganh)
                .ThenInclude(x => x!.NganhDaoTao)
            .Include(x => x.KhoaTuyenSinh)
            .Include(x => x.NguonChuongTrinh);
    }

    private IQueryable<ChuongTrinhDaoTao> ApplyReadScope(
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

    private IQueryable<ChuongTrinhDaoTao> FilterByOpenedSpecializationAtOrganization(
        IQueryable<ChuongTrinhDaoTao> query,
        int organizationId)
    {
        return query.Where(program => _context.ChuyenNganhTheoCoSos
            .AsNoTracking()
            .Any(campusSpecialization =>
                campusSpecialization.MaChuyenNganh == program.MaChuyenNganh &&
                campusSpecialization.MaDonVi == organizationId &&
                campusSpecialization.ConHoatDong &&
                (campusSpecialization.TrangThai == ApprovedStatus || campusSpecialization.TrangThai == ActiveStatus)));
    }

    private async Task<ChuongTrinhDaoTao> GetManagedTrainingProgramAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(x => x.MaChuongTrinh == id, cancellationToken);
        if (program is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình đào tạo.");
        }

        return program;
    }

    private async Task<ChuyenNganh> ValidateTrainingProgramRequestAsync(
        int? excludedProgramId,
        int specializationId,
        int cohortId,
        string programCode,
        string version,
        int semesterCount,
        int trainingDurationMonths,
        int? requiredCredits,
        int? minCreditsPerTerm,
        int? maxCreditsPerTerm,
        string status,
        int? sourceProgramId,
        string? changeNote,
        DateOnly? effectiveDate,
        DateOnly? expirationDate,
        CancellationToken cancellationToken)
    {
        ValidateStatus(status);
        ValidateTrainingProgramData(
            semesterCount,
            trainingDurationMonths,
            requiredCredits,
            minCreditsPerTerm,
            maxCreditsPerTerm,
            effectiveDate,
            expirationDate);

        var specialization = await ValidateSpecializationAsync(specializationId, cancellationToken);
        await ValidateCohortAsync(cohortId, cancellationToken);
        await EnsureCodeUniqueAsync(programCode, excludedProgramId, cancellationToken);
        await EnsureProgramUniqueAsync(specializationId, cohortId, version, excludedProgramId, cancellationToken);

        if (status == ActiveStatus)
        {
            await EnsureNoActiveProgramConflictAsync(specializationId, cohortId, excludedProgramId, cancellationToken);
        }

        await ValidateSourceProgramAsync(sourceProgramId, excludedProgramId, specialization, changeNote, cancellationToken);
        return specialization;
    }

    private async Task<ChuyenNganh> ValidateSpecializationAsync(
        int specializationId,
        CancellationToken cancellationToken)
    {
        var specialization = await _context.ChuyenNganhs
            .AsNoTracking()
            .Include(x => x.NganhDaoTao)
            .FirstOrDefaultAsync(x => x.MaChuyenNganh == specializationId, cancellationToken);

        if (specialization is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chuyên ngành.");
        }

        if (!specialization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chuyên ngành không hoạt động.");
        }

        if (specialization.NganhDaoTao is not null && !specialization.NganhDaoTao.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngành đào tạo của chuyên ngành không hoạt động.");
        }

        return specialization;
    }

    private async Task ValidateCohortAsync(int cohortId, CancellationToken cancellationToken)
    {
        var cohort = await _context.KhoaTuyenSinhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaTuyenSinh == cohortId, cancellationToken);

        if (cohort is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa tuyển sinh.");
        }

        if (!cohort.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa tuyển sinh không hoạt động.");
        }
    }

    private async Task ValidateTargetCohortAsync(int cohortId, CancellationToken cancellationToken)
    {
        var cohort = await _context.KhoaTuyenSinhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaTuyenSinh == cohortId, cancellationToken);

        if (cohort is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa tuyển sinh mới.");
        }

        if (!cohort.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa tuyển sinh mới không hoạt động.");
        }
    }

    private async Task ValidateSourceProgramAsync(
        int? sourceProgramId,
        int? currentProgramId,
        ChuyenNganh targetSpecialization,
        string? changeNote,
        CancellationToken cancellationToken)
    {
        if (!sourceProgramId.HasValue)
        {
            return;
        }

        if (currentProgramId.HasValue && sourceProgramId.Value == currentProgramId.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình nguồn không được trỏ tới chính chương trình hiện tại.");
        }

        var sourceProgram = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Include(x => x.ChuyenNganh)
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == sourceProgramId.Value, cancellationToken);

        if (sourceProgram is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương trình nguồn.");
        }

        if (!sourceProgram.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình nguồn không hoạt động.");
        }

        var sourceMajorId = sourceProgram.ChuyenNganh?.MaNganh;
        if (sourceProgram.MaChuyenNganh != targetSpecialization.MaChuyenNganh &&
            sourceMajorId.HasValue &&
            sourceMajorId.Value != targetSpecialization.MaNganh &&
            string.IsNullOrWhiteSpace(changeNote))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cần ghi chú thay đổi khi clone từ chương trình khác ngành.");
        }
    }

    private async Task EnsureCodeUniqueAsync(
        string programCode,
        int? excludedProgramId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaCodeChuongTrinh == programCode &&
                (!excludedProgramId.HasValue || x.MaChuongTrinh != excludedProgramId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Mã chương trình đào tạo đã tồn tại.");
        }
    }

    private async Task EnsureProgramUniqueAsync(
        int specializationId,
        int cohortId,
        string version,
        int? excludedProgramId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaChuyenNganh == specializationId &&
                x.MaKhoaTuyenSinh == cohortId &&
                x.Version == version &&
                (!excludedProgramId.HasValue || x.MaChuongTrinh != excludedProgramId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chương trình đào tạo đã tồn tại cho chuyên ngành, khóa tuyển sinh và phiên bản này.");
        }
    }

    private async Task EnsureNoActiveProgramConflictAsync(
        int specializationId,
        int cohortId,
        int? excludedProgramId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaChuyenNganh == specializationId &&
                x.MaKhoaTuyenSinh == cohortId &&
                x.TrangThai == ActiveStatus &&
                (!excludedProgramId.HasValue || x.MaChuongTrinh != excludedProgramId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đã có chương trình active cho chuyên ngành và khóa tuyển sinh này.");
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

    private CurrentUserContext EnsureSuperAdmin()
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được tạo, cập nhật hoặc gửi duyệt chương trình đào tạo.");
        }

        return currentUser;
    }

    private CurrentUserContext EnsureChairman()
    {
        var currentUser = GetCurrentUser();
        if (currentUser.Role != AuthRoles.Chairman)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ Chủ tịch được duyệt, từ chối, kích hoạt hoặc lưu trữ chương trình đào tạo.");
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

    private static bool IsGlobalReader(CurrentUserContext currentUser)
    {
        return currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Chairman;
    }

    private static void EnsureOrganizationInScope(
        HashSet<int> allowedOrganizationIds,
        int organizationId,
        string message)
    {
        if (!allowedOrganizationIds.Contains(organizationId))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, message);
        }
    }

    private static string NormalizeCode(string? value)
    {
        return NormalizeRequiredText(value, "Mã chương trình").ToUpperInvariant();
    }

    private static string NormalizeStatus(string? value, bool useDefaultDraft = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (useDefaultDraft)
            {
                return DraftStatus;
            }

            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái không được để trống.");
        }

        var status = value.Trim().ToLowerInvariant();
        ValidateStatus(status);
        return status;
    }

    private static void ValidateStatus(string status)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái chương trình đào tạo không hợp lệ.");
        }
    }

    private static void ValidateTrainingProgramData(
        int semesterCount,
        int trainingDurationMonths,
        int? requiredCredits,
        int? minCreditsPerTerm,
        int? maxCreditsPerTerm,
        DateOnly? effectiveDate,
        DateOnly? expirationDate)
    {
        if (semesterCount <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số học kỳ phải lớn hơn 0.");
        }

        if (trainingDurationMonths <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thời gian đào tạo phải lớn hơn 0.");
        }

        if (requiredCredits.HasValue && requiredCredits.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tổng tín chỉ yêu cầu phải lớn hơn 0.");
        }

        if (minCreditsPerTerm.HasValue && minCreditsPerTerm.Value < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ tối thiểu mỗi kỳ phải lớn hơn hoặc bằng 0.");
        }

        if (maxCreditsPerTerm.HasValue && maxCreditsPerTerm.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ tối đa mỗi kỳ phải lớn hơn 0.");
        }

        if (minCreditsPerTerm.HasValue &&
            maxCreditsPerTerm.HasValue &&
            minCreditsPerTerm.Value > maxCreditsPerTerm.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tín chỉ tối thiểu mỗi kỳ không được lớn hơn số tín chỉ tối đa.");
        }

        if (effectiveDate.HasValue &&
            expirationDate.HasValue &&
            expirationDate.Value < effectiveDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày hết hiệu lực phải lớn hơn hoặc bằng ngày hiệu lực.");
        }
    }

    private static void ValidateSubmittableProgram(ChuongTrinhDaoTao program)
    {
        if (program.MaChuyenNganh <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình chưa có chuyên ngành.");
        }

        if (program.MaKhoaTuyenSinh <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình chưa có khóa tuyển sinh.");
        }

        NormalizeCode(program.MaCodeChuongTrinh);
        NormalizeRequiredText(program.TenChuongTrinh, "Tên chương trình");
        NormalizeRequiredText(program.Version, "Phiên bản");
        ValidateTrainingProgramData(
            program.SoHocKy,
            program.ThoiGianDaoTaoThang,
            program.TongTinChiYeuCau,
            program.SoTinChiToiThieuMoiKy,
            program.SoTinChiToiDaMoiKy,
            program.NgayHieuLuc,
            program.NgayHetHieuLuc);
    }

    private static string NormalizeRequiredText(string? value, string fieldName)
    {
        var normalizedValue = value?.Trim();
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

    private async Task<Dictionary<int, int>> CloneCourseSyllabusesAsync(
        int sourceProgramId,
        int newProgramId,
        List<MonHocTrongChuongTrinh> distinctSubjects,
        CancellationToken cancellationToken)
    {
        var sourceIds = distinctSubjects
            .Select(x => x.MaChuongTrinhMonHoc)
            .ToHashSet();

        var sourceSyllabuses = await _context.CourseSyllabuses
            .AsNoTracking()
            .Where(x => x.MaChuongTrinhMonHoc.HasValue
                     && sourceIds.Contains(x.MaChuongTrinhMonHoc.Value)
                     && x.ConHoatDong)
            .ToListAsync(cancellationToken);

        if (sourceSyllabuses.Count == 0)
            return new Dictionary<int, int>();

        var newSubjects = await _context.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == newProgramId && x.ConHoatDong)
            .ToListAsync(cancellationToken);

        var oldToNewIdMap = new Dictionary<int, int>();
        foreach (var sourceSubject in distinctSubjects)
        {
            var newSubject = newSubjects.FirstOrDefault(x => x.MaMonHoc == sourceSubject.MaMonHoc);
            if (newSubject != null)
            {
                oldToNewIdMap[sourceSubject.MaChuongTrinhMonHoc] = newSubject.MaChuongTrinhMonHoc;
            }
        }

        foreach (var sourceSyllabus in sourceSyllabuses)
        {
            if (!sourceSyllabus.MaChuongTrinhMonHoc.HasValue) continue;
            if (!oldToNewIdMap.TryGetValue(sourceSyllabus.MaChuongTrinhMonHoc.Value, out var newMaChuongTrinhMonHoc))
                continue;

            _context.CourseSyllabuses.Add(new CourseSyllabus
            {
                MaMonHoc = sourceSyllabus.MaMonHoc,
                MaChuyenNganh = sourceSyllabus.MaChuyenNganh,
                MaDonVi = sourceSyllabus.MaDonVi,
                MaChuongTrinhMonHoc = newMaChuongTrinhMonHoc,
                TenSyllabus = sourceSyllabus.TenSyllabus,
                Version = sourceSyllabus.Version + "-clone-" + newProgramId,
                HocKyDuKien = sourceSyllabus.HocKyDuKien,
                BatBuoc = sourceSyllabus.BatBuoc,
                TrangThai = DraftStatus,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
        }

        return oldToNewIdMap;
    }

    private static TrainingProgramDto ToDto(ChuongTrinhDaoTao program)
    {
        var specialization = program.ChuyenNganh;
        var major = specialization?.NganhDaoTao;
        var cohort = program.KhoaTuyenSinh;

        return new TrainingProgramDto
        {
            MaChuongTrinh = program.MaChuongTrinh,
            MaChuyenNganh = program.MaChuyenNganh,
            MaCodeChuyenNganh = specialization?.MaCodeChuyenNganh ?? string.Empty,
            TenChuyenNganh = specialization?.TenChuyenNganh ?? string.Empty,
            MaNganh = specialization?.MaNganh ?? 0,
            TenNganh = major?.TenNganh ?? string.Empty,
            MaKhoaTuyenSinh = program.MaKhoaTuyenSinh,
            MaCodeKhoa = cohort?.MaCodeKhoa ?? string.Empty,
            TenKhoa = cohort?.TenKhoa ?? string.Empty,
            MaCodeChuongTrinh = program.MaCodeChuongTrinh,
            TenChuongTrinh = program.TenChuongTrinh,
            Version = program.Version,
            SoHocKy = program.SoHocKy,
            ThoiGianDaoTaoThang = program.ThoiGianDaoTaoThang,
            TongTinChiYeuCau = program.TongTinChiYeuCau,
            SoTinChiToiThieuMoiKy = program.SoTinChiToiThieuMoiKy,
            SoTinChiToiDaMoiKy = program.SoTinChiToiDaMoiKy,
            TrangThai = program.TrangThai,
            MoTa = program.MoTa,
            NguonChuongTrinhId = program.NguonChuongTrinhId,
            TenNguonChuongTrinh = program.NguonChuongTrinh?.TenChuongTrinh,
            GhiChuThayDoi = program.GhiChuThayDoi,
            NgayHieuLuc = program.NgayHieuLuc,
            NgayHetHieuLuc = program.NgayHetHieuLuc,
            NguoiGuiDuyetId = program.NguoiGuiDuyetId,
            ThoiGianGuiDuyet = program.ThoiGianGuiDuyet,
            NguoiDuyetId = program.NguoiDuyetId,
            ThoiGianDuyet = program.ThoiGianDuyet,
            GhiChuDuyet = program.GhiChuDuyet,
            NguoiTuChoiId = program.NguoiTuChoiId,
            ThoiGianTuChoi = program.ThoiGianTuChoi,
            LyDoTuChoi = program.LyDoTuChoi,
            ConHoatDong = program.ConHoatDong,
            NgayTao = program.NgayTao,
            NgayCapNhat = program.NgayCapNhat
        };
    }
}

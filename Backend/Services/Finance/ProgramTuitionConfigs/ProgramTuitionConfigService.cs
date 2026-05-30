using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Finance.ProgramTuitionConfigs;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Finance.ProgramTuitionConfigs;

public class ProgramTuitionConfigService : IProgramTuitionConfigService
{
    private const string FixedPerTermCalculationType = "co_dinh_theo_hoc_ky";

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public ProgramTuitionConfigService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<ProgramTuitionConfigListItemDto>> GetAsync(
        ProgramTuitionConfigQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();

        var queryBase =
            from config in _context.CauHinhHocPhiChuongTrinhs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on config.MaDonVi equals organization.MaDonVi
            join program in _context.ChuongTrinhDaoTaos.AsNoTracking()
                on config.MaChuongTrinhDaoTao equals program.MaChuongTrinh
            join term in _context.HocKys.AsNoTracking()
                on config.MaHocKy equals term.MaHocKy
            where allowedOrganizationIdList.Contains(config.MaDonVi)
            select new
            {
                Config = config,
                Organization = organization,
                Program = program,
                Term = term
            };

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            queryBase = queryBase.Where(x =>
                x.Organization.TenDonVi.ToLower().Contains(keyword) ||
                x.Program.MaCodeChuongTrinh.ToLower().Contains(keyword) ||
                x.Program.TenChuongTrinh.ToLower().Contains(keyword) ||
                x.Term.MaCodeHocKy.ToLower().Contains(keyword) ||
                x.Term.TenHocKy.ToLower().Contains(keyword) ||
                (x.Config.GhiChu != null && x.Config.GhiChu.ToLower().Contains(keyword)));
        }

        if (parameters.MaDonVi.HasValue)
        {
            EnsureOrganizationInScope(
                allowedOrganizationIds,
                parameters.MaDonVi.Value,
                "Bạn không có quyền xem cấu hình học phí của đơn vị này.");

            queryBase = queryBase.Where(x => x.Config.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.MaChuongTrinhDaoTao.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.MaChuongTrinhDaoTao == parameters.MaChuongTrinhDaoTao.Value);
        }

        if (parameters.MaHocKy.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.MaHocKy == parameters.MaHocKy.Value);
        }

        if (parameters.NamHocTrongChuongTrinh.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.NamHocTrongChuongTrinh == parameters.NamHocTrongChuongTrinh.Value);
        }

        if (parameters.HocKyTrongNam.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.HocKyTrongNam == parameters.HocKyTrongNam.Value);
        }

        if (parameters.SoThuTuHocKy.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.SoThuTuHocKy == parameters.SoThuTuHocKy.Value);
        }

        if (parameters.ConHoatDong.HasValue)
        {
            queryBase = queryBase.Where(x => x.Config.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalItems = await queryBase.CountAsync(cancellationToken);
        var rows = await queryBase
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Program.TenChuongTrinh)
            .ThenBy(x => x.Config.SoThuTuHocKy)
            .ThenBy(x => x.Term.NgayBatDau)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new
            {
                x.Config.MaCauHinhHocPhi,
                x.Config.MaDonVi,
                x.Organization.TenDonVi,
                x.Config.MaChuongTrinhDaoTao,
                x.Program.MaCodeChuongTrinh,
                x.Program.TenChuongTrinh,
                x.Config.MaHocKy,
                x.Term.MaCodeHocKy,
                x.Term.TenHocKy,
                x.Config.NamHocTrongChuongTrinh,
                x.Config.HocKyTrongNam,
                x.Config.SoThuTuHocKy,
                x.Config.LoaiCachTinhHocPhi,
                x.Config.SoTienHocPhi,
                x.Config.TienHocLieu,
                x.Config.TongTienDuKien,
                x.Config.ConHoatDong,
                x.Term.NgayBatDau,
                x.Term.NgayKetThuc
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x =>
        {
            var editability = GetTermEditability(x.NgayBatDau, x.NgayKetThuc, today);
            return new ProgramTuitionConfigListItemDto
            {
                Id = x.MaCauHinhHocPhi,
                MaDonVi = x.MaDonVi,
                TenDonVi = x.TenDonVi,
                MaChuongTrinhDaoTao = x.MaChuongTrinhDaoTao,
                MaCodeChuongTrinh = x.MaCodeChuongTrinh,
                TenChuongTrinh = x.TenChuongTrinh,
                MaHocKy = x.MaHocKy,
                MaCodeHocKy = x.MaCodeHocKy,
                TenHocKy = x.TenHocKy,
                NamHocTrongChuongTrinh = x.NamHocTrongChuongTrinh,
                HocKyTrongNam = x.HocKyTrongNam,
                SoThuTuHocKy = x.SoThuTuHocKy,
                LoaiCachTinhHocPhi = x.LoaiCachTinhHocPhi,
                SoTienHocPhi = x.SoTienHocPhi,
                TienHocLieu = x.TienHocLieu,
                TongTienDuKien = x.TongTienDuKien,
                ConHoatDong = x.ConHoatDong,
                CoDuocSua = editability.CanEdit,
                LyDoKhongDuocSua = editability.Reason
            };
        }).ToList();

        return new PagedResultDto<ProgramTuitionConfigListItemDto>
        {
            Items = items,
            PageIndex = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ProgramTuitionConfigDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var allowedOrganizationIdList = allowedOrganizationIds.ToList();

        var row = await (
            from config in _context.CauHinhHocPhiChuongTrinhs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on config.MaDonVi equals organization.MaDonVi
            join program in _context.ChuongTrinhDaoTaos.AsNoTracking()
                on config.MaChuongTrinhDaoTao equals program.MaChuongTrinh
            join term in _context.HocKys.AsNoTracking()
                on config.MaHocKy equals term.MaHocKy
            where config.MaCauHinhHocPhi == id && allowedOrganizationIdList.Contains(config.MaDonVi)
            select new
            {
                config.MaCauHinhHocPhi,
                config.MaDonVi,
                organization.TenDonVi,
                config.MaChuongTrinhDaoTao,
                program.MaCodeChuongTrinh,
                program.TenChuongTrinh,
                config.MaHocKy,
                term.MaCodeHocKy,
                term.TenHocKy,
                config.NamHocTrongChuongTrinh,
                config.HocKyTrongNam,
                config.SoThuTuHocKy,
                config.LoaiCachTinhHocPhi,
                config.SoTienHocPhi,
                config.TienHocLieu,
                config.TongTienDuKien,
                config.ConHoatDong,
                config.GhiChu,
                config.NgayTao,
                config.NgayCapNhat,
                term.NgayBatDau,
                term.NgayKetThuc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy cấu hình học phí chương trình.");
        }

        var editability = GetTermEditability(
            row.NgayBatDau,
            row.NgayKetThuc,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return new ProgramTuitionConfigDetailDto
        {
            Id = row.MaCauHinhHocPhi,
            MaDonVi = row.MaDonVi,
            TenDonVi = row.TenDonVi,
            MaChuongTrinhDaoTao = row.MaChuongTrinhDaoTao,
            MaCodeChuongTrinh = row.MaCodeChuongTrinh,
            TenChuongTrinh = row.TenChuongTrinh,
            MaHocKy = row.MaHocKy,
            MaCodeHocKy = row.MaCodeHocKy,
            TenHocKy = row.TenHocKy,
            NamHocTrongChuongTrinh = row.NamHocTrongChuongTrinh,
            HocKyTrongNam = row.HocKyTrongNam,
            SoThuTuHocKy = row.SoThuTuHocKy,
            LoaiCachTinhHocPhi = row.LoaiCachTinhHocPhi,
            SoTienHocPhi = row.SoTienHocPhi,
            TienHocLieu = row.TienHocLieu,
            TongTienDuKien = row.TongTienDuKien,
            ConHoatDong = row.ConHoatDong,
            CoDuocSua = editability.CanEdit,
            LyDoKhongDuocSua = editability.Reason,
            GhiChu = row.GhiChu,
            NgayTao = row.NgayTao,
            NgayCapNhat = row.NgayCapNhat
        };
    }

    public async Task<ProgramTuitionConfigDetailDto> CreateAsync(
        CreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureSuperAdmin();

        var calculationType = NormalizeCalculationType(request.LoaiCachTinhHocPhi);
        ValidateAmounts(request.SoTienHocPhi, request.TienHocLieu);
        ValidateProgramTermPosition(
            request.NamHocTrongChuongTrinh,
            request.HocKyTrongNam,
            request.SoThuTuHocKy);

        await ValidateConfigurationScopeAsync(
            request.MaDonVi,
            request.MaChuongTrinhDaoTao,
            request.MaHocKy,
            request.SoThuTuHocKy,
            cancellationToken);

        await ValidateActiveDuplicateAsync(
            request.MaDonVi,
            request.MaChuongTrinhDaoTao,
            request.MaHocKy,
            null,
            cancellationToken);

        var config = new CauHinhHocPhiChuongTrinh
        {
            MaDonVi = request.MaDonVi,
            MaChuongTrinhDaoTao = request.MaChuongTrinhDaoTao,
            MaHocKy = request.MaHocKy,
            NamHocTrongChuongTrinh = request.NamHocTrongChuongTrinh,
            HocKyTrongNam = request.HocKyTrongNam,
            SoThuTuHocKy = request.SoThuTuHocKy,
            LoaiCachTinhHocPhi = calculationType,
            SoTienHocPhi = request.SoTienHocPhi,
            TienHocLieu = request.TienHocLieu,
            TongTienDuKien = CalculateTotal(request.SoTienHocPhi, request.TienHocLieu),
            ConHoatDong = true,
            GhiChu = NormalizeOptionalText(request.GhiChu),
            NgayTao = DateTime.UtcNow
        };

        _context.CauHinhHocPhiChuongTrinhs.Add(config);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            "ProgramTuitionConfig",
            config.MaCauHinhHocPhi.ToString(),
            "CREATE",
            null,
            await CreateAuditSnapshotAsync(config, cancellationToken),
            currentUser.UserId,
            config.MaDonVi,
            "Tạo cấu hình học phí chương trình đào tạo.",
            cancellationToken);

        return await GetByIdAsync(config.MaCauHinhHocPhi, cancellationToken);
    }

    public async Task<BulkProgramTuitionConfigResultDto> BulkCreateAsync(
        BulkCreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureSuperAdmin();

        var calculationType = NormalizeCalculationType(request.LoaiCachTinhHocPhi);
        var totalTerms = ValidateBulkRequestShape(request);

        foreach (var item in request.Configs)
        {
            ValidateAmounts(item.SoTienHocPhi, item.TienHocLieu);
            ValidateProgramTermPosition(
                item.NamHocTrongChuongTrinh,
                item.HocKyTrongNam,
                item.SoThuTuHocKy,
                request.SoHocKyMoiNam);
        }

        await ValidateTrainingProgramCapacityAsync(
            request.MaDonVi,
            request.MaChuongTrinhDaoTao,
            totalTerms,
            cancellationToken);

        var termIds = request.Configs.Select(x => x.MaHocKy).Distinct().ToList();
        var terms = await _context.HocKys
            .Where(x => termIds.Contains(x.MaHocKy))
            .ToDictionaryAsync(x => x.MaHocKy, cancellationToken);

        foreach (var termId in termIds)
        {
            if (!terms.TryGetValue(termId, out var term))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Học kỳ {termId} không tồn tại.");
            }

            if (term.MaDonVi != request.MaDonVi)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ phải thuộc cùng cơ sở với cấu hình học phí.");
            }
        }

        var existingConfigs = await _context.CauHinhHocPhiChuongTrinhs
            .Where(x =>
                x.ConHoatDong &&
                x.MaDonVi == request.MaDonVi &&
                x.MaChuongTrinhDaoTao == request.MaChuongTrinhDaoTao)
            .ToListAsync(cancellationToken);

        if (existingConfigs.Count > 0 && !request.ConfirmReplace)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Chương trình đào tạo này đã có cấu hình học phí. Vui lòng xác nhận thay thế nếu muốn ghi đè.");
        }

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var createdCount = 0;
        var updatedCount = 0;
        var skippedCount = 0;
        var existingByOrder = existingConfigs
            .GroupBy(x => x.SoThuTuHocKy)
            .ToDictionary(x => x.Key, x => x.First());
        var existingByTerm = existingConfigs
            .GroupBy(x => x.MaHocKy)
            .ToDictionary(x => x.Key, x => x.First());
        var updatedAuditEvents = new List<PendingAuditEvent>();
        var createdConfigs = new List<CauHinhHocPhiChuongTrinh>();

        foreach (var item in request.Configs.OrderBy(x => x.SoThuTuHocKy))
        {
            existingByOrder.TryGetValue(item.SoThuTuHocKy, out var existingConfig);

            if (existingConfig is null &&
                existingByTerm.TryGetValue(item.MaHocKy, out var existingWithSameTerm))
            {
                existingConfig = existingWithSameTerm;
            }

            if (existingConfig is not null)
            {
                var existingTerm = terms.TryGetValue(existingConfig.MaHocKy, out var loadedExistingTerm)
                    ? loadedExistingTerm
                    : await _context.HocKys.FirstAsync(x => x.MaHocKy == existingConfig.MaHocKy, cancellationToken);
                var editability = GetTermEditability(
                    existingTerm.NgayBatDau,
                    existingTerm.NgayKetThuc,
                    today);

                if (!editability.CanEdit)
                {
                    skippedCount++;
                    continue;
                }

                var oldValue = await CreateAuditSnapshotAsync(existingConfig, cancellationToken);
                ApplyConfigValues(
                    existingConfig,
                    request.MaDonVi,
                    request.MaChuongTrinhDaoTao,
                    item.MaHocKy,
                    item.NamHocTrongChuongTrinh,
                    item.HocKyTrongNam,
                    item.SoThuTuHocKy,
                    calculationType,
                    item.SoTienHocPhi,
                    item.TienHocLieu,
                    item.GhiChu,
                    true,
                    now);
                updatedAuditEvents.Add(new PendingAuditEvent(
                    existingConfig,
                    oldValue,
                    await CreateAuditSnapshotAsync(existingConfig, cancellationToken)));
                updatedCount++;
                continue;
            }

            var config = new CauHinhHocPhiChuongTrinh
            {
                NgayTao = now
            };

            ApplyConfigValues(
                config,
                request.MaDonVi,
                request.MaChuongTrinhDaoTao,
                item.MaHocKy,
                item.NamHocTrongChuongTrinh,
                item.HocKyTrongNam,
                item.SoThuTuHocKy,
                calculationType,
                item.SoTienHocPhi,
                item.TienHocLieu,
                item.GhiChu,
                true,
                now);

            _context.CauHinhHocPhiChuongTrinhs.Add(config);
            createdConfigs.Add(config);
            createdCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var auditEvent in updatedAuditEvents)
        {
            await _auditLogService.LogAsync(
                "ProgramTuitionConfig",
                auditEvent.Config.MaCauHinhHocPhi.ToString(),
                "UPDATE",
                auditEvent.OldValue,
                auditEvent.NewValue,
                currentUser.UserId,
                auditEvent.Config.MaDonVi,
                "Thay thế cấu hình học phí chương trình đào tạo trong thao tác hàng loạt.",
                cancellationToken);
        }

        foreach (var config in createdConfigs)
        {
            await _auditLogService.LogAsync(
                "ProgramTuitionConfig",
                config.MaCauHinhHocPhi.ToString(),
                "CREATE",
                null,
                await CreateAuditSnapshotAsync(config, cancellationToken),
                currentUser.UserId,
                config.MaDonVi,
                "Tạo cấu hình học phí chương trình đào tạo trong thao tác hàng loạt.",
                cancellationToken);
        }

        await _auditLogService.LogAsync(
            "ProgramTuitionConfig",
            request.MaChuongTrinhDaoTao.ToString(),
            request.ConfirmReplace ? "BULK_REPLACE" : "BULK_CREATE",
            null,
            new
            {
                request.MaDonVi,
                request.MaChuongTrinhDaoTao,
                request.SoNamDaoTao,
                request.SoHocKyMoiNam,
                TongSoDongYeuCau = request.Configs.Count,
                SoDongTaoMoi = createdCount,
                SoDongThayTheCapNhat = updatedCount,
                SoDongBoQua = skippedCount
            },
            currentUser.UserId,
            request.MaDonVi,
            request.ConfirmReplace
                ? "Thay thế hàng loạt cấu hình học phí chương trình đào tạo."
                : "Tạo hàng loạt cấu hình học phí chương trình đào tạo.",
            cancellationToken);

        var items = await GetDetailsByProgramAsync(
            request.MaDonVi,
            request.MaChuongTrinhDaoTao,
            cancellationToken);

        return new BulkProgramTuitionConfigResultDto
        {
            TongSoDongYeuCau = request.Configs.Count,
            SoDongTaoMoi = createdCount,
            SoDongThayTheCapNhat = updatedCount,
            SoDongBoQua = skippedCount,
            Items = items
        };
    }

    public async Task<ProgramTuitionConfigDetailDto> UpdateAsync(
        int id,
        UpdateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureSuperAdmin();

        var config = await GetManagedConfigAsync(id, cancellationToken);
        await EnsureConfigCanBeChangedAsync(config, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(config, cancellationToken);
        var calculationType = NormalizeCalculationType(request.LoaiCachTinhHocPhi);
        ValidateAmounts(request.SoTienHocPhi, request.TienHocLieu);
        ValidateProgramTermPosition(
            request.NamHocTrongChuongTrinh,
            request.HocKyTrongNam,
            request.SoThuTuHocKy,
            3);

        await ValidateConfigurationScopeAsync(
            request.MaDonVi,
            request.MaChuongTrinhDaoTao,
            request.MaHocKy,
            request.SoThuTuHocKy,
            cancellationToken);

        if (request.ConHoatDong)
        {
            await ValidateActiveDuplicateAsync(
                request.MaDonVi,
                request.MaChuongTrinhDaoTao,
                request.MaHocKy,
                id,
                cancellationToken);
        }

        config.MaDonVi = request.MaDonVi;
        config.MaChuongTrinhDaoTao = request.MaChuongTrinhDaoTao;
        config.MaHocKy = request.MaHocKy;
        config.NamHocTrongChuongTrinh = request.NamHocTrongChuongTrinh;
        config.HocKyTrongNam = request.HocKyTrongNam;
        config.SoThuTuHocKy = request.SoThuTuHocKy;
        config.LoaiCachTinhHocPhi = calculationType;
        config.SoTienHocPhi = request.SoTienHocPhi;
        config.TienHocLieu = request.TienHocLieu;
        config.TongTienDuKien = CalculateTotal(request.SoTienHocPhi, request.TienHocLieu);
        config.ConHoatDong = request.ConHoatDong;
        config.GhiChu = NormalizeOptionalText(request.GhiChu);
        config.NgayCapNhat = DateTime.UtcNow;
        var newValue = await CreateAuditSnapshotAsync(config, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            "ProgramTuitionConfig",
            config.MaCauHinhHocPhi.ToString(),
            "UPDATE",
            oldValue,
            newValue,
            currentUser.UserId,
            config.MaDonVi,
            "Cập nhật cấu hình học phí chương trình đào tạo.",
            cancellationToken);

        return await GetByIdAsync(config.MaCauHinhHocPhi, cancellationToken);
    }

    public async Task<ProgramTuitionConfigDetailDto> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureSuperAdmin();

        var config = await GetManagedConfigAsync(id, cancellationToken);
        await EnsureConfigCanBeChangedAsync(config, cancellationToken);
        var oldValue = await CreateAuditSnapshotAsync(config, cancellationToken);
        config.ConHoatDong = false;
        config.NgayCapNhat = DateTime.UtcNow;
        var newValue = await CreateAuditSnapshotAsync(config, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            "ProgramTuitionConfig",
            config.MaCauHinhHocPhi.ToString(),
            "DEACTIVATE",
            oldValue,
            newValue,
            currentUser.UserId,
            config.MaDonVi,
            "Vô hiệu hóa cấu hình học phí chương trình đào tạo.",
            cancellationToken);

        return await GetByIdAsync(config.MaCauHinhHocPhi, cancellationToken);
    }

    private async Task<CauHinhHocPhiChuongTrinh> GetManagedConfigAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var config = await _context.CauHinhHocPhiChuongTrinhs
            .FirstOrDefaultAsync(x => x.MaCauHinhHocPhi == id, cancellationToken);

        if (config is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy cấu hình học phí chương trình.");
        }

        return config;
    }

    private async Task EnsureConfigCanBeChangedAsync(
        CauHinhHocPhiChuongTrinh config,
        CancellationToken cancellationToken)
    {
        var term = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == config.MaHocKy, cancellationToken);

        if (term is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại.");
        }

        var editability = GetTermEditability(
            term.NgayBatDau,
            term.NgayKetThuc,
            DateOnly.FromDateTime(DateTime.UtcNow));

        if (!editability.CanEdit)
        {
            throw new ApiException(StatusCodes.Status409Conflict, editability.Reason!);
        }
    }

    private async Task<IReadOnlyList<ProgramTuitionConfigDetailDto>> GetDetailsByProgramAsync(
        int organizationId,
        int trainingProgramId,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from config in _context.CauHinhHocPhiChuongTrinhs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on config.MaDonVi equals organization.MaDonVi
            join program in _context.ChuongTrinhDaoTaos.AsNoTracking()
                on config.MaChuongTrinhDaoTao equals program.MaChuongTrinh
            join term in _context.HocKys.AsNoTracking()
                on config.MaHocKy equals term.MaHocKy
            where config.ConHoatDong &&
                  config.MaDonVi == organizationId &&
                  config.MaChuongTrinhDaoTao == trainingProgramId
            orderby config.SoThuTuHocKy, term.NgayBatDau
            select new
            {
                config.MaCauHinhHocPhi,
                config.MaDonVi,
                organization.TenDonVi,
                config.MaChuongTrinhDaoTao,
                program.MaCodeChuongTrinh,
                program.TenChuongTrinh,
                config.MaHocKy,
                term.MaCodeHocKy,
                term.TenHocKy,
                config.NamHocTrongChuongTrinh,
                config.HocKyTrongNam,
                config.SoThuTuHocKy,
                config.LoaiCachTinhHocPhi,
                config.SoTienHocPhi,
                config.TienHocLieu,
                config.TongTienDuKien,
                config.ConHoatDong,
                config.GhiChu,
                config.NgayTao,
                config.NgayCapNhat,
                term.NgayBatDau,
                term.NgayKetThuc
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return rows.Select(x =>
        {
            var editability = GetTermEditability(x.NgayBatDau, x.NgayKetThuc, today);
            return new ProgramTuitionConfigDetailDto
            {
                Id = x.MaCauHinhHocPhi,
                MaDonVi = x.MaDonVi,
                TenDonVi = x.TenDonVi,
                MaChuongTrinhDaoTao = x.MaChuongTrinhDaoTao,
                MaCodeChuongTrinh = x.MaCodeChuongTrinh,
                TenChuongTrinh = x.TenChuongTrinh,
                MaHocKy = x.MaHocKy,
                MaCodeHocKy = x.MaCodeHocKy,
                TenHocKy = x.TenHocKy,
                NamHocTrongChuongTrinh = x.NamHocTrongChuongTrinh,
                HocKyTrongNam = x.HocKyTrongNam,
                SoThuTuHocKy = x.SoThuTuHocKy,
                LoaiCachTinhHocPhi = x.LoaiCachTinhHocPhi,
                SoTienHocPhi = x.SoTienHocPhi,
                TienHocLieu = x.TienHocLieu,
                TongTienDuKien = x.TongTienDuKien,
                ConHoatDong = x.ConHoatDong,
                CoDuocSua = editability.CanEdit,
                LyDoKhongDuocSua = editability.Reason,
                GhiChu = x.GhiChu,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat
            };
        }).ToList();
    }

    private async Task<object> CreateAuditSnapshotAsync(
        CauHinhHocPhiChuongTrinh config,
        CancellationToken cancellationToken)
    {
        var organizationName = await _context.DonVis
            .AsNoTracking()
            .Where(x => x.MaDonVi == config.MaDonVi)
            .Select(x => x.TenDonVi)
            .FirstOrDefaultAsync(cancellationToken);
        var program = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == config.MaChuongTrinhDaoTao)
            .Select(x => new { x.MaCodeChuongTrinh, x.TenChuongTrinh })
            .FirstOrDefaultAsync(cancellationToken);
        var term = await _context.HocKys
            .AsNoTracking()
            .Where(x => x.MaHocKy == config.MaHocKy)
            .Select(x => new { x.MaCodeHocKy, x.TenHocKy, x.NgayBatDau, x.NgayKetThuc })
            .FirstOrDefaultAsync(cancellationToken);

        return new
        {
            Id = config.MaCauHinhHocPhi,
            config.MaDonVi,
            TenDonVi = organizationName,
            config.MaChuongTrinhDaoTao,
            program?.MaCodeChuongTrinh,
            program?.TenChuongTrinh,
            config.MaHocKy,
            term?.MaCodeHocKy,
            term?.TenHocKy,
            term?.NgayBatDau,
            term?.NgayKetThuc,
            config.NamHocTrongChuongTrinh,
            config.HocKyTrongNam,
            config.SoThuTuHocKy,
            config.LoaiCachTinhHocPhi,
            config.SoTienHocPhi,
            config.TienHocLieu,
            config.TongTienDuKien,
            config.ConHoatDong,
            config.GhiChu,
            config.NgayTao,
            config.NgayCapNhat
        };
    }

    private async Task ValidateTrainingProgramCapacityAsync(
        int organizationId,
        int trainingProgramId,
        int totalTerms,
        CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);

        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở không tồn tại hoặc không hoạt động.");
        }

        if (organization.CapDonVi == "root")
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình học phí phải gắn với cơ sở hoặc cơ sở con, không gắn với đơn vị root.");
        }

        var program = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == trainingProgramId, cancellationToken);

        if (program is null || !program.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình đào tạo không tồn tại hoặc không hoạt động.");
        }

        if (totalTerms > program.SoHocKy)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Tổng số học kỳ cấu hình không được vượt quá {program.SoHocKy} học kỳ của chương trình đào tạo.");
        }
    }

    private async Task ValidateConfigurationScopeAsync(
        int organizationId,
        int trainingProgramId,
        int termId,
        int programTermOrder,
        CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);

        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở không tồn tại hoặc không hoạt động.");
        }

        if (organization.CapDonVi == "root")
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình học phí phải gắn với cơ sở hoặc cơ sở con, không gắn với đơn vị root.");
        }

        var program = await _context.ChuongTrinhDaoTaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == trainingProgramId, cancellationToken);

        if (program is null || !program.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chương trình đào tạo không tồn tại hoặc không hoạt động.");
        }

        if (programTermOrder > program.SoHocKy)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Số thứ tự học kỳ phải từ 1 đến {program.SoHocKy} theo chương trình đào tạo.");
        }

        var term = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == termId, cancellationToken);

        if (term is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại.");
        }

        if (term.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ phải thuộc cùng cơ sở với cấu hình học phí.");
        }

        var mappedProgramTermOrder = await _context.ChuongTrinhHocKys
            .AsNoTracking()
            .Where(x => x.MaChuongTrinh == trainingProgramId && x.MaHocKy == termId)
            .Select(x => (int?)x.ThuTuHocKy)
            .FirstOrDefaultAsync(cancellationToken);

        if (mappedProgramTermOrder.HasValue && mappedProgramTermOrder.Value != programTermOrder)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Số thứ tự học kỳ trong chương trình không khớp với mapping học kỳ đã có.");
        }
    }

    private async Task ValidateActiveDuplicateAsync(
        int organizationId,
        int trainingProgramId,
        int termId,
        int? excludedConfigId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.CauHinhHocPhiChuongTrinhs
            .AsNoTracking()
            .AnyAsync(x =>
                x.ConHoatDong &&
                x.MaDonVi == organizationId &&
                x.MaChuongTrinhDaoTao == trainingProgramId &&
                x.MaHocKy == termId &&
                (!excludedConfigId.HasValue || x.MaCauHinhHocPhi != excludedConfigId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Đã tồn tại cấu hình học phí đang hoạt động cho cơ sở, chương trình và học kỳ này.");
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
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý cấu hình học phí chương trình.");
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

    private static string NormalizeCalculationType(string? value)
    {
        var calculationType = string.IsNullOrWhiteSpace(value)
            ? FixedPerTermCalculationType
            : value.Trim();

        if (calculationType != FixedPerTermCalculationType)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "MVP hiện chỉ hỗ trợ cách tính học phí co_dinh_theo_hoc_ky.");
        }

        return calculationType;
    }

    private static void ValidateAmounts(decimal tuitionAmount, decimal materialAmount)
    {
        if (tuitionAmount < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số tiền học phí phải lớn hơn hoặc bằng 0.");
        }

        if (materialAmount < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tiền học liệu phải lớn hơn hoặc bằng 0.");
        }
    }

    private static void ValidateProgramTermPosition(
        int yearInProgram,
        int termInYear,
        int programTermOrder,
        int maxTermsPerYear = 3)
    {
        if (yearInProgram < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Năm học trong chương trình phải lớn hơn hoặc bằng 1.");
        }

        if (termInYear < 1 || termInYear > maxTermsPerYear)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Học kỳ trong năm phải từ 1 đến {maxTermsPerYear}.");
        }

        if (programTermOrder < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số thứ tự học kỳ phải lớn hơn hoặc bằng 1.");
        }
    }

    private static int ValidateBulkRequestShape(BulkCreateProgramTuitionConfigRequest request)
    {
        if (request.Configs.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách cấu hình học phí không được rỗng.");
        }

        if (request.SoNamDaoTao < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số năm đào tạo phải lớn hơn hoặc bằng 1.");
        }

        if (request.SoHocKyMoiNam is < 1 or > 3)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số học kỳ mỗi năm trong MVP chỉ được từ 1 đến 3.");
        }

        var totalTerms = request.SoNamDaoTao * request.SoHocKyMoiNam;
        if (request.Configs.Count != totalTerms)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Số lượng cấu hình phải bằng {request.SoNamDaoTao} x {request.SoHocKyMoiNam} = {totalTerms} dòng.");
        }

        var duplicatedOrder = request.Configs
            .GroupBy(x => x.SoThuTuHocKy)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicatedOrder is not null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không được trùng số thứ tự học kỳ {duplicatedOrder.Key} trong cùng request.");
        }

        var duplicatedYearTerm = request.Configs
            .GroupBy(x => new { x.NamHocTrongChuongTrinh, x.HocKyTrongNam })
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicatedYearTerm is not null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không được trùng năm {duplicatedYearTerm.Key.NamHocTrongChuongTrinh}, học kỳ {duplicatedYearTerm.Key.HocKyTrongNam} trong cùng request.");
        }

        var duplicatedTerm = request.Configs
            .GroupBy(x => x.MaHocKy)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicatedTerm is not null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không được trùng mã học kỳ {duplicatedTerm.Key} trong cùng request.");
        }

        foreach (var item in request.Configs)
        {
            if (item.NamHocTrongChuongTrinh > request.SoNamDaoTao)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Năm học trong chương trình vượt quá số năm đào tạo.");
            }

            if (item.SoThuTuHocKy > totalTerms)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Số thứ tự học kỳ vượt quá tổng số học kỳ của chương trình.");
            }
        }

        return totalTerms;
    }

    private static void ApplyConfigValues(
        CauHinhHocPhiChuongTrinh config,
        int organizationId,
        int trainingProgramId,
        int termId,
        int yearInProgram,
        int termInYear,
        int programTermOrder,
        string calculationType,
        decimal tuitionAmount,
        decimal materialAmount,
        string? note,
        bool isActive,
        DateTime updatedAt)
    {
        config.MaDonVi = organizationId;
        config.MaChuongTrinhDaoTao = trainingProgramId;
        config.MaHocKy = termId;
        config.NamHocTrongChuongTrinh = yearInProgram;
        config.HocKyTrongNam = termInYear;
        config.SoThuTuHocKy = programTermOrder;
        config.LoaiCachTinhHocPhi = calculationType;
        config.SoTienHocPhi = tuitionAmount;
        config.TienHocLieu = materialAmount;
        config.TongTienDuKien = CalculateTotal(tuitionAmount, materialAmount);
        config.ConHoatDong = isActive;
        config.GhiChu = NormalizeOptionalText(note);
        config.NgayCapNhat = updatedAt;
    }

    private static (bool CanEdit, string? Reason) GetTermEditability(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly today)
    {
        if (today < startDate)
        {
            return (true, null);
        }

        if (today > endDate)
        {
            return (false, "Không thể sửa học phí của học kỳ đã kết thúc.");
        }

        return (false, "Không thể sửa học phí của học kỳ đang diễn ra.");
    }

    private static decimal CalculateTotal(decimal tuitionAmount, decimal materialAmount)
    {
        return tuitionAmount + materialAmount;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record PendingAuditEvent(
        CauHinhHocPhiChuongTrinh Config,
        object OldValue,
        object NewValue);

}

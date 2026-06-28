using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class RewardCampaignService : IRewardCampaignService
{
    private const string EntityType = "DotKhenThuong";
    private const string CreateAction = "CREATE_REWARD_CAMPAIGN";
    private const string UpdateAction = "UPDATE_REWARD_CAMPAIGN";
    private const string CancelAction = "CANCEL_REWARD_CAMPAIGN";

    private static readonly string[] ActiveStatuses =
    [
        RewardDisciplineConstants.RewardCampaignStatuses.Draft,
        RewardDisciplineConstants.RewardCampaignStatuses.Evaluating,
        RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval,
        RewardDisciplineConstants.RewardCampaignStatuses.Approved,
        RewardDisciplineConstants.RewardCampaignStatuses.Published
    ];

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public RewardCampaignService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<RewardCampaignListItemDto>> GetAsync(
        RewardCampaignQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanRead(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var pageIndex = Math.Max(1, parameters.PageIndex);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var query = ApplyReadScope(_context.DotKhenThuongs.AsNoTracking(), currentUser, allowedOrganizationIds);

        if (parameters.MaHocKy.HasValue)
        {
            query = query.Where(x => x.MaHocKy == parameters.MaHocKy.Value);
        }

        if (parameters.MaDonVi.HasValue)
        {
            EnsureOrganizationInReadScope(currentUser, allowedOrganizationIds, parameters.MaDonVi.Value);
            query = query.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiDot))
        {
            var type = NormalizeCampaignType(parameters.LoaiDot);
            query = query.Where(x => x.LoaiDot == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.TrangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.TenDot.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await CreateDetailQuery(query)
            .OrderByDescending(x => x.Campaign.NgayTao)
            .ThenByDescending(x => x.Campaign.MaDotKhenThuong)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<RewardCampaignListItemDto>
        {
            Items = rows.Select(ToListItemDto).ToList(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<RewardCampaignDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanRead(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var query = ApplyReadScope(_context.DotKhenThuongs.AsNoTracking(), currentUser, allowedOrganizationIds)
            .Where(x => x.MaDotKhenThuong == id);

        var row = await CreateDetailQuery(query).FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt khen thưởng.");
        }

        return ToDetailDto(row);
    }

    public async Task<RewardCampaignDetailDto> CreateTop100Async(
        CreateTop100RewardCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var term = await ValidateTermAsync(request.MaHocKy, request.MaDonVi, cancellationToken);
        await ValidateOrganizationAsync(request.MaDonVi, cancellationToken);
        await ValidateCertificateTemplateAsync(request.MaMauBangKhen, cancellationToken);
        var criteriaJson = NormalizeCriteriaJson(request.TieuChiXetJson);
        var tenDot = NormalizeRequiredText(request.TenDot, "Tên đợt");
        var soLuongToiDa = request.SoLuongToiDa ?? 100;
        ValidateMaxQuantity(soLuongToiDa);
        await EnsureNoDuplicateActiveAsync(term.MaHocKy, request.MaDonVi, null, cancellationToken);

        var campaign = new DotKhenThuong
        {
            MaHocKy = term.MaHocKy,
            MaDonVi = request.MaDonVi,
            TenDot = tenDot,
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = soLuongToiDa,
            TieuChiXetJson = criteriaJson,
            MaMauBangKhen = request.MaMauBangKhen,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Draft,
            NguoiTao = currentUser.UserId,
            NgayTao = DateTime.UtcNow,
            GhiChu = NormalizeOptionalText(request.GhiChu)
        };

        _context.DotKhenThuongs.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            campaign.MaDotKhenThuong.ToString(),
            CreateAction,
            null,
            CreateAuditSnapshot(campaign),
            currentUser.UserId,
            campaign.MaDonVi,
            "Tạo đợt khen thưởng Top 100 học kỳ.",
            cancellationToken);

        return await LoadDetailAsync(campaign.MaDotKhenThuong, cancellationToken);
    }

    public async Task<RewardCampaignDetailDto> UpdateAsync(
        int id,
        UpdateRewardCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var campaign = await _context.DotKhenThuongs.FirstOrDefaultAsync(x => x.MaDotKhenThuong == id, cancellationToken);
        if (campaign is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt khen thưởng.");
        }

        if (!campaign.TrangThai.Equals(RewardDisciplineConstants.RewardCampaignStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được cập nhật đợt khen thưởng đang ở trạng thái nháp.");
        }

        var oldSnapshot = CreateAuditSnapshot(campaign);
        var term = await ValidateTermAsync(request.MaHocKy, request.MaDonVi, cancellationToken);
        await ValidateOrganizationAsync(request.MaDonVi, cancellationToken);
        await ValidateCertificateTemplateAsync(request.MaMauBangKhen, cancellationToken);
        var criteriaJson = NormalizeCriteriaJson(request.TieuChiXetJson);
        var tenDot = NormalizeRequiredText(request.TenDot, "Tên đợt");
        ValidateMaxQuantity(request.SoLuongToiDa);
        await EnsureNoDuplicateActiveAsync(term.MaHocKy, request.MaDonVi, id, cancellationToken);

        campaign.MaHocKy = term.MaHocKy;
        campaign.MaDonVi = request.MaDonVi;
        campaign.TenDot = tenDot;
        campaign.SoLuongToiDa = request.SoLuongToiDa;
        campaign.TieuChiXetJson = criteriaJson;
        campaign.MaMauBangKhen = request.MaMauBangKhen;
        campaign.GhiChu = NormalizeOptionalText(request.GhiChu);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            campaign.MaDotKhenThuong.ToString(),
            UpdateAction,
            oldSnapshot,
            CreateAuditSnapshot(campaign),
            currentUser.UserId,
            campaign.MaDonVi,
            "Cập nhật đợt khen thưởng Top 100 học kỳ.",
            cancellationToken);

        return await LoadDetailAsync(campaign.MaDotKhenThuong, cancellationToken);
    }

    public async Task<RewardCampaignDetailDto> CancelAsync(
        int id,
        CancelRewardCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var reason = NormalizeRequiredText(request.LyDoHuy ?? request.GhiChu ?? string.Empty, "Lý do hủy");
        var campaign = await _context.DotKhenThuongs.FirstOrDefaultAsync(x => x.MaDotKhenThuong == id, cancellationToken);
        if (campaign is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt khen thưởng.");
        }

        if (campaign.TrangThai.Equals(RewardDisciplineConstants.RewardCampaignStatuses.Published, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không được hủy đợt khen thưởng đã công bố.");
        }

        if (campaign.TrangThai.Equals(RewardDisciplineConstants.RewardCampaignStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đợt khen thưởng đã bị hủy.");
        }

        var oldSnapshot = CreateAuditSnapshot(campaign);
        campaign.TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Cancelled;
        campaign.GhiChu = AppendCancelReason(campaign.GhiChu, reason);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(
            EntityType,
            campaign.MaDotKhenThuong.ToString(),
            CancelAction,
            oldSnapshot,
            CreateAuditSnapshot(campaign),
            currentUser.UserId,
            campaign.MaDonVi,
            "Hủy đợt khen thưởng Top 100 học kỳ.",
            cancellationToken);

        return await LoadDetailAsync(campaign.MaDotKhenThuong, cancellationToken);
    }

    private async Task<RewardCampaignDetailDto> LoadDetailAsync(int id, CancellationToken cancellationToken)
    {
        var row = await CreateDetailQuery(_context.DotKhenThuongs.AsNoTracking().Where(x => x.MaDotKhenThuong == id))
            .FirstAsync(cancellationToken);
        return ToDetailDto(row);
    }

    private IQueryable<DotKhenThuong> ApplyReadScope(
        IQueryable<DotKhenThuong> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return query;
        }

        var allowedOrganizationIdList = allowedOrganizationIds.ToList();
        return query.Where(x => x.MaDonVi.HasValue && allowedOrganizationIdList.Contains(x.MaDonVi.Value));
    }

    private IQueryable<RewardCampaignQueryRow> CreateDetailQuery(IQueryable<DotKhenThuong> campaigns)
    {
        return
            from campaign in campaigns
            join term in _context.HocKys.AsNoTracking()
                on campaign.MaHocKy equals term.MaHocKy
            join organization in _context.DonVis.AsNoTracking()
                on campaign.MaDonVi equals organization.MaDonVi into organizationGroup
            from organization in organizationGroup.DefaultIfEmpty()
            join template in _context.MauBangKhens.AsNoTracking()
                on campaign.MaMauBangKhen equals template.MaMauBangKhen into templateGroup
            from template in templateGroup.DefaultIfEmpty()
            join creator in _context.NguoiDungs.AsNoTracking()
                on campaign.NguoiTao equals creator.MaNguoiDung into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            join approver in _context.NguoiDungs.AsNoTracking()
                on campaign.NguoiDuyet equals approver.MaNguoiDung into approverGroup
            from approver in approverGroup.DefaultIfEmpty()
            select new RewardCampaignQueryRow
            {
                Campaign = campaign,
                Term = term,
                Organization = organization,
                Template = template,
                CreatorName = creator != null ? creator.HoTen : null,
                ApproverName = approver != null ? approver.HoTen : null
            };
    }

    private async Task<HocKy> ValidateTermAsync(int maHocKy, int? maDonVi, CancellationToken cancellationToken)
    {
        var term = await _context.HocKys.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == maHocKy, cancellationToken);
        if (term is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại.");
        }

        if (maDonVi.HasValue && term.MaDonVi != maDonVi.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không thuộc cơ sở đã chọn.");
        }

        return term;
    }

    private async Task ValidateOrganizationAsync(int? maDonVi, CancellationToken cancellationToken)
    {
        if (!maDonVi.HasValue)
        {
            return;
        }

        var exists = await _context.DonVis.AsNoTracking()
            .AnyAsync(x => x.MaDonVi == maDonVi.Value && x.ConHoatDong, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không tồn tại hoặc đã ngưng hoạt động.");
        }
    }

    private async Task ValidateCertificateTemplateAsync(int? maMauBangKhen, CancellationToken cancellationToken)
    {
        if (!maMauBangKhen.HasValue)
        {
            return;
        }

        var valid = await _context.MauBangKhens.AsNoTracking()
            .AnyAsync(x =>
                x.MaMauBangKhen == maMauBangKhen.Value &&
                x.ConHoatDong &&
                x.LoaiMau == RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
                cancellationToken);
        if (!valid)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mẫu bằng khen không tồn tại, không hoạt động hoặc không đúng loại Top 100 học kỳ.");
        }
    }

    private async Task EnsureNoDuplicateActiveAsync(
        int maHocKy,
        int? maDonVi,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var duplicate = await _context.DotKhenThuongs.AsNoTracking()
            .AnyAsync(x =>
                x.MaHocKy == maHocKy &&
                x.MaDonVi == maDonVi &&
                x.LoaiDot == RewardDisciplineConstants.RewardCampaignTypes.Top100Semester &&
                ActiveStatuses.Contains(x.TrangThai) &&
                (!excludeId.HasValue || x.MaDotKhenThuong != excludeId.Value),
                cancellationToken);

        if (duplicate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đã có đợt Top 100 học kỳ đang hoạt động cho học kỳ và cơ sở này.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _context.DonVis.AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.Admin)
        {
            return await _context.DonVis.AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        var organizations = await _context.DonVis.AsNoTracking()
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

    private static string NormalizeCriteriaJson(JsonElement? json)
    {
        if (!json.HasValue || json.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null!;
        }

        if (json.Value.ValueKind == JsonValueKind.Object)
        {
            return json.Value.GetRawText();
        }

        if (json.Value.ValueKind == JsonValueKind.String)
        {
            var raw = json.Value.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null!;
            }

            try
            {
                using var document = JsonDocument.Parse(raw);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Tiêu chí xét phải là JSON object.");
                }

                return document.RootElement.GetRawText();
            }
            catch (JsonException)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Tiêu chí xét không phải JSON hợp lệ.");
            }
        }

        throw new ApiException(StatusCodes.Status400BadRequest, "Tiêu chí xét phải là JSON object.");
    }

    private static string NormalizeCampaignType(string value)
    {
        var normalized = value.Trim();
        if (!normalized.Equals(RewardDisciplineConstants.RewardCampaignTypes.Top100Semester, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đợt khen thưởng không hợp lệ.");
        }

        return RewardDisciplineConstants.RewardCampaignTypes.Top100Semester;
    }

    private static string NormalizeStatus(string value)
    {
        var normalized = value.Trim();
        var canonical = RewardDisciplineConstants.RewardCampaignStatuses.All
            .FirstOrDefault(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái đợt khen thưởng không hợp lệ.");
        }

        return canonical;
    }

    private static void ValidateMaxQuantity(int value)
    {
        if (value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số lượng tối đa phải lớn hơn 0.");
        }
    }

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string AppendCancelReason(string? currentNote, string reason)
    {
        var cancelNote = $"[Hủy] {reason}";
        return string.IsNullOrWhiteSpace(currentNote)
            ? cancelNote
            : $"{currentNote.Trim()}{Environment.NewLine}{cancelNote}";
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        return currentUser;
    }

    private static void EnsureCanRead(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng.");
        }
    }

    private static void EnsureSuperAdmin(CurrentUserContext currentUser)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quản lý đợt khen thưởng Top 100.");
        }
    }

    private static void EnsureOrganizationInReadScope(
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds,
        int maDonVi)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin && !allowedOrganizationIds.Contains(maDonVi))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng của đơn vị này.");
        }
    }

    private static object CreateAuditSnapshot(DotKhenThuong campaign)
    {
        return new
        {
            campaign.MaDotKhenThuong,
            campaign.MaHocKy,
            campaign.MaDonVi,
            campaign.TenDot,
            campaign.LoaiDot,
            campaign.SoLuongToiDa,
            campaign.MaMauBangKhen,
            campaign.TrangThai,
            campaign.NguoiTao,
            campaign.NgayTao,
            campaign.NguoiDuyet,
            campaign.NgayDuyet,
            campaign.NgayCongBo,
            HasTieuChiXetJson = !string.IsNullOrWhiteSpace(campaign.TieuChiXetJson),
            TieuChiXetJsonLength = campaign.TieuChiXetJson?.Length ?? 0,
            campaign.GhiChu
        };
    }

    private static RewardCampaignListItemDto ToListItemDto(RewardCampaignQueryRow row)
    {
        return new RewardCampaignListItemDto
        {
            MaDotKhenThuong = row.Campaign.MaDotKhenThuong,
            MaHocKy = row.Campaign.MaHocKy,
            TenHocKy = row.Term.TenHocKy,
            MaDonVi = row.Campaign.MaDonVi,
            TenDonVi = row.Organization?.TenDonVi,
            TenDot = row.Campaign.TenDot,
            LoaiDot = row.Campaign.LoaiDot,
            SoLuongToiDa = row.Campaign.SoLuongToiDa,
            MaMauBangKhen = row.Campaign.MaMauBangKhen,
            TenMauBangKhen = row.Template?.TenMau,
            TrangThai = row.Campaign.TrangThai,
            NguoiTao = row.Campaign.NguoiTao,
            TenNguoiTao = row.CreatorName,
            NgayTao = row.Campaign.NgayTao
        };
    }

    private static RewardCampaignDetailDto ToDetailDto(RewardCampaignQueryRow row)
    {
        var dto = new RewardCampaignDetailDto
        {
            MaDotKhenThuong = row.Campaign.MaDotKhenThuong,
            MaHocKy = row.Campaign.MaHocKy,
            TenHocKy = row.Term.TenHocKy,
            MaDonVi = row.Campaign.MaDonVi,
            TenDonVi = row.Organization?.TenDonVi,
            TenDot = row.Campaign.TenDot,
            LoaiDot = row.Campaign.LoaiDot,
            SoLuongToiDa = row.Campaign.SoLuongToiDa,
            TieuChiXetJson = row.Campaign.TieuChiXetJson,
            MaMauBangKhen = row.Campaign.MaMauBangKhen,
            TenMauBangKhen = row.Template?.TenMau,
            TrangThai = row.Campaign.TrangThai,
            NguoiTao = row.Campaign.NguoiTao,
            TenNguoiTao = row.CreatorName,
            NgayTao = row.Campaign.NgayTao,
            NguoiDuyet = row.Campaign.NguoiDuyet,
            TenNguoiDuyet = row.ApproverName,
            NgayDuyet = row.Campaign.NgayDuyet,
            NgayCongBo = row.Campaign.NgayCongBo,
            GhiChu = row.Campaign.GhiChu,
            HocKy = new RewardCampaignTermDto
            {
                MaHocKy = row.Term.MaHocKy,
                MaCodeHocKy = row.Term.MaCodeHocKy,
                TenHocKy = row.Term.TenHocKy,
                NamHoc = row.Term.NamHoc,
                MaDonVi = row.Term.MaDonVi,
                NgayBatDau = row.Term.NgayBatDau,
                NgayKetThuc = row.Term.NgayKetThuc
            },
            DonVi = row.Organization is null
                ? null
                : new RewardCampaignOrganizationDto
                {
                    MaDonVi = row.Organization.MaDonVi,
                    TenDonVi = row.Organization.TenDonVi,
                    CapDonVi = row.Organization.CapDonVi
                },
            MauBangKhen = row.Template is null
                ? null
                : new RewardCampaignCertificateTemplateDto
                {
                    MaMauBangKhen = row.Template.MaMauBangKhen,
                    TenMau = row.Template.TenMau,
                    LoaiMau = row.Template.LoaiMau,
                    ConHoatDong = row.Template.ConHoatDong
                }
        };

        return dto;
    }

    private sealed class RewardCampaignQueryRow
    {
        public DotKhenThuong Campaign { get; init; } = null!;
        public HocKy Term { get; init; } = null!;
        public DonVi? Organization { get; init; }
        public MauBangKhen? Template { get; init; }
        public string? CreatorName { get; init; }
        public string? ApproverName { get; init; }
    }
}

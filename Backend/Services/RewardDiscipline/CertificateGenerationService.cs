using System.Globalization;
using System.Text;
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

public class CertificateGenerationService : ICertificateGenerationService
{
    private readonly IRewardDisciplineNotificationService _notificationService;
    private const string CampaignEntity = "DotKhenThuong";
    private const string RewardEntity = "KhenThuong";

    private static readonly HashSet<string> AllowedFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "hoTen",
        "mssv",
        "tenHocKy",
        "danhHieu",
        "xepHang",
        "diemXet",
        "ngayCap"
    };

    private static readonly HashSet<string> AllowedAlignments = new(StringComparer.OrdinalIgnoreCase)
    {
        "left",
        "center",
        "right"
    };

    private readonly ApplicationDbContext _context;
    private readonly ICertificatePdfStorageService _storage;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CertificateGenerationService(
        ApplicationDbContext context,
        ICertificatePdfStorageService storage,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor,
        IRewardDisciplineNotificationService notificationService)
    {
        _context = context;
        _storage = storage;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    
        _notificationService = notificationService;}

    public async Task<GenerateRewardCertificatesResultDto> GenerateAsync(
        int campaignId,
        GenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, cancellationToken);
        EnsureApprovedCampaign(campaign);

        MauBangKhen? overrideTemplate = null;
        if (request.MaMauBangKhen.HasValue)
        {
            overrideTemplate = await LoadActiveTemplateAsync(request.MaMauBangKhen.Value, cancellationToken);
        }

        return await GenerateInternalAsync(
            campaign,
            currentUser,
            forceRegenerate: request.ForceRegenerate,
            onlyFailed: request.OnlyFailed,
            overrideTemplate,
            rewardIds: null,
            auditAction: RewardDisciplineConstants.RewardAuditActions.GenerateRewardCertificates,
            note: request.GhiChu,
            cancellationToken);
    }

    public async Task<GenerateRewardCertificatesResultDto> RegenerateAsync(
        int campaignId,
        RegenerateRewardCertificatesRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lý do sinh lại PDF bằng khen là bắt buộc.");
        }

        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, cancellationToken);
        EnsureApprovedCampaign(campaign);

        MauBangKhen? overrideTemplate = null;
        if (request.MaMauBangKhen.HasValue)
        {
            overrideTemplate = await LoadActiveTemplateAsync(request.MaMauBangKhen.Value, cancellationToken);
        }

        var rewardIds = request.RewardIds?
            .Where(x => x > 0)
            .Distinct()
            .ToHashSet();

        return await GenerateInternalAsync(
            campaign,
            currentUser,
            forceRegenerate: true,
            onlyFailed: false,
            overrideTemplate,
            rewardIds,
            auditAction: RewardDisciplineConstants.RewardAuditActions.RegenerateRewardCertificates,
            note: request.Reason,
            cancellationToken);
    }

    public async Task<PagedResultDto<RewardCertificateListItemDto>> GetCertificatesAsync(
        int campaignId,
        RewardCertificateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, cancellationToken);

        var pageIndex = Math.Max(1, parameters.PageIndex);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var query = BuildCertificateQuery(campaign.MaDotKhenThuong);

        if (!string.IsNullOrWhiteSpace(parameters.TrangThaiPdf))
        {
            var status = NormalizeRewardStatus(parameters.TrangThaiPdf);
            query = query.Where(x => x.Reward.TrangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.Reward.HoTenSnapshot != null && x.Reward.HoTenSnapshot.ToLower().Contains(keyword)) ||
                (x.StudentName != null && x.StudentName.ToLower().Contains(keyword)) ||
                (x.Reward.MssvSnapshot != null && x.Reward.MssvSnapshot.ToLower().Contains(keyword)) ||
                (x.StudentEmail != null && x.StudentEmail.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.Reward.XepHang == null)
            .ThenBy(x => x.Reward.XepHang)
            .ThenBy(x => x.Reward.MaKhenThuong)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = rows.Select(ToCertificateListItem).ToList();

        return new PagedResultDto<RewardCertificateListItemDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<RewardCertificateDownloadDto> DownloadAsync(
        int rewardId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var row = await BuildCertificateQuery(null)
            .FirstOrDefaultAsync(x => x.Reward.MaKhenThuong == rewardId, cancellationToken);
        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");
        }

        await EnsureRewardReadableAsync(row.Reward, currentUser, cancellationToken);
        if (string.IsNullOrWhiteSpace(row.Reward.UrlPdfBangKhen))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Bằng khen chưa có file PDF.");
        }

        var file = await _storage.TryReadAsync(row.Reward.UrlPdfBangKhen, cancellationToken);
        if (file is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy file PDF bằng khen.");
        }

        return new RewardCertificateDownloadDto
        {
            Content = file.Content,
            FileName = file.FileName,
            ContentType = "application/pdf"
        };
    }

    private async Task<GenerateRewardCertificatesResultDto> GenerateInternalAsync(
        DotKhenThuong campaign,
        CurrentUserContext currentUser,
        bool forceRegenerate,
        bool onlyFailed,
        MauBangKhen? overrideTemplate,
        HashSet<int>? rewardIds,
        string auditAction,
        string? note,
        CancellationToken cancellationToken)
    {
        var rewardsQuery = _context.KhenThuongs
            .Include(x => x.HocSinh)
            .Include(x => x.HocKy)
            .Where(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong && !x.DaHuy);

        if (rewardIds is { Count: > 0 })
        {
            rewardsQuery = rewardsQuery.Where(x => rewardIds.Contains(x.MaKhenThuong));
        }

        var rewards = await rewardsQuery
            .OrderBy(x => x.XepHang == null)
            .ThenBy(x => x.XepHang)
            .ThenBy(x => x.MaKhenThuong)
            .ToListAsync(cancellationToken);

        if (rewards.Count == 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đợt khen thưởng chưa có quyết định khen thưởng để sinh PDF.");
        }

        var items = new List<RewardCertificateGenerationItemDto>();
        foreach (var reward in rewards)
        {
            if (onlyFailed && !ShouldGenerateWhenOnlyFailed(reward))
            {
                items.Add(CreateSkippedItem(reward, "Bỏ qua vì không ở trạng thái lỗi hoặc thiếu file PDF."));
                continue;
            }

            if (!forceRegenerate &&
                !string.IsNullOrWhiteSpace(reward.UrlPdfBangKhen) &&
                reward.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfGenerated)
            {
                items.Add(CreateSkippedItem(reward, "Bằng khen đã có PDF."));
                continue;
            }

            try
            {
                var template = overrideTemplate
                    ?? await ResolveRewardTemplateAsync(reward, campaign, cancellationToken);
                var fields = ParseConfigFields(template.CauHinhJson);
                var data = ResolveRewardData(reward);
                var pdf = BuildPdf(template, fields, data);
                var stored = await _storage.SaveAsync(reward.MaKhenThuong, pdf, cancellationToken);
                var now = DateTime.UtcNow;

                reward.MaMauBangKhen = template.MaMauBangKhen;
                reward.UrlPdfBangKhen = stored.Url;
                reward.TrangThai = RewardDisciplineConstants.RewardStatuses.PdfGenerated;
                reward.NgaySinhPdf = now;
                reward.NgayCapNhat = now;
                reward.LoiSinhPdf = null;
                reward.SoLanSinhPdf += 1;

                await _context.SaveChangesAsync(cancellationToken);
                items.Add(new RewardCertificateGenerationItemDto
                {
                    MaKhenThuong = reward.MaKhenThuong,
                    Status = "success",
                    UrlPdfBangKhen = stored.Url
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var now = DateTime.UtcNow;
                reward.TrangThai = RewardDisciplineConstants.RewardStatuses.PdfFailed;
                reward.LoiSinhPdf = Truncate(ex.Message, 2000);
                reward.NgaySinhPdf = now;
                reward.NgayCapNhat = now;
                reward.SoLanSinhPdf += 1;
                await _context.SaveChangesAsync(cancellationToken);

                await _auditLogService.LogAsync(
                    RewardEntity,
                    reward.MaKhenThuong.ToString(CultureInfo.InvariantCulture),
                    RewardDisciplineConstants.RewardAuditActions.RewardCertificateGenerationFailed,
                    null,
                    new { reward.MaKhenThuong, Error = reward.LoiSinhPdf },
                    currentUser.UserId,
                    reward.MaDonVi,
                    "Sinh PDF bằng khen thất bại.",
                    cancellationToken);

                items.Add(new RewardCertificateGenerationItemDto
                {
                    MaKhenThuong = reward.MaKhenThuong,
                    Status = "failed",
                    Error = reward.LoiSinhPdf
                });
            }
        }

        var result = new GenerateRewardCertificatesResultDto
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            Total = items.Count,
            SuccessCount = items.Count(x => x.Status == "success"),
            SkippedCount = items.Count(x => x.Status == "skipped"),
            FailedCount = items.Count(x => x.Status == "failed"),
            Items = items
        };

        await _auditLogService.LogAsync(
            CampaignEntity,
            campaign.MaDotKhenThuong.ToString(CultureInfo.InvariantCulture),
            auditAction,
            null,
            new
            {
                campaign.MaDotKhenThuong,
                result.Total,
                result.SuccessCount,
                result.SkippedCount,
                result.FailedCount,
                ForceRegenerate = forceRegenerate,
                OnlyFailed = onlyFailed,
                Note = note
            },
            currentUser.UserId,
            campaign.MaDonVi,
            "Sinh PDF bằng khen Top 100.",
            cancellationToken);

        return result;
    }

    private async Task<MauBangKhen> ResolveRewardTemplateAsync(
        KhenThuong reward,
        DotKhenThuong campaign,
        CancellationToken cancellationToken)
    {
        var templateId = reward.MaMauBangKhen ?? campaign.MaMauBangKhen;
        if (!templateId.HasValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khen thưởng chưa có mẫu bằng khen.");
        }

        return await LoadActiveTemplateAsync(templateId.Value, cancellationToken);
    }

    private async Task<MauBangKhen> LoadActiveTemplateAsync(int templateId, CancellationToken cancellationToken)
    {
        var template = await _context.MauBangKhens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMauBangKhen == templateId, cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu bằng khen.");
        }

        if (!template.ConHoatDong ||
            template.LoaiMau != RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mẫu bằng khen không hoạt động hoặc không đúng loại Top 100 học kỳ.");
        }

        return template;
    }

    private static bool ShouldGenerateWhenOnlyFailed(KhenThuong reward)
    {
        return reward.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfFailed ||
               string.IsNullOrWhiteSpace(reward.UrlPdfBangKhen);
    }

    private static RewardCertificateGenerationItemDto CreateSkippedItem(KhenThuong reward, string reason)
    {
        return new RewardCertificateGenerationItemDto
        {
            MaKhenThuong = reward.MaKhenThuong,
            Status = "skipped",
            UrlPdfBangKhen = reward.UrlPdfBangKhen,
            SkippedReason = reason
        };
    }

    private static void EnsureApprovedCampaign(DotKhenThuong campaign)
    {
        if (campaign.TrangThai != RewardDisciplineConstants.RewardCampaignStatuses.Approved)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được sinh PDF khi đợt khen thưởng đã duyệt.");
        }
    }

    private IQueryable<CertificateQueryRow> BuildCertificateQuery(int? campaignId)
    {
        var query =
            from reward in _context.KhenThuongs.AsNoTracking()
            join student in _context.NguoiDungs.AsNoTracking()
                on reward.MaHocSinh equals student.MaNguoiDung into studentGroup
            from student in studentGroup.DefaultIfEmpty()
            join term in _context.HocKys.AsNoTracking()
                on reward.MaHocKy equals term.MaHocKy into termGroup
            from term in termGroup.DefaultIfEmpty()
            join template in _context.MauBangKhens.AsNoTracking()
                on reward.MaMauBangKhen equals template.MaMauBangKhen into templateGroup
            from template in templateGroup.DefaultIfEmpty()
            select new CertificateQueryRow
            {
                Reward = reward,
                StudentName = student != null ? student.HoTen : null,
                StudentEmail = student != null ? student.Email : null,
                TermName = term != null ? term.TenHocKy : null,
                TemplateName = template != null ? template.TenMau : null
            };

        if (campaignId.HasValue)
        {
            query = query.Where(x => x.Reward.MaDotKhenThuong == campaignId.Value && !x.Reward.DaHuy);
        }

        return query;
    }

    private async Task EnsureRewardReadableAsync(KhenThuong reward, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return;
        }

        if (currentUser.Role == AuthRoles.Admin)
        {
            if (reward.MaDotKhenThuong.HasValue)
            {
                var campaignIsGlobal = await _context.DotKhenThuongs.AsNoTracking()
                    .AnyAsync(x => x.MaDotKhenThuong == reward.MaDotKhenThuong.Value && !x.MaDonVi.HasValue, cancellationToken);
                if (campaignIsGlobal)
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem bằng khen thuộc đợt toàn hệ thống.");
                }
            }

            return;
        }

        if (currentUser.Role != AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem bằng khen.");
        }

        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        if (!allowedOrganizationIds.Contains(reward.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem bằng khen của đơn vị này.");
        }
    }

    private async Task<DotKhenThuong> LoadCampaignInScopeAsync(
        int campaignId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.DotKhenThuongs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDotKhenThuong == campaignId, cancellationToken);
        if (campaign is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt khen thưởng.");
        }

        if (campaign.LoaiDot != RewardDisciplineConstants.RewardCampaignTypes.Top100Semester)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ đợt khen thưởng Top 100 học kỳ.");
        }

        await EnsureCampaignReadableAsync(campaign, currentUser, cancellationToken);
        return campaign;
    }

    private async Task EnsureCampaignReadableAsync(DotKhenThuong campaign, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return;
        }

        if (!campaign.MaDonVi.HasValue)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng toàn hệ thống.");
        }

        if (currentUser.Role == AuthRoles.Admin)
        {
            return;
        }

        if (currentUser.Role != AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng.");
        }

        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        if (!allowedOrganizationIds.Contains(campaign.MaDonVi.Value))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng của đơn vị này.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Admin)
        {
            return await _context.DonVis.AsNoTracking().Select(x => x.MaDonVi).ToHashSetAsync(cancellationToken);
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

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        return currentUser;
    }

    private static void EnsureSuperAdmin(CurrentUserContext currentUser)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được sinh PDF bằng khen.");
        }
    }

    private static string NormalizeRewardStatus(string value)
    {
        var normalized = value.Trim();
        var canonical = RewardDisciplineConstants.RewardStatuses.All
            .FirstOrDefault(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái PDF bằng khen không hợp lệ.");
        }

        return canonical;
    }

    private static RewardCertificateListItemDto ToCertificateListItem(CertificateQueryRow row)
    {
        return new RewardCertificateListItemDto
        {
            MaKhenThuong = row.Reward.MaKhenThuong,
            MaDotKhenThuong = row.Reward.MaDotKhenThuong ?? 0,
            MaHocSinh = row.Reward.MaHocSinh,
            HoTen = FirstNonBlank(row.Reward.HoTenSnapshot, row.StudentName),
            Mssv = FirstNonBlank(row.Reward.MssvSnapshot, row.StudentEmail),
            MaHocKy = row.Reward.MaHocKy,
            TenHocKy = FirstNonBlank(row.Reward.TenHocKySnapshot, row.TermName),
            XepHang = row.Reward.XepHang,
            DiemXet = row.Reward.DiemXet,
            DanhHieu = FirstNonBlank(row.Reward.DanhHieuSnapshot, "Top 100 học kỳ"),
            MaMauBangKhen = row.Reward.MaMauBangKhen,
            TenMauBangKhen = row.TemplateName,
            TrangThai = row.Reward.TrangThai,
            TrangThaiSinhPdf = row.Reward.TrangThai,
            UrlPdfBangKhen = row.Reward.UrlPdfBangKhen,
            NgaySinhPdf = row.Reward.NgaySinhPdf,
            LoiSinhPdf = row.Reward.LoiSinhPdf,
            SoLanSinhPdf = row.Reward.SoLanSinhPdf
        };
    }

    private static Dictionary<string, string?> ResolveRewardData(KhenThuong reward)
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["hoTen"] = FirstNonBlank(reward.HoTenSnapshot, reward.HocSinh?.HoTen),
            ["mssv"] = FirstNonBlank(reward.MssvSnapshot, reward.HocSinh?.Email, reward.MaHocSinh.ToString(CultureInfo.InvariantCulture)),
            ["tenHocKy"] = FirstNonBlank(reward.TenHocKySnapshot, reward.HocKy?.TenHocKy),
            ["danhHieu"] = FirstNonBlank(reward.DanhHieuSnapshot, "Top 100 học kỳ"),
            ["xepHang"] = reward.XepHang?.ToString(CultureInfo.InvariantCulture),
            ["diemXet"] = reward.DiemXet?.ToString("0.##", CultureInfo.InvariantCulture),
            ["ngayCap"] = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
    }

    private static IReadOnlyList<CertificateRenderField> ParseConfigFields(string? configJson)
    {
        try
        {
            using var document = JsonDocument.Parse(configJson ?? string.Empty);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !TryGetPropertyIgnoreCase(root, "fields", out var fields) ||
                fields.ValueKind != JsonValueKind.Array ||
                fields.GetArrayLength() is < 1 or > 50)
            {
                throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
            }

            var result = new List<CertificateRenderField>();
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fields.EnumerateArray())
            {
                if (field.ValueKind != JsonValueKind.Object)
                {
                    throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
                }

                var key = GetRequiredString(field, "key");
                if (!AllowedFieldKeys.Contains(key) || !keys.Add(key))
                {
                    throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
                }

                var align = GetRequiredString(field, "align").ToLowerInvariant();
                if (!AllowedAlignments.Contains(align))
                {
                    throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
                }

                result.Add(new CertificateRenderField(
                    key,
                    GetRequiredDecimal(field, "x"),
                    GetRequiredDecimal(field, "y"),
                    GetRequiredDecimal(field, "fontSize"),
                    align,
                    GetOptionalBoolean(field, "bold")));
            }

            return result;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
        }
    }

    private static byte[] BuildPdf(
        MauBangKhen template,
        IReadOnlyList<CertificateRenderField> fields,
        IReadOnlyDictionary<string, string?> data)
    {
        var pageWidth = template.HuongGiay == RewardDisciplineConstants.PaperOrientations.A4Portrait ? 595m : 842m;
        var pageHeight = template.HuongGiay == RewardDisciplineConstants.PaperOrientations.A4Portrait ? 842m : 595m;
        var content = new StringBuilder();
        content.AppendLine("q");
        content.AppendLine("BT");
        content.AppendLine("/F1 14 Tf");
        foreach (var field in fields)
        {
            var value = data.TryGetValue(field.Key, out var resolved) ? resolved : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var fontSize = Math.Clamp(field.FontSize, 1m, 200m);
            var x = template.ChieuRong <= 0 ? 50m : field.X / template.ChieuRong * pageWidth;
            var y = template.ChieuCao <= 0 ? 50m : pageHeight - (field.Y / template.ChieuCao * pageHeight);
            var safeValue = EscapePdfText(ToAsciiText(value));
            var textWidth = safeValue.Length * fontSize * 0.5m;
            if (field.Align == "center")
            {
                x -= textWidth / 2;
            }
            else if (field.Align == "right")
            {
                x -= textWidth;
            }

            content.Append(FormattableString.Invariant($"/F1 {fontSize:0.##} Tf "));
            content.Append(FormattableString.Invariant($"1 0 0 1 {Math.Max(0, x):0.##} {Math.Max(0, y):0.##} Tm "));
            content.Append('(').Append(safeValue).AppendLine(") Tj");
        }

        content.AppendLine("ET");
        content.AppendLine("Q");

        return WriteSimplePdf(pageWidth, pageHeight, content.ToString());
    }

    private static byte[] WriteSimplePdf(decimal pageWidth, decimal pageHeight, string content)
    {
        var contentBytes = Encoding.ASCII.GetBytes(content);
        var builder = new StringBuilder();
        var offsets = new List<int> { 0 };

        void Append(string value)
        {
            builder.Append(value);
        }

        void AppendInvariant(FormattableString value)
        {
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        void BeginObject(int id)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            AppendInvariant($"{id} 0 obj\n");
        }

        Append("%PDF-1.4\n");
        BeginObject(1);
        Append("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        BeginObject(2);
        Append("<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        BeginObject(3);
        AppendInvariant($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth:0.##} {pageHeight:0.##}] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
        BeginObject(4);
        Append("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        BeginObject(5);
        AppendInvariant($"<< /Length {contentBytes.Length} >>\nstream\n");
        Append(content);
        Append("endstream\nendobj\n");

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        Append("xref\n0 6\n");
        Append("0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
        {
            AppendInvariant($"{offsets[i]:0000000000} 00000 n \n");
        }

        Append("trailer\n<< /Size 6 /Root 1 0 R >>\n");
        AppendInvariant($"startxref\n{xrefOffset}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
        }

        return property.GetString()!.Trim();
    }

    private static decimal GetRequiredDecimal(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetDecimal(out var value) ||
            value < 0)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.");
        }

        return value;
    }

    private static bool GetOptionalBoolean(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu bằng khen không hợp lệ.")
        };
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string ToAsciiText(string value)
    {
        return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, Encoding.UTF8.GetBytes(value)));
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed record CertificateRenderField(
        string Key,
        decimal X,
        decimal Y,
        decimal FontSize,
        string Align,
        bool Bold);

    private sealed class CertificateQueryRow
    {
        public KhenThuong Reward { get; init; } = null!;
        public string? StudentName { get; init; }
        public string? StudentEmail { get; init; }
        public string? TermName { get; init; }
        public string? TemplateName { get; init; }
    }
}

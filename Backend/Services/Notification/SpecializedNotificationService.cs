using System.Net;
using System.Text.RegularExpressions;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.DTOs.Notifications;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Applications;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Notification;

public class SpecializedNotificationService : ISpecializedNotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly INotificationTemplateService _templateService;

    public SpecializedNotificationService(
        ApplicationDbContext dbContext,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IApplicationCampusScopeService scopeService,
        INotificationTemplateService templateService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _scopeService = scopeService;
        _templateService = templateService;
    }

    public Task<SpecializedNotificationCategoryDto> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SpecializedNotificationCategoryDto
        {
            Categories = new List<string>
            {
                NotificationConstants.Types.Tuition,
                NotificationConstants.Types.Academic,
                NotificationConstants.Types.Urgent,
                NotificationConstants.Types.Maintenance,
                NotificationConstants.Types.GeneralTargeted
            }
        });
    }

    public async Task<PreviewSpecializedRecipientsResultDto> PreviewRecipientsAsync(
        PreviewSpecializedRecipientsRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        var (studentIds, warnings, unsupportedFilters) = await ResolveRecipientIdsAsync(request.Target, actor, cancellationToken);
        
        var total = studentIds.Count;
        var sampleIds = studentIds.Take(100).ToList();
        
        var samples = await _dbContext.NguoiDungs
            .Where(x => sampleIds.Contains(x.MaNguoiDung))
            .Select(x => new SpecializedRecipientPreviewItemDto
            {
                MaNguoiDung = x.MaNguoiDung,
                HoTen = x.HoTen,
                Email = x.Email,
                VaiTroChinh = x.VaiTroChinh,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null
            })
            .ToListAsync(cancellationToken);

        return new PreviewSpecializedRecipientsResultDto
        {
            TotalRecipients = total,
            SampleRecipients = samples,
            Warnings = warnings,
            UnsupportedFilters = unsupportedFilters
        };
    }

    public Task<SpecializedNotificationSendResultDto> SendTuitionNotificationAsync(
        SendTuitionNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        return SendSpecializedNotificationAsync(
            request, 
            NotificationConstants.Types.Tuition, 
            NotificationConstants.EventCodes.TUITION_NOTICE, 
            NotificationConstants.SpecializedNotificationAuditActions.SEND_TUITION_NOTIFICATION, 
            actor, 
            cancellationToken);
    }

    public Task<SpecializedNotificationSendResultDto> SendAcademicNotificationAsync(
        SendAcademicNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        return SendSpecializedNotificationAsync(
            request, 
            NotificationConstants.Types.Academic, 
            NotificationConstants.EventCodes.ACADEMIC_NOTICE, 
            NotificationConstants.SpecializedNotificationAuditActions.SEND_ACADEMIC_NOTIFICATION, 
            actor, 
            cancellationToken);
    }

    public Task<SpecializedNotificationSendResultDto> SendUrgentNotificationAsync(
        SendUrgentNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        return SendSpecializedNotificationAsync(
            request, 
            NotificationConstants.Types.Urgent, 
            NotificationConstants.EventCodes.URGENT_NOTICE, 
            NotificationConstants.SpecializedNotificationAuditActions.SEND_URGENT_NOTIFICATION, 
            actor, 
            cancellationToken);
    }

    public Task<SpecializedNotificationSendResultDto> SendMaintenanceNotificationAsync(
        SendMaintenanceNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        return SendSpecializedNotificationAsync(
            request, 
            NotificationConstants.Types.Maintenance, 
            NotificationConstants.EventCodes.MAINTENANCE_NOTICE, 
            NotificationConstants.SpecializedNotificationAuditActions.SEND_MAINTENANCE_NOTIFICATION, 
            actor, 
            cancellationToken);
    }

    private async Task<SpecializedNotificationSendResultDto> SendSpecializedNotificationAsync(
        BaseSpecializedNotificationRequest request,
        string categoryType,
        string defaultEventCode,
        string auditAction,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var (recipientIds, warnings, unsupportedFilters) = await ResolveRecipientIdsAsync(request.Target, actor, cancellationToken);
        if (recipientIds.Count == 0)
        {
            return new SpecializedNotificationSendResultDto { Success = false, Message = "Không có người nhận hợp lệ." };
        }

        string finalTitle = request.Title ?? string.Empty;
        string finalBody = request.Body ?? string.Empty;

        if (request.TemplateId.HasValue)
        {
            var template = await _templateService.GetTemplateDetailAsync(request.TemplateId.Value, actor, cancellationToken);
            finalTitle = RenderTemplate(template.TieuDeMau ?? string.Empty, request.Variables ?? new Dictionary<string, string>());
            finalBody = RenderTemplate(template.NoiDungMau ?? string.Empty, request.Variables ?? new Dictionary<string, string>());
        }

        // Idempotency check
        string idempotencyKey = string.IsNullOrWhiteSpace(request.ClientRequestId) 
            ? GenerateIdempotencyKey(request.Target, finalTitle, finalBody, categoryType) 
            : request.ClientRequestId;
            
        var exists = await _dbContext.ThongBaos.AnyAsync(x => x.DoiTuongLienKet == "IdempotencyKey" && x.LoaiSuKien == idempotencyKey, cancellationToken);
        if (exists)
        {
            return new SpecializedNotificationSendResultDto { Success = true, Message = "Thông báo đã được gửi trước đó (Idempotent)." };
        }

        int senderMaDonVi = request.Target.MaDonVi ?? (actor.AllowedCampusIds?.FirstOrDefault() ?? 0);

        var sysRequest = new SystemNotificationRequest
        {
            TieuDe = finalTitle,
            NoiDungText = finalBody,
            LoaiThongBao = categoryType,
            MucDo = categoryType == NotificationConstants.Types.Urgent ? NotificationConstants.Levels.Urgent : NotificationConstants.Levels.Info,
            DoiTuongLienKet = "IdempotencyKey",
            LoaiSuKien = idempotencyKey,
            MaDonVi = senderMaDonVi,
            NguoiTao = actor.Claims.UserId
        };

        await _notificationService.CreateSystemNotificationAsync(sysRequest, recipientIds, cancellationToken);

        await _auditLogService.LogAsync(
            "ThongBao",
            "BATCH",
            auditAction,
            null,
            new { request.AuditNote, Target = request.Target, RecipientCount = recipientIds.Count, IdempotencyKey = idempotencyKey },
            actor.Claims.UserId,
            senderMaDonVi,
            request.AuditNote,
            cancellationToken);

        return new SpecializedNotificationSendResultDto { Success = true, Message = "Gửi thông báo thành công." };
    }

    private string GenerateIdempotencyKey(SpecializedNotificationTargetDto target, string title, string body, string category)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        string input = $"{target.TargetType}_{target.MaDonVi}_{target.MaLop}_{target.MaNganh}_{title}_{body}_{category}";
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes).Replace("+", "").Replace("/", "").Substring(0, 32);
    }

    private static string RenderTemplate(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template)) return "";
        var regex = new Regex(@"\{\{([a-zA-Z0-9_]+)\}\}");
        return regex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            if (variables.TryGetValue(key, out var val))
            {
                return WebUtility.HtmlEncode(val ?? "");
            }
            return match.Value;
        });
    }

    private async Task<(List<int> recipientIds, List<string> warnings, List<string> unsupportedFilters)> ResolveRecipientIdsAsync(
        SpecializedNotificationTargetDto target,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var unsupported = new List<string>();
        var query = _dbContext.NguoiDungs.AsNoTracking().Where(x => x.TrangThai == UserStatuses.DbActive);

        // Apply Scope Security
        if (!actor.IsGlobal)
        {
            var allowed = actor.AllowedCampusIds ?? new HashSet<int>();
            query = query.Where(x => allowed.Contains(x.MaDonVi));
        }

        switch (target.TargetType.ToLowerInvariant())
        {
            case "all_students":
                if (!actor.IsGlobal && actor.Role != AuthRoles.Admin)
                {
                    warnings.Add("Chỉ quản trị viên cấp cao mới có quyền gửi cho toàn bộ sinh viên. Danh sách đã được giới hạn theo cơ sở của bạn.");
                }
                query = query.Where(x => x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student));
                break;
                
            case "campus":
                if (target.MaDonVi.HasValue)
                {
                    if (!actor.IsGlobal && (actor.AllowedCampusIds == null || !actor.AllowedCampusIds.Contains(target.MaDonVi.Value)))
                    {
                        throw new ApiException((int)HttpStatusCode.Forbidden, "Không có quyền truy cập cơ sở này.");
                    }
                    query = query.Where(x => x.MaDonVi == target.MaDonVi.Value);
                }
                else
                {
                    warnings.Add("Chưa cung cấp MaDonVi cho loại mục tiêu campus.");
                    return (new List<int>(), warnings, unsupported);
                }
                break;
                
            case "class":
                if (target.MaLop.HasValue)
                {
                    var lop = await _dbContext.LopHanhChinhs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLop == target.MaLop.Value, cancellationToken);
                    if (lop != null && !actor.IsGlobal && (actor.AllowedCampusIds == null || !actor.AllowedCampusIds.Contains(lop.MaDonVi)))
                    {
                        throw new ApiException((int)HttpStatusCode.Forbidden, "Không có quyền truy cập lớp này.");
                    }
                    query = query.Where(x => x.MaLop == target.MaLop.Value);
                }
                else
                {
                    warnings.Add("Chưa cung cấp MaLop cho loại mục tiêu class.");
                    return (new List<int>(), warnings, unsupported);
                }
                break;
                
            case "major":
                if (target.MaNganh.HasValue)
                {
                    var lopIds = await _dbContext.LopHanhChinhs.AsNoTracking()
                        .Where(l => l.ChuongTrinh != null && 
                                    l.ChuongTrinh.ChuyenNganh != null && 
                                    l.ChuongTrinh.ChuyenNganh.MaNganh == target.MaNganh.Value)
                        .Select(l => l.MaLop)
                        .ToListAsync(cancellationToken);
                        
                    query = query.Where(x => x.MaLop.HasValue && lopIds.Contains(x.MaLop.Value));
                }
                else
                {
                    warnings.Add("Chưa cung cấp MaNganh cho loại mục tiêu major.");
                    return (new List<int>(), warnings, unsupported);
                }
                break;
                
            case "department":
                unsupported.Add("department");
                warnings.Add("Loại mục tiêu department không được hỗ trợ trong phiên bản hiện tại do thiếu bảng định danh.");
                return (new List<int>(), warnings, unsupported);
                
            case "custom_students":
                if (target.StudentIds != null && target.StudentIds.Any())
                {
                    query = query.Where(x => target.StudentIds.Contains(x.MaNguoiDung) && x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student));
                }
                else
                {
                    return (new List<int>(), warnings, unsupported);
                }
                break;
                
            case "admins":
                if (target.RoleCodes != null && target.RoleCodes.Any())
                {
                    var dbRoles = target.RoleCodes.Select(AuthRoles.ToDatabaseCode).ToList();
                    query = query.Where(x => dbRoles.Contains(x.VaiTroChinh));
                }
                else
                {
                    var adminRoles = new[] { AuthRoles.ToDatabaseCode(AuthRoles.Admin), AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin) };
                    query = query.Where(x => adminRoles.Contains(x.VaiTroChinh));
                }
                break;
                
            default:
                warnings.Add($"Loại mục tiêu {target.TargetType} không hợp lệ.");
                return (new List<int>(), warnings, unsupported);
        }

        if (!string.IsNullOrWhiteSpace(target.Keyword))
        {
            var kw = target.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.HoTen.ToLower().Contains(kw) || x.Email.ToLower().Contains(kw));
        }

        var ids = await query.Select(x => x.MaNguoiDung).ToListAsync(cancellationToken);
        return (ids, warnings, unsupported);
    }
}

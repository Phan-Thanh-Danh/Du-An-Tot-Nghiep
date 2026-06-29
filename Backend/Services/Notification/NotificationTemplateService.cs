using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.Applications;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Services.Notification;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly IAuditLogService _auditLogService;

    public NotificationTemplateService(
        ApplicationDbContext dbContext,
        IApplicationCampusScopeService scopeService,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<NotificationTemplateListItemDto>> GetTemplatesAsync(
        NotificationTemplateQueryParameters query,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var dbQuery = _dbContext.MauThongBaos.AsNoTracking();

        // Scope filter: SuperAdmin sees all. Admin/CampusAdmin sees global (MaDonVi == null) + scoped templates.
        if (!actor.IsGlobal)
        {
            var allowed = actor.AllowedCampusIds ?? new HashSet<int>();
            dbQuery = dbQuery.Where(x => x.MaDonVi == null || allowed.Contains(x.MaDonVi.Value));
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            dbQuery = dbQuery.Where(x => 
                (x.MaMau != null && x.MaMau.Contains(query.Keyword)) ||
                (x.TenMau != null && x.TenMau.Contains(query.Keyword)) ||
                (x.MauTieuDe != null && x.MauTieuDe.Contains(query.Keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.LoaiThongBao))
        {
            dbQuery = dbQuery.Where(x => x.LoaiThongBao == query.LoaiThongBao);
        }

        if (query.DangHoatDong.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.DangHoatDong == query.DangHoatDong.Value);
        }

        if (query.MaDonVi.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.MaDonVi == query.MaDonVi.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(x => x.NgayTao)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new NotificationTemplateListItemDto
            {
                MaMauThongBao = x.MaMauTb,
                MaMau = x.MaMau,
                TenMau = x.TenMau,
                LoaiThongBao = x.LoaiThongBao,
                TieuDeMau = x.MauTieuDe,
                DangHoatDong = x.DangHoatDong,
                LaHeThong = x.LaHeThong,
                MaDonVi = x.MaDonVi,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<NotificationTemplateListItemDto>
        {
            Items = items,
            TotalItems = totalCount,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<NotificationTemplateDetailDto> GetTemplateDetailAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.MauThongBaos.AsNoTracking().FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        await EnsureHasAccessAsync(template, actor, cancellationToken);

        var dto = new NotificationTemplateDetailDto
        {
            MaMauThongBao = template.MaMauTb,
            MaDonVi = template.MaDonVi,
            TenMau = template.TenMau,
            MaMau = template.MaMau,
            LoaiThongBao = template.LoaiThongBao,
            TieuDeMau = template.MauTieuDe,
            NoiDungMau = template.MauNoiDung,
            KenhThongBao = template.KenhGui,
            MucDoUuTien = template.MucDoUuTien,
            DoiTuongMacDinh = template.DoiTuongMacDinh,
            BienChoPhepJson = template.BienChoPhepJson,
            DangHoatDong = template.DangHoatDong,
            LaHeThong = template.LaHeThong,
            NgayTao = template.NgayTao,
            NguoiTao = template.NguoiTao,
            NgayCapNhat = template.NgayCapNhat,
            NguoiCapNhat = template.NguoiCapNhat,
            DetectedPlaceholders = ExtractPlaceholders(template.MauTieuDe ?? "", template.MauNoiDung)
        };

        return dto;
    }

    public async Task<NotificationTemplateDetailDto> CreateTemplateAsync(
        CreateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        if (actor.Role != AuthRoles.SuperAdmin && actor.Role != AuthRoles.Admin && actor.Role != AuthRoles.CampusAdmin)
            throw new ApiException((int)HttpStatusCode.Forbidden, "Không có quyền tạo mẫu thông báo.");

        // Validate scope
        if (request.MaDonVi.HasValue && actor.Role != AuthRoles.SuperAdmin)
        {
            bool canAccess = await _scopeService.CanAccessCampusAsync(actor, request.MaDonVi.Value, cancellationToken);
            if (!canAccess)
                throw new ApiException((int)HttpStatusCode.Forbidden, "Không thể tạo mẫu thông báo ngoài phạm vi quản lý.");
        }
        else if (!request.MaDonVi.HasValue && actor.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException((int)HttpStatusCode.Forbidden, "Chỉ SuperAdmin mới có thể tạo mẫu thông báo toàn hệ thống (không gán cơ sở).");
        }

        // Validate uniqueness
        var exists = await _dbContext.MauThongBaos.AnyAsync(x => x.MaMau == request.MaMau && x.MaDonVi == request.MaDonVi, cancellationToken);
        if (exists)
            throw new ApiException((int)HttpStatusCode.BadRequest, "Mã mẫu đã tồn tại trong phạm vi này.");

        ValidateJsonConfig(request.BienChoPhepJson);

        var template = new MauThongBao
        {
            MaDonVi = request.MaDonVi,
            TenMau = request.TenMau,
            MaMau = request.MaMau,
            LoaiThongBao = request.LoaiThongBao,
            LoaiSuKien = request.LoaiThongBao, // Synchronize for backward compatibility
            MauTieuDe = request.TieuDeMau,
            MauNoiDung = request.NoiDungMau,
            KenhGui = request.KenhThongBao,
            MucDoUuTien = request.MucDoUuTien,
            DoiTuongMacDinh = request.DoiTuongMacDinh,
            BienChoPhepJson = request.BienChoPhepJson,
            DangHoatDong = request.DangHoatDong,
            LaHeThong = false,
            NgayTao = DateTime.UtcNow,
            NguoiTao = actor.Claims.UserId
        };

        _dbContext.MauThongBaos.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "MauThongBao",
            template.MaMauTb.ToString(),
            NotificationConstants.NotificationTemplateAuditActions.Create,
            null,
            template,
            actor.Claims.UserId,
            template.MaDonVi,
            null,
            cancellationToken);

        return await GetTemplateDetailAsync(template.MaMauTb, actor, cancellationToken);
    }

    public async Task<NotificationTemplateDetailDto> UpdateTemplateAsync(
        int id,
        UpdateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.MauThongBaos.FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        await EnsureHasAccessAsync(template, actor, cancellationToken);

        if (template.LaHeThong && actor.Role != AuthRoles.SuperAdmin)
            throw new ApiException((int)HttpStatusCode.Forbidden, "Chỉ SuperAdmin mới được cập nhật mẫu hệ thống.");

        if (template.MaMau != request.MaMau)
        {
            if (template.LaHeThong && actor.Role != AuthRoles.SuperAdmin)
                throw new ApiException((int)HttpStatusCode.Forbidden, "Không thể đổi mã mẫu hệ thống.");

            var exists = await _dbContext.MauThongBaos.AnyAsync(x => x.MaMau == request.MaMau && x.MaDonVi == template.MaDonVi && x.MaMauTb != id, cancellationToken);
            if (exists)
                throw new ApiException((int)HttpStatusCode.BadRequest, "Mã mẫu đã tồn tại trong phạm vi này.");
        }

        ValidateJsonConfig(request.BienChoPhepJson);

        var oldData = JsonSerializer.Serialize(template);

        template.TenMau = request.TenMau;
        template.MaMau = request.MaMau;
        template.LoaiThongBao = request.LoaiThongBao;
        template.LoaiSuKien = request.LoaiThongBao;
        template.MauTieuDe = request.TieuDeMau;
        template.MauNoiDung = request.NoiDungMau;
        template.KenhGui = request.KenhThongBao;
        template.MucDoUuTien = request.MucDoUuTien;
        template.DoiTuongMacDinh = request.DoiTuongMacDinh;
        template.BienChoPhepJson = request.BienChoPhepJson;
        template.NgayCapNhat = DateTime.UtcNow;
        template.NguoiCapNhat = actor.Claims.UserId;

        if (request.LaHeThong.HasValue && actor.Role == AuthRoles.SuperAdmin)
        {
            template.LaHeThong = request.LaHeThong.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "MauThongBao",
            template.MaMauTb.ToString(),
            NotificationConstants.NotificationTemplateAuditActions.Update,
            oldData,
            template,
            actor.Claims.UserId,
            template.MaDonVi,
            null,
            cancellationToken);

        return await GetTemplateDetailAsync(template.MaMauTb, actor, cancellationToken);
    }

    public async Task ActivateTemplateAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.MauThongBaos.FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        await EnsureHasAccessAsync(template, actor, cancellationToken);

        template.DangHoatDong = true;
        template.NgayCapNhat = DateTime.UtcNow;
        template.NguoiCapNhat = actor.Claims.UserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "MauThongBao",
            template.MaMauTb.ToString(),
            NotificationConstants.NotificationTemplateAuditActions.Activate,
            null,
            null,
            actor.Claims.UserId,
            template.MaDonVi,
            null,
            cancellationToken);
    }

    public async Task DeactivateTemplateAsync(
        int id,
        DeactivateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.MauThongBaos.FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        await EnsureHasAccessAsync(template, actor, cancellationToken);

        template.DangHoatDong = false;
        template.NgayCapNhat = DateTime.UtcNow;
        template.NguoiCapNhat = actor.Claims.UserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "MauThongBao",
            template.MaMauTb.ToString(),
            NotificationConstants.NotificationTemplateAuditActions.Deactivate,
            null,
            new { request.Reason },
            actor.Claims.UserId,
            template.MaDonVi,
            null,
            cancellationToken);
    }

    public async Task DeleteTemplateAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        if (actor.Role != AuthRoles.SuperAdmin)
            throw new ApiException((int)HttpStatusCode.Forbidden, "Chỉ SuperAdmin mới có quyền xoá mẫu thông báo.");

        var template = await _dbContext.MauThongBaos.FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        // We choose to deactivate instead of hard delete, especially if system template
        if (template.LaHeThong)
        {
            template.DangHoatDong = false;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _auditLogService.LogAsync(
                "MauThongBao",
                template.MaMauTb.ToString(),
                NotificationConstants.NotificationTemplateAuditActions.DeleteOrDisable,
                null,
                "Deactivated system template instead of hard deleting.",
                actor.Claims.UserId,
                template.MaDonVi,
                null,
                cancellationToken);
            return;
        }

        _dbContext.MauThongBaos.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "MauThongBao",
            id.ToString(),
            NotificationConstants.NotificationTemplateAuditActions.DeleteOrDisable,
            template,
            null,
            actor.Claims.UserId,
            template.MaDonVi,
            null,
            cancellationToken);
    }

    public async Task<PreviewNotificationTemplateResultDto> PreviewTemplateAsync(
        int id,
        PreviewNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.MauThongBaos.AsNoTracking().FirstOrDefaultAsync(x => x.MaMauTb == id, cancellationToken);
        if (template == null)
            throw new ApiException((int)HttpStatusCode.NotFound, "Không tìm thấy mẫu thông báo.");

        await EnsureHasAccessAsync(template, actor, cancellationToken);

        var detected = ExtractPlaceholders(template.MauTieuDe ?? "", template.MauNoiDung);
        var providedVars = request.Variables ?? new Dictionary<string, string>();

        var missing = detected.Where(k => !providedVars.ContainsKey(k)).ToList();
        var unused = providedVars.Keys.Where(k => !detected.Contains(k)).ToList();

        var renderedTitle = RenderTemplate(template.MauTieuDe ?? "", providedVars);
        var renderedBody = RenderTemplate(template.MauNoiDung, providedVars);
        
        var warnings = new List<string>();
        if (missing.Any())
            warnings.Add("Một số biến chưa được gán giá trị và được giữ nguyên.");

        return new PreviewNotificationTemplateResultDto
        {
            RenderedTitle = renderedTitle,
            RenderedBody = renderedBody,
            MissingVariables = missing,
            UnusedVariables = unused,
            DetectedPlaceholders = detected,
            Warnings = warnings
        };
    }

    private async Task EnsureHasAccessAsync(MauThongBao template, ApplicationActorContext actor, CancellationToken cancellationToken)
    {
        if (actor.IsGlobal)
            return;

        if (template.MaDonVi == null) // Global
            return; // Can view/use global templates

        if (actor.AllowedCampusIds == null || !actor.AllowedCampusIds.Contains(template.MaDonVi.Value))
            throw new ApiException((int)HttpStatusCode.Forbidden, "Không có quyền truy cập mẫu thông báo này.");
    }

    private static List<string> ExtractPlaceholders(string title, string body)
    {
        var regex = new Regex(@"\{\{([a-zA-Z0-9_]+)\}\}");
        var matches = regex.Matches(title + " " + body);
        var placeholders = new HashSet<string>();
        foreach (Match match in matches)
        {
            placeholders.Add(match.Groups[1].Value);
        }
        return placeholders.ToList();
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
            return match.Value; // Keep placeholder if missing
        });
    }

    private static void ValidateJsonConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;

        try
        {
            using var doc = JsonDocument.Parse(json);
        }
        catch
        {
            throw new ApiException((int)HttpStatusCode.BadRequest, "Cấu hình JSON không hợp lệ.");
        }
    }
}

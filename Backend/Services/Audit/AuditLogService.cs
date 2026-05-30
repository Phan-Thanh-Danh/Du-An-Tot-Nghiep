using System.Text.Json;
using System.Text.Json.Nodes;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Audit;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] ContainsSensitiveNames = ["password", "token", "otp", "secret"];
    private static readonly HashSet<string> ExactSensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "matkhau",
        "matkhauhash",
        "passwordhash",
        "refreshtoken",
        "accesstoken",
        "tokenhash",
        "connectionstring"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string entityType,
        string entityId,
        string action,
        object? oldValue,
        object? newValue,
        int? changedBy,
        int? maDonVi,
        string? description,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var auditLog = new NhatKyKiemToan
            {
                MaDonVi = maDonVi,
                LoaiDoiTuong = TruncateRequired(entityType, 100),
                MaDoiTuong = TruncateRequired(entityId, 100),
                HanhDong = TruncateRequired(action, 50),
                GiaTriCu = SerializeSanitized(oldValue),
                GiaTriMoi = SerializeSanitized(newValue),
                NguoiThayDoi = changedBy,
                ThoiDiemThayDoi = DateTime.UtcNow,
                DiaChiIp = Truncate(GetClientIp(httpContext), 45),
                UserAgent = Truncate(httpContext?.Request.Headers.UserAgent.ToString(), 512),
                MoTa = Truncate(description, 500),
                TraceId = Truncate(httpContext?.TraceIdentifier, 100)
            };

            await _context.NhatKyKiemToans.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            DetachPendingAuditLogs();
            _logger.LogWarning(ex, "Không thể ghi audit log cho {EntityType} {EntityId} {Action}.", entityType, entityId, action);
        }
    }

    public async Task AddAsync(
        int campusId,
        string entityName,
        int entityId,
        string action,
        int actorUserId,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            entityName,
            entityId.ToString(),
            action,
            oldValue,
            newValue,
            actorUserId,
            campusId,
            null,
            cancellationToken);
    }

    public async Task<PagedResultDto<AuditLogListItemDto>> GetAsync(
        AuditLogQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (parameters.FromDate.HasValue &&
            parameters.ToDate.HasValue &&
            parameters.FromDate.Value > parameters.ToDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "fromDate không được lớn hơn toDate.");
        }

        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyFilters(
            _context.NhatKyKiemToans.AsNoTracking(),
            parameters,
            currentUser,
            allowedOrganizationIds);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ThoiDiemThayDoi)
            .ThenByDescending(x => x.MaKiemToan)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(log => new AuditLogListItemDto
            {
                Id = log.MaKiemToan,
                MaDonVi = log.MaDonVi,
                TenDonVi = _context.DonVis
                    .Where(organization => organization.MaDonVi == log.MaDonVi)
                    .Select(organization => organization.TenDonVi)
                    .FirstOrDefault(),
                EntityType = log.LoaiDoiTuong,
                EntityId = log.MaDoiTuong,
                Action = log.HanhDong,
                ChangedBy = log.NguoiThayDoi,
                ChangedByName = _context.NguoiDungs
                    .Where(user => user.MaNguoiDung == log.NguoiThayDoi)
                    .Select(user => user.HoTen)
                    .FirstOrDefault(),
                ChangedAt = log.ThoiDiemThayDoi,
                Description = log.MoTa,
                IpAddress = log.DiaChiIp
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AuditLogListItemDto>
        {
            Items = items,
            PageIndex = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AuditLogDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var row = await ApplyScope(
                _context.NhatKyKiemToans.AsNoTracking(),
                currentUser,
                allowedOrganizationIds)
            .Where(log => log.MaKiemToan == id)
            .Select(log => new AuditLogDetailDto
            {
                Id = log.MaKiemToan,
                MaDonVi = log.MaDonVi,
                TenDonVi = _context.DonVis
                    .Where(organization => organization.MaDonVi == log.MaDonVi)
                    .Select(organization => organization.TenDonVi)
                    .FirstOrDefault(),
                EntityType = log.LoaiDoiTuong,
                EntityId = log.MaDoiTuong,
                Action = log.HanhDong,
                ChangedBy = log.NguoiThayDoi,
                ChangedByName = _context.NguoiDungs
                    .Where(user => user.MaNguoiDung == log.NguoiThayDoi)
                    .Select(user => user.HoTen)
                    .FirstOrDefault(),
                ChangedAt = log.ThoiDiemThayDoi,
                Description = log.MoTa,
                IpAddress = log.DiaChiIp,
                OldValue = log.GiaTriCu,
                NewValue = log.GiaTriMoi,
                UserAgent = log.UserAgent,
                TraceId = log.TraceId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy audit log.");
        }

        return row;
    }

    private IQueryable<NhatKyKiemToan> ApplyFilters(
        IQueryable<NhatKyKiemToan> query,
        AuditLogQueryParameters parameters,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        query = ApplyScope(query, currentUser, allowedOrganizationIds);

        if (!string.IsNullOrWhiteSpace(parameters.EntityType))
        {
            var entityType = parameters.EntityType.Trim();
            query = query.Where(x => x.LoaiDoiTuong == entityType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.EntityId))
        {
            var entityId = parameters.EntityId.Trim();
            query = query.Where(x => x.MaDoiTuong == entityId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Action))
        {
            var action = parameters.Action.Trim();
            query = query.Where(x => x.HanhDong == action);
        }

        if (parameters.ChangedBy.HasValue)
        {
            query = query.Where(x => x.NguoiThayDoi == parameters.ChangedBy.Value);
        }

        if (parameters.MaDonVi.HasValue)
        {
            if (currentUser.Role == AuthRoles.CampusAdmin &&
                !allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem audit log của đơn vị này.");
            }

            query = query.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(x => x.ThoiDiemThayDoi >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(x => x.ThoiDiemThayDoi <= parameters.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.LoaiDoiTuong.ToLower().Contains(keyword) ||
                x.MaDoiTuong.ToLower().Contains(keyword) ||
                x.HanhDong.ToLower().Contains(keyword) ||
                (x.MoTa != null && x.MoTa.ToLower().Contains(keyword)) ||
                (x.DiaChiIp != null && x.DiaChiIp.Contains(keyword)) ||
                _context.DonVis.Any(organization =>
                    organization.MaDonVi == x.MaDonVi &&
                    organization.TenDonVi.ToLower().Contains(keyword)) ||
                _context.NguoiDungs.Any(user =>
                    user.MaNguoiDung == x.NguoiThayDoi &&
                    user.HoTen.ToLower().Contains(keyword)));
        }

        return query;
    }

    private IQueryable<NhatKyKiemToan> ApplyScope(
        IQueryable<NhatKyKiemToan> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        if (currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Admin)
        {
            return query;
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            return query.Where(x =>
                x.MaDonVi.HasValue &&
                allowedOrganizationIds.Contains(x.MaDonVi.Value));
        }

        throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem audit log.");
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Admin)
        {
            return [];
        }

        if (currentUser.Role != AuthRoles.CampusAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem audit log.");
        }

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

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }

    private static string? SerializeSanitized(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var node = JsonSerializer.SerializeToNode(value, JsonOptions);
        SanitizeNode(node);
        return node?.ToJsonString(JsonOptions);
    }

    private static void SanitizeNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var property in jsonObject.ToList())
                {
                    if (IsSensitiveProperty(property.Key))
                    {
                        jsonObject[property.Key] = "***";
                        continue;
                    }

                    SanitizeNode(property.Value);
                }

                break;
            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    SanitizeNode(item);
                }

                break;
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var normalized = propertyName
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return ExactSensitiveNames.Contains(normalized) ||
               ContainsSensitiveNames.Any(normalized.Contains);
    }

    private static string NormalizeRequired(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string TruncateRequired(string value, int maxLength)
    {
        var normalized = NormalizeRequired(value);
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? GetClientIp(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private void DetachPendingAuditLogs()
    {
        var pendingAuditEntries = _context.ChangeTracker
            .Entries<NhatKyKiemToan>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var entry in pendingAuditEntries)
        {
            entry.State = EntityState.Detached;
        }
    }
}

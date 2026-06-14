using System.Globalization;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Notifications;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Notifications;

public class NotificationService : INotificationService
{
    private const string ManualType = "manual";
    private const string SentStatus = "da_gui";

    private static readonly HashSet<string> ValidNotificationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "manual",
        "system",
        "schedule_changed",
        "session_cancelled",
        "attendance_unlock_approved",
        "attendance_unlock_rejected"
    };

    private static readonly HashSet<string> ValidLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "info",
        "warning",
        "important"
    };

    private static readonly HashSet<string> ValidTargetTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "users",
        "class",
        "course",
        "campus"
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public NotificationService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<NotificationDto>> GetMyNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(parameters.NgayTu, parameters.NgayDen);
        var currentUser = GetCurrentUser();
        var query = ApplyUserFilters(_context.ThongBaos.AsNoTracking()
            .Where(x => x.MaNguoiNhan == currentUser.UserId && x.TrangThai == SentStatus), parameters);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaThongBao)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<NotificationDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<NotificationDetailDto> GetMyNotificationDetailAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var notification = await _context.ThongBaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.MaThongBao == notificationId &&
                x.MaNguoiNhan == currentUser.UserId &&
                x.TrangThai == SentStatus,
                cancellationToken);

        if (notification is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        return ToDetailDto(notification);
    }

    public async Task<UnreadCountDto> GetMyUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var count = await _context.ThongBaos
            .CountAsync(x =>
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaDoc &&
                x.TrangThai == SentStatus,
                cancellationToken);

        return new UnreadCountDto { UnreadCount = count };
    }

    public async Task<NotificationDto> MarkAsReadAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var notification = await _context.ThongBaos
            .FirstOrDefaultAsync(x =>
                x.MaThongBao == notificationId &&
                x.MaNguoiNhan == currentUser.UserId &&
                x.TrangThai == SentStatus,
                cancellationToken);

        if (notification is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        if (!notification.DaDoc)
        {
            notification.DaDoc = true;
            notification.DocLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ToDto(notification);
    }

    public async Task<UnreadCountDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var notifications = await _context.ThongBaos
            .Where(x =>
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaDoc &&
                x.TrangThai == SentStatus)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.DaDoc = true;
            notification.DocLuc = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetMyUnreadCountAsync(cancellationToken);
    }

    public async Task<AdminNotificationDto> CreateManualNotificationAsync(
        CreateManualNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanCreateManualNotification(currentUser);
        var normalized = NormalizeManualRequest(request);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var recipients = await ResolveManualRecipientsAsync(
            normalized.TargetType,
            normalized.TargetIds,
            allowedOrganizationIds,
            cancellationToken);

        if (recipients.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không có người nhận hợp lệ cho thông báo.");
        }

        var groupId = Guid.NewGuid();
        var notifications = BuildNotifications(
            groupId,
            recipients,
            normalized.TieuDe,
            normalized.TomTat,
            normalized.NoiDungJson,
            normalized.NoiDungText,
            ManualType,
            normalized.MucDo,
            normalized.TargetType,
            normalized.TargetIds.Count == 1 ? normalized.TargetIds[0] : null,
            currentUser.UserId);

        await _context.ThongBaos.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            "ThongBao",
            groupId.ToString(),
            "CREATE_MANUAL_NOTIFICATION",
            null,
            new
            {
                MaNhomThongBao = groupId,
                normalized.TieuDe,
                normalized.TargetType,
                normalized.TargetIds,
                RecipientCount = recipients.Count
            },
            currentUser.UserId,
            currentUser.CampusId,
            "Tạo thông báo thủ công.",
            cancellationToken);

        return await GetAdminNotificationByGroupAsync(groupId, cancellationToken);
    }

    public async Task<PagedResultDto<AdminNotificationDto>> GetAdminNotificationsAsync(
        AdminNotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(parameters.NgayTu, parameters.NgayDen);
        var currentUser = GetCurrentUser();
        EnsureCanCreateManualNotification(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var rows = await ApplyAdminFilters(_context.ThongBaos.AsNoTracking()
                .Where(x => allowedOrganizationIds.Contains(x.MaDonVi)), parameters)
            .ToListAsync(cancellationToken);

        var groups = rows
            .GroupBy(x => x.MaNhomThongBao)
            .Select(ToAdminDto)
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaThongBao)
            .ToList();

        var totalItems = groups.Count;
        var items = groups
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return new PagedResultDto<AdminNotificationDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminNotificationDto> GetAdminNotificationDetailAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanCreateManualNotification(currentUser);
        var row = await _context.ThongBaos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaThongBao == notificationId, cancellationToken);

        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var groupRows = await _context.ThongBaos
            .AsNoTracking()
            .Where(x => x.MaNhomThongBao == row.MaNhomThongBao && allowedOrganizationIds.Contains(x.MaDonVi))
            .ToListAsync(cancellationToken);

        if (groupRows.Count == 0)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông báo thuộc cơ sở này.");
        }

        return ToAdminDto(groupRows.GroupBy(x => x.MaNhomThongBao).Single());
    }

    public async Task CreateSystemNotificationAsync(
        SystemNotificationRequest request,
        IReadOnlyCollection<int> recipientIds,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSystemRequest(request);
        var recipients = await GetActiveRecipientsAsync(recipientIds, cancellationToken);
        if (recipients.Count == 0)
        {
            return;
        }

        var notifications = BuildNotifications(
            Guid.NewGuid(),
            recipients,
            normalized.TieuDe,
            normalized.TomTat,
            normalized.NoiDungJson,
            normalized.NoiDungText,
            normalized.LoaiThongBao,
            normalized.MucDo,
            normalized.DoiTuongLienKet,
            normalized.MaDoiTuongLienKet,
            normalized.NguoiTao);

        await _context.ThongBaos.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task SendToUsersAsync(
        SystemNotificationRequest request,
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        return CreateSystemNotificationAsync(request, userIds, cancellationToken);
    }

    public async Task SendToClassAsync(
        SystemNotificationRequest request,
        int classId,
        IReadOnlyCollection<int>? additionalUserIds = null,
        CancellationToken cancellationToken = default)
    {
        var studentIds = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                x.MaLop == classId &&
                x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                x.TrangThai == UserStatuses.DbActive)
            .Select(x => x.MaNguoiDung)
            .ToListAsync(cancellationToken);

        var recipients = studentIds.Concat(additionalUserIds ?? []).ToHashSet();
        await CreateSystemNotificationAsync(request, recipients, cancellationToken);
    }

    public async Task SendToCourseAsync(
        SystemNotificationRequest request,
        int courseId,
        IReadOnlyCollection<int>? additionalUserIds = null,
        CancellationToken cancellationToken = default)
    {
        var course = await _context.KhoaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == courseId, cancellationToken);
        if (course is null)
        {
            return;
        }

        var studentIds = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                x.MaLop == course.MaLop &&
                x.MaDonVi == course.MaDonVi &&
                x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                x.TrangThai == UserStatuses.DbActive)
            .Select(x => x.MaNguoiDung)
            .ToListAsync(cancellationToken);

        var recipients = studentIds
            .Concat(additionalUserIds ?? [])
            .Append(course.MaGiaoVien)
            .ToHashSet();

        await CreateSystemNotificationAsync(request, recipients, cancellationToken);
    }

    public async Task SendToCampusAsync(
        SystemNotificationRequest request,
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var userIds = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => x.MaDonVi == organizationId && x.TrangThai == UserStatuses.DbActive)
            .Select(x => x.MaNguoiDung)
            .ToListAsync(cancellationToken);

        await CreateSystemNotificationAsync(request, userIds, cancellationToken);
    }

    private IQueryable<ThongBao> ApplyUserFilters(
        IQueryable<ThongBao> query,
        NotificationQueryParameters parameters)
    {
        if (parameters.DaDoc.HasValue)
        {
            query = query.Where(x => x.DaDoc == parameters.DaDoc.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiThongBao))
        {
            var type = NormalizeNotificationType(parameters.LoaiThongBao);
            query = query.Where(x => x.LoaiSuKien == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.MucDo))
        {
            var level = NormalizeLevel(parameters.MucDo);
            query = query.Where(x => x.MucDo == level);
        }

        return ApplyDateFilters(query, parameters.NgayTu, parameters.NgayDen);
    }

    private IQueryable<ThongBao> ApplyAdminFilters(
        IQueryable<ThongBao> query,
        AdminNotificationQueryParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.LoaiThongBao))
        {
            var type = NormalizeNotificationType(parameters.LoaiThongBao);
            query = query.Where(x => x.LoaiSuKien == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.MucDo))
        {
            var level = NormalizeLevel(parameters.MucDo);
            query = query.Where(x => x.MucDo == level);
        }

        return ApplyDateFilters(query, parameters.NgayTu, parameters.NgayDen);
    }

    private static IQueryable<ThongBao> ApplyDateFilters(
        IQueryable<ThongBao> query,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        if (startDate.HasValue)
        {
            query = query.Where(x => x.NgayTao >= startDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.NgayTao < endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        return query;
    }

    private async Task<AdminNotificationDto> GetAdminNotificationByGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var rows = await _context.ThongBaos
            .AsNoTracking()
            .Where(x => x.MaNhomThongBao == groupId)
            .ToListAsync(cancellationToken);

        return ToAdminDto(rows.GroupBy(x => x.MaNhomThongBao).Single());
    }

    private async Task<List<RecipientInfo>> ResolveManualRecipientsAsync(
        string targetType,
        IReadOnlyCollection<int> targetIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        return targetType switch
        {
            "users" => await ResolveUserRecipientsAsync(targetIds, allowedOrganizationIds, cancellationToken),
            "class" => await ResolveClassRecipientsAsync(targetIds, allowedOrganizationIds, cancellationToken),
            "course" => await ResolveCourseRecipientsAsync(targetIds, allowedOrganizationIds, cancellationToken),
            "campus" => await ResolveCampusRecipientsAsync(targetIds, allowedOrganizationIds, cancellationToken),
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "targetType không hợp lệ.")
        };
    }

    private async Task<List<RecipientInfo>> ResolveUserRecipientsAsync(
        IReadOnlyCollection<int> userIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var users = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => userIds.Contains(x.MaNguoiDung))
            .ToListAsync(cancellationToken);

        if (users.Count != userIds.Distinct().Count())
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách người nhận có user không tồn tại.");
        }

        if (users.Any(x => x.TrangThai != UserStatuses.DbActive))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách người nhận có user không hoạt động.");
        }

        if (users.Any(x => !allowedOrganizationIds.Contains(x.MaDonVi)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số người nhận.");
        }

        return users.Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi)).DistinctBy(x => x.UserId).ToList();
    }

    private async Task<List<RecipientInfo>> ResolveClassRecipientsAsync(
        IReadOnlyCollection<int> classIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var classes = await _context.LopHanhChinhs
            .AsNoTracking()
            .Where(x => classIds.Contains(x.MaLop))
            .ToListAsync(cancellationToken);

        if (classes.Count != classIds.Distinct().Count())
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách lớp có lớp không tồn tại.");
        }

        if (classes.Any(x => !allowedOrganizationIds.Contains(x.MaDonVi)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số lớp.");
        }

        var classIdSet = classes.Select(x => x.MaLop).ToHashSet();
        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                x.MaLop.HasValue &&
                classIdSet.Contains(x.MaLop.Value) &&
                x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> ResolveCourseRecipientsAsync(
        IReadOnlyCollection<int> courseIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var courses = await _context.KhoaHocs
            .AsNoTracking()
            .Where(x => courseIds.Contains(x.MaKhoaHoc))
            .ToListAsync(cancellationToken);

        if (courses.Count != courseIds.Distinct().Count())
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách khóa học có khóa học không tồn tại.");
        }

        if (courses.Any(x => !allowedOrganizationIds.Contains(x.MaDonVi)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số khóa học.");
        }

        var classIds = courses.Select(x => x.MaLop).ToHashSet();
        var recipients = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                x.MaLop.HasValue &&
                classIds.Contains(x.MaLop.Value) &&
                x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student) &&
                x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi))
            .ToListAsync(cancellationToken);

        var teacherIds = courses.Select(x => x.MaGiaoVien).ToHashSet();
        recipients.AddRange(await GetActiveRecipientsAsync(teacherIds, cancellationToken));
        return recipients.DistinctBy(x => x.UserId).ToList();
    }

    private async Task<List<RecipientInfo>> ResolveCampusRecipientsAsync(
        IReadOnlyCollection<int> organizationIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Any(x => !allowedOrganizationIds.Contains(x)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số cơ sở.");
        }

        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => organizationIds.Contains(x.MaDonVi) && x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> GetActiveRecipientsAsync(
        IReadOnlyCollection<int> recipientIds,
        CancellationToken cancellationToken)
    {
        if (recipientIds.Count == 0)
        {
            return [];
        }

        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => recipientIds.Contains(x.MaNguoiDung) && x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static List<ThongBao> BuildNotifications(
        Guid groupId,
        IReadOnlyCollection<RecipientInfo> recipients,
        string title,
        string? summary,
        string? contentJson,
        string? contentText,
        string type,
        string level,
        string? linkedEntityType,
        int? linkedEntityId,
        int? creatorId)
    {
        var now = DateTime.UtcNow;
        var body = FirstNonEmpty(contentText, summary, title);
        return recipients
            .GroupBy(x => x.UserId)
            .Select(x => x.First())
            .Select(recipient => new ThongBao
            {
                MaNhomThongBao = groupId,
                MaNguoiNhan = recipient.UserId,
                MaDonVi = recipient.OrganizationId,
                LoaiSuKien = type,
                TieuDe = title,
                TomTat = summary,
                NoiDung = body,
                NoiDungJson = contentJson,
                NoiDungText = contentText,
                MucDo = level,
                DoiTuongLienKet = linkedEntityType,
                MaDoiTuongLienKet = linkedEntityId,
                NguoiTao = creatorId,
                TrangThai = SentStatus,
                DaDoc = false,
                DocLuc = null,
                NgayTao = now
            })
            .ToList();
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

    private static void EnsureCanCreateManualNotification(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thông báo.");
        }
    }

    private static NormalizedManualRequest NormalizeManualRequest(CreateManualNotificationRequest request)
    {
        var title = RequireTrimmed(request.TieuDe, "Tiêu đề thông báo không được để trống.");
        var targetType = NormalizeTargetType(request.TargetType);
        if (request.TargetIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách targetIds không được để trống.");
        }

        var contentJson = NormalizeJson(request.NoiDungJson);
        var contentText = NormalizeOptional(request.NoiDungText);
        var summary = NormalizeOptional(request.TomTat);

        if (string.IsNullOrWhiteSpace(contentText) && string.IsNullOrWhiteSpace(summary))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cần cung cấp noiDungText hoặc tomTat cho thông báo.");
        }

        return new NormalizedManualRequest(
            title,
            summary,
            contentJson,
            contentText,
            NormalizeLevel(request.MucDo),
            targetType,
            request.TargetIds.Distinct().ToList());
    }

    private static SystemNotificationRequest NormalizeSystemRequest(SystemNotificationRequest request)
    {
        return new SystemNotificationRequest
        {
            TieuDe = RequireTrimmed(request.TieuDe, "Tiêu đề thông báo không được để trống."),
            TomTat = NormalizeOptional(request.TomTat),
            NoiDungText = NormalizeOptional(request.NoiDungText),
            NoiDungJson = NormalizeJson(request.NoiDungJson),
            LoaiThongBao = NormalizeNotificationType(request.LoaiThongBao),
            MucDo = NormalizeLevel(request.MucDo),
            DoiTuongLienKet = NormalizeOptional(request.DoiTuongLienKet),
            MaDoiTuongLienKet = request.MaDoiTuongLienKet,
            MaDonVi = request.MaDonVi,
            NguoiTao = request.NguoiTao
        };
    }

    private static string NormalizeNotificationType(string value)
    {
        var type = RequireTrimmed(value, "Loại thông báo không được để trống.").ToLowerInvariant();
        if (!ValidNotificationTypes.Contains(type))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại thông báo không hợp lệ.");
        }

        return type;
    }

    private static string NormalizeLevel(string value)
    {
        var level = RequireTrimmed(value, "Mức độ thông báo không được để trống.").ToLowerInvariant();
        if (!ValidLevels.Contains(level))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mức độ thông báo không hợp lệ.");
        }

        return level;
    }

    private static string NormalizeTargetType(string value)
    {
        var targetType = RequireTrimmed(value, "targetType không được để trống.").ToLowerInvariant();
        if (!ValidTargetTypes.Contains(targetType))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "targetType không hợp lệ.");
        }

        return targetType;
    }

    private static string? NormalizeJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            using var _ = JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "noiDungJson phải là JSON hợp lệ.");
        }

        return value.Trim();
    }

    private static string RequireTrimmed(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))!.Trim();
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    }

    private static NotificationDto ToDto(ThongBao notification)
    {
        return new NotificationDto
        {
            MaThongBao = notification.MaThongBao,
            MaNhomThongBao = notification.MaNhomThongBao,
            TieuDe = notification.TieuDe,
            TomTat = notification.TomTat,
            LoaiThongBao = notification.LoaiSuKien,
            MucDo = notification.MucDo,
            DoiTuongLienKet = notification.DoiTuongLienKet,
            MaDoiTuongLienKet = notification.MaDoiTuongLienKet,
            DaDoc = notification.DaDoc,
            DocLuc = notification.DocLuc,
            NgayTao = notification.NgayTao
        };
    }

    private static NotificationDetailDto ToDetailDto(ThongBao notification)
    {
        var dto = new NotificationDetailDto
        {
            NoiDung = notification.NoiDung,
            NoiDungJson = notification.NoiDungJson,
            NoiDungText = notification.NoiDungText
        };

        var baseDto = ToDto(notification);
        dto.MaThongBao = baseDto.MaThongBao;
        dto.MaNhomThongBao = baseDto.MaNhomThongBao;
        dto.TieuDe = baseDto.TieuDe;
        dto.TomTat = baseDto.TomTat;
        dto.LoaiThongBao = baseDto.LoaiThongBao;
        dto.MucDo = baseDto.MucDo;
        dto.DoiTuongLienKet = baseDto.DoiTuongLienKet;
        dto.MaDoiTuongLienKet = baseDto.MaDoiTuongLienKet;
        dto.DaDoc = baseDto.DaDoc;
        dto.DocLuc = baseDto.DocLuc;
        dto.NgayTao = baseDto.NgayTao;
        return dto;
    }

    private static AdminNotificationDto ToAdminDto(IGrouping<Guid, ThongBao> group)
    {
        var rows = group.ToList();
        var first = rows.OrderBy(x => x.MaThongBao).First();
        return new AdminNotificationDto
        {
            MaThongBao = first.MaThongBao,
            MaNhomThongBao = group.Key,
            TieuDe = first.TieuDe,
            TomTat = first.TomTat,
            LoaiThongBao = first.LoaiSuKien,
            MucDo = first.MucDo,
            DoiTuongLienKet = first.DoiTuongLienKet,
            MaDoiTuongLienKet = first.MaDoiTuongLienKet,
            RecipientCount = rows.Count,
            ReadCount = rows.Count(x => x.DaDoc),
            UnreadCount = rows.Count(x => !x.DaDoc),
            NguoiTao = first.NguoiTao,
            NgayTao = first.NgayTao
        };
    }

    private sealed record RecipientInfo(int UserId, int OrganizationId);

    private sealed record NormalizedManualRequest(
        string TieuDe,
        string? TomTat,
        string? NoiDungJson,
        string? NoiDungText,
        string MucDo,
        string TargetType,
        IReadOnlyList<int> TargetIds);
}

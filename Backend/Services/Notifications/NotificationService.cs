using System.Text;
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
        var startDate = parameters.FromDate ?? parameters.NgayTu;
        var endDate = parameters.ToDate ?? parameters.NgayDen;
        ValidateDateRange(startDate, endDate);

        var currentUser = GetCurrentUser();
        var query = _context.ThongBaoNguoiNhans
            .AsNoTracking()
            .Include(x => x.ThongBao)
            .Where(x =>
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaAn &&
                x.ThongBao != null &&
                x.ThongBao.TrangThai == NotificationConstants.Statuses.Sent);

        var readFilter = parameters.IsRead ?? parameters.DaDoc;
        if (readFilter.HasValue)
        {
            query = query.Where(x => x.DaDoc == readFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiThongBao))
        {
            var type = NormalizeNotificationType(parameters.LoaiThongBao);
            query = query.Where(x => x.ThongBao!.LoaiThongBao == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.MucDo))
        {
            var level = NormalizeLevel(parameters.MucDo);
            query = query.Where(x => x.ThongBao!.MucDo == level);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.NhanLuc >= startDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.NhanLuc < endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ThongBao!.TieuDe != null && x.ThongBao.TieuDe.ToLower().Contains(keyword) ||
                x.ThongBao!.TomTatNoiDung != null && x.ThongBao.TomTatNoiDung.ToLower().Contains(keyword) ||
                x.ThongBao!.NoiDungText != null && x.ThongBao.NoiDungText.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.NhanLuc)
            .ThenByDescending(x => x.MaThongBaoNguoiNhan)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<NotificationDto>
        {
            Items = rows.Select(x => ToDto(x.ThongBao!, x)).ToList(),
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
        var row = await _context.ThongBaoNguoiNhans
            .AsNoTracking()
            .Include(x => x.ThongBao)
            .FirstOrDefaultAsync(x =>
                x.MaThongBao == notificationId &&
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaAn &&
                x.ThongBao != null &&
                x.ThongBao.TrangThai == NotificationConstants.Statuses.Sent,
                cancellationToken);

        if (row?.ThongBao is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        return ToDetailDto(row.ThongBao, row);
    }

    public async Task<UnreadCountDto> GetMyUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var count = await _context.ThongBaoNguoiNhans.CountAsync(x =>
            x.MaNguoiNhan == currentUser.UserId &&
            !x.DaDoc &&
            !x.DaAn &&
            x.ThongBao != null &&
            x.ThongBao.TrangThai == NotificationConstants.Statuses.Sent,
            cancellationToken);

        return new UnreadCountDto { UnreadCount = count };
    }

    public async Task<NotificationDto> MarkAsReadAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var row = await _context.ThongBaoNguoiNhans
            .Include(x => x.ThongBao)
            .FirstOrDefaultAsync(x =>
                x.MaThongBao == notificationId &&
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaAn &&
                x.ThongBao != null &&
                x.ThongBao.TrangThai == NotificationConstants.Statuses.Sent,
                cancellationToken);

        if (row?.ThongBao is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        if (!row.DaDoc)
        {
            row.DaDoc = true;
            row.DocLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ToDto(row.ThongBao, row);
    }

    public async Task<UnreadCountDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;
        var rows = await _context.ThongBaoNguoiNhans
            .Where(x =>
                x.MaNguoiNhan == currentUser.UserId &&
                !x.DaDoc &&
                !x.DaAn &&
                x.ThongBao != null &&
                x.ThongBao.TrangThai == NotificationConstants.Statuses.Sent)
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.DaDoc = true;
            row.DocLuc = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetMyUnreadCountAsync(cancellationToken);
    }

    public async Task HideAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var row = await _context.ThongBaoNguoiNhans
            .FirstOrDefaultAsync(x =>
                x.MaThongBao == notificationId &&
                x.MaNguoiNhan == currentUser.UserId,
                cancellationToken);

        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        if (!row.DaAn)
        {
            row.DaAn = true;
            row.AnLuc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<NotificationRecipientPreviewResultDto> PreviewRecipientsAsync(
        PreviewNotificationRecipientsRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageNotifications(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var recipients = await ResolveRecipientsAsync(request, currentUser, allowedOrganizationIds, cancellationToken);

        return new NotificationRecipientPreviewResultDto
        {
            Count = recipients.Count,
            Recipients = recipients
                .OrderBy(x => x.HoTen)
                .Take(100)
                .Select(ToPreviewDto)
                .ToList()
        };
    }

    public async Task<AdminNotificationDto> CreateManualNotificationAsync(
        CreateManualNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageNotifications(currentUser);
        var normalized = NormalizeManualRequest(request);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var recipients = await ResolveRecipientsAsync(normalized.RecipientRequest, currentUser, allowedOrganizationIds, cancellationToken);

        if (recipients.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không có người nhận hợp lệ cho thông báo.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        var notificationId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var notification = BuildNotification(normalized, recipients, currentUser.UserId);
            await _context.ThongBaos.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var recipientRows = recipients
                .Select(recipient => new ThongBaoNguoiNhan
                {
                    MaThongBao = notification.MaThongBao,
                    MaNguoiNhan = recipient.UserId,
                    MaDonVi = recipient.OrganizationId,
                    DaDoc = false,
                    DaAn = false,
                    NhanLuc = notification.GuiLuc ?? notification.NgayTao,
                    NgayTao = notification.NgayTao
                })
                .ToList();

            await _context.ThongBaoNguoiNhans.AddRangeAsync(recipientRows, cancellationToken);
            await _context.NhatKyThongBaos.AddRangeAsync(recipientRows.Select(x => new NhatKyThongBao
            {
                MaThongBao = notification.MaThongBao,
                MaNguoiNhan = x.MaNguoiNhan,
                MaDonVi = x.MaDonVi,
                TrangThai = "da_gui",
                KenhGui = "thong_bao_day",
                GuiLuc = notification.GuiLuc
            }), cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return notification.MaThongBao;
        });

        await _auditLogService.LogAsync(
            "ThongBao",
            notificationId.ToString(),
            "CREATE_NOTIFICATION",
            null,
            new
            {
                MaThongBao = notificationId,
                normalized.TieuDe,
                normalized.LoaiThongBao,
                normalized.PhamViGui,
                RecipientCount = recipients.Count
            },
            currentUser.UserId,
            normalized.MaDonVi ?? recipients.First().OrganizationId,
            "Tạo và gửi thông báo.",
            cancellationToken);

        return await GetAdminNotificationDetailAsync(notificationId, cancellationToken);
    }

    public async Task<PagedResultDto<AdminNotificationDto>> GetAdminNotificationsAsync(
        AdminNotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var startDate = parameters.FromDate ?? parameters.NgayTu;
        var endDate = parameters.ToDate ?? parameters.NgayDen;
        ValidateDateRange(startDate, endDate);

        var currentUser = GetCurrentUser();
        EnsureCanManageNotifications(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyAdminFilters(_context.ThongBaos.AsNoTracking(), parameters, allowedOrganizationIds, startDate, endDate);

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.GuiLuc ?? x.NgayTao)
            .ThenByDescending(x => x.MaThongBao)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new
            {
                Notification = x,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null,
                TenNguoiTao = x.NguoiTaoNavigation != null ? x.NguoiTaoNavigation.HoTen : null,
                RecipientCount = x.NguoiNhans.Count,
                ReadCount = x.NguoiNhans.Count(r => r.DaDoc),
                HiddenCount = x.NguoiNhans.Count(r => r.DaAn)
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminNotificationDto>
        {
            Items = rows.Select(x => ToAdminDto(
                x.Notification,
                x.RecipientCount,
                x.ReadCount,
                x.HiddenCount,
                x.TenDonVi,
                x.TenNguoiTao)).ToList(),
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
        EnsureCanManageNotifications(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var row = await _context.ThongBaos
            .AsNoTracking()
            .Where(x => x.MaThongBao == notificationId)
            .Select(x => new
            {
                Notification = x,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null,
                TenNguoiTao = x.NguoiTaoNavigation != null ? x.NguoiTaoNavigation.HoTen : null,
                RecipientCount = x.NguoiNhans.Count,
                ReadCount = x.NguoiNhans.Count(r => r.DaDoc),
                HiddenCount = x.NguoiNhans.Count(r => r.DaAn)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        EnsureNotificationInScope(row.Notification, allowedOrganizationIds);
        return ToAdminDto(row.Notification, row.RecipientCount, row.ReadCount, row.HiddenCount, row.TenDonVi, row.TenNguoiTao);
    }

    public async Task<PagedResultDto<AdminNotificationRecipientDto>> GetAdminNotificationRecipientsAsync(
        int notificationId,
        AdminNotificationRecipientQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var notification = await GetScopedNotificationAsync(notificationId, cancellationToken);
        var query = _context.ThongBaoNguoiNhans
            .AsNoTracking()
            .Where(x => x.MaThongBao == notification.MaThongBao);

        if (parameters.DaDoc.HasValue)
        {
            query = query.Where(x => x.DaDoc == parameters.DaDoc.Value);
        }

        if (parameters.DaAn.HasValue)
        {
            query = query.Where(x => x.DaAn == parameters.DaAn.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.NguoiNhan != null &&
                (x.NguoiNhan.HoTen.ToLower().Contains(keyword) ||
                 x.NguoiNhan.Email.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.NguoiNhan!.HoTen)
            .ThenBy(x => x.MaNguoiNhan)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new AdminNotificationRecipientDto
            {
                MaThongBaoNguoiNhan = x.MaThongBaoNguoiNhan,
                MaThongBao = x.MaThongBao,
                MaNguoiNhan = x.MaNguoiNhan,
                HoTen = x.NguoiNhan != null ? x.NguoiNhan.HoTen : string.Empty,
                Email = x.NguoiNhan != null ? x.NguoiNhan.Email : string.Empty,
                VaiTroChinh = x.NguoiNhan != null ? x.NguoiNhan.VaiTroChinh : string.Empty,
                MaDonVi = x.MaDonVi,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null,
                DaDoc = x.DaDoc,
                DocLuc = x.DocLuc,
                DaAn = x.DaAn,
                AnLuc = x.AnLuc,
                NhanLuc = x.NhanLuc
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminNotificationRecipientDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<NotificationStatisticsDto> GetAdminNotificationStatisticsAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await GetScopedNotificationAsync(notificationId, cancellationToken);
        var rows = _context.ThongBaoNguoiNhans.AsNoTracking().Where(x => x.MaThongBao == notification.MaThongBao);
        var total = await rows.CountAsync(cancellationToken);
        var read = await rows.CountAsync(x => x.DaDoc, cancellationToken);
        var hidden = await rows.CountAsync(x => x.DaAn, cancellationToken);

        return new NotificationStatisticsDto
        {
            MaThongBao = notification.MaThongBao,
            TongNguoiNhan = total,
            TongDaDoc = read,
            TongChuaDoc = total - read,
            TongDaAn = hidden
        };
    }

    public async Task<AdminNotificationDto> CancelNotificationAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await GetScopedNotificationAsync(notificationId, cancellationToken);
        if (notification.TrangThai == NotificationConstants.Statuses.Sent)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể hủy thông báo đã gửi.");
        }

        if (notification.TrangThai != NotificationConstants.Statuses.Cancelled)
        {
            notification.TrangThai = NotificationConstants.Statuses.Cancelled;
            notification.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            var currentUser = GetCurrentUser();
            await _auditLogService.LogAsync(
                "ThongBao",
                notification.MaThongBao.ToString(),
                "CANCEL_NOTIFICATION",
                null,
                new { notification.MaThongBao, notification.TrangThai },
                currentUser.UserId,
                notification.MaDonVi,
                "Hủy thông báo.",
                cancellationToken);
        }

        return await GetAdminNotificationDetailAsync(notification.MaThongBao, cancellationToken);
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

        await CreateNotificationAsync(normalized, recipients, cancellationToken);
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
        var course = await _context.KhoaHocs.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == courseId, cancellationToken);
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

    private async Task CreateNotificationAsync(
        NormalizedNotification normalized,
        IReadOnlyCollection<RecipientInfo> recipients,
        CancellationToken cancellationToken)
    {
        if (recipients.Count == 0)
        {
            return;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var notification = BuildNotification(normalized, recipients, normalized.NguoiTao);
            await _context.ThongBaos.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var recipientRows = recipients
                .GroupBy(x => x.UserId)
                .Select(x => x.First())
                .Select(recipient => new ThongBaoNguoiNhan
                {
                    MaThongBao = notification.MaThongBao,
                    MaNguoiNhan = recipient.UserId,
                    MaDonVi = recipient.OrganizationId,
                    NhanLuc = notification.GuiLuc ?? notification.NgayTao,
                    NgayTao = notification.NgayTao
                })
                .ToList();

            await _context.ThongBaoNguoiNhans.AddRangeAsync(recipientRows, cancellationToken);
            await _context.NhatKyThongBaos.AddRangeAsync(recipientRows.Select(x => new NhatKyThongBao
            {
                MaThongBao = notification.MaThongBao,
                MaNguoiNhan = x.MaNguoiNhan,
                MaDonVi = x.MaDonVi,
                TrangThai = "da_gui",
                KenhGui = "thong_bao_day",
                GuiLuc = notification.GuiLuc
            }), cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private IQueryable<ThongBao> ApplyAdminFilters(
        IQueryable<ThongBao> query,
        AdminNotificationQueryParameters parameters,
        HashSet<int> allowedOrganizationIds,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        query = query.Where(x => allowedOrganizationIds.Contains(x.MaDonVi));

        var requestedOrganizationId = parameters.MaDonVi ?? parameters.CampusId;
        if (requestedOrganizationId.HasValue)
        {
            if (!allowedOrganizationIds.Contains(requestedOrganizationId.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông báo của đơn vị này.");
            }

            query = query.Where(x => x.MaDonVi == requestedOrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiThongBao))
        {
            var type = NormalizeNotificationType(parameters.LoaiThongBao);
            query = query.Where(x => x.LoaiThongBao == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.MucDo))
        {
            var level = NormalizeLevel(parameters.MucDo);
            query = query.Where(x => x.MucDo == level);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.TrangThai == status);
        }

        if (parameters.NguoiTao.HasValue)
        {
            query = query.Where(x => x.NguoiTao == parameters.NguoiTao.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.NgayTao >= startDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.NgayTao < endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.TieuDe != null && x.TieuDe.ToLower().Contains(keyword) ||
                x.TomTatNoiDung != null && x.TomTatNoiDung.ToLower().Contains(keyword) ||
                x.NoiDungText != null && x.NoiDungText.ToLower().Contains(keyword));
        }

        return query;
    }

    private async Task<ThongBao> GetScopedNotificationAsync(int notificationId, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageNotifications(currentUser);
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var notification = await _context.ThongBaos.FirstOrDefaultAsync(x => x.MaThongBao == notificationId, cancellationToken);
        if (notification is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy thông báo.");
        }

        EnsureNotificationInScope(notification, allowedOrganizationIds);
        return notification;
    }

    private static void EnsureNotificationInScope(ThongBao notification, HashSet<int> allowedOrganizationIds)
    {
        if (!allowedOrganizationIds.Contains(notification.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem thông báo thuộc đơn vị này.");
        }
    }

    private async Task<List<RecipientInfo>> ResolveRecipientsAsync(
        PreviewNotificationRecipientsRequest request,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var scope = NormalizeScope(string.IsNullOrWhiteSpace(request.PhamViGui) ? request.TargetType : request.PhamViGui);
        return scope switch
        {
            NotificationConstants.Scopes.AllSystem => await ResolveAllSystemRecipientsAsync(currentUser, cancellationToken),
            NotificationConstants.Scopes.Organization or NotificationConstants.Scopes.LegacyCampus =>
                await ResolveOrganizationRecipientsAsync(GetScopeIds(request), allowedOrganizationIds, cancellationToken),
            NotificationConstants.Scopes.Class or NotificationConstants.Scopes.LegacyClass =>
                await ResolveClassRecipientsAsync(request.TargetIds, allowedOrganizationIds, cancellationToken),
            NotificationConstants.Scopes.Course or NotificationConstants.Scopes.LegacyCourse =>
                await ResolveCourseRecipientsAsync(request.TargetIds, allowedOrganizationIds, cancellationToken),
            NotificationConstants.Scopes.User or NotificationConstants.Scopes.LegacyUsers =>
                await ResolveUserRecipientsAsync(request.TargetIds, allowedOrganizationIds, cancellationToken),
            NotificationConstants.Scopes.Role =>
                await ResolveRoleRecipientsAsync(request.RoleCodes, request.MaDonVi ?? request.CampusId, allowedOrganizationIds, cancellationToken),
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Phạm vi gửi thông báo không hợp lệ.")
        };
    }

    private async Task<List<RecipientInfo>> ResolveAllSystemRecipientsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được gửi thông báo toàn hệ thống.");
        }

        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> ResolveOrganizationRecipientsAsync(
        IReadOnlyCollection<int> organizationIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách đơn vị nhận không được để trống.");
        }

        if (organizationIds.Any(x => !allowedOrganizationIds.Contains(x)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số đơn vị.");
        }

        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => organizationIds.Contains(x.MaDonVi) && x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> ResolveRoleRecipientsAsync(
        IReadOnlyCollection<string> roleCodes,
        int? organizationId,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var normalizedRoles = roleCodes
            .Select(x => AuthRoles.ToDatabaseCode(RequireTrimmed(x, "Vai trò nhận không được để trống.")))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedRoles.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách vai trò nhận không được để trống.");
        }

        var scopedOrganizationIds = allowedOrganizationIds;
        if (organizationId.HasValue)
        {
            if (!allowedOrganizationIds.Contains(organizationId.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới đơn vị này.");
            }

            scopedOrganizationIds = [organizationId.Value];
        }

        return await _context.NguoiDungs
            .AsNoTracking()
            .Where(x =>
                scopedOrganizationIds.Contains(x.MaDonVi) &&
                normalizedRoles.Contains(x.VaiTroChinh) &&
                x.TrangThai == UserStatuses.DbActive)
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> ResolveUserRecipientsAsync(
        IReadOnlyCollection<int> userIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = userIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách người nhận không được để trống.");
        }

        var users = await _context.NguoiDungs
            .AsNoTracking()
            .Where(x => distinctIds.Contains(x.MaNguoiDung))
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null, x.TrangThai))
            .ToListAsync(cancellationToken);

        if (users.Count != distinctIds.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách người nhận có user không tồn tại.");
        }

        if (users.Any(x => x.Status != UserStatuses.DbActive))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách người nhận có user không hoạt động.");
        }

        if (users.Any(x => !allowedOrganizationIds.Contains(x.OrganizationId)))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền gửi thông báo tới một số người nhận.");
        }

        return users.DistinctBy(x => x.UserId).ToList();
    }

    private async Task<List<RecipientInfo>> ResolveClassRecipientsAsync(
        IReadOnlyCollection<int> classIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = classIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách lớp không được để trống.");
        }

        var classes = await _context.LopHanhChinhs.AsNoTracking().Where(x => distinctIds.Contains(x.MaLop)).ToListAsync(cancellationToken);
        if (classes.Count != distinctIds.Count)
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
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RecipientInfo>> ResolveCourseRecipientsAsync(
        IReadOnlyCollection<int> courseIds,
        HashSet<int> allowedOrganizationIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = courseIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách khóa học không được để trống.");
        }

        var courses = await _context.KhoaHocs.AsNoTracking().Where(x => distinctIds.Contains(x.MaKhoaHoc)).ToListAsync(cancellationToken);
        if (courses.Count != distinctIds.Count)
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
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .ToListAsync(cancellationToken);

        var teacherIds = courses.Select(x => x.MaGiaoVien).ToHashSet();
        recipients.AddRange(await GetActiveRecipientsAsync(teacherIds, cancellationToken));
        return recipients.DistinctBy(x => x.UserId).ToList();
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
            .Select(x => new RecipientInfo(x.MaNguoiDung, x.MaDonVi, x.HoTen, x.Email, x.VaiTroChinh, x.DonVi != null ? x.DonVi.TenDonVi : null))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyCollection<int> GetScopeIds(PreviewNotificationRecipientsRequest request)
    {
        if (request.TargetIds.Count > 0)
        {
            return request.TargetIds;
        }

        var id = request.MaDonVi ?? request.CampusId;
        return id.HasValue ? [id.Value] : [];
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _context.DonVis.AsNoTracking().Select(x => x.MaDonVi).ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role == AuthRoles.CampusAdmin)
        {
            var organizations = await _context.DonVis.AsNoTracking().Select(x => new { x.MaDonVi, x.MaDonViCha }).ToListAsync(cancellationToken);
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

        if (currentUser.Role is AuthRoles.Admin or AuthRoles.AcademicStaff)
        {
            return [currentUser.CampusId];
        }

        throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thông báo.");
    }

    private static void EnsureCanManageNotifications(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý thông báo.");
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

    private static NormalizedNotification NormalizeManualRequest(CreateManualNotificationRequest request)
    {
        var scope = NormalizeScope(string.IsNullOrWhiteSpace(request.PhamViGui) ? request.TargetType : request.PhamViGui);
        var content = NormalizeContent(request.TieuDe, request.TomTatNoiDung ?? request.TomTat, request.NoiDungJson, request.NoiDungText);
        var targetIds = request.TargetIds.Distinct().ToList();
        return new NormalizedNotification(
            TieuDe: content.Title,
            TomTatNoiDung: content.Summary,
            NoiDungJson: content.ContentJson,
            NoiDungText: content.ContentText,
            LoaiThongBao: NormalizeNotificationType(request.LoaiThongBao),
            MucDo: NormalizeLevel(request.MucDo),
            PhamViGui: scope,
            LoaiDoiTuongLienKet: NormalizeOptional(request.LoaiDoiTuongLienKet ?? request.DoiTuongLienKet),
            MaDoiTuongLienKet: request.MaDoiTuongLienKet,
            DuongDan: NormalizeOptional(request.DuongDan),
            MaDonVi: request.MaDonVi ?? request.CampusId,
            NguoiTao: null,
            RecipientRequest: new PreviewNotificationRecipientsRequest
            {
                PhamViGui = scope,
                TargetType = scope,
                TargetIds = targetIds,
                RoleCodes = request.RoleCodes,
                MaDonVi = request.MaDonVi,
                CampusId = request.CampusId
            });
    }

    private static NormalizedNotification NormalizeSystemRequest(SystemNotificationRequest request)
    {
        var content = NormalizeContent(request.TieuDe, request.TomTat, request.NoiDungJson, request.NoiDungText);
        return new NormalizedNotification(
            TieuDe: content.Title,
            TomTatNoiDung: content.Summary,
            NoiDungJson: content.ContentJson,
            NoiDungText: content.ContentText,
            LoaiThongBao: NormalizeNotificationType(request.LoaiThongBao),
            MucDo: NormalizeLevel(request.MucDo),
            PhamViGui: NotificationConstants.Scopes.User,
            LoaiDoiTuongLienKet: NormalizeOptional(request.DoiTuongLienKet),
            MaDoiTuongLienKet: request.MaDoiTuongLienKet,
            DuongDan: NormalizeOptional(request.DuongDan),
            MaDonVi: request.MaDonVi,
            NguoiTao: request.NguoiTao,
            RecipientRequest: new PreviewNotificationRecipientsRequest { PhamViGui = NotificationConstants.Scopes.User });
    }

    private static ContentParts NormalizeContent(string? title, string? summary, string? contentJson, string? contentText)
    {
        var normalizedTitle = RequireTrimmed(title, "Tiêu đề thông báo không được để trống.");
        var normalizedJson = NormalizeEditorJson(contentJson, out var extractedText);
        var normalizedText = NormalizeOptional(contentText) ?? extractedText;
        var normalizedSummary = NormalizeOptional(summary) ?? MakeSummary(normalizedText);

        if (string.IsNullOrWhiteSpace(normalizedText) && string.IsNullOrWhiteSpace(normalizedSummary))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cần cung cấp nội dung thông báo.");
        }

        return new ContentParts(normalizedTitle, normalizedSummary, normalizedJson, normalizedText);
    }

    private static string? NormalizeEditorJson(string? value, out string? extractedText)
    {
        extractedText = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 200_000)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "noiDungJson vượt quá kích thước cho phép.");
        }

        if (normalized.Contains("data:image", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "noiDungJson không được chứa ảnh base64.");
        }

        try
        {
            using var document = JsonDocument.Parse(normalized);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "noiDungJson phải là JSON object hợp lệ.");
            }

            if (document.RootElement.TryGetProperty("blocks", out var blocks) &&
                blocks.ValueKind != JsonValueKind.Array)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "blocks trong noiDungJson phải là array.");
            }

            extractedText = ExtractEditorText(document.RootElement);
            return normalized;
        }
        catch (JsonException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "noiDungJson phải là JSON hợp lệ.");
        }
    }

    private static string? ExtractEditorText(JsonElement root)
    {
        if (!root.TryGetProperty("blocks", out var blocks) || blocks.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (var block in blocks.EnumerateArray())
        {
            if (!block.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            AppendStringProperty(builder, data, "text");
            AppendStringProperty(builder, data, "caption");
            if (data.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        AppendText(builder, item.GetString());
                    }
                    else if (item.ValueKind == JsonValueKind.Object)
                    {
                        AppendStringProperty(builder, item, "content");
                        AppendStringProperty(builder, item, "text");
                    }
                }
            }

            if (data.TryGetProperty("content", out var table) && table.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in table.EnumerateArray())
                {
                    if (row.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var cell in row.EnumerateArray())
                    {
                        if (cell.ValueKind == JsonValueKind.String)
                        {
                            AppendText(builder, cell.GetString());
                        }
                    }
                }
            }
        }

        var text = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static void AppendStringProperty(StringBuilder builder, JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        {
            AppendText(builder, value.GetString());
        }
    }

    private static void AppendText(StringBuilder builder, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(value.Trim());
    }

    private static string? MakeSummary(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        return normalized.Length <= 250 ? normalized : normalized[..250];
    }

    private static ThongBao BuildNotification(
        NormalizedNotification normalized,
        IReadOnlyCollection<RecipientInfo> recipients,
        int? creatorId)
    {
        var now = DateTime.UtcNow;
        var firstRecipient = recipients.First();
        var organizationId = normalized.MaDonVi ?? firstRecipient.OrganizationId;
        var legacyRecipient = firstRecipient.UserId;
        var body = normalized.NoiDungText ?? normalized.TomTatNoiDung ?? normalized.TieuDe;
        return new ThongBao
        {
            MaNhomThongBao = Guid.NewGuid(),
            MaNguoiNhan = legacyRecipient,
            MaDonVi = organizationId,
            LoaiSuKien = normalized.LoaiThongBao,
            LoaiThongBao = normalized.LoaiThongBao,
            TieuDe = normalized.TieuDe,
            TomTat = normalized.TomTatNoiDung,
            TomTatNoiDung = normalized.TomTatNoiDung,
            NoiDung = body,
            NoiDungJson = normalized.NoiDungJson,
            NoiDungText = normalized.NoiDungText,
            MucDo = normalized.MucDo,
            DoiTuongLienKet = normalized.LoaiDoiTuongLienKet,
            LoaiDoiTuongLienKet = normalized.LoaiDoiTuongLienKet,
            MaDoiTuongLienKet = normalized.MaDoiTuongLienKet,
            PhamViGui = normalized.PhamViGui,
            DuongDan = normalized.DuongDan,
            NguoiTao = creatorId ?? normalized.NguoiTao,
            TrangThai = NotificationConstants.Statuses.Sent,
            DaDoc = false,
            DocLuc = null,
            GuiLuc = now,
            NgayTao = now
        };
    }

    private static NotificationDto ToDto(ThongBao notification, ThongBaoNguoiNhan recipient)
    {
        return new NotificationDto
        {
            MaThongBao = notification.MaThongBao,
            MaNhomThongBao = notification.MaNhomThongBao,
            TieuDe = notification.TieuDe,
            TomTat = notification.TomTatNoiDung ?? notification.TomTat,
            TomTatNoiDung = notification.TomTatNoiDung ?? notification.TomTat,
            LoaiThongBao = notification.LoaiThongBao,
            MucDo = notification.MucDo,
            DoiTuongLienKet = notification.LoaiDoiTuongLienKet ?? notification.DoiTuongLienKet,
            LoaiDoiTuongLienKet = notification.LoaiDoiTuongLienKet ?? notification.DoiTuongLienKet,
            MaDoiTuongLienKet = notification.MaDoiTuongLienKet,
            DuongDan = notification.DuongDan,
            DaDoc = recipient.DaDoc,
            DocLuc = recipient.DocLuc,
            DaAn = recipient.DaAn,
            AnLuc = recipient.AnLuc,
            NhanLuc = recipient.NhanLuc,
            NgayTao = notification.NgayTao
        };
    }

    private static NotificationDetailDto ToDetailDto(ThongBao notification, ThongBaoNguoiNhan recipient)
    {
        var dto = new NotificationDetailDto
        {
            NoiDung = notification.NoiDung,
            NoiDungJson = notification.NoiDungJson,
            NoiDungText = notification.NoiDungText
        };
        var baseDto = ToDto(notification, recipient);
        dto.MaThongBao = baseDto.MaThongBao;
        dto.MaNhomThongBao = baseDto.MaNhomThongBao;
        dto.TieuDe = baseDto.TieuDe;
        dto.TomTat = baseDto.TomTat;
        dto.TomTatNoiDung = baseDto.TomTatNoiDung;
        dto.LoaiThongBao = baseDto.LoaiThongBao;
        dto.MucDo = baseDto.MucDo;
        dto.DoiTuongLienKet = baseDto.DoiTuongLienKet;
        dto.LoaiDoiTuongLienKet = baseDto.LoaiDoiTuongLienKet;
        dto.MaDoiTuongLienKet = baseDto.MaDoiTuongLienKet;
        dto.DuongDan = baseDto.DuongDan;
        dto.DaDoc = baseDto.DaDoc;
        dto.DocLuc = baseDto.DocLuc;
        dto.DaAn = baseDto.DaAn;
        dto.AnLuc = baseDto.AnLuc;
        dto.NhanLuc = baseDto.NhanLuc;
        dto.NgayTao = baseDto.NgayTao;
        return dto;
    }

    private static AdminNotificationDto ToAdminDto(
        ThongBao notification,
        int recipientCount,
        int readCount,
        int hiddenCount,
        string? organizationName,
        string? creatorName)
    {
        return new AdminNotificationDto
        {
            MaThongBao = notification.MaThongBao,
            MaNhomThongBao = notification.MaNhomThongBao,
            TieuDe = notification.TieuDe,
            TomTat = notification.TomTatNoiDung ?? notification.TomTat,
            TomTatNoiDung = notification.TomTatNoiDung ?? notification.TomTat,
            LoaiThongBao = notification.LoaiThongBao,
            MucDo = notification.MucDo,
            DoiTuongLienKet = notification.LoaiDoiTuongLienKet ?? notification.DoiTuongLienKet,
            LoaiDoiTuongLienKet = notification.LoaiDoiTuongLienKet ?? notification.DoiTuongLienKet,
            MaDoiTuongLienKet = notification.MaDoiTuongLienKet,
            PhamViGui = notification.PhamViGui,
            TrangThai = notification.TrangThai,
            MaDonVi = notification.MaDonVi,
            TenDonVi = organizationName,
            DuongDan = notification.DuongDan,
            RecipientCount = recipientCount,
            ReadCount = readCount,
            UnreadCount = recipientCount - readCount,
            HiddenCount = hiddenCount,
            NguoiTao = notification.NguoiTao,
            TenNguoiTao = creatorName,
            GuiLuc = notification.GuiLuc,
            NgayTao = notification.NgayTao,
            NgayCapNhat = notification.NgayCapNhat
        };
    }

    private static NotificationRecipientPreviewDto ToPreviewDto(RecipientInfo recipient)
    {
        return new NotificationRecipientPreviewDto
        {
            MaNguoiNhan = recipient.UserId,
            HoTen = recipient.HoTen,
            Email = recipient.Email,
            VaiTroChinh = recipient.RoleCode,
            MaDonVi = recipient.OrganizationId,
            TenDonVi = recipient.OrganizationName
        };
    }

    private static string NormalizeNotificationType(string value)
    {
        var type = RequireTrimmed(value, "Loại thông báo không được để trống.").ToLowerInvariant();
        if (!NotificationConstants.Types.All.Contains(type))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại thông báo không hợp lệ.");
        }

        return type;
    }

    private static string NormalizeLevel(string value)
    {
        var level = RequireTrimmed(value, "Mức độ thông báo không được để trống.").ToLowerInvariant();
        if (!NotificationConstants.Levels.All.Contains(level))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mức độ thông báo không hợp lệ.");
        }

        return level;
    }

    private static string NormalizeScope(string? value)
    {
        var scope = RequireTrimmed(value, "Phạm vi gửi thông báo không được để trống.").ToLowerInvariant();
        if (!NotificationConstants.Scopes.All.Contains(scope))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Phạm vi gửi thông báo không hợp lệ.");
        }

        return scope;
    }

    private static string NormalizeStatus(string value)
    {
        var status = RequireTrimmed(value, "Trạng thái thông báo không được để trống.").ToLowerInvariant();
        if (!NotificationConstants.Statuses.All.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái thông báo không hợp lệ.");
        }

        return status;
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

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    }

    private sealed record ContentParts(string Title, string? Summary, string? ContentJson, string? ContentText);

    private sealed record NormalizedNotification(
        string TieuDe,
        string? TomTatNoiDung,
        string? NoiDungJson,
        string? NoiDungText,
        string LoaiThongBao,
        string MucDo,
        string PhamViGui,
        string? LoaiDoiTuongLienKet,
        int? MaDoiTuongLienKet,
        string? DuongDan,
        int? MaDonVi,
        int? NguoiTao,
        PreviewNotificationRecipientsRequest RecipientRequest);

    private sealed record RecipientInfo(
        int UserId,
        int OrganizationId,
        string HoTen,
        string Email,
        string RoleCode,
        string? OrganizationName,
        string? Status = null);
}

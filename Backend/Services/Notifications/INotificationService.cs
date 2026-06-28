using Backend.DTOs.Common;
using Backend.DTOs.Notifications;

namespace Backend.Services.Notifications;

public interface INotificationService
{
    Task<PagedResultDto<NotificationDto>> GetMyNotificationsAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<NotificationDetailDto> GetMyNotificationDetailAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<UnreadCountDto> GetMyUnreadCountAsync(CancellationToken cancellationToken = default);

    Task<NotificationDto> MarkAsReadAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<UnreadCountDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default);

    Task HideAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<NotificationRecipientPreviewResultDto> PreviewRecipientsAsync(
        PreviewNotificationRecipientsRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminNotificationDto> CreateManualNotificationAsync(
        CreateManualNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AdminNotificationDto>> GetAdminNotificationsAsync(
        AdminNotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AdminNotificationDto> GetAdminNotificationDetailAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AdminNotificationRecipientDto>> GetAdminNotificationRecipientsAsync(
        int notificationId,
        AdminNotificationRecipientQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<NotificationStatisticsDto> GetAdminNotificationStatisticsAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<AdminNotificationDto> CancelNotificationAsync(
        int notificationId,
        CancellationToken cancellationToken = default);

    Task CreateSystemNotificationAsync(
        SystemNotificationRequest request,
        IReadOnlyCollection<int> recipientIds,
        CancellationToken cancellationToken = default);

    Task SendToUsersAsync(
        SystemNotificationRequest request,
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default);

    Task SendToClassAsync(
        SystemNotificationRequest request,
        int classId,
        IReadOnlyCollection<int>? additionalUserIds = null,
        CancellationToken cancellationToken = default);

    Task SendToCourseAsync(
        SystemNotificationRequest request,
        int courseId,
        IReadOnlyCollection<int>? additionalUserIds = null,
        CancellationToken cancellationToken = default);

    Task SendToCampusAsync(
        SystemNotificationRequest request,
        int organizationId,
        CancellationToken cancellationToken = default);
}

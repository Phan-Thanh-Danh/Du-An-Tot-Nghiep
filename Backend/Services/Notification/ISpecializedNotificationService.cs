using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.Services.Applications;

namespace Backend.Services.Notification;

public interface ISpecializedNotificationService
{
    Task<PreviewSpecializedRecipientsResultDto> PreviewRecipientsAsync(
        PreviewSpecializedRecipientsRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);

    Task<SpecializedNotificationSendResultDto> SendTuitionNotificationAsync(
        SendTuitionNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);

    Task<SpecializedNotificationSendResultDto> SendAcademicNotificationAsync(
        SendAcademicNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);

    Task<SpecializedNotificationSendResultDto> SendUrgentNotificationAsync(
        SendUrgentNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);

    Task<SpecializedNotificationSendResultDto> SendMaintenanceNotificationAsync(
        SendMaintenanceNotificationRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default);

    Task<SpecializedNotificationCategoryDto> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}

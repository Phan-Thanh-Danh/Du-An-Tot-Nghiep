using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.Models;
using Backend.Services.Applications;

namespace Backend.Services.Notification;

public interface INotificationTemplateService
{
    Task<PagedResultDto<NotificationTemplateListItemDto>> GetTemplatesAsync(
        NotificationTemplateQueryParameters query,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task<NotificationTemplateDetailDto> GetTemplateDetailAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task<NotificationTemplateDetailDto> CreateTemplateAsync(
        CreateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task<NotificationTemplateDetailDto> UpdateTemplateAsync(
        int id,
        UpdateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task ActivateTemplateAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task DeactivateTemplateAsync(
        int id,
        DeactivateNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task DeleteTemplateAsync(
        int id,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);

    Task<PreviewNotificationTemplateResultDto> PreviewTemplateAsync(
        int id,
        PreviewNotificationTemplateRequest request,
        ApplicationActorContext actor,
        CancellationToken cancellationToken);
}

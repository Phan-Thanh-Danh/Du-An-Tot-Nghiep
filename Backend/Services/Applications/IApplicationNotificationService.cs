using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationNotificationService
{
    Task NotifySubmittedAsync(DonTu application, CancellationToken cancellationToken = default);
    Task NotifyAssignedAsync(DonTu application, int? assigneeId, CancellationToken cancellationToken = default);
    Task NotifySupplementRequestedAsync(DonTu application, string? publicMessage, CancellationToken cancellationToken = default);
    Task NotifyApprovedAsync(DonTu application, CancellationToken cancellationToken = default);
    Task NotifyRejectedAsync(DonTu application, string? publicReason, CancellationToken cancellationToken = default);
    Task NotifyCancelledAsync(DonTu application, CancellationToken cancellationToken = default);
    Task NotifyProcessingSucceededAsync(DonTu application, CancellationToken cancellationToken = default);
    Task NotifyProcessingFailedAsync(DonTu application, string? publicMessage, CancellationToken cancellationToken = default);
    Task NotifyManualProcessingRequiredAsync(DonTu application, CancellationToken cancellationToken = default);
    Task NotifyProcessingRecordedAsync(DonTu application, CancellationToken cancellationToken = default);
}

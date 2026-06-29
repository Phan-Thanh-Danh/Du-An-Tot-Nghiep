namespace Backend.Services.RewardDiscipline;

public interface IRewardDisciplineNotificationService
{
    // Reward Notifications
    Task NotifyRewardApprovedAsync(int rewardId, CancellationToken cancellationToken = default);
    Task NotifyRewardCertificateReadyAsync(int rewardId, CancellationToken cancellationToken = default);
    Task NotifyRewardIssuedAsync(int rewardId, CancellationToken cancellationToken = default);
    Task NotifyRewardCanceledAsync(int rewardId, CancellationToken cancellationToken = default);
    Task NotifyRewardRestoredAsync(int rewardId, CancellationToken cancellationToken = default);
    
    // Discipline Notifications
    Task NotifyDisciplineActivatedAsync(int recordId, CancellationToken cancellationToken = default);
    Task NotifyDisciplineExpiredAsync(int recordId, CancellationToken cancellationToken = default);
    Task NotifyDisciplineEffectRemovedAsync(int recordId, CancellationToken cancellationToken = default);
    Task NotifyDisciplineVoidedAsync(int recordId, CancellationToken cancellationToken = default);
    
    Task NotifyDisciplineAppealSubmittedAsync(int appealId, CancellationToken cancellationToken = default);
    Task NotifyDisciplineAppealResolvedAsync(int appealId, CancellationToken cancellationToken = default);
    
    // Manual endpoints
    Task ResendRewardNotificationAsync(int rewardId, string reason, CancellationToken cancellationToken = default);
    Task ResendDisciplineRecordNotificationAsync(int recordId, string reason, CancellationToken cancellationToken = default);
    Task ResendDisciplineAppealNotificationAsync(int appealId, string reason, CancellationToken cancellationToken = default);
}

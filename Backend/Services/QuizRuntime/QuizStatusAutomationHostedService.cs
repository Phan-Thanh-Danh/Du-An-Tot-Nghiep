namespace Backend.Services.QuizRuntime;

public class QuizStatusAutomationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuizStatusAutomationHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

    public QuizStatusAutomationHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<QuizStatusAutomationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IQuizAvailabilityService>();
                var changed = await service.SynchronizeScheduledQuizzesAsync(DateTime.UtcNow, stoppingToken);
                if (changed > 0)
                {
                    _logger.LogInformation("Synchronized {Count} quiz status records", changed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize quiz statuses");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}

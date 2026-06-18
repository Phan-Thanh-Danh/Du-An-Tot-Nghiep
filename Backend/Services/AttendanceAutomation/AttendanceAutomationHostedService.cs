using Microsoft.Extensions.Options;

namespace Backend.Services.AttendanceAutomation;

public class AttendanceAutomationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<AttendanceAutomationOptions> _optionsMonitor;
    private readonly ILogger<AttendanceAutomationHostedService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AttendanceAutomationHostedService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<AttendanceAutomationOptions> optionsMonitor,
        ILogger<AttendanceAutomationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            var delay = TimeSpan.FromSeconds(options.IntervalSeconds <= 0 ? 60 : options.IntervalSeconds);

            try
            {
                if (options.Enabled)
                {
                    await RunOnceAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Attendance automation hosted service failed during a scheduled run.");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        if (!await _semaphore.WaitAsync(0, stoppingToken))
        {
            _logger.LogInformation("Attendance automation skipped because a previous run is still active.");
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAttendanceAutomationService>();
            var result = await service.ProcessDueAttendanceAsync(cancellationToken: stoppingToken);

            if (result.AutoSubmitted > 0 || result.AutoLocked > 0)
            {
                _logger.LogInformation(
                    "Attendance automation processed {AutoSubmitted} auto submit and {AutoLocked} auto lock items.",
                    result.AutoSubmitted,
                    result.AutoLocked);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override void Dispose()
    {
        _semaphore.Dispose();
        base.Dispose();
    }
}

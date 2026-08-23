using System.Threading;
using System.Threading.Tasks;
using Backend.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.AI;

public interface IAiRequestGate
{
    int CurrentQueueLength { get; }
    Task<T> ExecuteWithGateAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);
}

public class AiRequestGate : IAiRequestGate
{
    private readonly SemaphoreSlim _semaphore;
    private readonly OllamaOptions _options;
    private readonly ILogger<AiRequestGate> _logger;
    private int _waitingCount;

    public AiRequestGate(IOptions<OllamaOptions> options, ILogger<AiRequestGate> logger)
    {
        _options = options.Value;
        _logger = logger;
        var maxConcurrent = Math.Max(1, _options.MaxConcurrentChatRequests);
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public int CurrentQueueLength => Volatile.Read(ref _waitingCount);

    public async Task<T> ExecuteWithGateAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var maxQueue = Math.Max(1, _options.MaxQueueSize);

        if (Volatile.Read(ref _waitingCount) >= maxQueue)
        {
            _logger.LogWarning("AI request rejected: Queue limit ({MaxQueue}) reached.", maxQueue);
            throw new ApiException(429, "Hệ thống AI đang tiếp nhận quá nhiều yêu cầu cùng lúc. Vui lòng thử lại sau giây lát.");
        }

        Interlocked.Increment(ref _waitingCount);
        var entered = false;
        try
        {
            var timeoutMs = _options.TimeoutSeconds > 0 ? _options.TimeoutSeconds * 1000 : 180000;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            entered = await _semaphore.WaitAsync(timeoutMs, cts.Token);
            if (!entered)
            {
                throw new ApiException(504, "Hệ thống AI đang bận xử lý yêu cầu trước đó. Vui lòng thử lại sau.");
            }

            Interlocked.Decrement(ref _waitingCount);
            return await action(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(504, "Hết thời gian chờ phản hồi từ mô hình AI.");
        }
        finally
        {
            if (entered)
            {
                _semaphore.Release();
            }
            else
            {
                // In case it threw before decrementing
                Interlocked.Decrement(ref _waitingCount);
            }
        }
    }
}

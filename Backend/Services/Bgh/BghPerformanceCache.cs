using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services.Bgh;

public sealed class BghPerformanceCache : IBghPerformanceCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BghPerformanceCache> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _trackedKeys = new(StringComparer.Ordinal);
    private long _hits;
    private long _misses;
    private long _factoryExecutions;

    public BghPerformanceCache(IMemoryCache cache, ILogger<BghPerformanceCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan lifetime,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<T>(key, out var cached) && cached is not null)
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);
        var gate = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<T>(key, out cached) && cached is not null)
            {
                Interlocked.Increment(ref _hits);
                return cached;
            }

            Interlocked.Increment(ref _factoryExecutions);
            var value = await factory(cancellationToken);
            _trackedKeys[key] = 0;
            _cache.Set(
                key,
                value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = lifetime,
                    Size = 1
                }.RegisterPostEvictionCallback((evictedKey, _, reason, _) =>
                {
                    if (evictedKey is string cacheKey)
                    {
                        _trackedKeys.TryRemove(cacheKey, out _);
                        _logger.LogDebug("BGH cache evicted {CacheKey} ({Reason})", cacheKey, reason);
                    }
                }));
            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _trackedKeys.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            _cache.Remove(key);
        }
    }

    public BghCacheMetrics GetMetrics() => new(
        Interlocked.Read(ref _hits),
        Interlocked.Read(ref _misses),
        Interlocked.Read(ref _factoryExecutions),
        _trackedKeys.Count);
}

namespace Backend.Services.Bgh;

public interface IBghPerformanceCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan lifetime,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default);

    void RemoveByPrefix(string prefix);

    BghCacheMetrics GetMetrics();
}

public sealed record BghCacheMetrics(
    long Hits,
    long Misses,
    long FactoryExecutions,
    int TrackedKeys)
{
    public double HitRate => Hits + Misses == 0
        ? 0
        : Math.Round(Hits * 100d / (Hits + Misses), 1);
}

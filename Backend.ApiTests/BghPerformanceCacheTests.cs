using Backend.DTOs.Auth;
using Backend.Services.Bgh;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests;

public class BghPerformanceCacheTests
{
    [Test]
    public async Task GetOrCreateAsync_ShouldSingleFlightAndRecordCacheHit()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
        var cache = new BghPerformanceCache(memory, NullLogger<BghPerformanceCache>.Instance);
        var factoryCalls = 0;

        async Task<int> Factory(CancellationToken _)
        {
            Interlocked.Increment(ref factoryCalls);
            await Task.Delay(25);
            return 42;
        }

        var values = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => cache.GetOrCreateAsync("bgh:test", TimeSpan.FromMinutes(1), Factory)));
        var warmValue = await cache.GetOrCreateAsync(
            "bgh:test",
            TimeSpan.FromMinutes(1),
            _ => Task.FromResult(99));

        Assert.Multiple(() =>
        {
            Assert.That(values, Is.All.EqualTo(42));
            Assert.That(warmValue, Is.EqualTo(42));
            Assert.That(factoryCalls, Is.EqualTo(1));
            Assert.That(cache.GetMetrics().FactoryExecutions, Is.EqualTo(1));
            Assert.That(cache.GetMetrics().Hits, Is.GreaterThanOrEqualTo(8));
        });
    }

    [Test]
    public async Task RemoveByPrefixAndExpiration_ShouldForceFreshValue()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
        var cache = new BghPerformanceCache(memory, NullLogger<BghPerformanceCache>.Instance);

        await cache.GetOrCreateAsync("bgh:role:1:1:item", TimeSpan.FromMinutes(1), _ => Task.FromResult(1));
        cache.RemoveByPrefix("bgh:role:1:1:");
        var afterInvalidation = await cache.GetOrCreateAsync(
            "bgh:role:1:1:item",
            TimeSpan.FromMilliseconds(20),
            _ => Task.FromResult(2));
        await Task.Delay(60);
        var afterExpiration = await cache.GetOrCreateAsync(
            "bgh:role:1:1:item",
            TimeSpan.FromMinutes(1),
            _ => Task.FromResult(3));

        Assert.Multiple(() =>
        {
            Assert.That(afterInvalidation, Is.EqualTo(2));
            Assert.That(afterExpiration, Is.EqualTo(3));
        });
    }

    [Test]
    public void CacheKey_ShouldIsolateUserRoleAndCampus()
    {
        var first = Context("Principal", 10, 1);
        var otherCampus = Context("Principal", 10, 2);
        var otherUser = Context("Principal", 11, 1);
        var otherRole = Context("Admin", 10, 1);

        var keys = new[] { first, otherCampus, otherUser, otherRole }
            .Select(context => BghCacheKey.For(context, "dashboard", "all"))
            .ToArray();

        Assert.That(keys.Distinct().Count(), Is.EqualTo(4));
    }

    private static HttpContext Context(string role, int userId, int campusId)
    {
        var context = new DefaultHttpContext();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            Role = role,
            UserId = userId,
            CampusId = campusId
        };
        return context;
    }
}

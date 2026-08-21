using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Services.Bgh;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BghResponseCacheAttribute(int lifetimeSeconds) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cache = context.HttpContext.RequestServices.GetRequiredService<IBghPerformanceCache>();
        var request = context.HttpContext.Request;
        var query = string.Join('&', request.Query
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"{pair.Key}={pair.Value}"));
        var route = string.Join('&', context.RouteData.Values
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"{pair.Key}={pair.Value}"));
        var key = BghCacheKey.For(
            context.HttpContext,
            $"response:{request.Path.Value?.ToLowerInvariant()}",
            route,
            query);

        var snapshot = await cache.GetOrCreateAsync(
            key,
            TimeSpan.FromSeconds(Math.Max(1, lifetimeSeconds)),
            async _ => Snapshot(await next()),
            context.HttpContext.RequestAborted);

        // SWR và memory cache chịu trách nhiệm tái sử dụng dữ liệu. Buộc browser
        // revalidate để mutation vừa thành công không bị HTTP cache trả bản cũ.
        context.HttpContext.Response.Headers.CacheControl = "private, no-cache";
        context.Result = snapshot.ToActionResult();
    }

    private static CachedActionSnapshot Snapshot(ActionExecutedContext executed)
    {
        if (executed.Result is ObjectResult objectResult)
        {
            return new CachedActionSnapshot(
                objectResult.Value,
                objectResult.StatusCode ?? StatusCodes.Status200OK);
        }

        if (executed.Result is StatusCodeResult statusCodeResult)
        {
            return new CachedActionSnapshot(null, statusCodeResult.StatusCode);
        }

        return new CachedActionSnapshot(null, StatusCodes.Status204NoContent);
    }

    private sealed record CachedActionSnapshot(object? Value, int StatusCode)
    {
        public IActionResult ToActionResult() => Value is null
            ? new StatusCodeResult(StatusCode)
            : new ObjectResult(Value) { StatusCode = StatusCode };
    }
}

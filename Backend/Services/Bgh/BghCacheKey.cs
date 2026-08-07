using Backend.DTOs.Auth;

namespace Backend.Services.Bgh;

public static class BghCacheKey
{
    public static string For(HttpContext context, string resource, params object?[] dimensions)
    {
        var user = context.Items["CurrentUser"] as CurrentUserContext;
        var scope = $"bgh:{user?.Role ?? "unknown"}:{user?.UserId ?? 0}:{user?.CampusId ?? 0}";
        var suffix = dimensions.Length == 0
            ? string.Empty
            : ":" + string.Join(':', dimensions.Select(Normalize));
        return $"{scope}:{resource}{suffix}";
    }

    public static string ScopePrefix(HttpContext context)
    {
        var user = context.Items["CurrentUser"] as CurrentUserContext;
        return $"bgh:{user?.Role ?? "unknown"}:{user?.UserId ?? 0}:{user?.CampusId ?? 0}:";
    }

    private static string Normalize(object? value) => value switch
    {
        null => "all",
        string text when string.IsNullOrWhiteSpace(text) => "all",
        string text => Uri.EscapeDataString(text.Trim().ToLowerInvariant()),
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "all"
    };
}

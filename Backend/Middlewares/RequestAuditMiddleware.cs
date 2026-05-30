using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.Services.Audit;

namespace Backend.Middlewares;

public class RequestAuditMiddleware
{
    private const int MaxAuditBodyBytes = 32 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] ContainsSensitiveNames = ["password", "token", "otp", "secret"];
    private static readonly HashSet<string> ExactSensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "matkhau",
        "matkhauhash",
        "passwordhash",
        "refreshtoken",
        "accesstoken",
        "tokenhash",
        "connectionstring"
    };

    private readonly RequestDelegate _next;

    public RequestAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var requestBody = await ReadRequestBodyAsync(context);
        var startedAt = Stopwatch.GetTimestamp();
        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            capturedException = exception;
            throw;
        }
        finally
        {
            if (context.Items["CurrentUser"] is CurrentUserContext currentUser)
            {
                var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                var auditValue = new
                {
                    Request = new
                    {
                        Method = context.Request.Method,
                        Path = context.Request.Path.Value,
                        Query = CreateSanitizedQuery(context),
                        Body = requestBody
                    },
                    Response = new
                    {
                        context.Response.StatusCode,
                        Succeeded = capturedException is null && context.Response.StatusCode < 400
                    },
                    Error = capturedException is null
                        ? null
                        : new
                        {
                            Type = capturedException.GetType().Name,
                            capturedException.Message
                        },
                    DurationMs = Math.Round(elapsedMs, 2)
                };

                await auditLogService.LogAsync(
                    "HttpRequest",
                    context.Request.Path.Value ?? "/",
                    $"HTTP_{context.Request.Method.ToUpperInvariant()}",
                    null,
                    auditValue,
                    currentUser.UserId,
                    currentUser.CampusId,
                    "Ghi nhận request API tự động.",
                    context.RequestAborted);
            }
        }
    }

    private static bool ShouldSkip(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return true;
        }

        if (HttpMethods.IsOptions(context.Request.Method))
        {
            return true;
        }

        if (context.Request.Path.StartsWithSegments("/api/audit-logs"))
        {
            return true;
        }

        return false;
    }

    private static async Task<object?> ReadRequestBodyAsync(HttpContext context)
    {
        var request = context.Request;
        if (!RequestMayHaveBody(request) || request.ContentLength is 0)
        {
            return null;
        }

        if (request.ContentLength > MaxAuditBodyBytes)
        {
            return new
            {
                Skipped = true,
                Reason = "Request body vượt giới hạn audit.",
                request.ContentLength
            };
        }

        if (request.ContentType is null ||
            !request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return new
            {
                Skipped = true,
                Reason = "Content-Type không phải application/json.",
                request.ContentType,
                request.ContentLength
            };
        }

        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(rawBody);
            SanitizeNode(node);
            return node;
        }
        catch (JsonException)
        {
            return new
            {
                Skipped = true,
                Reason = "Request body không phải JSON hợp lệ.",
                Length = rawBody.Length
            };
        }
    }

    private static bool RequestMayHaveBody(HttpRequest request)
    {
        return HttpMethods.IsPost(request.Method) ||
               HttpMethods.IsPut(request.Method) ||
               HttpMethods.IsPatch(request.Method) ||
               HttpMethods.IsDelete(request.Method);
    }

    private static IReadOnlyDictionary<string, object?> CreateSanitizedQuery(HttpContext context)
    {
        return context.Request.Query.ToDictionary(
            x => x.Key,
            x => IsSensitiveProperty(x.Key)
                ? (object?)"***"
                : x.Value.Count == 1
                    ? x.Value[0]
                    : x.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static void SanitizeNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var property in jsonObject.ToList())
                {
                    if (IsSensitiveProperty(property.Key))
                    {
                        jsonObject[property.Key] = "***";
                        continue;
                    }

                    SanitizeNode(property.Value);
                }

                break;
            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    SanitizeNode(item);
                }

                break;
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var normalized = propertyName
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return ExactSensitiveNames.Contains(normalized) ||
               ContainsSensitiveNames.Any(normalized.Contains);
    }
}

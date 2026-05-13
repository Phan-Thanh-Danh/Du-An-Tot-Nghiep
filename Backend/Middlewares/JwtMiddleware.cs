using System.Security.Claims;
using System.Text.Json;
using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, JwtHelper jwtHelper)
    {
        if (IsPublicEndpoint(context))
        {
            await _next(context);
            return;
        }

        var authorizationHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            await _next(context);
            return;
        }

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "Header xác thực không hợp lệ.");
            return;
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var principal = jwtHelper.ValidateToken(token);
        if (principal is null)
        {
            await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "Token không hợp lệ hoặc đã hết hạn.");
            return;
        }

        var currentUser = CreateCurrentUser(principal);
        if (currentUser is null)
        {
            await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "Thông tin xác thực trong token không hợp lệ.");
            return;
        }

        context.User = principal;
        context.Items["CurrentUser"] = currentUser;

        await _next(context);
    }

    private static CurrentUserContext? CreateCurrentUser(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(CustomClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Email);
        var role = principal.FindFirstValue(CustomClaimTypes.Role)
            ?? principal.FindFirstValue(ClaimTypes.Role);
        var status = principal.FindFirstValue(CustomClaimTypes.Status);

        if (!int.TryParse(principal.FindFirstValue(CustomClaimTypes.UserId), out var userId) ||
            !int.TryParse(principal.FindFirstValue(CustomClaimTypes.CampusId), out var campusId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(role) ||
            string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return new CurrentUserContext
        {
            UserId = userId,
            Email = email,
            Role = role,
            CampusId = campusId,
            Status = status
        };
    }

    private static bool IsPublicEndpoint(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return true;
        }

        return context.Request.Path.StartsWithSegments("/api/auth/login");
    }

    private static async Task WriteJsonAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            success = false,
            message,
            errors = new[] { message },
            traceId = context.TraceIdentifier,
            statusCode
        }));
    }
}

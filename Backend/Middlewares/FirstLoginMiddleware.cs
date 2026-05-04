using System.Text.Json;
using Backend.Constants;
using Backend.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Middlewares;

public class FirstLoginMiddleware
{
    private readonly RequestDelegate _next;

    public FirstLoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicOrAllowedEndpoint(context))
        {
            await _next(context);
            return;
        }

        if (context.Items["CurrentUser"] is CurrentUserContext currentUser &&
            currentUser.Status == UserStatuses.FirstLogin)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                statusCode = StatusCodes.Status403Forbidden,
                message = "You must change your password before continuing.",
                traceId = context.TraceIdentifier
            }));
            return;
        }

        await _next(context);
    }

    private static bool IsPublicOrAllowedEndpoint(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return true;
        }

        return context.Request.Path.StartsWithSegments("/api/auth/login") ||
               context.Request.Path.StartsWithSegments("/api/auth/change-password");
    }
}

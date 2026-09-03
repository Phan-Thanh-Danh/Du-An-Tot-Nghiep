using System.Text.Json;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException exception)
        {
            await WriteErrorAsync(context, exception.StatusCode, exception.Message, exception.ErrorCode);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrent publish conflict detected. TraceId: {TraceId}",
                context.TraceIdentifier);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Thao tác publish bị xung đột do có yêu cầu khác đang " +
                          "xử lý cùng lúc. Vui lòng tải lại trang và thử lại.",
                errorCode = "CONCURRENT_CONFLICT",
                errors = new[] { ex.Message },
                traceId = context.TraceIdentifier,
                statusCode = 409
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);

            var message = _environment.IsDevelopment()
                ? exception.Message
                : "Đã xảy ra lỗi không mong muốn.";

            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, message);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, string? errorCode = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message,
            errorCode,
            errors = new[] { message },
            traceId = context.TraceIdentifier,
            statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

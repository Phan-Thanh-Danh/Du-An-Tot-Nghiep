using System.Text.Json;
using Backend.Exceptions;

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
            await WriteErrorAsync(context, exception.StatusCode, exception.Message);
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

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
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
            errors = new[] { message },
            traceId = context.TraceIdentifier,
            statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

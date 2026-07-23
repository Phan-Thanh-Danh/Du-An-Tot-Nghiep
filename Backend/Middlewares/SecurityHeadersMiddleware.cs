using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Backend.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            // Cấu hình CSP để chặn đứng luồng mạng của các Extension AI
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; connect-src 'self' https://localhost:5097 wss://localhost:5097 https://127.0.0.1:17892;");

            await _next(context);
        }
    }
}

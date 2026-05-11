using System.Text.Json;
using Backend.Data;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Services;
using Backend.Services.Auth;
using Backend.Services.Organizations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:5174"];

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new
        {
            statusCode = StatusCodes.Status400BadRequest,
            message = "Dữ liệu không hợp lệ.",
            traceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtHelper = new JwtHelper(builder.Configuration);
        options.TokenValidationParameters = jwtHelper.GetTokenValidationParameters();
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await WriteJsonAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Vui lòng đăng nhập để tiếp tục.");
            },
            OnForbidden = async context =>
            {
                await WriteJsonAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập tài nguyên này.");
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("AcademicOperations", policy => policy.RequireRole("Admin", "SuperAdmin", "AcademicStaff", "CampusAdmin"));
    options.AddPolicy("Reports", policy => policy.RequireRole("Admin", "SuperAdmin", "Principal", "CampusAdmin"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

await Data.SeedRolesAsync(app.Services);

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<FirstLoginMiddleware>();
app.UseMiddleware<CampusScopeMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task WriteJsonAsync(HttpContext context, int statusCode, string message)
{
    if (context.Response.HasStarted)
    {
        return;
    }

    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        statusCode,
        message,
        traceId = context.TraceIdentifier
    }));
}

using System.Text.Json;
using Backend.Data;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Services;
using Backend.Services.AdminUsers;
using Backend.Services.AdministrativeClasses;
using Backend.Services.Audit;
using Backend.Services.Auth;
using Backend.Services.Organizations;
using Backend.Services.Rbac;
using Backend.Services.Security;
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
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? $"Trường {x.Key} không hợp lệ."
                : error.ErrorMessage))
            .ToList();
        var response = new
        {
            success = false,
            message = "Dữ liệu không hợp lệ.",
            errors,
            traceId = context.HttpContext.TraceIdentifier,
            statusCode = StatusCodes.Status400BadRequest
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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdministrativeClassService, AdministrativeClassService>();
builder.Services.AddScoped<IRbacRepository, RbacRepository>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
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
    options.AddPolicy("AdminUserManagement", policy => policy.RequireRole("Admin", "SuperAdmin", "CampusAdmin"));
    options.AddPolicy("RbacManagement", policy => policy.RequireRole("Admin", "SuperAdmin", "CampusAdmin"));
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
        success = false,
        message,
        errors = new[] { message },
        traceId = context.TraceIdentifier,
        statusCode
    }));
}

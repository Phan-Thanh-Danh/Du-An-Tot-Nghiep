using System.Text.Json;
using Backend.Data;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Services.Auth;
using Backend.Services.Organizations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new
        {
            statusCode = StatusCodes.Status400BadRequest,
            message = "Validation failed.",
            traceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();

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
                await WriteJsonAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Authentication is required.");
            },
            OnForbidden = async context =>
            {
                await WriteJsonAsync(context.HttpContext, StatusCodes.Status403Forbidden, "You are not allowed to access this resource.");
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

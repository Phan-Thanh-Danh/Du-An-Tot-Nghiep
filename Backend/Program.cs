using System.Text.Json;
using Backend.Data;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Services;
using Backend.Services.AdminUsers;
using Backend.Services.AdministrativeClasses;
using Backend.Services.AcademicTerms;
using Backend.Services.Attendance;
using Backend.Services.AttendanceAutomation;
using Backend.Services.AttendanceUnlock;
using Backend.Services.Audit;
using Backend.Services.Auth;
using Backend.Services.Buildings;
using Backend.Services.BuoiHoc;
using Backend.Services.CaHoc;
using Backend.Services.CampusSpecializations;
using Backend.Services.Cohorts;
using Backend.Services.Courses;
using Backend.Services.CourseSyllabuses;
using Backend.Services.Curriculum;
using Backend.Services.Floors;
using Backend.Services.Finance.ProgramTuitionConfigs;
using Backend.Services.Finance.TuitionPayments;
using Backend.Services.Majors;
using Backend.Services.Notifications;
using Backend.Services.Organizations;
using Backend.Services.Rbac;
using Backend.Services.Rooms;
using Backend.Services.Security;
using Backend.Services.Specializations;
using Backend.Services.Storage;
using Backend.Services.Subjects;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.TrainingProgramSubjects;
using Backend.Services.TrainingPrograms;
using Backend.Services.Exam;
using Backend.Hubs;
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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<PayOSOptions>(builder.Configuration.GetSection("PayOS"));
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
builder.Services.AddScoped<IAcademicTermService, AcademicTermService>();
builder.Services.AddScoped<IRbacRepository, RbacRepository>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<INganhDaoTaoService, NganhDaoTaoService>();
builder.Services.AddScoped<IChuyenNganhService, ChuyenNganhService>();
builder.Services.AddScoped<IChuyenNganhTheoCoSoService, ChuyenNganhTheoCoSoService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseSyllabusService, CourseSyllabusService>();
builder.Services.AddScoped<ICohortService, CohortService>();
builder.Services.AddScoped<ITrainingProgramSubjectService, TrainingProgramSubjectService>();
builder.Services.AddScoped<ITrainingProgramService, TrainingProgramService>();
builder.Services.AddScoped<ITrainingProgramTermService, TrainingProgramTermService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ICaHocService, CaHocService>();
builder.Services.AddScoped<IProgramTuitionConfigService, ProgramTuitionConfigService>();
builder.Services.AddScoped<IVietQrService, VietQrService>();
builder.Services.AddHttpClient<IPayOsService, PayOsService>();
builder.Services.AddScoped<ITuitionPaymentService, TuitionPaymentService>();
builder.Services.AddScoped<ICurriculumService, CurriculumService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IThoiKhoaBieuService, ThoiKhoaBieuService>();
builder.Services.AddScoped<IScheduleConflictService, ScheduleConflictService>();
builder.Services.AddScoped<IBuoiHocService, BuoiHocService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.Configure<AttendanceAutomationOptions>(builder.Configuration.GetSection("AttendanceAutomation"));
builder.Services.AddScoped<IAttendanceAutomationService, AttendanceAutomationService>();
builder.Services.AddHostedService<AttendanceAutomationHostedService>();
builder.Services.AddScoped<IAttendanceUnlockService, AttendanceUnlockService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSignalR();

var r2Settings = builder.Configuration.GetSection("R2Storage").Get<R2StorageSettings>()
    ?? new R2StorageSettings();
builder.Services.AddSingleton(r2Settings);
builder.Services.AddSingleton<IR2StorageService, R2StorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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
    options.AddPolicy("AcademicOperations", policy => policy.RequireRole("Admin", "SuperAdmin", "AcademicStaff", "CampusAdmin", "Chairman", "HoiDongQuanLyNoiDung"));
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

    if (app.Environment.IsDevelopment())
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Không thể tắt SQL Server IDENTITY_CACHE cho database dev.");
        }
    }
}

await Data.SeedRolesAsync(app.Services);

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<FirstLoginMiddleware>();
app.UseMiddleware<CampusScopeMiddleware>();
app.UseMiddleware<RequestAuditMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ExamMonitoringHub>("/hubs/exam-monitoring");

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

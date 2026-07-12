using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Helpers;
using Backend.Hubs;
using Backend.Middlewares;
using Backend.Services;
using Backend.Services.AcademicTerms;
using Backend.Services.AdministrativeClasses;
using Backend.Services.AdminUsers;
using Backend.Services.Attendance;
using Backend.Services.AttendanceAutomation;
using Backend.Services.AttendanceUnlock;
using Backend.Services.Audit;
using Backend.Services.Auth;
using Backend.Services.Applications;
using Backend.Services.Buildings;
using Backend.Services.BuoiHoc;
using Backend.Services.CaHoc;
using Backend.Services.CampusSpecializations;
using Backend.Services.Cohorts;
using Backend.Services.Courses;
using Backend.Services.CourseSyllabuses;
using Backend.Services.Curriculum;
using Backend.Services.Exam;
using Backend.Services.Finance.ProgramTuitionConfigs;
using Backend.Services.Finance.TuitionPayments;
using Backend.Services.Floors;
using Backend.Services.LearningProgress;
using Backend.Services.Majors;
using Backend.Services.Notifications;
using Backend.Services.Organizations;
using Backend.Services.QuestionBank;
using Backend.Services.QuizAttempts;
using Backend.Services.QuizGrading;
using Backend.Services.QuizManagement;
using Backend.Services.QuizRuntime;
using Backend.Services.Rbac;
using Backend.Services.Registrations;
using Backend.Services.Rooms;
using Backend.Services.RewardDiscipline;
using Backend.Services.Notification;
using Backend.Services.Security;
using Backend.Services.Specializations;
using Backend.Services.Storage;
using Backend.Services.Subjects;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Backend.Configuration;
using Backend.Services.TrainingPrograms;
using Backend.Services.TrainingProgramSubjects;

using Backend.Services.AcademicSchedulingContext;
using Backend.Services.TeachingPreferences;
using Backend.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:5174"];

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context
            .ModelState.Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x =>
                x.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"Trường {x.Key} không hợp lệ."
                        : error.ErrorMessage
                )
            )
            .ToList();
        var response = new
        {
            success = false,
            message = "Dữ liệu không hợp lệ.",
            errors,
            traceId = context.HttpContext.TraceIdentifier,
            statusCode = StatusCodes.Status400BadRequest,
        };

        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            )
    )
);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<PayOSOptions>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<Backend.Services.SuperAdmin.ISuperAdminService, Backend.Services.SuperAdmin.SuperAdminService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IApplicationStateMachine, ApplicationStateMachine>();
builder.Services.AddScoped<IApplicationTemplateValidator, ApplicationTemplateValidator>();
builder.Services.AddScoped<IApplicationSchemaService, ApplicationSchemaService>();
builder.Services.AddScoped<IApplicationFormDataValidator, ApplicationFormDataValidator>();
builder.Services.AddScoped<IApplicationReferenceValidator, ApplicationReferenceValidator>();
builder.Services.AddScoped<IApplicationEvidenceValidator, ApplicationEvidenceValidator>();
builder.Services.AddScoped<IApplicationNotificationService, ApplicationNotificationService>();
builder.Services.AddScoped<IStudentApplicationService, StudentApplicationService>();
builder.Services.AddScoped<Backend.Services.Blocks.IBlockService, Backend.Services.Blocks.BlockService>();
builder.Services.AddScoped<Backend.Services.QuyDoiTinChis.IQuyDoiTinChiService, Backend.Services.QuyDoiTinChis.QuyDoiTinChiService>();
builder.Services.AddScoped<Backend.Services.LopHanhChinhs.ILopHanhChinhService, Backend.Services.LopHanhChinhs.LopHanhChinhService>();
builder.Services.Configure<SmartTimetableScoringOptions>(
    builder.Configuration.GetSection(SmartTimetableScoringOptions.SectionName));
builder.Services.AddScoped<IScheduleCandidateScoringService, ScheduleCandidateScoringService>();

builder.Services.AddScoped<IApplicationCampusScopeService, ApplicationCampusScopeService>();
builder.Services.AddScoped<IApplicationAdminQueueService, ApplicationAdminQueueService>();
builder.Services.AddScoped<IApplicationReportService, ApplicationReportService>();
builder.Services.AddScoped<IApplicationAssignmentService, ApplicationAssignmentService>();
builder.Services.AddScoped<IApplicationAdminEvidenceService, ApplicationAdminEvidenceService>();
builder.Services.AddScoped<IApplicationDecisionPermissionEvaluator, ApplicationDecisionPermissionEvaluator>();
builder.Services.AddScoped<IApplicationApprovalPreconditionValidator, ApplicationApprovalPreconditionValidator>();
builder.Services.AddScoped<IApplicationDecisionService, ApplicationDecisionService>();
builder.Services.AddScoped<IApplicationProcessingStateMachine, ApplicationProcessingStateMachine>();
builder.Services.AddScoped<IApplicationProcessingPermissionEvaluator, ApplicationProcessingPermissionEvaluator>();
builder.Services.AddScoped<IApplicationProcessingResultSanitizer, ApplicationProcessingResultSanitizer>();
builder.Services.AddScoped<IApplicationPostApprovalHandler, ConfirmationApplicationPostApprovalHandler>();
builder.Services.AddScoped<IApplicationPostApprovalProcessingService, ApplicationPostApprovalProcessingService>();
builder.Services.AddScoped<IApplicationWorkflowService, ApplicationWorkflowService>();
builder.Services.AddOptions<ApplicationQueueOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationQueueOptions.SectionName))
    .Validate(options => options.SlaWarningBeforeHours is >= 1 and <= 168, "ApplicationQueue:SlaWarningBeforeHours must be from 1 to 168.")
    .ValidateOnStart();
builder.Services.AddOptions<ApplicationEvidenceStorageOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationEvidenceStorageOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<ApplicationEvidenceStorageOptions>, ApplicationEvidenceStorageOptionsValidator>();
builder.Services.AddScoped<IApplicationEvidenceFileInspector, ApplicationEvidenceFileInspector>();
builder.Services.AddScoped<IApplicationEvidenceService, ApplicationEvidenceService>();
builder.Services.AddScoped<IApplicationSubmissionRule, LeaveApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, RetakeExamApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, GradeAppealApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, AcademicPauseApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, ChangeCampusApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, ConfirmationApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, CertificateApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, OtherApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, WithdrawalApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, TransferSchoolApplicationSubmissionRule>();
builder.Services.AddScoped<IApplicationSubmissionRule, ChangeMajorApplicationSubmissionRule>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdministrativeClassService, AdministrativeClassService>();
builder.Services.AddScoped<IAcademicTermService, AcademicTermService>();
builder.Services.AddScoped<IRbacRepository, RbacRepository>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<Backend.Services.TeacherSchedule.ITeacherScheduleService, Backend.Services.TeacherSchedule.TeacherScheduleService>();
builder.Services.AddScoped<Backend.Services.StudentSchedule.IStudentScheduleService, Backend.Services.StudentSchedule.StudentScheduleService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<INganhDaoTaoService, NganhDaoTaoService>();
builder.Services.AddScoped<IChuyenNganhService, ChuyenNganhService>();
builder.Services.AddScoped<IChuyenNganhTheoCoSoService, ChuyenNganhTheoCoSoService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseAssignmentSuggestionService, CourseAssignmentSuggestionService>();
builder.Services.AddScoped<ICourseTeacherEligibilityService, CourseTeacherEligibilityService>();
builder.Services.AddScoped<ITeacherAcademicWorkloadService, TeacherAcademicWorkloadService>();
builder.Services.AddScoped<ITeachingPreferenceCoverageService, TeachingPreferenceCoverageService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
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
builder.Services.AddScoped<IQuizManagementService, QuizManagementService>();
builder.Services.AddScoped<IQuizAvailabilityService, QuizAvailabilityService>();
builder.Services.AddScoped<IQuizGradingService, QuizGradingService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IThoiKhoaBieuService, ThoiKhoaBieuService>();
builder.Services.AddScoped<IScheduleConflictService, ScheduleConflictService>();
builder.Services.AddScoped<IBuoiHocService, BuoiHocService>();
builder.Services.AddScoped<ISmartTimetableService, SmartTimetableService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.Configure<AttendanceAutomationOptions>(
    builder.Configuration.GetSection("AttendanceAutomation")
);
builder.Services.AddScoped<IAttendanceAutomationService, AttendanceAutomationService>();
builder.Services.AddHostedService<AttendanceAutomationHostedService>();
builder.Services.AddHostedService<QuizStatusAutomationHostedService>();
builder.Services.AddScoped<IAttendanceUnlockService, AttendanceUnlockService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IQuestionBankService, QuestionBankService>();
builder.Services.AddScoped<IRewardCampaignService, RewardCampaignService>();
builder.Services.AddScoped<IRewardEvaluationService, RewardEvaluationService>();
builder.Services.AddScoped<ICertificateTemplateService, CertificateTemplateService>();
builder.Services.Configure<CertificateStorageOptions>(
    builder.Configuration.GetSection(CertificateStorageOptions.SectionName)
);
builder.Services.AddScoped<ICertificatePdfStorageService, LocalCertificatePdfStorageService>();
builder.Services.AddScoped<ICertificateGenerationService, CertificateGenerationService>();
builder.Services.AddScoped<IStudentRewardService, StudentRewardService>();
builder.Services.AddScoped<IRewardLifecycleService, RewardLifecycleService>();
builder.Services.AddScoped<IDisciplineRecordService, DisciplineRecordService>();
builder.Services.AddScoped<IDisciplineAppealService, DisciplineAppealService>();
builder.Services.AddScoped<IRewardDisciplineNotificationService, RewardDisciplineNotificationService>();
builder.Services.AddScoped<IRewardDisciplineReportService, RewardDisciplineReportService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<ISpecializedNotificationService, SpecializedNotificationService>();


builder.Services.Configure<LearningProgressOptions>(
    builder.Configuration.GetSection(LearningProgressOptions.SectionName)
);
builder.Services.AddScoped<IStudentContentAccessService, StudentContentAccessService>();
builder.Services.AddScoped<ILearningProgressCalculator, LearningProgressCalculator>();
builder.Services.AddScoped<ILearningProgressSyncService, LearningProgressSyncService>();
builder.Services.AddScoped<ILearningProgressService, LearningProgressService>();
builder.Services.AddScoped<IAcademicSchedulingContextService, AcademicSchedulingContextService>();

builder.Services.Configure<TeachingPreferenceOptions>(
    builder.Configuration.GetSection(TeachingPreferenceOptions.SectionName)
);
builder.Services.AddScoped<ITeachingPreferenceService, TeachingPreferenceService>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var r2Settings =
    builder.Configuration.GetSection("R2Storage").Get<R2StorageSettings>()
    ?? new R2StorageSettings();
builder.Services.AddSingleton(r2Settings);
builder.Services.AddSingleton<IR2StorageService, R2StorageService>();
builder.Services.AddSingleton<IApplicationEvidenceObjectStore>(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var options = sp.GetRequiredService<IOptions<ApplicationEvidenceStorageOptions>>();
    var provider = options.Value.Provider.Trim();

    if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
    {
        if (environment.IsProduction())
        {
            throw new InvalidOperationException("Local application evidence storage is not allowed in Production.");
        }

        return new LocalApplicationEvidenceObjectStore(options, environment);
    }

    if (provider.Equals("R2", StringComparison.OrdinalIgnoreCase))
    {
        return new R2ApplicationEvidenceObjectStore(sp.GetRequiredService<R2StorageSettings>());
    }

    throw new InvalidOperationException("Application evidence storage provider must be R2 or Local.");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendDev",
        policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtHelper = new JwtHelper(builder.Configuration);
        options.TokenValidationParameters = jwtHelper.GetTokenValidationParameters();
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            // Đọc JWT từ query string cho SignalR WebSocket/SSE
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs/exam-monitoring"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await WriteJsonAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "Vui lòng đăng nhập để tiếp tục."
                );
            },
            OnForbidden = async context =>
            {
                await WriteJsonAsync(
                    context.HttpContext,
                    StatusCodes.Status403Forbidden,
                    "Bạn không có quyền truy cập tài nguyên này."
                );
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy(
        "AdminUserManagement",
        policy => policy.RequireRole("Admin", "SuperAdmin", "CampusAdmin", "AcademicStaff")
    );
    options.AddPolicy(
        "RbacManagement",
        policy => policy.RequireRole("Admin", "SuperAdmin", "CampusAdmin")
    );
    options.AddPolicy(
        "AcademicOperations",
        policy =>
            policy.RequireRole(
                "Admin",
                "SuperAdmin",
                "AcademicStaff",
                "CampusAdmin",
                "Chairman",
                "HoiDongQuanLyNoiDung"
            )
    );
    options.AddPolicy(
        "Reports",
        policy => policy.RequireRole("Admin", "SuperAdmin", "Principal", "CampusAdmin")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationStudent,
        policy => policy.RequireRole("Student")
    );
    options.AddPolicy(
        "ApplicationOperations",
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin", "AcademicStaff")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationQueueRead,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin", "AcademicStaff", "Principal")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationReceive,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin", "AcademicStaff")
    );
    options.AddPolicy(
        "AcademicScheduleConfig",
        policy => policy.RequireRole("Admin", "SuperAdmin", "AcademicStaff", "CampusAdmin")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationAssignmentManage,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationReviewOperate,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin", "AcademicStaff")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationSensitiveDecision,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "Principal")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationProcessingOperate,
        policy => policy.RequireRole("SuperAdmin", "Admin", "CampusAdmin", "SubCampusAdmin", "AcademicStaff")
    );
    options.AddPolicy(
        AuthPolicies.ApplicationSystemAdmin,
        policy => policy.RequireRole("SuperAdmin", "Admin")
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapGet("/dev/seed-test-term", async (ApplicationDbContext db) =>
    {
        var msg = await Backend.Data.Seeders.TestDataSeeder.SeedFreshTermForTestingAsync(db);
        return Results.Ok(new { message = msg });
    });
}

app.UseMiddleware<ExceptionMiddleware>();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    // Chạy BlockDataSeeder để migration data cũ nếu cần
    var blockSeeder = new Backend.Data.Seeders.BlockDataSeeder(context);
    await blockSeeder.SeedAsync();

    if (app.Environment.IsDevelopment())
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;"
            );
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Không thể tắt SQL Server IDENTITY_CACHE cho database dev.");
        }
    }
}

var seedProfile = builder.Configuration["SeedProfile"];
await Data.SeedRolesAsync(app.Services);
if (string.Equals(seedProfile, "LargeDemo", StringComparison.OrdinalIgnoreCase))
{
    app.Logger.LogInformation("Running LargeDemoSeeder...");
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await Backend.Data.Seeders.LargeDemoSeeder.SeedAsync(context);
    app.Logger.LogInformation("LargeDemoSeeder completed.");
}
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

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(
            new
            {
                success = false,
                message,
                errors = new[] { message },
                traceId = context.TraceIdentifier,
                statusCode,
            }
        )
    );
}

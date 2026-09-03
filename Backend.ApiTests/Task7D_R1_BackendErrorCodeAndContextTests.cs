using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Exceptions;
using Backend.Middlewares;
using Backend.Models;
using Backend.Configuration;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class Task7D_R1_BackendErrorCodeAndContextTests
{
    private ApplicationDbContext _db = null!;
    private AcademicSchedulingContextService _contextService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _contextService = new AcademicSchedulingContextService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Test]
    public async Task Task7D_R1_AttendanceTakesPriorityOverTimeoutLock()
    {
        var campusId = 1;
        var term = new HocKy
        {
            MaHocKy = 10,
            MaDonVi = campusId,
            TenHocKy = "HK Test Priority",
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100))
        };
        _db.HocKys.Add(term);

        var course = new KhoaHoc
        {
            MaKhoaHoc = 101,
            MaHocKy = 10,
            MaDonVi = campusId,
            TrangThai = "da_xuat_ban"
        };
        _db.KhoaHocs.Add(course);

        // Timetable published 60 mins ago (> 30 min)
        var tkb = new ThoiKhoaBieu
        {
            MaTkb = 1001,
            MaKhoaHoc = 101,
            NgayTao = DateTime.UtcNow.AddMinutes(-60),
            TrangThai = "da_xuat_ban"
        };
        _db.ThoiKhoaBieus.Add(tkb);

        var buoi = new BuoiHoc
        {
            MaBuoiHoc = 5001,
            MaKhoaHoc = 101,
            MaTkb = 1001,
            NgayHoc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };
        _db.BuoiHocs.Add(buoi);

        // Actual attendance record in DiemDanh
        var diemDanh = new DiemDanh
        {
            MaDiemDanh = 8001,
            MaDonVi = campusId,
            MaBuoiHoc = 5001,
            MaHocSinh = 1,
            TrangThai = "co_mat",
            GhiNhanLuc = DateTime.UtcNow
        };
        _db.DiemDanhs.Add(diemDanh);
        await _db.SaveChangesAsync();

        var context = await _contextService.GetContextAsync(campusId);

        // Attendance takes strict priority
        Assert.That(context.LockReasonCode, Is.EqualTo("SCHEDULE_LOCKED_BY_ATTENDANCE"));
        Assert.That(context.ReasonMessage, Does.Contain("điểm danh"));
    }

    [Test]
    public async Task Task7D_R1_TimeoutLockWhenNoAttendanceAndOver30Min()
    {
        var campusId = 1;
        var term = new HocKy
        {
            MaHocKy = 20,
            MaDonVi = campusId,
            TenHocKy = "HK Test Timeout",
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100))
        };
        _db.HocKys.Add(term);

        var course = new KhoaHoc
        {
            MaKhoaHoc = 201,
            MaHocKy = 20,
            MaDonVi = campusId,
            TrangThai = "da_xuat_ban"
        };
        _db.KhoaHocs.Add(course);

        // Timetable published 45 mins ago (> 30 min)
        var tkb = new ThoiKhoaBieu
        {
            MaTkb = 2001,
            MaKhoaHoc = 201,
            NgayTao = DateTime.UtcNow.AddMinutes(-45),
            TrangThai = "da_xuat_ban"
        };
        _db.ThoiKhoaBieus.Add(tkb);

        var buoi = new BuoiHoc
        {
            MaBuoiHoc = 6001,
            MaKhoaHoc = 201,
            MaTkb = 2001,
            NgayHoc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };
        _db.BuoiHocs.Add(buoi);
        await _db.SaveChangesAsync();

        var context = await _contextService.GetContextAsync(campusId);

        Assert.That(context.LockReasonCode, Is.EqualTo("SCHEDULE_LOCKED_AFTER_EDIT_WINDOW"));
        Assert.That(context.ReasonMessage, Does.Contain("30 phút"));
    }

    [Test]
    public async Task Task7D_R1_EditableWhenWithin30MinAndNoAttendance()
    {
        var campusId = 1;
        var term = new HocKy
        {
            MaHocKy = 30,
            MaDonVi = campusId,
            TenHocKy = "HK Test Window",
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100))
        };
        _db.HocKys.Add(term);

        var course = new KhoaHoc
        {
            MaKhoaHoc = 301,
            MaHocKy = 30,
            MaDonVi = campusId,
            TrangThai = "da_xuat_ban"
        };
        _db.KhoaHocs.Add(course);

        // Timetable published 10 mins ago (<= 30 min)
        var tkb = new ThoiKhoaBieu
        {
            MaTkb = 3001,
            MaKhoaHoc = 301,
            NgayTao = DateTime.UtcNow.AddMinutes(-10),
            TrangThai = "da_xuat_ban"
        };
        _db.ThoiKhoaBieus.Add(tkb);

        var buoi = new BuoiHoc
        {
            MaBuoiHoc = 7001,
            MaKhoaHoc = 301,
            MaTkb = 3001,
            NgayHoc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };
        _db.BuoiHocs.Add(buoi);
        await _db.SaveChangesAsync();

        var context = await _contextService.GetContextAsync(campusId);

        Assert.That(context.LockReasonCode, Is.Null);
    }

    [Test]
    public async Task Task7D_R1_ApiExceptionErrorCodeSerialization()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        var middleware = new ExceptionMiddleware(
            next: (innerHttpContext) => throw new ApiException(StatusCodes.Status409Conflict, "Lịch học đã bị khóa vì có điểm danh", "SCHEDULE_LOCKED_BY_ATTENDANCE"),
            logger: NullLogger<ExceptionMiddleware>.Instance,
            environment: mockEnv.Object
        );

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseText = await reader.ReadToEndAsync();

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));

        using var doc = JsonDocument.Parse(responseText);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("errorCode").GetString(), Is.EqualTo("SCHEDULE_LOCKED_BY_ATTENDANCE"));
        Assert.That(root.GetProperty("message").GetString(), Is.EqualTo("Lịch học đã bị khóa vì có điểm danh"));
        Assert.That(root.GetProperty("statusCode").GetInt32(), Is.EqualTo(409));
    }

    [Test]
    public void Task7D_R1_GetCurrentGenerationJob_CampusIsolation_StaffCannotQueryOtherCampus()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 101,
            Role = AuthRoles.AcademicStaff,
            CampusId = 1
        };
        httpContextAccessor.HttpContext = httpContext;

        // Job belongs to Campus 2
        var draftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 999,
            DraftId = draftId,
            MaHocKy = 40,
            MaDonVi = 2, // Campus 2
            TrangThai = "dang_chay",
            NgayTao = DateTime.UtcNow
        });
        _db.SaveChanges();

        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);
        var capacityService = new CourseCapacityService(_db);

        var service = new SmartTimetableService(
            _db,
            httpContextAccessor,
            new Mock<IAuditLogService>().Object,
            NullLogger<SmartTimetableService>.Instance,
            _contextService,
            scoringService,
            solver,
            new Mock<IScheduleNotificationService>().Object,
            scoringOptions,
            capacityService
        );

        // When staff from Campus 1 queries for job in term 40, they must NOT see Campus 2 job
        var result = service.GetCurrentGenerationJobAsync(40).GetAwaiter().GetResult();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Task7D_R1_GetCurrentGenerationJob_StaffWithoutCampus_ThrowsForbiddenCampus()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 102,
            Role = AuthRoles.AcademicStaff,
            CampusId = 0
        };
        httpContextAccessor.HttpContext = httpContext;

        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);
        var capacityService = new CourseCapacityService(_db);

        var service = new SmartTimetableService(
            _db,
            httpContextAccessor,
            new Mock<IAuditLogService>().Object,
            NullLogger<SmartTimetableService>.Instance,
            _contextService,
            scoringService,
            solver,
            new Mock<IScheduleNotificationService>().Object,
            scoringOptions,
            capacityService
        );

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetCurrentGenerationJobAsync(1);
        });

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }
}

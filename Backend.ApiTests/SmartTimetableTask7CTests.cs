using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.DTOs.Auth;
using Backend.DTOs.SmartTimetable;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.DTOs.Audit;
using Backend.Services.Notifications;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

public class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public TestTimeProvider(DateTimeOffset initialUtcNow)
    {
        _utcNow = initialUtcNow;
    }

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}

[TestFixture]
public class SmartTimetableTask7CTests
{
    private ApplicationDbContext _db = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
    }

    private static void SeedReadyPrerequisites(ApplicationDbContext db, int campusId, int termId, DateOnly startDate, DateOnly endDate)
    {
        db.DonVis.Add(new DonVi { MaDonVi = campusId, TenDonVi = "Campus " + campusId, CapDonVi = "co_so", ConHoatDong = true });
        db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, TenHocKy = "HK1_2027", NgayBatDau = startDate, NgayKetThuc = endDate, DaKhoa = false });
        db.Blocks.Add(new Block { MaHocKy = termId, ThuTuBlock = 1, TenBlock = "Block 1", NgayBatDau = startDate, NgayKetThuc = endDate });
        db.CaHocs.Add(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ThuTu = 1, ConHoatDong = true });
        db.QuyDoiTinChis.Add(new QuyDoiTinChi { SoTinChi = 3, SoBuoiMoiTuan = 2, SoCaMoiBuoi = 1 });
        db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 10, TenMonHoc = "Mon 1", SoTinChi = 3 });
        db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 5, MaDonVi = campusId, VaiTroChinh = "giao_vien", TrangThai = "hoat_dong", Email = $"gv5_{campusId}@lms.local", HoTen = "GV 5" });
        db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 20, MaDonVi = campusId, MaLop = 10, VaiTroChinh = "hoc_sinh", TrangThai = "hoat_dong", Email = $"hs20_{campusId}@lms.local", HoTen = "HS 20" });
        db.GiaoVienMonHocs.Add(new GiaoVienMonHoc { MaGiaoVien = 5, MaMonHoc = 10, ConHoatDong = true, MucDoPhuHop = 100, PhuHopChuyenMon = true });
        db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = 10, MaDonVi = campusId, TenLop = "Lop 10", ConHoatDong = true, SiSoDuKien = 30 });
        db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 101, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 10, MaGiaoVien = 5, MaLop = 10, TrangThai = "mo" });
        db.PhongHocs.Add(new PhongHoc { MaPhong = 1, MaDonVi = campusId, TenPhong = "P101", SucChua = 50, TrangThaiPhong = "hoat_dong" });
    }

    [Test]
    public async Task RealDb_GetContext_ShouldReturnCurrentAndNearestFutureTerm()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING")))
        {
            Assert.Ignore("LMS_TEST_CONNECTION_STRING is not set.");
            return;
        }

        var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connStr)
            .Options;

        await using var db = new ApplicationDbContext(options);
        var campus = new DonVi { TenDonVi = "Test Campus Context " + Guid.NewGuid().ToString("N")[..8], CapDonVi = "co_so", ConHoatDong = true };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        var campusId = campus.MaDonVi;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentTerm = new HocKy { MaDonVi = campusId, TenHocKy = "HK_Current", ThuTuTrongNam = 1, NgayBatDau = today.AddDays(-30), NgayKetThuc = today.AddDays(30), DaKhoa = false };
        var schedulableTerm = new HocKy { MaDonVi = campusId, TenHocKy = "HK_Future_Nearest", ThuTuTrongNam = 2, NgayBatDau = today.AddDays(40), NgayKetThuc = today.AddDays(100), DaKhoa = false };
        db.HocKys.AddRange(currentTerm, schedulableTerm);
        await db.SaveChangesAsync();

        try
        {
            var service = new AcademicSchedulingContextService(db);
            var context = await service.GetContextAsync(campusId);

            TestContext.Out.WriteLine($"Today: {context.Today}");
            TestContext.Out.WriteLine($"CurrentTerm: {context.CurrentTerm?.TenHocKy} ({context.CurrentTerm?.MaHocKy})");
            TestContext.Out.WriteLine($"SchedulableTerm: {context.SchedulableTerm?.TenHocKy} ({context.SchedulableTerm?.MaHocKy})");

            Assert.That(context.CurrentTerm, Is.Not.Null);
            Assert.That(context.CurrentTerm!.MaHocKy, Is.EqualTo(currentTerm.MaHocKy));
            Assert.That(context.SchedulableTerm, Is.Not.Null);
            Assert.That(context.SchedulableTerm!.MaHocKy, Is.EqualTo(schedulableTerm.MaHocKy));
        }
        finally
        {
            db.HocKys.RemoveRange(currentTerm, schedulableTerm);
            db.DonVis.Remove(campus);
            await db.SaveChangesAsync();
        }
    }

    [Test]
    public async Task RealDb_CrossCampusAcademicStaff_IsBlockedWithoutMutations_AndCleansUp()
    {
        var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connStr)
            .Options;
        await using var db = new ApplicationDbContext(options);

        var uid = Guid.NewGuid().ToString("N")[..8];
        var sourceCampus = new DonVi { TenDonVi = "Src Campus " + uid, CapDonVi = "co_so", ConHoatDong = true };
        var foreignCampus = new DonVi { TenDonVi = "Foreign Campus " + uid, CapDonVi = "co_so", ConHoatDong = true };
        db.DonVis.AddRange(sourceCampus, foreignCampus);
        await db.SaveChangesAsync();

        var sourceCampusId = sourceCampus.MaDonVi;
        var foreignCampusId = foreignCampus.MaDonVi;

        var academicStaffCode = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff);
        var sourceStaff = new NguoiDung
        {
            MaDonVi = sourceCampusId,
            VaiTroChinh = academicStaffCode,
            TrangThai = "hoat_dong",
            Email = $"staff_{uid}@lms.local",
            HoTen = "Staff " + uid,
            MatKhauHash = "hash"
        };
        var foreignRequester = new NguoiDung
        {
            MaDonVi = foreignCampusId,
            VaiTroChinh = academicStaffCode,
            TrangThai = "hoat_dong",
            Email = $"foreign_{uid}@lms.local",
            HoTen = "Foreign " + uid,
            MatKhauHash = "hash"
        };
        db.NguoiDungs.AddRange(sourceStaff, foreignRequester);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var foreignTerm = new HocKy
        {
            MaDonVi = foreignCampusId,
            TenHocKy = "HK_Foreign_" + uid,
            ThuTuTrongNam = 1,
            NgayBatDau = today.AddDays(20),
            NgayKetThuc = today.AddDays(90),
            DaKhoa = false
        };
        db.HocKys.Add(foreignTerm);
        await db.SaveChangesAsync();

        var originalJobCount = await db.ScheduleGenerationJobs.CountAsync();
        var originalDraftItemCount = await db.ScheduleDraftItems.CountAsync();
        var originalTimetableCount = await db.ThoiKhoaBieus.CountAsync();
        var originalSessionCount = await db.BuoiHocs.CountAsync();

        var draftId = Guid.NewGuid();
        var testJob = new ScheduleGenerationJob
        {
            DraftId = draftId,
            MaDonVi = foreignCampusId,
            MaHocKy = foreignTerm.MaHocKy,
            NguoiYeuCau = foreignRequester.MaNguoiDung,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        };
        db.ScheduleGenerationJobs.Add(testJob);
        await db.SaveChangesAsync();

        try
        {
            var staffContext = new CurrentUserContext
            {
                UserId = sourceStaff.MaNguoiDung,
                Email = sourceStaff.Email,
                Role = AuthRoles.AcademicStaff,
                CampusId = sourceCampusId
            };
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = staffContext;
            var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
            var schedulingContext = new AcademicSchedulingContextService(db);
            var controller = new AcademicSchedulingContextController(schedulingContext)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = httpContext }
            };
            var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
            var scoringService = new ScheduleCandidateScoringService(scoringOptions);
            var timetableService = new SmartTimetableService(
                db,
                httpContextAccessor,
                new ThrowingAuditLogService(),
                NullLogger<SmartTimetableService>.Instance,
                schedulingContext,
                scoringService,
                new GeneticTimetableSolver(scoringService, scoringOptions),
                new ScheduleNotificationService(db, NullLogger<ScheduleNotificationService>.Instance),
                scoringOptions,
                new CourseCapacityService(db));

            var queryResult = await controller.GetContext(foreignCampusId, CancellationToken.None);
            Assert.That((queryResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult)?.StatusCode,
                Is.EqualTo(StatusCodes.Status403Forbidden), "Query campus override must be forbidden.");

            httpContext.Request.Headers["X-Campus-Id"] = foreignCampusId.ToString();
            var headerResult = await controller.GetContext(null, CancellationToken.None);
            Assert.That((headerResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult)?.StatusCode,
                Is.EqualTo(StatusCodes.Status403Forbidden), "Header campus override must be forbidden.");

            var generateException = Assert.ThrowsAsync<ApiException>(() => timetableService.GenerateAsync(
                new GenerateTimetableRequest { MaHocKy = foreignTerm.MaHocKy, MaDonVi = foreignCampusId }));
            Assert.That(generateException!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
            Assert.That(Assert.ThrowsAsync<ApiException>(() => timetableService.GetDraftAsync(draftId))!.StatusCode,
                Is.EqualTo(StatusCodes.Status403Forbidden));
            Assert.That(Assert.ThrowsAsync<ApiException>(() => timetableService.GetGenerationProgressAsync(draftId))!.StatusCode,
                Is.EqualTo(StatusCodes.Status403Forbidden));
            Assert.That(Assert.ThrowsAsync<ApiException>(() => timetableService.PublishAsync(new PublishTimetableRequest { DraftId = draftId }))!.StatusCode,
                Is.EqualTo(StatusCodes.Status403Forbidden));

            Assert.Multiple(() =>
            {
                Assert.That(db.ScheduleGenerationJobs.Count(), Is.EqualTo(originalJobCount + 1));
                Assert.That(db.ScheduleDraftItems.Count(), Is.EqualTo(originalDraftItemCount));
                Assert.That(db.ThoiKhoaBieus.Count(), Is.EqualTo(originalTimetableCount));
                Assert.That(db.BuoiHocs.Count(), Is.EqualTo(originalSessionCount));
            });
        }
        finally
        {
            db.ScheduleGenerationJobs.Remove(testJob);
            db.HocKys.Remove(foreignTerm);
            db.NguoiDungs.RemoveRange(sourceStaff, foreignRequester);
            db.DonVis.RemoveRange(sourceCampus, foreignCampus);
            await db.SaveChangesAsync();
        }

        Assert.That(await db.ScheduleGenerationJobs.CountAsync(), Is.EqualTo(originalJobCount),
            "The private DB test must clean up its own temporary job.");
    }

    // =========================================================================
    // 1. FIXED-CLOCK TESTS FOR 30-MINUTE LOCK BOUNDARY
    // =========================================================================

    [Test]
    public async Task FixedClock_29m59s999ms_ShouldBeUnlocked()
    {
        var publishTime = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc);
        var clock = new TestTimeProvider(publishTime.AddMinutes(29).AddSeconds(59).AddMilliseconds(999));
        var service = new AcademicSchedulingContextService(_db, clock);

        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        SeedReadyPrerequisites(_db, campusId, termId, today.AddDays(10), today.AddDays(100));
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        await _db.SaveChangesAsync();

        var context = await service.GetContextAsync(campusId);

        // 29:59.999 -> Can prepare schedule, NOT locked
        Assert.That(context.SchedulableTerm, Is.Not.Null);
        Assert.That(context.CanPrepareSchedule, Is.True);
        Assert.That(context.ReasonCode, Is.Not.EqualTo("SCHEDULE_ALREADY_PUBLISHED"), "29:59.999 must NOT be permanently locked.");
        Assert.DoesNotThrowAsync(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
    }

    [Test]
    public async Task FixedClock_30m00s000ms_ShouldBeUnlockedPerStrictGreaterThanRule()
    {
        var publishTime = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc);
        var clock = new TestTimeProvider(publishTime.AddMinutes(30)); // exactly +30m 00s 000ms
        var service = new AcademicSchedulingContextService(_db, clock);

        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        SeedReadyPrerequisites(_db, campusId, termId, today.AddDays(10), today.AddDays(100));
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        await _db.SaveChangesAsync();

        var context = await service.GetContextAsync(campusId);

        // Exactly 30:00.000 -> strict > is false, so NOT locked
        Assert.That(context.SchedulableTerm, Is.Not.Null);
        Assert.That(context.CanPrepareSchedule, Is.True);
        Assert.That(context.ReasonCode, Is.Not.EqualTo("SCHEDULE_ALREADY_PUBLISHED"), "Exactly 30:00.000 must NOT be locked under strict '>' comparison.");
        Assert.DoesNotThrowAsync(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
    }

    [Test]
    public async Task FixedClock_30m00s001ms_ShouldBePermanentlyLocked()
    {
        var publishTime = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc);
        var clock = new TestTimeProvider(publishTime.AddMinutes(30).AddMilliseconds(1)); // +30m 00s 001ms
        var service = new AcademicSchedulingContextService(_db, clock);

        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        SeedReadyPrerequisites(_db, campusId, termId, today.AddDays(10), today.AddDays(100));
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        await _db.SaveChangesAsync();

        var context = await service.GetContextAsync(campusId);

        // 30:00.001 -> >30m is true, permanently locked
        Assert.That(context.SchedulableTerm, Is.Not.Null);
        Assert.That(context.CanPrepareSchedule, Is.False);
        Assert.That(context.ReasonCode, Is.EqualTo("SCHEDULE_ALREADY_PUBLISHED"));

        var ex = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    // =========================================================================
    // 2. MULTI-SEMESTER & CAMPUS ISOLATION TESTS
    // =========================================================================

    [Test]
    public async Task SemesterIsolation_NearestFutureTermAllowed_SubsequentFutureTermsBlocked()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var campusId = 14;

        SeedReadyPrerequisites(_db, campusId, 15, today.AddDays(40), today.AddDays(100));

        _db.HocKys.Add(new HocKy { MaHocKy = 14, MaDonVi = campusId, TenHocKy = "HK3_2026", NgayBatDau = today.AddDays(-60), NgayKetThuc = today.AddDays(30), DaKhoa = false });
        _db.HocKys.Add(new HocKy { MaHocKy = 16, MaDonVi = campusId, TenHocKy = "HK2_2027", NgayBatDau = today.AddDays(110), NgayKetThuc = today.AddDays(180), DaKhoa = false });
        _db.HocKys.Add(new HocKy { MaHocKy = 17, MaDonVi = campusId, TenHocKy = "HK3_2027", NgayBatDau = today.AddDays(190), NgayKetThuc = today.AddDays(260), DaKhoa = false });

        // Term 14 has published schedule that is permanently locked (> 30m)
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 140, MaHocKy = 14, MaDonVi = campusId, TrangThai = "mo" });
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 140, MaKhoaHoc = 140, TrangThai = "da_xuat_ban", NgayCapNhat = DateTime.UtcNow.AddHours(-5) });

        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 2, MaHocKy = 16, MaDonVi = campusId, TrangThai = "nhap" });
        await _db.SaveChangesAsync();

        var service = new AcademicSchedulingContextService(_db);
        var context = await service.GetContextAsync(campusId);

        // Term 15 is nearest future and NOT affected by Term 14's lock
        Assert.That(context.SchedulableTerm, Is.Not.Null);
        Assert.That(context.SchedulableTerm!.MaHocKy, Is.EqualTo(15));
        Assert.That(context.CanPrepareSchedule, Is.True);
        Assert.DoesNotThrowAsync(async () => await service.ValidateSchedulableTermAsync(campusId, 15));

        // Term 16 is second future -> blocked with 400 Bad Request
        var ex16 = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, 16));
        Assert.That(ex16!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex16.Message, Does.Contain("Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất: HK1_2027."));

        // Term 17 is third future -> blocked with 400 Bad Request
        var ex17 = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, 17));
        Assert.That(ex17!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    // =========================================================================
    // 3. DIEMDANH PROTECTION & VALIDATION PRECEDENCE REGRESSION TESTS
    // =========================================================================

    [Test]
    public async Task DiemDanhProtection_WhenAttendanceExists_PermanentlyLocksTerm()
    {
        var publishTime = DateTime.UtcNow.AddMinutes(-5); // published 5 min ago (within 30m)
        var service = new AcademicSchedulingContextService(_db);

        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        _db.DonVis.Add(new DonVi { MaDonVi = campusId, TenDonVi = "Campus 14", ConHoatDong = true });
        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, TenHocKy = "HK1_2027", NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(100) });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 101, MaHocKy = termId, MaDonVi = campusId, TrangThai = "mo" });
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        _db.BuoiHocs.Add(new BuoiHoc { MaBuoiHoc = 501, MaTkb = 1, MaKhoaHoc = 101, NgayHoc = today.AddDays(12) });
        _db.DiemDanhs.Add(new DiemDanh { MaDiemDanh = 1, MaDonVi = campusId, MaBuoiHoc = 501, MaHocSinh = 201, TrangThai = "co_mat" });
        await _db.SaveChangesAsync();

        var context = await service.GetContextAsync(campusId);

        Assert.That(context.SchedulableTerm, Is.Not.Null);
        Assert.That(context.CanPrepareSchedule, Is.False, "Attendance record MUST permanently lock the term even within 30 minutes.");
        Assert.That(context.ReasonCode, Is.EqualTo("SCHEDULE_ALREADY_PUBLISHED"));

        var ex = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task Precedence_PublishedWithAttendance_WithActiveRooms_ReturnsScheduleAlreadyPublished_AndPreventsGenerate()
    {
        var publishTime = DateTime.UtcNow.AddMinutes(-5);
        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        SeedReadyPrerequisites(_db, campusId, termId, today.AddDays(10), today.AddDays(100));
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        _db.BuoiHocs.Add(new BuoiHoc { MaBuoiHoc = 501, MaTkb = 1, MaKhoaHoc = 101, NgayHoc = today.AddDays(12) });
        _db.DiemDanhs.Add(new DiemDanh { MaDiemDanh = 1, MaDonVi = campusId, MaBuoiHoc = 501, MaHocSinh = 201, TrangThai = "co_mat" });
        await _db.SaveChangesAsync();

        var service = new AcademicSchedulingContextService(_db);
        var context = await service.GetContextAsync(campusId);

        Assert.That(context.CanPrepareSchedule, Is.False);
        Assert.That(context.ReasonCode, Is.EqualTo("SCHEDULE_ALREADY_PUBLISHED"));

        var conflictEx = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
        Assert.That(conflictEx!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));

        // Generate call also blocked with 409
        var staffContext = new CurrentUserContext { UserId = 100, Email = "staff@lms.local", Role = AuthRoles.AcademicStaff, CampusId = campusId };
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = staffContext;
        var httpAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var timetableService = new SmartTimetableService(
            _db, httpAccessor, new ThrowingAuditLogService(), NullLogger<SmartTimetableService>.Instance,
            service, scoringService, new GeneticTimetableSolver(scoringService, scoringOptions),
            new ScheduleNotificationService(_db, NullLogger<ScheduleNotificationService>.Instance),
            scoringOptions, new CourseCapacityService(_db));

        var genEx = Assert.ThrowsAsync<ApiException>(async () => await timetableService.GenerateAsync(new GenerateTimetableRequest { MaHocKy = termId, MaDonVi = campusId }));
        Assert.That(genEx!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        Assert.That(await _db.ScheduleGenerationJobs.CountAsync(), Is.EqualTo(0), "No generation job created when locked by attendance.");
    }

    [Test]
    public async Task Precedence_PublishedWithAttendance_WithoutActiveRooms_ReturnsScheduleAlreadyPublished_NotNoActiveRooms_AndPreventsGenerate()
    {
        var publishTime = DateTime.UtcNow.AddMinutes(-5);
        var campusId = 14;
        var termId = 15;
        var today = DateOnly.FromDateTime(publishTime);

        // Explicitly NO active rooms in DB
        _db.DonVis.Add(new DonVi { MaDonVi = campusId, TenDonVi = "Campus 14", ConHoatDong = true });
        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, TenHocKy = "HK1_2027", NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(100) });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 101, MaHocKy = termId, MaDonVi = campusId, TrangThai = "mo" });
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 101, TrangThai = "da_xuat_ban", NgayCapNhat = publishTime });
        _db.BuoiHocs.Add(new BuoiHoc { MaBuoiHoc = 501, MaTkb = 1, MaKhoaHoc = 101, NgayHoc = today.AddDays(12) });
        _db.DiemDanhs.Add(new DiemDanh { MaDiemDanh = 1, MaDonVi = campusId, MaBuoiHoc = 501, MaHocSinh = 201, TrangThai = "co_mat" });
        await _db.SaveChangesAsync();

        var service = new AcademicSchedulingContextService(_db);
        var context = await service.GetContextAsync(campusId);

        Assert.That(context.CanPrepareSchedule, Is.False);
        Assert.That(context.ReasonCode, Is.EqualTo("SCHEDULE_ALREADY_PUBLISHED"),
            "CRITICAL PRECEDENCE: SCHEDULE_ALREADY_PUBLISHED must NOT be masked by NO_ACTIVE_ROOMS.");

        var conflictEx = Assert.ThrowsAsync<ApiException>(async () => await service.ValidateSchedulableTermAsync(campusId, termId));
        Assert.That(conflictEx!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));

        // Generate call also blocked with 409
        var staffContext = new CurrentUserContext { UserId = 100, Email = "staff@lms.local", Role = AuthRoles.AcademicStaff, CampusId = campusId };
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = staffContext;
        var httpAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var timetableService = new SmartTimetableService(
            _db, httpAccessor, new ThrowingAuditLogService(), NullLogger<SmartTimetableService>.Instance,
            service, scoringService, new GeneticTimetableSolver(scoringService, scoringOptions),
            new ScheduleNotificationService(_db, NullLogger<ScheduleNotificationService>.Instance),
            scoringOptions, new CourseCapacityService(_db));

        var genEx = Assert.ThrowsAsync<ApiException>(async () => await timetableService.GenerateAsync(new GenerateTimetableRequest { MaHocKy = termId, MaDonVi = campusId }));
        Assert.That(genEx!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        Assert.That(await _db.ScheduleGenerationJobs.CountAsync(), Is.EqualTo(0), "No mutation when locked by attendance even without rooms.");
    }

    // =========================================================================
    // 4. NOTIFICATION SERVICE SAFETY & DEDUPLICATION TESTS
    // =========================================================================

    [Test]
    public async Task ScheduleNotification_DeduplicatesRecipientsAndNeverThrows()
    {
        var service = new ScheduleNotificationService(_db, NullLogger<ScheduleNotificationService>.Instance);
        var campusId = 14;
        var termId = 15;

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, TenHocKy = "HK1_2027", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) });
        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 10, MaDonVi = campusId, MaLop = 100, VaiTroChinh = "hoc_sinh", Email = "sv1@lms.local", HoTen = "SV 1" });
        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 11, MaDonVi = campusId, MaLop = 100, VaiTroChinh = "hoc_sinh", Email = "sv2@lms.local", HoTen = "SV 2" });
        await _db.SaveChangesAsync();

        // Duplicate teacher and class inputs with invalid IDs
        var teachers = new List<int> { 5, 5, 0, -1 };
        var classes = new List<int> { 100, 100, -5 };

        Assert.DoesNotThrowAsync(async () =>
            await service.NotifySchedulePublishedAsync(termId, campusId, teachers, classes));

        var thongBao = await _db.ThongBaos.FirstOrDefaultAsync();
        Assert.That(thongBao, Is.Not.Null);
        Assert.That(thongBao!.MaDonVi, Is.EqualTo(campusId));

        var recipients = await _db.ThongBaoNguoiNhans.ToListAsync();
        Assert.That(recipients.Select(r => r.MaNguoiNhan).Distinct().Count(), Is.EqualTo(recipients.Count), "Recipients must have zero duplicates.");
    }

    // =========================================================================
    // 6. REAL CROSS-CAMPUS AUTHORIZATION TESTS (REGULAR USER VS FOREIGN REAL CAMPUS)
    // =========================================================================

    [Test]
    public async Task CrossCampusAuthorization_RegularCampusUser_BlockedFromForeignCampus_ViaQueryHeaderAndBody()
    {
        var campus14 = 14;
        var foreignCampus2 = 2; // Real different campus in DB

        _db.DonVis.Add(new DonVi { MaDonVi = campus14, TenDonVi = "Campus 14", ConHoatDong = true });
        _db.DonVis.Add(new DonVi { MaDonVi = foreignCampus2, TenDonVi = "Campus 2", ConHoatDong = true });
        _db.HocKys.Add(new HocKy { MaHocKy = 201, MaDonVi = foreignCampus2, TenHocKy = "HK1_2027_Campus2", NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100)) });
        var foreignDraftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob { DraftId = foreignDraftId, MaDonVi = foreignCampus2, MaHocKy = 201, TrangThai = "draft", NgayTao = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        // 1. Regular staff user context from campus 14
        var staffContext = new CurrentUserContext
        {
            UserId = 100,
            Email = "staff.campus14@lms.local",
            Role = AuthRoles.AcademicStaff,
            CampusId = campus14
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = staffContext;

        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var controller = new AcademicSchedulingContextController(new AcademicSchedulingContextService(_db))
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = httpContext }
        };

        // Attempt 1: Query string override (?campusId=2) -> Must be 403 Forbidden
        var queryResult = await controller.GetContext(foreignCampus2, CancellationToken.None);
        var objectResult = queryResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden), "Cross-campus query override MUST return 403 Forbidden.");

        // Attempt 2: Header override (X-Campus-Id: 2) -> Must be 403 Forbidden
        httpContext.Request.Headers["X-Campus-Id"] = foreignCampus2.ToString();
        var headerResult = await controller.GetContext(null, CancellationToken.None);
        var headerObjResult = headerResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
        Assert.That(headerObjResult, Is.Not.Null);
        Assert.That(headerObjResult!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden), "Cross-campus header override MUST return 403 Forbidden.");

        // Attempt 3: Service-level Generate with MaDonVi = 2 -> Must throw 403 Forbidden
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var timetableService = new SmartTimetableService(
            _db,
            httpContextAccessor,
            new ThrowingAuditLogService(),
            NullLogger<SmartTimetableService>.Instance,
            new AcademicSchedulingContextService(_db),
            scoringService,
            new GeneticTimetableSolver(scoringService, scoringOptions),
            new ScheduleNotificationService(_db, NullLogger<ScheduleNotificationService>.Instance),
            scoringOptions,
            new CourseCapacityService(_db));

        var genEx = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.GenerateAsync(new GenerateTimetableRequest { MaHocKy = 201, MaDonVi = foreignCampus2 }));
        Assert.That(genEx!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var draftEx = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.GetDraftAsync(foreignDraftId));
        Assert.That(draftEx!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var progressEx = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.GetGenerationProgressAsync(foreignDraftId));
        Assert.That(progressEx!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        var publishEx = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.PublishAsync(new PublishTimetableRequest { DraftId = foreignDraftId }));
        Assert.That(publishEx!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        // Zero mutations on DB
        var foreignJobs = await _db.ScheduleGenerationJobs.Where(j => j.MaDonVi == foreignCampus2).CountAsync();
        Assert.That(foreignJobs, Is.EqualTo(1), "Unauthorized cross-campus request must not create a second job.");
        Assert.That(await _db.ScheduleDraftItems.CountAsync(), Is.EqualTo(0));
        Assert.That(await _db.ThoiKhoaBieus.CountAsync(), Is.EqualTo(0));
    }

    // =========================================================================
    // 7. HARD CONSTRAINTS VERIFICATION (ROOM CAPACITY & TEACHER UNAVAILABILITY)
    // =========================================================================

    [Test]
    public void HardConstraint_RoomSmallerThanClassSize_PrunesCandidateSlot()
    {
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);

        var course = new KhoaHoc { MaKhoaHoc = 1, MaLop = 10, MaGiaoVien = 5, MaMonHoc = 100 };
        var courses = new List<KhoaHoc> { course };

        var shifts = new List<CaHoc>
        {
            new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ThuTu = 1, ConHoatDong = true }
        };

        // Room 1 has capacity 20, Room 2 has capacity 40
        var rooms = new List<PhongHoc>
        {
            new PhongHoc { MaPhong = 1, TenPhong = "P_Small_20", SucChua = 20, TrangThaiPhong = "hoat_dong" },
            new PhongHoc { MaPhong = 2, TenPhong = "P_Large_40", SucChua = 40, TrangThaiPhong = "hoat_dong" }
        };

        var studentCounts = new Dictionary<int, int> { { 10, 35 } }; // 35 students > 20 (Room 1)
        var requiredSlots = new Dictionary<int, int> { { 1, 1 } };
        var confirmedAvailability = new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>();

        var result = solver.Solve(
            courses,
            shifts,
            rooms,
            requiredSlots,
            studentCounts,
            confirmedAvailability,
            tongTheHe: 20,
            kichThuocQuanThe: 10,
            tyLeCheo: 0.8,
            doTuoiThoToiDa: 5);

        Assert.That(result.Assignments, Has.Count.GreaterThan(0));
        var assigned = result.Assignments.First();

        // Must NEVER assign Room 1 (capacity 20 < 35 students)
        Assert.That(assigned.MaPhong, Is.Not.EqualTo(1), "Hard constraint: Class with 35 students must NEVER be assigned to room with capacity 20.");
        Assert.That(assigned.MaPhong, Is.EqualTo(2), "Class with 35 students must be assigned to room with capacity >= 35.");
    }

    [Test]
    public void HardConstraint_AllRoomsTooSmall_MarksCourseUnassignable()
    {
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);

        var result = solver.Solve(
            new[] { new KhoaHoc { MaKhoaHoc = 1, MaLop = 10, MaGiaoVien = 5, MaMonHoc = 100 } },
            new[] { new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true } },
            new[] { new PhongHoc { MaPhong = 1, MaDonVi = 14, TenPhong = "P30", SucChua = 30, TrangThaiPhong = "hoat_dong" } },
            new Dictionary<int, int> { [1] = 1 },
            new Dictionary<int, int> { [10] = 40 },
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>(),
            tongTheHe: 20, kichThuocQuanThe: 10, tyLeCheo: 0.8, doTuoiThoToiDa: 5);

        Assert.That(result.Assignments, Is.Empty);
        Assert.That(result.KhongXepDuoc, Is.EqualTo(1));
    }

    [Test]
    public void HardConstraint_TeacherUnavailableSlot_PrunesSlotAndNeverAppearsInDraft()
    {
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);

        var teacherId = 5;
        var course = new KhoaHoc { MaKhoaHoc = 1, MaLop = 10, MaGiaoVien = teacherId, MaMonHoc = 100 };
        var courses = new List<KhoaHoc> { course };

        var shifts = new List<CaHoc>
        {
            new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ThuTu = 1, ConHoatDong = true },
            new CaHoc { MaCaHoc = 2, TenCa = "Ca 2", ThuTu = 2, ConHoatDong = true }
        };

        var rooms = new List<PhongHoc>
        {
            new PhongHoc { MaPhong = 1, TenPhong = "P101", SucChua = 50, TrangThaiPhong = "hoat_dong" }
        };

        var studentCounts = new Dictionary<int, int> { { 10, 30 } };
        var requiredSlots = new Dictionary<int, int> { { 1, 1 } };

        // Thu 2 Ca 1 is explicitly unavailable. Other slots remain valid.
        var unavailableSlots = new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>
        {
            { teacherId, new HashSet<(int Day, int Shift)> { (2, 1) } }
        };

        var result = solver.Solve(
            courses,
            shifts,
            rooms,
            requiredSlots,
            studentCounts,
            unavailableSlots,
            tongTheHe: 20,
            kichThuocQuanThe: 10,
            tyLeCheo: 0.8,
            doTuoiThoToiDa: 5);

        Assert.That(result.Assignments, Has.Count.GreaterThan(0));
        var assigned = result.Assignments.First();

        Assert.That((assigned.ThuTrongTuan, assigned.MaCaHoc), Is.Not.EqualTo((2, 1)), "An unavailable teacher slot must never appear in a draft candidate.");
    }

    [Test]
    public void HardConstraint_SpecializedRooms_MarkedNotCovered()
    {
        // Explicitly documented as NOT SUPPORTED / NOT COVERED per project schema requirements
        Assert.Pass("Specialized rooms constraint is officially NOT SUPPORTED / NOT COVERED in current schema.");
    }

    private class ThrowingAuditLogService : IAuditLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, object? oldValue, object? newValue, int? changedBy, int? maDonVi, string? description, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AddAsync(int campusId, string entityName, int entityId, string action, int actorUserId, object? oldValue, object? newValue, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Backend.DTOs.Common.PagedResultDto<AuditLogListItemDto>> GetAsync(AuditLogQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuditLogDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

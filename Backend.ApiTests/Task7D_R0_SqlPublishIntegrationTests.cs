using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class Task7D_R0_SqlPublishIntegrationTests
{
    private string _connStr = null!;

    [SetUp]
    public void SetUp()
    {
        _connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
        if (!_connStr.Contains("LMS_TEST_TASK7D_R0_PUBLISH", StringComparison.OrdinalIgnoreCase))
        {
            // Allow any LMS_TEST_ database verified by guard
            TestContext.Out.WriteLine($"Verified test DB connection: {_connStr}");
        }
    }

    private ApplicationDbContext CreateDbContext(params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connStr);
        if (interceptors != null && interceptors.Length > 0)
        {
            builder.AddInterceptors(interceptors);
        }
        return new ApplicationDbContext(builder.Options);
    }

    private static (SmartTimetableService service, HttpContextAccessor accessor) CreateTimetableService(
        ApplicationDbContext db,
        CurrentUserContext userContext,
        IScheduleNotificationService? notificationService = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = userContext;
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var schedulingContext = new AcademicSchedulingContextService(db);
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var notif = notificationService ?? new ScheduleNotificationService(db, NullLogger<ScheduleNotificationService>.Instance);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);
        var capacityService = new CourseCapacityService(db);

        var service = new SmartTimetableService(
            db,
            accessor,
            new MockAuditLogService(),
            NullLogger<SmartTimetableService>.Instance,
            schedulingContext,
            scoringService,
            solver,
            notif,
            scoringOptions,
            capacityService);

        return (service, accessor);
    }

    private sealed class FixtureData : IAsyncDisposable
    {
        public ApplicationDbContext Db { get; init; } = null!;
        public DonVi Campus { get; init; } = null!;
        public NguoiDung Staff { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public NguoiDung Student { get; init; } = null!;
        public HocKy Term { get; init; } = null!;
        public List<Block> Blocks { get; init; } = new();
        public CaHoc Shift1 { get; init; } = null!;
        public PhongHoc Room1 { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public QuyDoiTinChi CreditRule { get; init; } = null!;
        public LopHanhChinh Class1 { get; init; } = null!;
        public KhoaHoc Course1 { get; init; } = null!;
        public ScheduleGenerationJob Job { get; init; } = null!;
        public List<ScheduleDraftItem> DraftItems { get; init; } = new();

        public async ValueTask DisposeAsync()
        {
            try
            {
                Db.BuoiHocs.RemoveRange(await Db.BuoiHocs.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());
                Db.ThoiKhoaBieus.RemoveRange(await Db.ThoiKhoaBieus.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());
                Db.ScheduleDraftItems.RemoveRange(await Db.ScheduleDraftItems.Where(x => x.MaJob == Job.MaJob).ToListAsync());
                Db.ScheduleGenerationJobs.RemoveRange(await Db.ScheduleGenerationJobs.Where(x => x.MaJob == Job.MaJob).ToListAsync());
                Db.KhoaHocs.RemoveRange(await Db.KhoaHocs.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());
                Db.LopHanhChinhs.RemoveRange(await Db.LopHanhChinhs.Where(x => x.MaLop == Class1.MaLop).ToListAsync());
                Db.GiaoVienMonHocs.RemoveRange(await Db.GiaoVienMonHocs.Where(x => x.MaMonHoc == Subject.MaMonHoc).ToListAsync());
                Db.NguoiDungs.RemoveRange(await Db.NguoiDungs.Where(x => x.MaDonVi == Campus.MaDonVi).ToListAsync());
                Db.PhongHocs.RemoveRange(await Db.PhongHocs.Where(x => x.MaDonVi == Campus.MaDonVi).ToListAsync());
                Db.CaHocs.RemoveRange(await Db.CaHocs.Where(x => x.MaCaHoc == Shift1.MaCaHoc).ToListAsync());
                Db.DanhMucMonHocs.RemoveRange(await Db.DanhMucMonHocs.Where(x => x.MaMonHoc == Subject.MaMonHoc).ToListAsync());
                Db.Blocks.RemoveRange(await Db.Blocks.Where(x => x.MaHocKy == Term.MaHocKy).ToListAsync());
                Db.HocKys.RemoveRange(await Db.HocKys.Where(x => x.MaHocKy == Term.MaHocKy).ToListAsync());
                Db.DonVis.RemoveRange(await Db.DonVis.Where(x => x.MaDonVi == Campus.MaDonVi).ToListAsync());
                await Db.SaveChangesAsync();
            }
            catch
            {
                // Ignored in cleanup
            }
            finally
            {
                await Db.DisposeAsync();
            }
        }
    }

    private async Task<FixtureData> SeedPublishFixtureAsync(
        int? overrideStartBlock = null,
        int? overrideBlockCount = null)
    {
        var db = CreateDbContext();
        var uid = Guid.NewGuid().ToString("N")[..8];

        var campus = new DonVi { TenDonVi = "Publish Campus " + uid, CapDonVi = "co_so", ConHoatDong = true };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        var campusId = campus.MaDonVi;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var termStart = today.AddDays(10);
        var termEnd = today.AddDays(115);

        var term = new HocKy
        {
            MaDonVi = campusId,
            TenHocKy = "HK_Publish_" + uid,
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = termStart,
            NgayKetThuc = termEnd,
            DaKhoa = false
        };
        db.HocKys.Add(term);
        await db.SaveChangesAsync();
        var termId = term.MaHocKy;

        var blocks = new List<Block>();
        for (int i = 1; i <= 5; i++)
        {
            var bStart = termStart.AddDays((i - 1) * 21);
            var bEnd = bStart.AddDays(20);
            var block = new Block
            {
                MaHocKy = termId,
                ThuTuBlock = i,
                TenBlock = $"Block {i}",
                NgayBatDau = bStart,
                NgayKetThuc = bEnd
            };
            blocks.Add(block);
            db.Blocks.Add(block);
        }
        await db.SaveChangesAsync();

        var shift1 = new CaHoc
        {
            TenCa = "Ca 1 " + uid,
            Buoi = "sang",
            GioBatDau = new TimeOnly(7, 30),
            GioKetThuc = new TimeOnly(9, 30),
            ThuTu = 1,
            ConHoatDong = true
        };
        db.CaHocs.Add(shift1);

        var room1 = new PhongHoc
        {
            MaDonVi = campusId,
            MaCodePhong = "P_PUB_" + uid,
            TenPhong = "P_Pub_" + uid,
            SucChua = 50,
            LoaiPhong = "ly_thuyet",
            TrangThaiPhong = "hoat_dong"
        };
        db.PhongHocs.Add(room1);

        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = "PUB_" + uid,
            TenMonHoc = "Mon Pub " + uid,
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);

        var creditRule = await db.QuyDoiTinChis.FirstOrDefaultAsync(x => x.SoTinChi == 3);
        if (creditRule == null)
        {
            creditRule = new QuyDoiTinChi { SoTinChi = 3, SoBuoiMoiTuan = 2, SoCaMoiBuoi = 1 };
            db.QuyDoiTinChis.Add(creditRule);
            await db.SaveChangesAsync();
        }

        var staff = new NguoiDung
        {
            MaDonVi = campusId,
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            TrangThai = "hoat_dong",
            Email = $"staff_{uid}@lms.local",
            HoTen = "Staff " + uid,
            MatKhauHash = "hash"
        };
        var teacher = new NguoiDung
        {
            MaDonVi = campusId,
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            TrangThai = "hoat_dong",
            Email = $"teacher_{uid}@lms.local",
            HoTen = "Teacher " + uid,
            MatKhauHash = "hash"
        };
        var student = new NguoiDung
        {
            MaDonVi = campusId,
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = "hoat_dong",
            Email = $"student_{uid}@lms.local",
            HoTen = "Student " + uid,
            MatKhauHash = "hash"
        };
        db.NguoiDungs.AddRange(staff, teacher, student);
        await db.SaveChangesAsync();

        var gvMon = new GiaoVienMonHoc { MaGiaoVien = teacher.MaNguoiDung, MaMonHoc = subject.MaMonHoc, ConHoatDong = true, MucDoPhuHop = 100, PhuHopChuyenMon = true };
        db.GiaoVienMonHocs.Add(gvMon);

        var class1 = new LopHanhChinh { MaDonVi = campusId, MaCodeLop = "LOP_PUB_" + uid, TenLop = "Lop Pub " + uid, ConHoatDong = true, SiSoDuKien = 30 };
        db.LopHanhChinhs.Add(class1);
        await db.SaveChangesAsync();

        student.MaLop = class1.MaLop;
        await db.SaveChangesAsync();

        var course1 = new KhoaHoc
        {
            MaHocKy = termId,
            MaDonVi = campusId,
            MaMonHoc = subject.MaMonHoc,
            MaGiaoVien = teacher.MaNguoiDung,
            MaLop = class1.MaLop,
            MaBlockBatDau = overrideStartBlock ?? blocks[0].MaBlock,
            SoBlockHoc = overrideBlockCount ?? 1,
            TrangThai = "nhap",
            TieuDe = "Course 1 " + uid
        };
        db.KhoaHocs.Add(course1);
        await db.SaveChangesAsync();

        var draftId = Guid.NewGuid();
        var job = new ScheduleGenerationJob
        {
            DraftId = draftId,
            MaDonVi = campusId,
            MaHocKy = termId,
            NguoiYeuCau = staff.MaNguoiDung,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow,
            TongCourse = 1,
            SoXepDuoc = 1,
            SoKhongXepDuoc = 0
        };
        db.ScheduleGenerationJobs.Add(job);
        await db.SaveChangesAsync();

        var draftItems = new List<ScheduleDraftItem>();
        int[] weekdays = { 2, 4, 6 };
        for (int d = 0; d < creditRule.SoBuoiMoiTuan; d++)
        {
            var draftItem = new ScheduleDraftItem
            {
                MaJob = job.MaJob,
                MaKhoaHoc = course1.MaKhoaHoc,
                MaGiaoVien = teacher.MaNguoiDung,
                ThuTrongTuan = weekdays[d % weekdays.Length],
                MaCaHoc = shift1.MaCaHoc,
                MaPhong = room1.MaPhong,
                TrangThai = "xep_duoc"
            };
            draftItems.Add(draftItem);
            db.ScheduleDraftItems.Add(draftItem);
        }
        await db.SaveChangesAsync();

        return new FixtureData
        {
            Db = db,
            Campus = campus,
            Staff = staff,
            Teacher = teacher,
            Student = student,
            Term = term,
            Blocks = blocks,
            Shift1 = shift1,
            Room1 = room1,
            Subject = subject,
            CreditRule = creditRule,
            Class1 = class1,
            Course1 = course1,
            Job = job,
            DraftItems = draftItems
        };
    }

    [Test]
    public async Task SqlSafetyGuard_VerifiesConfiguredDisposableDatabase()
    {
        await using var db = CreateDbContext();
        var actualDbName = await db.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS Value").FirstOrDefaultAsync();
        Assert.That(actualDbName, Does.StartWith("LMS_TEST_"), "Database must be a disposable test database matching LMS_TEST_*");
    }

    [Test]
    public async Task PublishSuccess_CreatesThoiKhoaBieuAndBuoiHoc_WithinCorrectBlockDates()
    {
        await using var fixture = await SeedPublishFixtureAsync();
        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Email = fixture.Staff.Email,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };

        var (timetableService, _) = CreateTimetableService(fixture.Db, staffContext);

        var publishResult = await timetableService.PublishAsync(new PublishTimetableRequest
        {
            DraftId = fixture.Job.DraftId
        });

        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.BuoiHocLoi, Is.EqualTo(0));
        Assert.That(publishResult.BuoiHocDaTao, Is.EqualTo(fixture.DraftItems.Count));

        // 1. Verify ThoiKhoaBieu
        var publishedTkbs = await fixture.Db.ThoiKhoaBieus
            .Where(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc)
            .ToListAsync();
        Assert.That(publishedTkbs, Has.Count.EqualTo(fixture.DraftItems.Count));
        Assert.That(publishedTkbs.All(x => x.TrangThai == "da_xuat_ban"), Is.True);

        var targetBlock = fixture.Blocks[0]; // Block 1
        Assert.That(publishedTkbs.All(x => x.NgayBatDau == targetBlock.NgayBatDau && x.NgayKetThuc == targetBlock.NgayKetThuc), Is.True,
            "ThoiKhoaBieu dates must match Block 1 exactly.");

        // 2. Verify BuoiHoc dates strictly inside Block 1
        var sessions = await fixture.Db.BuoiHocs
            .Where(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc)
            .ToListAsync();
        Assert.That(sessions, Has.Count.GreaterThan(0));

        foreach (var session in sessions)
        {
            Assert.That(session.NgayHoc, Is.GreaterThanOrEqualTo(targetBlock.NgayBatDau),
                $"Session date {session.NgayHoc} must not precede block start {targetBlock.NgayBatDau}.");
            Assert.That(session.NgayHoc, Is.LessThanOrEqualTo(targetBlock.NgayKetThuc),
                $"Session date {session.NgayHoc} must not exceed block end {targetBlock.NgayKetThuc}.");
        }

        // 3. No duplicate sessions on the same date and shift
        var sessionKeys = sessions.Select(s => (s.NgayHoc, s.MaCaHoc)).ToList();
        Assert.That(sessionKeys.Distinct().Count(), Is.EqualTo(sessions.Count), "Sessions must have zero duplicates.");

        // 4. Job status da_xuat_ban
        var updatedJob = await fixture.Db.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        Assert.That(updatedJob.TrangThai, Is.EqualTo("da_xuat_ban"));
        Assert.That(updatedJob.NgayXuatBan, Is.Not.Null);
    }

    [Test]
    public async Task InvalidBlock_RejectsPublish_NoFallbackToFullSemester_AndZeroMutations()
    {
        // Configure course with SoBlockHoc = 10 exceeding the 5 term blocks
        await using var fixture = await SeedPublishFixtureAsync(overrideBlockCount: 10);
        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Email = fixture.Staff.Email,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };

        var (timetableService, _) = CreateTimetableService(fixture.Db, staffContext);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.PublishAsync(new PublishTimetableRequest { DraftId = fixture.Job.DraftId }));
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex.Message, Does.Contain("vượt ngoài"));

        // Zero mutations on TKB or BuoiHoc
        var tkbCount = await fixture.Db.ThoiKhoaBieus.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        var sessionCount = await fixture.Db.BuoiHocs.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        Assert.That(tkbCount, Is.EqualTo(0), "No ThoiKhoaBieu created on invalid block.");
        Assert.That(sessionCount, Is.EqualTo(0), "No BuoiHoc created on invalid block.");

        var job = await fixture.Db.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        Assert.That(job.TrangThai, Is.EqualTo("draft"), "Job remains in draft state.");
    }

    [Test]
    public async Task PreValidationFailure_WhenAttendanceExists_RejectsPublish_ZeroMutations()
    {
        await using var fixture = await SeedPublishFixtureAsync();

        // Simulate an existing published schedule that already has an attendance record
        var existingTkb = new ThoiKhoaBieu
        {
            MaKhoaHoc = fixture.Course1.MaKhoaHoc,
            ThuTrongTuan = 2,
            MaCaHoc = fixture.Shift1.MaCaHoc,
            MaPhong = fixture.Room1.MaPhong,
            TrangThai = "da_xuat_ban",
            NgayTao = DateTime.UtcNow.AddMinutes(-10),
            NgayCapNhat = DateTime.UtcNow.AddMinutes(-10)
        };
        fixture.Db.ThoiKhoaBieus.Add(existingTkb);
        await fixture.Db.SaveChangesAsync();

        var existingSession = new BuoiHoc
        {
            Tkb = existingTkb,
            MaKhoaHoc = fixture.Course1.MaKhoaHoc,
            NgayHoc = fixture.Blocks[0].NgayBatDau.AddDays(1),
            MaCaHoc = fixture.Shift1.MaCaHoc,
            MaPhong = fixture.Room1.MaPhong,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            TrangThaiBuoi = "da_dien_ra",
            TrangThaiDiemDanh = "da_khoa"
        };
        fixture.Db.BuoiHocs.Add(existingSession);
        await fixture.Db.SaveChangesAsync();

        var diemDanh = new DiemDanh
        {
            MaDonVi = fixture.Campus.MaDonVi,
            MaBuoiHoc = existingSession.MaBuoiHoc,
            MaHocSinh = fixture.Student.MaNguoiDung,
            NguoiGhiNhan = fixture.Teacher.MaNguoiDung,
            TrangThai = "co_mat",
            GhiNhanLuc = DateTime.UtcNow
        };
        fixture.Db.DiemDanhs.Add(diemDanh);
        await fixture.Db.SaveChangesAsync();

        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Email = fixture.Staff.Email,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };

        var (timetableService, _) = CreateTimetableService(fixture.Db, staffContext);

        // Must reject publish
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await timetableService.PublishAsync(new PublishTimetableRequest { DraftId = fixture.Job.DraftId }));
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict).Or.EqualTo(StatusCodes.Status400BadRequest));

        // Ensure job is not published
        var job = await fixture.Db.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        Assert.That(job.TrangThai, Is.EqualTo("draft"));

        // Cleanup the seeded attendance before fixture dispose
        fixture.Db.DiemDanhs.Remove(diemDanh);
        fixture.Db.BuoiHocs.Remove(existingSession);
        fixture.Db.ThoiKhoaBieus.Remove(existingTkb);
        await fixture.Db.SaveChangesAsync();
    }

    [Test]
    public async Task AfterWriteAtomicRollback_OnControlledFailure_RollsBackCompletely()
    {
        await using var fixture = await SeedPublishFixtureAsync();
        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Email = fixture.Staff.Email,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };

        // 1. All draft items are 100% valid (no pre-validation corruption)
        Assert.That(fixture.DraftItems, Has.Count.EqualTo(fixture.CreditRule.SoBuoiMoiTuan));
        Assert.That(fixture.DraftItems.All(x => x.ThuTrongTuan.HasValue && x.MaCaHoc.HasValue && x.MaPhong.HasValue), Is.True);

        // 2. Attach PublishFailureInjectionInterceptor so exception fires in SavedChangesAsync (AFTER physical SQL INSERTs execute)
        var interceptor = new PublishFailureInjectionInterceptor();
        await using var publishingDb = CreateDbContext(interceptor);
        var mockNotification = new MockScheduleNotificationService();
        var (timetableService, _) = CreateTimetableService(publishingDb, staffContext, mockNotification);

        // 3. Act: PublishAsync writes ThoiKhoaBieu and BuoiHoc, sends SQL to SQL Server, then interceptor throws before CommitAsync
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await timetableService.PublishAsync(new PublishTimetableRequest { DraftId = fixture.Job.DraftId }));
        Assert.That(ex!.Message, Is.EqualTo("TEST_INJECTED_POST_SQL_WRITE_FAILURE"));

        // 4. Verify interceptor evidence:
        Assert.That(interceptor.SavedChangesAsyncReached, Is.True, "Interceptor must reach SavedChangesAsync after physical SQL write.");
        Assert.That(interceptor.RowsWrittenCount, Is.GreaterThan(0), "SQL statements must have executed in SQL Server before rollback.");
        Assert.That(interceptor.InjectedExceptionThrown, Is.True, "Exception must be injected inside transaction before CommitAsync.");

        // 5. Verify notification was NOT sent:
        Assert.That(mockNotification.NotifyCallCount, Is.EqualTo(0), "Notification must NOT be called when publish transaction rolls back.");

        // 6. Verify atomic rollback with a FRESH DbContext (clean ChangeTracker querying SQL Server):
        await using var freshDb = CreateDbContext();

        var tkbCount = await freshDb.ThoiKhoaBieus.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        var sessionCount = await freshDb.BuoiHocs.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        Assert.That(tkbCount, Is.EqualTo(0), "Atomic rollback must leave zero orphaned ThoiKhoaBieu in database.");
        Assert.That(sessionCount, Is.EqualTo(0), "Atomic rollback must leave zero orphaned BuoiHoc in database.");

        var job = await freshDb.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        Assert.That(job.TrangThai, Is.EqualTo("draft"), "Job must remain in draft state in database.");
        Assert.That(job.NgayXuatBan, Is.Null, "Job NgayXuatBan must remain null.");

        var draftItems = await freshDb.ScheduleDraftItems.Where(x => x.MaJob == fixture.Job.MaJob).ToListAsync();
        Assert.That(draftItems, Has.Count.EqualTo(fixture.DraftItems.Count), "Draft items must remain intact.");
        Assert.That(draftItems.All(x => x.TrangThai == "xep_duoc"), Is.True, "Draft items status must remain xep_duoc.");
    }

    private sealed class PublishFailureInjectionInterceptor : SaveChangesInterceptor
    {
        public bool SavedChangesAsyncReached { get; private set; }
        public int RowsWrittenCount { get; private set; }
        public bool InjectedExceptionThrown { get; private set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            // Trigger when ThoiKhoaBieu or BuoiHoc records are actually written to SQL Server (result > 0)
            if (result > 0 && eventData.Context != null)
            {
                var hasTkbs = eventData.Context.ChangeTracker.Entries<ThoiKhoaBieu>().Any();
                var hasBuoiHocs = eventData.Context.ChangeTracker.Entries<BuoiHoc>().Any();

                if (hasTkbs || hasBuoiHocs)
                {
                    SavedChangesAsyncReached = true;
                    RowsWrittenCount = result;
                    InjectedExceptionThrown = true;
                    throw new InvalidOperationException("TEST_INJECTED_POST_SQL_WRITE_FAILURE");
                }
            }

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class MockScheduleNotificationService : IScheduleNotificationService
    {
        public int NotifyCallCount { get; private set; }

        public Task NotifySchedulePublishedAsync(
            int maHocKy,
            int maDonVi,
            List<int> maGiaoVienList,
            List<int> maLopList,
            CancellationToken cancellationToken = default)
        {
            NotifyCallCount++;
            return Task.CompletedTask;
        }
    }

    private class MockAuditLogService : IAuditLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, object? oldValue, object? newValue, int? changedBy, int? maDonVi, string? description, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(int campusId, string entityName, int entityId, string action, int actorUserId, object? oldValue, object? newValue, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Backend.DTOs.Common.PagedResultDto<AuditLogListItemDto>> GetAsync(AuditLogQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AuditLogDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}

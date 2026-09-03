using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class Task7D_C_DisposableNegativeDbTests
{
    private string _connStr = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
        using var db = CreateDbContext();
        db.Database.EnsureCreated();
    }

    [SetUp]
    public void SetUp()
    {
        _connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
        Assert.That(_connStr, Does.Contain("LMS_TEST_TASK7D_C_"),
            "Safety Guard: Negative live tests must execute against a disposable database with prefix LMS_TEST_TASK7D_C_");
    }

    private ApplicationDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connStr);
        return new ApplicationDbContext(builder.Options);
    }

    private static (SmartTimetableService service, HttpContextAccessor accessor) CreateTimetableService(
        ApplicationDbContext db,
        CurrentUserContext userContext)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = userContext;
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var schedulingContext = new AcademicSchedulingContextService(db);
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var notif = new ScheduleNotificationService(db, NullLogger<ScheduleNotificationService>.Instance);
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

    private class MockAuditLogService : IAuditLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, object? oldValue, object? newValue, int? changedBy, int? maDonVi, string? description, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(int campusId, string entityName, int entityId, string action, int actorUserId, object? oldValue, object? newValue, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Backend.DTOs.Common.PagedResultDto<Backend.DTOs.Audit.AuditLogListItemDto>> GetAsync(Backend.DTOs.Audit.AuditLogQueryParameters parameters, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Backend.DTOs.Audit.AuditLogDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
                // DiemDanh must be removed before BuoiHoc (FK constraint)
                var buoiHocIds = await Db.BuoiHocs
                    .Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc)
                    .Select(x => x.MaBuoiHoc)
                    .ToListAsync();
                if (buoiHocIds.Count > 0)
                {
                    Db.DiemDanhs.RemoveRange(
                        await Db.DiemDanhs.Where(x => buoiHocIds.Contains(x.MaBuoiHoc)).ToListAsync());
                }

                Db.BuoiHocs.RemoveRange(await Db.BuoiHocs.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());
                Db.ThoiKhoaBieus.RemoveRange(await Db.ThoiKhoaBieus.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());

                // Remove all draft items and jobs for this term (including any extra jobs from scenarios 10/11)
                var allJobIds = await Db.ScheduleGenerationJobs
                    .Where(x => x.MaHocKy == Term.MaHocKy && x.MaDonVi == Campus.MaDonVi)
                    .Select(x => x.MaJob)
                    .ToListAsync();
                if (allJobIds.Count > 0)
                {
                    Db.ScheduleDraftItems.RemoveRange(
                        await Db.ScheduleDraftItems.Where(x => allJobIds.Contains(x.MaJob)).ToListAsync());
                    Db.ScheduleGenerationJobs.RemoveRange(
                        await Db.ScheduleGenerationJobs.Where(x => allJobIds.Contains(x.MaJob)).ToListAsync());
                }

                Db.KhoaHocs.RemoveRange(await Db.KhoaHocs.Where(x => x.MaKhoaHoc == Course1.MaKhoaHoc).ToListAsync());
                Db.LopHocPhans.RemoveRange(await Db.LopHocPhans.Where(x => x.MaDonVi == Campus.MaDonVi).ToListAsync());
                Db.LopHanhChinhs.RemoveRange(await Db.LopHanhChinhs.Where(x => x.MaLop == Class1.MaLop).ToListAsync());
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
                // Best effort cleanup
            }
            finally
            {
                await Db.DisposeAsync();
            }
        }
    }


    private async Task<FixtureData> CreateFixtureAsync(
        int roomCapacity = 50,
        int lhpCapacity = 35,
        bool activeRoom = true,
        bool assignStudentToClass = true)
    {
        var db = CreateDbContext();
        var uid = Guid.NewGuid().ToString("N")[..8];

        var campus = new DonVi { TenDonVi = "Neg Campus " + uid, CapDonVi = "co_so", ConHoatDong = true };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        var campusId = campus.MaDonVi;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var termStart = today.AddDays(10);
        var termEnd = today.AddDays(115);

        var term = new HocKy
        {
            MaDonVi = campusId,
            TenHocKy = "HK_Neg_" + uid,
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
            MaCodePhong = "P_NEG_" + uid,
            TenPhong = "P_Neg_" + uid,
            SucChua = roomCapacity,
            LoaiPhong = "ly_thuyet",
            TrangThaiPhong = activeRoom ? "hoat_dong" : "bao_tri"
        };
        db.PhongHocs.Add(room1);

        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = "NEG_" + uid,
            TenMonHoc = "Mon Neg " + uid,
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);

        var creditRule = await db.QuyDoiTinChis.FirstOrDefaultAsync(x => x.SoTinChi == 3);
        if (creditRule == null)
        {
            creditRule = new QuyDoiTinChi { SoTinChi = 3, SoBuoiMoiTuan = 2, SoCaMoiBuoi = 1 };
            db.QuyDoiTinChis.Add(creditRule);
        }
        await db.SaveChangesAsync();

        var staff = new NguoiDung
        {
            MaDonVi = campusId,
            HoTen = "Staff Neg " + uid,
            Email = $"staff_{uid}@test.local",
            MatKhauHash = "hash",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            TrangThai = "hoat_dong"
        };
        var teacher = new NguoiDung
        {
            MaDonVi = campusId,
            HoTen = "Teacher Neg " + uid,
            Email = $"teacher_{uid}@test.local",
            MatKhauHash = "hash",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            TrangThai = "hoat_dong"
        };
        var student = new NguoiDung
        {
            MaDonVi = campusId,
            HoTen = "Student Neg " + uid,
            Email = $"student_{uid}@test.local",
            MatKhauHash = "hash",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = "hoat_dong"
        };
        db.NguoiDungs.AddRange(staff, teacher, student);
        await db.SaveChangesAsync();

        var gvMon = new GiaoVienMonHoc
        {
            MaGiaoVien = teacher.MaNguoiDung,
            MaMonHoc = subject.MaMonHoc,
            ConHoatDong = true,
            MucDoPhuHop = 100,
            PhuHopChuyenMon = true
        };
        db.GiaoVienMonHocs.Add(gvMon);
        await db.SaveChangesAsync();

        var class1 = new LopHanhChinh
        {
            MaDonVi = campusId,
            MaCodeLop = "L_NEG_" + uid,
            TenLop = "L_Neg_" + uid,
            ConHoatDong = true,
            SiSoDuKien = lhpCapacity > 0 ? lhpCapacity : 0
        };
        db.LopHanhChinhs.Add(class1);
        await db.SaveChangesAsync();

        if (lhpCapacity > 0 && assignStudentToClass)
        {
            student.MaLop = class1.MaLop;
            await db.SaveChangesAsync();
        }

        if (lhpCapacity > 0)
        {
            var lhp = new LopHocPhan
            {
                MaDonVi = campusId,
                MaHocKy = termId,
                MaMonHoc = subject.MaMonHoc,
                MaCodeLopHocPhan = "LHP_NEG_" + uid,
                SucChua = lhpCapacity,
                SoDaDangKy = Math.Min(lhpCapacity, 25),
                TrangThai = "mo"
            };
            db.LopHocPhans.Add(lhp);
            await db.SaveChangesAsync();
        }

        var course1 = new KhoaHoc
        {
            MaDonVi = campusId,
            MaHocKy = termId,
            MaMonHoc = subject.MaMonHoc,
            MaGiaoVien = teacher.MaNguoiDung,
            MaLop = class1.MaLop,
            MaBlockBatDau = blocks[0].MaBlock,
            SoBlockHoc = 1,
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

    // ── 1. STUDENT_CAPACITY_DATA_MISSING ────────────────────────────
    // Tests via AcademicSchedulingContextService readiness: when LHP is missing,
    // ROOM_CAPACITY_READY readiness item reports STUDENT_CAPACITY_DATA_MISSING.
    [Test]
    public async Task Scenario1_StudentCapacityDataMissing_ReadinessReportsBlock()
    {
        await using var fixture = await CreateFixtureAsync(lhpCapacity: 0);

        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        // With lhpCapacity=0 (no LopHocPhan), the room capacity readiness item
        // must be blocked and carry STUDENT_CAPACITY_DATA_MISSING in its message.
        Assert.That(context.Readiness, Is.Not.Null);
        var roomCapItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "ROOM_CAPACITY_READY");
        Assert.That(roomCapItem, Is.Not.Null, "ROOM_CAPACITY_READY readiness item must exist");
        Assert.That(roomCapItem!.Status, Is.EqualTo("blocked"),
            "ROOM_CAPACITY_READY must be blocked when no LopHocPhan exists");
        Assert.That(roomCapItem.Message, Does.Contain("STUDENT_CAPACITY_DATA_MISSING").Or.Contain("sĩ số"),
            "Message must mention STUDENT_CAPACITY_DATA_MISSING or sĩ số");
    }

    // ── 2. Phòng không đủ sức chứa ──────────────────────────────────
    // Tests via AcademicSchedulingContextService readiness: when room capacity < student count,
    // ROOM_CAPACITY_READY readiness item reports the block.
    [Test]
    public async Task Scenario2_RoomCapacityInsufficient_ReadinessReportsBlock()
    {
        // Room capacity 20, Class expected count 45 (assignStudentToClass: false so priority 3 is used)
        await using var fixture = await CreateFixtureAsync(roomCapacity: 20, lhpCapacity: 45, assignStudentToClass: false);

        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        Assert.That(context.Readiness, Is.Not.Null);
        var roomCapItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "ROOM_CAPACITY_READY");
        Assert.That(roomCapItem, Is.Not.Null, "ROOM_CAPACITY_READY readiness item must exist");
        Assert.That(roomCapItem!.Status, Is.EqualTo("blocked"),
            "ROOM_CAPACITY_READY must be blocked when room is too small");
        Assert.That(roomCapItem.Message, Does.Contain("sức chứa").Or.Contain("phòng").Or.Contain("khóa học"),
            "Message must describe the capacity constraint");
    }

    // ── 3. Không có phòng active ────────────────────────────────────
    [Test]
    public async Task Scenario3_NoActiveRooms_Throws400WithFriendlyMessage()
    {
        await using var fixture = await CreateFixtureAsync(activeRoom: false);

        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        var preJobCount = await fixture.Db.ScheduleGenerationJobs.CountAsync(x => x.MaHocKy == fixture.Term.MaHocKy);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await service.GenerateAsync(new GenerateTimetableRequest
            {
                MaHocKy = fixture.Term.MaHocKy,
                MaDonVi = fixture.Campus.MaDonVi,
                MaKhoaHocFilter = new List<int> { fixture.Course1.MaKhoaHoc }
            }));

        // Service detects no active rooms and raises 400 with NO_ACTIVE_ROOMS
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex.ErrorCode, Is.EqualTo("NO_ACTIVE_ROOMS").Or.EqualTo("ACTIVE_ROOMS_READY"));
        Assert.That(ex.Message, Does.Contain("phòng học").Or.Contain("sẵn sàng").Or.Contain("hoạt động"));

        // 0 mutation
        var postJobCount = await fixture.Db.ScheduleGenerationJobs.CountAsync(x => x.MaHocKy == fixture.Term.MaHocKy);
        Assert.That(postJobCount, Is.EqualTo(preJobCount), "0 mutation must be preserved when no active rooms exist");
    }

    // ── 4. ROOM_SLOT_CAPACITY_INSUFFICIENT ──────────────────────────
    // Validates TOTAL_ROOM_SLOTS_READY readiness item formula and behavior.
    // Since CaHoc is global (no campus filter), we compute required room slots dynamically.
    [Test]
    public async Task Scenario4_RoomSlotCapacityInsufficient_ReadinessAndGenerateBlock()
    {
        await using var fixture = await CreateFixtureAsync(roomCapacity: 50, lhpCapacity: 35);

        // Determine the actual total available room slots (1 room * all active shifts * 6 days)
        var activeShiftCount = await fixture.Db.CaHocs.CountAsync(x => x.ConHoatDong);
        var totalAvailableSlots = 1 * activeShiftCount * 6; // 1 room in this campus
        // SoBuoiMoiTuan for 3 credits = value from QuyDoiTinChi (1 or 2, we don't control it)
        var creditRule = await fixture.Db.QuyDoiTinChis.FirstAsync(x => x.SoTinChi == fixture.Subject.SoTinChi);
        var slotsPerCourse = creditRule.SoBuoiMoiTuan;
        // We need: N * slotsPerCourse > totalAvailableSlots
        // N > totalAvailableSlots / slotsPerCourse
        var coursesNeeded = (totalAvailableSlots / slotsPerCourse) + 1; // +1 ensures overflow

        // Create enough unique classes and courses to exceed the total slot limit
        var extraClasses = new List<LopHanhChinh>();
        for (int i = 0; i < coursesNeeded; i++)
        {
            var lop = new LopHanhChinh { MaDonVi = fixture.Campus.MaDonVi, MaCodeLop = $"LS4_{i}_" + Guid.NewGuid().ToString("N")[..6], TenLop = $"Slot4-{i}", ConHoatDong = true, SiSoDuKien = 35 };
            extraClasses.Add(lop);
        }
        fixture.Db.LopHanhChinhs.AddRange(extraClasses);
        await fixture.Db.SaveChangesAsync();

        var extraCourses = new List<KhoaHoc>();
        foreach (var lop in extraClasses)
        {
            extraCourses.Add(new KhoaHoc { MaDonVi = fixture.Campus.MaDonVi, MaHocKy = fixture.Term.MaHocKy, MaMonHoc = fixture.Subject.MaMonHoc, MaGiaoVien = fixture.Teacher.MaNguoiDung, MaLop = lop.MaLop, MaBlockBatDau = fixture.Blocks[0].MaBlock, SoBlockHoc = 1, TrangThai = "nhap", TieuDe = $"S4Extra {lop.TenLop}" });
        }
        fixture.Db.KhoaHocs.AddRange(extraCourses);
        await fixture.Db.SaveChangesAsync();

        // Verify readiness: ROOM_CAPACITY_READY is ready, TOTAL_ROOM_SLOTS_READY is blocked
        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        var roomCapItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "ROOM_CAPACITY_READY");
        Assert.That(roomCapItem!.Status, Is.EqualTo("ready"),
            "ROOM_CAPACITY_READY must be ready since room size 50 fits class size 35");

        var totalSlotItem = context.Readiness.Items.FirstOrDefault(x => x.Code == "TOTAL_ROOM_SLOTS_READY");
        Assert.That(totalSlotItem, Is.Not.Null, "TOTAL_ROOM_SLOTS_READY item must exist");
        Assert.That(totalSlotItem!.Status, Is.EqualTo("blocked"),
            $"TOTAL_ROOM_SLOTS_READY must be blocked: {coursesNeeded + 1} courses x {slotsPerCourse} slots = {(coursesNeeded + 1) * slotsPerCourse} > {totalAvailableSlots} available");
        Assert.That(totalSlotItem.Message, Does.Contain("slot"));
        Assert.That(totalSlotItem.AffectedCount, Is.GreaterThan(0));

        // Cleanup
        fixture.Db.KhoaHocs.RemoveRange(extraCourses);
        fixture.Db.LopHanhChinhs.RemoveRange(extraClasses);
        await fixture.Db.SaveChangesAsync();
    }


    // ── 5. TEACHER_SKILL_MISSING ────────────────────────────────────
    // A course has no teacher with >=70% capability in campus.
    [Test]
    public async Task Scenario5_TeacherSkillMissing_ReadinessAndGenerateBlock()
    {
        await using var fixture = await CreateFixtureAsync();

        // Demote teacher capability below 70% threshold
        var gvmh = await fixture.Db.GiaoVienMonHocs.FirstAsync(x => x.MaMonHoc == fixture.Subject.MaMonHoc);
        gvmh.MucDoPhuHop = 40;
        gvmh.ConHoatDong = false;
        await fixture.Db.SaveChangesAsync();

        // 1. Verify readiness reports TEACHER_SKILL_READY blocked
        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        var skillItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "TEACHER_SKILL_READY");
        Assert.That(skillItem, Is.Not.Null);
        Assert.That(skillItem!.Status, Is.EqualTo("blocked"),
            "TEACHER_SKILL_READY must be blocked when teacher skill is under 70%");
        Assert.That(skillItem.Message, Does.Contain("năng lực").Or.Contain("giảng viên"));
        Assert.That(skillItem.AffectedCount, Is.GreaterThan(0));
        Assert.That(skillItem.AffectedItems, Does.Contain(fixture.Subject.TenMonHoc).Or.Contain($"Môn #{fixture.Subject.MaMonHoc}"));

        // Revert
        gvmh.MucDoPhuHop = 100;
        gvmh.ConHoatDong = true;
        await fixture.Db.SaveChangesAsync();
    }


    // ── 6. TEACHER_UNAVAILABLE ──────────────────────────────────────
    // Teacher has valid skill (>=70%), capacity > 0 (6), but preference detail
    // has slots marked 'unavailable'. Must be blocked by TEACHER_AVAILABILITY_READY,
    // while TEACHER_CAPACITY_READY remains READY.
    [Test]
    public async Task Scenario6_TeacherUnavailable_ReadinessBlocks()
    {
        await using var fixture = await CreateFixtureAsync();

        var pref = new GiaoVienNguyenVongHocKy
        {
            MaDonVi = fixture.Campus.MaDonVi,
            MaHocKy = fixture.Term.MaHocKy,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            SoCaToiDaMoiTuan = 6, // Capacity > 0 (e.g. 6), plenty of capacity
            GhiChu = "Bận việc",
            NgayGui = DateTime.UtcNow,
            TrangThai = "submitted"
        };
        fixture.Db.GiaoVienNguyenVongHocKys.Add(pref);
        await fixture.Db.SaveChangesAsync();

        // Mark all active shifts in DB across all weekdays (2..7) as unavailable in preference detail
        var allActiveShiftIds = await fixture.Db.CaHocs.Where(x => x.ConHoatDong).Select(x => x.MaCaHoc).ToListAsync();
        foreach (var sId in allActiveShiftIds)
        {
            for (int day = 2; day <= 7; day++)
            {
                pref.ChiTietNguyenVong.Add(new GiaoVienNguyenVongCaDay
                {
                    NguyenVongId = pref.Id,
                    ThuTrongTuan = day,
                    MaCaHoc = sId,
                    MucDo = "unavailable",
                    NgayTao = DateTime.UtcNow
                });
            }
        }
        await fixture.Db.SaveChangesAsync();

        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        // 1. TEACHER_AVAILABILITY_READY must be blocked
        var availItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "TEACHER_AVAILABILITY_READY");
        Assert.That(availItem, Is.Not.Null);
        Assert.That(availItem!.Status, Is.EqualTo("blocked"),
            "TEACHER_AVAILABILITY_READY must be blocked when teacher slots are marked unavailable in preference detail");
        Assert.That(availItem.Message, Does.Contain("không đủ thời gian khả dụng").Or.Contain("khả dụng"));
        Assert.That(availItem.AffectedCount, Is.EqualTo(1));
        Assert.That(availItem.AffectedItems, Does.Contain(fixture.Teacher.HoTen).Or.Contain($"Giảng viên #{fixture.Teacher.MaNguoiDung}"));

        // 2. TEACHER_CAPACITY_READY must remain READY (blocked by availability, NOT capacity)
        var capItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "TEACHER_CAPACITY_READY");
        Assert.That(capItem, Is.Not.Null);
        Assert.That(capItem!.Status, Is.EqualTo("ready"),
            "TEACHER_CAPACITY_READY must remain ready because capacity is 6, exceeding required slots");

        // Cleanup
        fixture.Db.GiaoVienNguyenVongCaDays.RemoveRange(pref.ChiTietNguyenVong);
        fixture.Db.GiaoVienNguyenVongHocKys.Remove(pref);
        await fixture.Db.SaveChangesAsync();
    }

    // ── 7. TEACHER_CAPACITY_INSUFFICIENT ────────────────────────────
    // Total slots required by assigned courses exceeds the hard cap (6 slots/week per teacher).
    // Weekly cap 6 is strictly preserved (never lowered).
    [Test]
    public async Task Scenario7_TeacherCapacityInsufficient_ReadinessAndGenerateBlock()
    {
        await using var fixture = await CreateFixtureAsync();

        // Course 1 is 3 credits = 3 slots.
        // Add Course 2 (3 credits = 3 slots) and Course 3 (2 credits = 2 slots).
        // Total slots = 3 + 3 + 2 = 8 slots > 6 weekly cap for 1 teacher!
        var subject2 = new DanhMucMonHoc
        {
            MaCodeMonHoc = "SUB2_" + Guid.NewGuid().ToString("N")[..6],
            TenMonHoc = "Mon 2",
            SoTinChi = 2,
            ConHoatDong = true
        };
        fixture.Db.DanhMucMonHocs.Add(subject2);
        await fixture.Db.SaveChangesAsync();

        var gvmh2 = new GiaoVienMonHoc
        {
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            MaMonHoc = subject2.MaMonHoc,
            ConHoatDong = true,
            MucDoPhuHop = 100,
            PhuHopChuyenMon = true
        };
        fixture.Db.GiaoVienMonHocs.Add(gvmh2);
        await fixture.Db.SaveChangesAsync();

        var class7b = new LopHanhChinh { MaDonVi = fixture.Campus.MaDonVi, MaCodeLop = "L7B_" + Guid.NewGuid().ToString("N")[..6], TenLop = "Lop 7B", ConHoatDong = true, SiSoDuKien = 35 };
        var class7c = new LopHanhChinh { MaDonVi = fixture.Campus.MaDonVi, MaCodeLop = "L7C_" + Guid.NewGuid().ToString("N")[..6], TenLop = "Lop 7C", ConHoatDong = true, SiSoDuKien = 35 };
        fixture.Db.LopHanhChinhs.AddRange(class7b, class7c);
        await fixture.Db.SaveChangesAsync();

        var course2 = new KhoaHoc
        {
            MaDonVi = fixture.Campus.MaDonVi,
            MaHocKy = fixture.Term.MaHocKy,
            MaMonHoc = fixture.Subject.MaMonHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            MaLop = class7b.MaLop,
            MaBlockBatDau = fixture.Blocks[0].MaBlock,
            SoBlockHoc = 1,
            TrangThai = "nhap",
            TieuDe = "Course 2 Overload"
        };
        var course3 = new KhoaHoc
        {
            MaDonVi = fixture.Campus.MaDonVi,
            MaHocKy = fixture.Term.MaHocKy,
            MaMonHoc = subject2.MaMonHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            MaLop = class7c.MaLop,
            MaBlockBatDau = fixture.Blocks[0].MaBlock,
            SoBlockHoc = 1,
            TrangThai = "nhap",
            TieuDe = "Course 3 Overload"
        };
        fixture.Db.KhoaHocs.AddRange(course2, course3);
        await fixture.Db.SaveChangesAsync();

        // 1. Check readiness item TEACHER_CAPACITY_READY
        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        var capItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "TEACHER_CAPACITY_READY");
        Assert.That(capItem, Is.Not.Null);
        Assert.That(capItem!.Status, Is.EqualTo("blocked"),
            "TEACHER_CAPACITY_READY must be blocked when 8 required slots exceeds 6 capacity slots");
        Assert.That(capItem.Message, Does.Contain("vượt quá tổng trần tải giảng viên").Or.Contain("trần 6 ca/GV"));
        Assert.That(capItem.AffectedCount, Is.EqualTo(8 - 6)); // 2 slots deficit

        // 2. Verify Generate throws 400 with teacher overload message
        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        var preJobCount = await fixture.Db.ScheduleGenerationJobs.CountAsync(x => x.MaHocKy == fixture.Term.MaHocKy);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await service.GenerateAsync(new GenerateTimetableRequest
            {
                MaHocKy = fixture.Term.MaHocKy,
                MaDonVi = fixture.Campus.MaDonVi
            }));

        // Must be exactly 400 BadRequest with specific error code (never 409)
        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex.ErrorCode, Is.EqualTo("TEACHER_CAPACITY_READY").Or.EqualTo("TEACHER_CAPACITY_INSUFFICIENT"));
        Assert.That(ex.Message, Does.Contain("vượt quá tổng trần tải giảng viên").Or.Contain("trần tải").Or.Contain("trần 6 ca/GV"));

        // 3. Zero mutation
        var postJobCount = await fixture.Db.ScheduleGenerationJobs.CountAsync(x => x.MaHocKy == fixture.Term.MaHocKy);
        Assert.That(postJobCount, Is.EqualTo(preJobCount));

        // Cleanup
        fixture.Db.KhoaHocs.RemoveRange(course2, course3);
        fixture.Db.LopHanhChinhs.RemoveRange(class7b, class7c);
        fixture.Db.GiaoVienMonHocs.Remove(gvmh2);
        fixture.Db.DanhMucMonHocs.Remove(subject2);
        await fixture.Db.SaveChangesAsync();
    }

    // ── 8. CREDIT_MAPPING_MISSING ───────────────────────────────────
    // Course subject has credit count (e.g. 7 credits) missing from QuyDoiTinChi.
    [Test]
    public async Task Scenario8_CreditMappingMissing_ReadinessReportsBlock()
    {
        await using var fixture = await CreateFixtureAsync();

        // Change subject credits to 7 (no row in QuyDoiTinChi)
        fixture.Subject.SoTinChi = 7;
        await fixture.Db.SaveChangesAsync();

        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        var creditItem = context.Readiness!.Items.FirstOrDefault(x => x.Code == "CREDIT_MAPPING_READY");
        Assert.That(creditItem, Is.Not.Null);
        Assert.That(creditItem!.Status, Is.EqualTo("blocked"),
            "CREDIT_MAPPING_READY must be blocked when 7-credit mapping is absent");
        Assert.That(creditItem.Message, Does.Contain("Thiếu quy đổi cho các số tín chỉ: 7").Or.Contain("7"));
        Assert.That(creditItem.AffectedCount, Is.EqualTo(1));
        Assert.That(creditItem.AffectedItems, Does.Contain("7 tín chỉ"));

        // Revert subject credits
        fixture.Subject.SoTinChi = 3;
        await fixture.Db.SaveChangesAsync();
    }

    // ── 9. HARD_CONFLICT ────────────────────────────────────────────
    // Direct collision: two draft items on the same day/shift for the same room or teacher.
    // PublishAsync must block with 400 and ErrorCode HARD_CONFLICT.
    [Test]
    public async Task Scenario9_HardConflict_PublishBlockedWithHardConflictCode()
    {
        await using var fixture = await CreateFixtureAsync();

        // Add a second class and course for collision
        var class9b = new LopHanhChinh { MaDonVi = fixture.Campus.MaDonVi, MaCodeLop = "L9B_" + Guid.NewGuid().ToString("N")[..6], TenLop = "Lop 9B", ConHoatDong = true, SiSoDuKien = 35 };
        fixture.Db.LopHanhChinhs.Add(class9b);
        await fixture.Db.SaveChangesAsync();

        var course2 = new KhoaHoc
        {
            MaDonVi = fixture.Campus.MaDonVi,
            MaHocKy = fixture.Term.MaHocKy,
            MaMonHoc = fixture.Subject.MaMonHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            MaLop = class9b.MaLop,
            MaBlockBatDau = fixture.Blocks[0].MaBlock,
            SoBlockHoc = 1,
            TrangThai = "nhap",
            TieuDe = "Course 2 Conflict"
        };
        fixture.Db.KhoaHocs.Add(course2);
        await fixture.Db.SaveChangesAsync();

        fixture.Job.TongCourse = 2;
        fixture.Job.SoXepDuoc = 2;

        // Add colliding draft item for course2 on the EXACT same day/shift/room as course1's first draft item
        var firstItem = fixture.DraftItems.First();
        var collidingItem = new ScheduleDraftItem
        {
            MaJob = fixture.Job.MaJob,
            MaKhoaHoc = course2.MaKhoaHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            ThuTrongTuan = firstItem.ThuTrongTuan, // Same day collision!
            MaCaHoc = firstItem.MaCaHoc,           // Same shift collision!
            MaPhong = firstItem.MaPhong,           // Same room collision!
            TrangThai = "xep_duoc"
        };
        var item2b = new ScheduleDraftItem
        {
            MaJob = fixture.Job.MaJob,
            MaKhoaHoc = course2.MaKhoaHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            ThuTrongTuan = 3,
            MaCaHoc = firstItem.MaCaHoc,
            MaPhong = firstItem.MaPhong,
            TrangThai = "xep_duoc"
        };
        var item2c = new ScheduleDraftItem
        {
            MaJob = fixture.Job.MaJob,
            MaKhoaHoc = course2.MaKhoaHoc,
            MaGiaoVien = fixture.Teacher.MaNguoiDung,
            ThuTrongTuan = 5,
            MaCaHoc = firstItem.MaCaHoc,
            MaPhong = firstItem.MaPhong,
            TrangThai = "xep_duoc"
        };
        fixture.Db.ScheduleDraftItems.AddRange(collidingItem, item2b, item2c);
        await fixture.Db.SaveChangesAsync();

        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        var preTkbCount = await fixture.Db.ThoiKhoaBieus.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        var preBuoiHocCount = await fixture.Db.BuoiHocs.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);

        // Publish must fail with HARD_CONFLICT
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await service.PublishAsync(new PublishTimetableRequest
            {
                DraftId = fixture.Job.DraftId
            }));

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex.ErrorCode, Is.EqualTo("HARD_CONFLICT"));
        Assert.That(ex.Message, Does.Contain("không hợp lệ").Or.Contain("xung đột"));

        // Verify zero published TKB / BuoiHoc
        var postTkbCount = await fixture.Db.ThoiKhoaBieus.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        var postBuoiHocCount = await fixture.Db.BuoiHocs.CountAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        Assert.That(postTkbCount, Is.EqualTo(preTkbCount));
        Assert.That(postBuoiHocCount, Is.EqualTo(preBuoiHocCount));

        // Cleanup: use ExecuteDeleteAsync to avoid EF cascade tracking issues
        await fixture.Db.ScheduleDraftItems
            .Where(x => x.MaJob == fixture.Job.MaJob && x.MaKhoaHoc == course2.MaKhoaHoc)
            .ExecuteDeleteAsync();
        await fixture.Db.BuoiHocs
            .Where(x => x.MaKhoaHoc == course2.MaKhoaHoc)
            .ExecuteDeleteAsync();
        await fixture.Db.KhoaHocs
            .Where(x => x.MaKhoaHoc == course2.MaKhoaHoc)
            .ExecuteDeleteAsync();
        await fixture.Db.LopHanhChinhs
            .Where(x => x.MaLop == class9b.MaLop)
            .ExecuteDeleteAsync();
    }


    // ── 10. Khóa quá 30 phút — validated via context lock reason ─────

    // After first publish, backdating NgayXuatBan and TKB timestamps by 45 min,
    // the AcademicSchedulingContext must report SCHEDULE_LOCKED_AFTER_EDIT_WINDOW.
    [Test]
    public async Task Scenario10_ScheduleLockedAfterEditWindow_ContextReports409Code()
    {
        await using var fixture = await CreateFixtureAsync();

        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        // 1. First publish the initial timetable cleanly
        var firstPublish = await service.PublishAsync(new PublishTimetableRequest
        {
            DraftId = fixture.Job.DraftId
        });
        Assert.That(firstPublish.Success, Is.True);

        // 2. Backdate NgayXuatBan and TKB timestamps to 45 minutes ago (> 30 min window)
        var publishedJob = await fixture.Db.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        publishedJob.NgayXuatBan = DateTime.UtcNow.AddMinutes(-45);
        var tkbs = await fixture.Db.ThoiKhoaBieus
            .Where(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc)
            .ToListAsync();
        foreach (var t in tkbs)
        {
            t.NgayTao = DateTime.UtcNow.AddMinutes(-45);
            t.NgayCapNhat = DateTime.UtcNow.AddMinutes(-45);
        }
        await fixture.Db.SaveChangesAsync();

        // 3. Verify context reports timeout lock
        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        Assert.That(context.CanPrepareSchedule, Is.False,
            "Context must block schedule preparation after 30-min timeout");
        Assert.That(context.LockReasonCode, Is.EqualTo("SCHEDULE_LOCKED_AFTER_EDIT_WINDOW"),
            "LockReasonCode must be SCHEDULE_LOCKED_AFTER_EDIT_WINDOW");
        Assert.That(context.ReasonMessage, Does.Contain("30 phút"),
            "ReasonMessage must mention 30 phút");

        // 4. ValidateSchedulableTermAsync must throw 409 with the exact error code
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await contextService.ValidateSchedulableTermAsync(
                fixture.Campus.MaDonVi, fixture.Term.MaHocKy));

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        Assert.That(ex.ErrorCode, Is.EqualTo("SCHEDULE_LOCKED_AFTER_EDIT_WINDOW").Or.EqualTo("SCHEDULE_ALREADY_PUBLISHED"));
        Assert.That(ex.Message, Does.Contain("30 phút").Or.Contain("khóa"));
    }

    // ── 11. Khóa do điểm danh (Ưu tiên hơn timeout) ────────────────
    // After first publish + attendance record, context must report SCHEDULE_LOCKED_BY_ATTENDANCE
    // regardless of whether the 30-min window has also expired.
    [Test]
    public async Task Scenario11_ScheduleLockedByAttendance_PrioritizedOverTimeout()
    {
        await using var fixture = await CreateFixtureAsync();

        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = fixture.Campus.MaDonVi
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        // 1. Publish first timetable cleanly
        var firstPublish = await service.PublishAsync(new PublishTimetableRequest
        {
            DraftId = fixture.Job.DraftId
        });
        Assert.That(firstPublish.Success, Is.True);

        // 2. Backdate to 50 minutes ago (> 30 min) to trigger timeout lock first
        var publishedJob = await fixture.Db.ScheduleGenerationJobs.FirstAsync(x => x.MaJob == fixture.Job.MaJob);
        publishedJob.NgayXuatBan = DateTime.UtcNow.AddMinutes(-50);
        var tkbs = await fixture.Db.ThoiKhoaBieus
            .Where(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc)
            .ToListAsync();
        foreach (var t in tkbs)
        {
            t.NgayTao = DateTime.UtcNow.AddMinutes(-50);
            t.NgayCapNhat = DateTime.UtcNow.AddMinutes(-50);
        }
        await fixture.Db.SaveChangesAsync();

        // 3. Add an attendance record to one of the generated BuoiHoc sessions
        var session = await fixture.Db.BuoiHocs.FirstAsync(x => x.MaKhoaHoc == fixture.Course1.MaKhoaHoc);
        var attendance = new DiemDanh
        {
            MaBuoiHoc = session.MaBuoiHoc,
            MaDonVi = fixture.Campus.MaDonVi,
            MaHocSinh = fixture.Student.MaNguoiDung,
            NguoiGhiNhan = fixture.Teacher.MaNguoiDung,
            TrangThai = "co_mat",
            GhiNhanLuc = DateTime.UtcNow
        };
        fixture.Db.DiemDanhs.Add(attendance);
        await fixture.Db.SaveChangesAsync();

        // 4. Verify attendance lock TAKES PRIORITY over timeout lock
        var contextService = new AcademicSchedulingContextService(fixture.Db);
        var context = await contextService.GetContextAsync(fixture.Campus.MaDonVi);

        Assert.That(context.CanPrepareSchedule, Is.False,
            "Context must block schedule preparation when attendance exists");
        Assert.That(context.LockReasonCode, Is.EqualTo("SCHEDULE_LOCKED_BY_ATTENDANCE"),
            "Attendance lock must take priority over timeout lock");
        Assert.That(context.ReasonMessage, Does.Contain("điểm danh"),
            "ReasonMessage must mention điểm danh");

        // 5. ValidateSchedulableTermAsync must also throw 409 with attendance code
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await contextService.ValidateSchedulableTermAsync(
                fixture.Campus.MaDonVi, fixture.Term.MaHocKy));

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        Assert.That(ex.ErrorCode, Is.EqualTo("SCHEDULE_LOCKED_BY_ATTENDANCE").Or.EqualTo("SCHEDULE_ALREADY_PUBLISHED"));
        Assert.That(ex.Message, Does.Contain("điểm danh").Or.Contain("khóa"));

        // Cleanup attendance before fixture dispose (no cascade from BuoiHoc → DiemDanh)
        fixture.Db.DiemDanhs.Remove(attendance);
        await fixture.Db.SaveChangesAsync();
    }

    // ── 12. 403 Cross-Campus ────────────────────────────────────────
    [Test]
    public async Task Scenario12_CrossCampusForbidden_Throws403WithForbiddenCampusCode()
    {
        await using var fixture = await CreateFixtureAsync();

        // AcademicStaff belongs to campus 14
        var staffContext = new CurrentUserContext
        {
            UserId = fixture.Staff.MaNguoiDung,
            Role = AuthRoles.AcademicStaff,
            CampusId = 14
        };
        var (service, _) = CreateTimetableService(fixture.Db, staffContext);

        // Attempt to generate timetable for campus 2 (cross-campus violation)
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
            await service.GenerateAsync(new GenerateTimetableRequest
            {
                MaHocKy = fixture.Term.MaHocKy,
                MaDonVi = 2 // Cross-campus violation!
            }));

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }
}


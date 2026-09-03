using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P27_CanonicalCapacityAndReadinessTests
{
    private ApplicationDbContext _db;
    private CourseCapacityService _capacityService;
    private AcademicSchedulingContextService _readinessService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _capacityService = new CourseCapacityService(_db);
        _readinessService = new AcademicSchedulingContextService(_db, null, _capacityService);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private DateOnly GetToday()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(
            Environment.OSVersion.Platform == PlatformID.Win32NT ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return DateOnly.FromDateTime(now);
    }

    // =========================================================================
    // 1. CANONICAL CAPACITY: 4-TIER PRECEDENCE & ZERO-FALLBACK PREVENTION
    // =========================================================================

    [Test]
    public async Task Capacity_Tier1_UsesValidEnrollmentsFirst()
    {
        const int campusId = 1;
        const int courseId = 100;
        const int sectionId = 200;
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        // Active student
        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 1, MaDonVi = campusId, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive });
        // Valid enrollment
        _db.DangKyHocPhans.Add(new DangKyHocPhan { MaDangKy = 1, MaLopHocPhan = sectionId, MaHocSinh = 1, TrangThai = "da_dang_ky" });

        var course = new KhoaHoc
        {
            MaKhoaHoc = courseId,
            MaDonVi = campusId,
            MaLopHocPhan = sectionId,
            MaLop = 50,
            TrangThai = "nhap"
        };
        await _db.SaveChangesAsync();

        var result = await _capacityService.GetRequiredCapacitiesAsync(new[] { course });

        Assert.That(result.ContainsKey(courseId), Is.True);
        var cap = result[courseId];
        Assert.That(cap.Value, Is.EqualTo(1));
        Assert.That(cap.Status, Is.EqualTo(RequiredCapacity.StatusReady));
        Assert.That(cap.Source, Is.EqualTo(RequiredCapacity.SourceRegistered));
        Assert.That(cap.IsKnown, Is.True);
    }

    [Test]
    public async Task Capacity_Tier2_FallbacksToAdministrativeClassStudents()
    {
        const int campusId = 1;
        const int courseId = 101;
        const int classId = 55;
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        // 3 active students in administrative class
        for (var i = 1; i <= 3; i++)
        {
            _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = i, MaLop = classId, MaDonVi = campusId, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive });
        }

        var course = new KhoaHoc
        {
            MaKhoaHoc = courseId,
            MaDonVi = campusId,
            MaLop = classId,
            TrangThai = "nhap"
        };
        await _db.SaveChangesAsync();

        var result = await _capacityService.GetRequiredCapacitiesAsync(new[] { course });

        Assert.That(result.ContainsKey(courseId), Is.True);
        var cap = result[courseId];
        Assert.That(cap.Value, Is.EqualTo(3));
        Assert.That(cap.Status, Is.EqualTo(RequiredCapacity.StatusReady));
        Assert.That(cap.Source, Is.EqualTo(RequiredCapacity.SourceClassStudents));
        Assert.That(cap.IsKnown, Is.True);
    }

    [Test]
    public async Task Capacity_Tier3_FallbacksToExpectedCount()
    {
        const int campusId = 1;
        const int courseId = 102;
        const int classId = 56;

        _db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = classId, MaDonVi = campusId, SiSoDuKien = 35 });

        var course = new KhoaHoc
        {
            MaKhoaHoc = courseId,
            MaDonVi = campusId,
            MaLop = classId,
            TrangThai = "nhap"
        };
        await _db.SaveChangesAsync();

        var result = await _capacityService.GetRequiredCapacitiesAsync(new[] { course });

        Assert.That(result.ContainsKey(courseId), Is.True);
        var cap = result[courseId];
        Assert.That(cap.Value, Is.EqualTo(35));
        Assert.That(cap.Status, Is.EqualTo(RequiredCapacity.StatusWarning));
        Assert.That(cap.Source, Is.EqualTo(RequiredCapacity.SourceExpected));
        Assert.That(cap.IsKnown, Is.True);
    }

    [Test]
    public async Task Capacity_Tier4_FailsWithMissingCodeWhenNoSources_NeverZeroFallback()
    {
        const int campusId = 1;
        const int courseId = 103;

        var course = new KhoaHoc
        {
            MaKhoaHoc = courseId,
            MaDonVi = campusId,
            MaLop = 999,
            TrangThai = "nhap"
        };
        await _db.SaveChangesAsync();

        var result = await _capacityService.GetRequiredCapacitiesAsync(new[] { course });

        Assert.That(result.ContainsKey(courseId), Is.True);
        var cap = result[courseId];
        Assert.That(cap.Value, Is.EqualTo(0));
        Assert.That(cap.Status, Is.EqualTo(RequiredCapacity.StatusBlocked));
        Assert.That(cap.Source, Is.EqualTo(RequiredCapacity.SourceMissing));
        Assert.That(cap.IsKnown, Is.False);
        Assert.That(cap.WarningCode, Is.EqualTo("STUDENT_CAPACITY_DATA_MISSING"));
    }

    // =========================================================================
    // 2. ROOM ELIGIBILITY POLICY
    // =========================================================================

    [Test]
    public void RoomEligibility_RejectsSmallOrInactiveRoom()
    {
        const int campusId = 1;
        var validCap = new RequiredCapacity(40, RequiredCapacity.StatusReady, RequiredCapacity.SourceClassStudents, true);
        var missingCap = new RequiredCapacity(0, RequiredCapacity.StatusBlocked, RequiredCapacity.SourceMissing, false);

        var roomGood = new PhongHoc { MaPhong = 1, MaDonVi = campusId, SucChua = 50, TrangThaiPhong = "hoat_dong" };
        var roomSmall = new PhongHoc { MaPhong = 2, MaDonVi = campusId, SucChua = 30, TrangThaiPhong = "hoat_dong" };
        var roomInactive = new PhongHoc { MaPhong = 3, MaDonVi = campusId, SucChua = 50, TrangThaiPhong = "bao_tri" };
        var roomDiffCampus = new PhongHoc { MaPhong = 4, MaDonVi = 2, SucChua = 50, TrangThaiPhong = "hoat_dong" };

        Assert.That(_capacityService.IsRoomEligible(roomGood, validCap, campusId), Is.True);
        Assert.That(_capacityService.IsRoomEligible(roomSmall, validCap, campusId), Is.False);
        Assert.That(_capacityService.IsRoomEligible(roomInactive, validCap, campusId), Is.False);
        Assert.That(_capacityService.IsRoomEligible(roomDiffCampus, validCap, campusId), Is.False);
        Assert.That(_capacityService.IsRoomEligible(roomGood, missingCap, campusId), Is.False);
    }

    // =========================================================================
    // 3. STRUCTURED READINESS: 11 ITEMS & ISOLATED NEGATIVE TESTS
    // =========================================================================

    [Test]
    public async Task Readiness_ReturnsAll11StructuredItems()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 10;

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        Assert.That(result.Readiness, Is.Not.Null);
        Assert.That(result.Readiness.Items, Is.Not.Null);
        Assert.That(result.Readiness.Items.Count, Is.EqualTo(11));

        var codes = result.Readiness.Items.Select(x => x.Code).ToList();
        Assert.That(codes, Does.Contain("COURSES_READY"));
        Assert.That(codes, Does.Contain("BLOCKS_READY"));
        Assert.That(codes, Does.Contain("CREDIT_MAPPING_READY"));
        Assert.That(codes, Does.Contain("TEACHER_SKILL_READY"));
        Assert.That(codes, Does.Contain("TEACHER_AVAILABILITY_READY"));
        Assert.That(codes, Does.Contain("TEACHER_CAPACITY_READY"));
        Assert.That(codes, Does.Contain("ACTIVE_ROOMS_READY"));
        Assert.That(codes, Does.Contain("ROOM_CAPACITY_READY"));
        Assert.That(codes, Does.Contain("ACTIVE_SHIFTS_READY"));
        Assert.That(codes, Does.Contain("TOTAL_ROOM_SLOTS_READY"));
        Assert.That(codes, Does.Contain("EXISTING_SCHEDULE_LOCK_READY"));
    }

    [Test]
    public async Task Readiness_BlocksWhenRoomCapacityInsufficient()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 11;
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, TenMonHoc = "Mon 1", SoTinChi = 3, ConHoatDong = true });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, MaLop = 10, TrangThai = "nhap" });
        // 50 students
        for (var i = 1; i <= 50; i++)
        {
            _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = i, MaLop = 10, MaDonVi = campusId, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive });
        }
        // Room capacity only 30
        _db.PhongHocs.Add(new PhongHoc { MaPhong = 1, MaDonVi = campusId, SucChua = 30, TrangThaiPhong = "hoat_dong" });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        Assert.That(result.SchedulableTerm, Is.Not.Null, $"Reason: {result.ReasonCode} - {result.ReasonMessage}");
        Assert.That(result.SchedulableTerm!.MaHocKy, Is.EqualTo(termId));

        var roomItem = result.Readiness.Items.FirstOrDefault(x => x.Code == "ROOM_CAPACITY_READY");
        Assert.That(roomItem, Is.Not.Null);
        Assert.That(roomItem!.Status, Is.EqualTo("blocked"), $"Room item message: {roomItem.Message}");
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task Readiness_BlocksWhenCreditMappingMissing()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 12;

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, SoTinChi = 5, ConHoatDong = true });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, TrangThai = "nhap" });
        // Only credit 2 and 3 mapped, credit 5 is missing
        _db.QuyDoiTinChis.Add(new QuyDoiTinChi { MaQuyDoi = 1, SoTinChi = 2, SoBuoiMoiTuan = 2, SoBlockHoc = 1, SoCaMoiBuoi = 1 });
        _db.QuyDoiTinChis.Add(new QuyDoiTinChi { MaQuyDoi = 2, SoTinChi = 3, SoBuoiMoiTuan = 3, SoBlockHoc = 1, SoCaMoiBuoi = 1 });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        var creditItem = result.Readiness.Items.FirstOrDefault(x => x.Code == "CREDIT_MAPPING_READY");
        Assert.That(creditItem, Is.Not.Null);
        Assert.That(creditItem!.Status, Is.EqualTo("blocked"));
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task Readiness_BlocksWhenTotalRoomSlotsInsufficient()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 13;

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, SoTinChi = 3, ConHoatDong = true });
        _db.QuyDoiTinChis.Add(new QuyDoiTinChi { MaQuyDoi = 1, SoTinChi = 3, SoBuoiMoiTuan = 3, SoBlockHoc = 1, SoCaMoiBuoi = 1 });

        // 10 courses, each requires 3 slots = 30 slots needed
        for (var i = 1; i <= 10; i++)
        {
            _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = i, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, TrangThai = "nhap" });
        }

        // Only 1 room and 1 shift = 1 * 1 * 6 = 6 available slots (30 > 6 -> Insufficient)
        _db.PhongHocs.Add(new PhongHoc { MaPhong = 1, MaDonVi = campusId, SucChua = 50, TrangThaiPhong = "hoat_dong" });
        _db.CaHocs.Add(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        var slotsItem = result.Readiness.Items.FirstOrDefault(x => x.Code == "TOTAL_ROOM_SLOTS_READY");
        Assert.That(slotsItem, Is.Not.Null);
        Assert.That(slotsItem!.Status, Is.EqualTo("blocked"));
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task Readiness_BlocksWhenTeacherSkillMissing()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 14;

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, TenMonHoc = "Mon 1", SoTinChi = 3, ConHoatDong = true });
        // Course has MaGiaoVien = 0 (unassigned)
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, MaGiaoVien = 0, TrangThai = "nhap" });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        var teacherSkillItem = result.Readiness.Items.FirstOrDefault(x => x.Code == "TEACHER_SKILL_READY");
        Assert.That(teacherSkillItem, Is.Not.Null);
        Assert.That(teacherSkillItem!.Status, Is.EqualTo("blocked"));
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task Readiness_BlocksWhenTeacherAvailabilityZero()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 15;
        const int teacherId = 99;
        var teacherRole = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);

        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, TenMonHoc = "Mon 1", SoTinChi = 3, ConHoatDong = true });
        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = teacherId, MaDonVi = campusId, VaiTroChinh = teacherRole, TrangThai = UserStatuses.DbActive });
        _db.GiaoVienMonHocs.Add(new GiaoVienMonHoc { MaGiaoVien = teacherId, MaMonHoc = 1, MucDoPhuHop = 90, ConHoatDong = true, PhuHopChuyenMon = true });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, MaGiaoVien = teacherId, TrangThai = "nhap" });

        // Teacher preference configured with 0 slots
        _db.GiaoVienNguyenVongHocKys.Add(new GiaoVienNguyenVongHocKy
        {
            MaGiaoVien = teacherId,
            MaHocKy = termId,
            MaDonVi = campusId,
            SoCaToiDaMoiTuan = 0
        });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);

        var availabilityItem = result.Readiness.Items.FirstOrDefault(x => x.Code == "TEACHER_AVAILABILITY_READY");
        Assert.That(availabilityItem, Is.Not.Null);
        Assert.That(availabilityItem!.Status, Is.EqualTo("blocked"));
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task Readiness_BlocksWhenTeacherCapacityIsInsufficient()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 16;
        const int teacherId = 99;
        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc { MaMonHoc = 1, SoTinChi = 3, ConHoatDong = true });
        _db.QuyDoiTinChis.Add(new QuyDoiTinChi { MaQuyDoi = 1, SoTinChi = 3, SoBuoiMoiTuan = 3, SoBlockHoc = 1, SoCaMoiBuoi = 1 });
        for (var id = 1; id <= 3; id++)
            _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = id, MaHocKy = termId, MaDonVi = campusId, MaMonHoc = 1, MaGiaoVien = teacherId, TrangThai = "nhap" });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);
        var item = result.Readiness.Items.Single(x => x.Code == "TEACHER_CAPACITY_READY");

        Assert.That(item.Status, Is.EqualTo("blocked"));
        Assert.That(item.AffectedItems, Is.Not.Empty);
    }

    [Test]
    public async Task Readiness_BlocksWhenNoShiftIsActive()
    {
        var today = GetToday();
        const int campusId = 1;
        const int termId = 17;
        _db.HocKys.Add(new HocKy { MaHocKy = termId, MaDonVi = campusId, NgayBatDau = today.AddDays(10), NgayKetThuc = today.AddDays(80) });
        _db.CaHocs.Add(new CaHoc { MaCaHoc = 1, TenCa = "Ca inactive", ConHoatDong = false });
        await _db.SaveChangesAsync();

        var result = await _readinessService.GetContextAsync(campusId);
        var item = result.Readiness.Items.Single(x => x.Code == "ACTIVE_SHIFTS_READY");

        Assert.That(item.Status, Is.EqualTo("blocked"));
        Assert.That(result.CanPrepareSchedule, Is.False);
    }
}

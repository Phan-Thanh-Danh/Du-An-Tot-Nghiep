using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P29_CanonicalCapacityConsumerTests
{
    private ApplicationDbContext _db = null!;
    private IHttpContextAccessor _http = null!;
    private CourseCapacityService _capacity = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _capacity = new CourseCapacityService(_db);
        var context = new DefaultHttpContext();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 900,
            Email = "staff@lms.test",
            Role = AuthRoles.AcademicStaff,
            CampusId = 1
        };
        _http = new HttpContextAccessor { HttpContext = context };
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void SqlSafetyGuard_RequiresAndVerifiesTheConfiguredDisposableDatabase()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING")))
            Assert.Ignore("SQL integration database is not configured for this process.");

        Assert.That(TestDatabaseSafetyGuard.GetVerifiedTestConnectionString(),
            Does.Contain("LMS_TEST_"));
    }

    [Test]
    public async Task Sql_CapacityPolicy_UsesOnlyCanonicalCampusScopedEnrollmentAndNeverMakesUnknownZero()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING")))
            Assert.Ignore("SQL integration database is not configured for this process.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(TestDatabaseSafetyGuard.GetVerifiedTestConnectionString())
            .Options;
        await using var sqlDb = new ApplicationDbContext(options);
        await using var transaction = await sqlDb.Database.BeginTransactionAsync();

        try
        {
            var suffix = Guid.NewGuid().ToString("N")[..12];
            var campus = new DonVi { TenDonVi = $"R0 campus {suffix}", CapDonVi = "co_so", ConHoatDong = true };
            var foreignCampus = new DonVi { TenDonVi = $"R0 foreign campus {suffix}", CapDonVi = "co_so", ConHoatDong = true };
            var subject = new DanhMucMonHoc { MaCodeMonHoc = $"R0{suffix}", TenMonHoc = "R0 SQL capacity", SoTinChi = 1, ConHoatDong = true };
            sqlDb.AddRange(campus, foreignCampus, subject);
            await sqlDb.SaveChangesAsync();

            var term = new HocKy
            {
                MaDonVi = campus.MaDonVi, MaCodeHocKy = $"R0-{suffix}", TenHocKy = "R0 SQL term",
                NamHoc = "2026-2027", ThuTuTrongNam = 1,
                NgayBatDau = new DateOnly(2027, 1, 1), NgayKetThuc = new DateOnly(2027, 4, 30)
            };
            var registeredClass = new LopHanhChinh { MaDonVi = campus.MaDonVi, MaCodeLop = $"R0R{suffix}", TenLop = "Registered", ConHoatDong = true, SiSoDuKien = 99 };
            var activeClass = new LopHanhChinh { MaDonVi = campus.MaDonVi, MaCodeLop = $"R0A{suffix}", TenLop = "Active", ConHoatDong = true, SiSoDuKien = 99 };
            var expectedClass = new LopHanhChinh { MaDonVi = campus.MaDonVi, MaCodeLop = $"R0E{suffix}", TenLop = "Expected", ConHoatDong = true, SiSoDuKien = 22 };
            var missingClass = new LopHanhChinh { MaDonVi = campus.MaDonVi, MaCodeLop = $"R0M{suffix}", TenLop = "Missing", ConHoatDong = true };
            var teacher = new NguoiDung { MaDonVi = campus.MaDonVi, Email = $"r0.teacher.{suffix}@test.local", HoTen = "R0 Teacher", VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher), TrangThai = UserStatuses.DbActive };
            sqlDb.AddRange(term, registeredClass, activeClass, expectedClass, missingClass, teacher);
            await sqlDb.SaveChangesAsync();

            var section = new LopHocPhan { MaDonVi = campus.MaDonVi, MaMonHoc = subject.MaMonHoc, MaHocKy = term.MaHocKy, MaCodeLopHocPhan = $"R0S{suffix}", SucChua = 99, TrangThai = "mo" };
            sqlDb.LopHocPhans.Add(section);
            await sqlDb.SaveChangesAsync();

            var registeredCourse = new KhoaHoc { MaDonVi = campus.MaDonVi, MaGiaoVien = teacher.MaNguoiDung, MaMonHoc = subject.MaMonHoc, MaHocKy = term.MaHocKy, MaLop = registeredClass.MaLop, MaLopHocPhan = section.MaLopHocPhan, TieuDe = "Registered", TrangThai = "nhap" };
            var activeCourse = new KhoaHoc { MaDonVi = campus.MaDonVi, MaGiaoVien = teacher.MaNguoiDung, MaMonHoc = subject.MaMonHoc, MaHocKy = term.MaHocKy, MaLop = activeClass.MaLop, TieuDe = "Active", TrangThai = "nhap" };
            var expectedCourse = new KhoaHoc { MaDonVi = campus.MaDonVi, MaGiaoVien = teacher.MaNguoiDung, MaMonHoc = subject.MaMonHoc, MaHocKy = term.MaHocKy, MaLop = expectedClass.MaLop, TieuDe = "Expected", TrangThai = "nhap" };
            var missingCourse = new KhoaHoc { MaDonVi = campus.MaDonVi, MaGiaoVien = teacher.MaNguoiDung, MaMonHoc = subject.MaMonHoc, MaHocKy = term.MaHocKy, MaLop = missingClass.MaLop, TieuDe = "Missing", TrangThai = "nhap" };
            sqlDb.KhoaHocs.AddRange(registeredCourse, activeCourse, expectedCourse, missingCourse);
            await sqlDb.SaveChangesAsync();

            var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
            var eligible = new NguoiDung { MaDonVi = campus.MaDonVi, Email = $"r0.eligible.{suffix}@test.local", HoTen = "Eligible", VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive };
            var foreign = new NguoiDung { MaDonVi = foreignCampus.MaDonVi, Email = $"r0.foreign.{suffix}@test.local", HoTen = "Foreign", VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive };
            var inactive = new NguoiDung { MaDonVi = campus.MaDonVi, Email = $"r0.inactive.{suffix}@test.local", HoTen = "Inactive", VaiTroChinh = studentRole, TrangThai = "bi_khoa" };
            var nonStudent = new NguoiDung { MaDonVi = campus.MaDonVi, Email = $"r0.nonstudent.{suffix}@test.local", HoTen = "Non student", VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher), TrangThai = UserStatuses.DbActive };
            var otherStatus = new NguoiDung { MaDonVi = campus.MaDonVi, Email = $"r0.otherstatus.{suffix}@test.local", HoTen = "Other status", VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive };
            var activeClassStudent1 = new NguoiDung { MaDonVi = campus.MaDonVi, MaLop = activeClass.MaLop, Email = $"r0.class1.{suffix}@test.local", HoTen = "Class 1", VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive };
            var activeClassStudent2 = new NguoiDung { MaDonVi = campus.MaDonVi, MaLop = activeClass.MaLop, Email = $"r0.class2.{suffix}@test.local", HoTen = "Class 2", VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive };
            sqlDb.NguoiDungs.AddRange(eligible, foreign, inactive, nonStudent, otherStatus, activeClassStudent1, activeClassStudent2);
            await sqlDb.SaveChangesAsync();

            sqlDb.DangKyHocPhans.AddRange(
                new DangKyHocPhan { MaHocSinh = eligible.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_dang_ky" },
                new DangKyHocPhan { MaHocSinh = foreign.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_dang_ky" },
                new DangKyHocPhan { MaHocSinh = inactive.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_dang_ky" },
                new DangKyHocPhan { MaHocSinh = nonStudent.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_dang_ky" });
            await sqlDb.SaveChangesAsync();

            var nonRegisteredStatus = new DangKyHocPhan { MaHocSinh = otherStatus.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_duyet" };
            sqlDb.DangKyHocPhans.Add(nonRegisteredStatus);
            Assert.That(Assert.ThrowsAsync<DbUpdateException>(() => sqlDb.SaveChangesAsync())!.InnerException?.Message,
                Does.Contain("CK_DangKyHocPhan_trang_thai"), "The real SQL check rejects non-registered enrollment statuses.");
            sqlDb.Entry(nonRegisteredStatus).State = EntityState.Detached;

            var duplicate = new DangKyHocPhan { MaHocSinh = eligible.MaNguoiDung, MaLopHocPhan = section.MaLopHocPhan, TrangThai = "da_dang_ky" };
            sqlDb.DangKyHocPhans.Add(duplicate);
            Assert.That(Assert.ThrowsAsync<DbUpdateException>(() => sqlDb.SaveChangesAsync())!.InnerException?.Message,
                Does.Contain("UQ_DangKyHocPhan"), "The real SQL unique key prevents duplicate enrollment rows.");
            sqlDb.Entry(duplicate).State = EntityState.Detached;

            var capacity = await new CourseCapacityService(sqlDb).GetRequiredCapacitiesAsync(new[] { registeredCourse, activeCourse, expectedCourse, missingCourse });

            Assert.Multiple(() =>
            {
                Assert.That(capacity[registeredCourse.MaKhoaHoc], Is.EqualTo(new RequiredCapacity(1, RequiredCapacity.StatusReady, RequiredCapacity.SourceRegistered)));
                Assert.That(capacity[activeCourse.MaKhoaHoc], Is.EqualTo(new RequiredCapacity(2, RequiredCapacity.StatusReady, RequiredCapacity.SourceClassStudents)));
                Assert.That(capacity[expectedCourse.MaKhoaHoc], Is.EqualTo(new RequiredCapacity(22, RequiredCapacity.StatusWarning, RequiredCapacity.SourceExpected)));
                Assert.That(capacity[missingCourse.MaKhoaHoc].Value, Is.Zero);
                Assert.That(capacity[missingCourse.MaKhoaHoc].IsKnown, Is.False);
                Assert.That(capacity[missingCourse.MaKhoaHoc].Source, Is.EqualTo(RequiredCapacity.SourceMissing));
                Assert.That(capacity[missingCourse.MaKhoaHoc].WarningCode, Is.EqualTo("STUDENT_CAPACITY_DATA_MISSING"));
            });
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    [Test]
    public async Task Capacity_OnlyCountsDistinctActiveStudentsFromCourseCampus()
    {
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        var course = new KhoaHoc { MaKhoaHoc = 1, MaDonVi = 1, MaLop = 10, MaLopHocPhan = 20, TrangThai = "nhap" };
        _db.KhoaHocs.Add(course);
        _db.NguoiDungs.AddRange(
            new NguoiDung { MaNguoiDung = 1, MaDonVi = 1, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive },
            new NguoiDung { MaNguoiDung = 2, MaDonVi = 2, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive },
            new NguoiDung { MaNguoiDung = 3, MaDonVi = 1, VaiTroChinh = studentRole, TrangThai = "bi_khoa" });
        _db.DangKyHocPhans.AddRange(
            new DangKyHocPhan { MaDangKy = 1, MaHocSinh = 1, MaLopHocPhan = 20, TrangThai = "da_dang_ky" },
            new DangKyHocPhan { MaDangKy = 2, MaHocSinh = 1, MaLopHocPhan = 20, TrangThai = "da_dang_ky" },
            new DangKyHocPhan { MaDangKy = 3, MaHocSinh = 2, MaLopHocPhan = 20, TrangThai = "da_dang_ky" },
            new DangKyHocPhan { MaDangKy = 4, MaHocSinh = 3, MaLopHocPhan = 20, TrangThai = "da_dang_ky" });
        await _db.SaveChangesAsync();

        var result = await _capacity.GetRequiredCapacitiesAsync(new[] { course });

        Assert.That(result[course.MaKhoaHoc].Value, Is.EqualTo(1));
        Assert.That(result[course.MaKhoaHoc].Source, Is.EqualTo(RequiredCapacity.SourceRegistered));
    }

    [Test]
    public async Task Create_RejectsUndersizedRoomUsingCanonicalCapacity()
    {
        await SeedScheduleDataAsync(roomCapacity: 10);
        var service = CreateScheduleService();

        var ex = Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(CreateRequest()));

        Assert.That(ex!.Message, Does.Contain("không đủ sức chứa"));
        Assert.That(await _db.ThoiKhoaBieus.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Update_RejectsUndersizedRoomUsingCanonicalCapacity()
    {
        await SeedScheduleDataAsync(roomCapacity: 10, includeSchedule: true);
        var service = CreateScheduleService();

        var ex = Assert.ThrowsAsync<ApiException>(() => service.UpdateAsync(1, UpdateRequest()));

        Assert.That(ex!.Message, Does.Contain("không đủ sức chứa"));
    }

    [Test]
    public async Task Create_RejectsInactiveRoomAndInactiveShift()
    {
        await SeedScheduleDataAsync(roomCapacity: 40, roomActive: false);
        var service = CreateScheduleService();
        Assert.That(Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(CreateRequest()))!.Message,
            Does.Contain("không hoạt động"));

        _db.PhongHocs.Single().TrangThaiPhong = "hoat_dong";
        _db.CaHocs.Single().ConHoatDong = false;
        await _db.SaveChangesAsync();
        Assert.That(Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(CreateRequest()))!.Message,
            Does.Contain("ca học không tồn tại hoặc không hoạt động").IgnoreCase);
    }

    [Test]
    public async Task LegacyConflictEndpoint_RejectsUndersizedRoomUsingCanonicalCapacity()
    {
        await SeedScheduleDataAsync(roomCapacity: 10);
        var service = new ScheduleConflictService(_db, _http, _capacity);

        var ex = Assert.ThrowsAsync<ApiException>(() => service.CheckConflictsAsync(new CheckScheduleConflictRequest
        {
            MaKhoaHoc = 1, ThuTrongTuan = 2, MaCaHoc = 1, MaPhong = 1
        }));

        Assert.That(ex!.Message, Does.Contain("không đủ sức chứa"));
    }

    [Test]
    public void PublishBlockDates_UseConfiguredStartBlockAndDuration()
    {
        var term = new HocKy { MaHocKy = 1, NgayBatDau = new DateOnly(2027, 1, 1), NgayKetThuc = new DateOnly(2027, 3, 31) };
        var blocks = new List<Block>
        {
            new() { MaBlock = 10, MaHocKy = 1, ThuTuBlock = 1, NgayBatDau = new DateOnly(2027, 1, 1), NgayKetThuc = new DateOnly(2027, 1, 14) },
            new() { MaBlock = 11, MaHocKy = 1, ThuTuBlock = 2, NgayBatDau = new DateOnly(2027, 1, 15), NgayKetThuc = new DateOnly(2027, 1, 28) },
            new() { MaBlock = 12, MaHocKy = 1, ThuTuBlock = 3, NgayBatDau = new DateOnly(2027, 1, 29), NgayKetThuc = new DateOnly(2027, 2, 11) }
        };
        var course = new KhoaHoc { MaKhoaHoc = 1, MaBlockBatDau = 11, SoBlockHoc = 2 };

        SmartTimetableService.ValidateCourseBlockRanges(new[] { course }, blocks, term);
        var dates = SmartTimetableService.ResolveCourseScheduleDates(course, blocks, term);

        Assert.That(dates.Start, Is.EqualTo(new DateOnly(2027, 1, 15)));
        Assert.That(dates.End, Is.EqualTo(new DateOnly(2027, 2, 11)));
    }

    [Test]
    public void PublishBlockDates_RejectOutOfRangeOrInvalidBlockConfiguration()
    {
        var term = new HocKy { MaHocKy = 1, NgayBatDau = new DateOnly(2027, 1, 1), NgayKetThuc = new DateOnly(2027, 1, 31) };
        var blocks = new List<Block>
        {
            new() { MaBlock = 10, MaHocKy = 1, ThuTuBlock = 1, NgayBatDau = new DateOnly(2027, 1, 1), NgayKetThuc = new DateOnly(2027, 1, 14) }
        };
        var course = new KhoaHoc { MaKhoaHoc = 1, MaBlockBatDau = 10, SoBlockHoc = 2 };

        var ex = Assert.Throws<ApiException>(() => SmartTimetableService.ValidateCourseBlockRanges(new[] { course }, blocks, term));

        Assert.That(ex!.Message, Does.Contain("phạm vi Block"));
    }

    private async Task SeedScheduleDataAsync(int roomCapacity, bool roomActive = true, bool includeSchedule = false)
    {
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        _db.HocKys.Add(new HocKy { MaHocKy = 1, MaDonVi = 1, NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)) });
        _db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = 10, MaDonVi = 1, MaCodeLop = "L10", TenLop = "Lớp 10", SiSoDuKien = 30, ConHoatDong = true });
        _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 100, MaDonVi = 1, VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher), TrangThai = UserStatuses.DbActive, HoTen = "GV" });
        _db.KhoaHocs.Add(new KhoaHoc { MaKhoaHoc = 1, MaDonVi = 1, MaHocKy = 1, MaLop = 10, MaGiaoVien = 100, TrangThai = "nhap" });
        for (var id = 1; id <= 30; id++)
            _db.NguoiDungs.Add(new NguoiDung { MaNguoiDung = id, MaDonVi = 1, MaLop = 10, VaiTroChinh = studentRole, TrangThai = UserStatuses.DbActive });
        _db.PhongHocs.Add(new PhongHoc { MaPhong = 1, MaDonVi = 1, TenPhong = "P1", SucChua = roomCapacity, TrangThaiPhong = roomActive ? "hoat_dong" : "bao_tri" });
        _db.CaHocs.Add(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true });
        if (includeSchedule)
            _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 1, MaPhong = 1, MaCaHoc = 1, ThuTrongTuan = 2, TrangThai = "nhap" });
        await _db.SaveChangesAsync();
    }

    private ThoiKhoaBieuService CreateScheduleService()
    {
        var context = new Mock<IAcademicSchedulingContextService>();
        context.Setup(x => x.ValidateSchedulableTermAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var conflict = new ScheduleConflictService(_db, _http, _capacity);
        return new ThoiKhoaBieuService(_db, _http, new Mock<IAuditLogService>().Object, conflict, context.Object, _capacity);
    }

    private static CreateThoiKhoaBieuRequest CreateRequest() => new()
    {
        MaKhoaHoc = 1, ThuTrongTuan = 2, MaCaHoc = 1, MaPhong = 1, TrangThai = "nhap"
    };

    private static UpdateThoiKhoaBieuRequest UpdateRequest() => new()
    {
        MaKhoaHoc = 1, ThuTrongTuan = 2, MaCaHoc = 1, MaPhong = 1, TrangThai = "nhap"
    };
}

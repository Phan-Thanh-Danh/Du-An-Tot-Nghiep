using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AcademicTerms;
using Backend.DTOs.Auth;
using Backend.DTOs.Blocks;
using Backend.DTOs.Courses;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Backend.DTOs.Rooms;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Middlewares;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.AcademicTerms;
using Backend.Services.Audit;
using Backend.Services.Blocks;
using Backend.Services.Courses;
using Backend.Services.LopHanhChinhs;
using Backend.Services.Notifications;
using Backend.Services.Rooms;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class Task7D_R1_CampusIsolationSecurityTests
{
    private ApplicationDbContext _db = null!;
    private HttpContextAccessor _httpContextAccessor = null!;

    private const int CampusA = 1; // HCM (Authorized)
    private const int CampusB = 2; // Dong Nai (Foreign)

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _httpContextAccessor = new HttpContextAccessor();

        // Default authenticated user: AcademicStaff at Campus A (NO SuperAdmin)
        SetAuthenticatedStaff(userId: 101, campusId: CampusA);

        SeedTwoCampuses();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private void SetAuthenticatedStaff(int userId, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, AuthRoles.AcademicStaff),
            new Claim("campusId", campusId.ToString())
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = userId,
            Role = AuthRoles.AcademicStaff,
            CampusId = campusId,
            Email = $"staff_{userId}@lms.local",
            Status = UserStatuses.DbActive
        };
        _httpContextAccessor.HttpContext = httpContext;
    }

    private void SeedTwoCampuses()
    {
        // Two Campuses
        _db.DonVis.Add(new DonVi { MaDonVi = CampusA, TenDonVi = "Cơ sở TP.HCM", ConHoatDong = true });
        _db.DonVis.Add(new DonVi { MaDonVi = CampusB, TenDonVi = "Cơ sở Đồng Nai", ConHoatDong = true });

        // Major & Specialization
        _db.NganhDaoTaos.Add(new NganhDaoTao { MaNganh = 1, TenNganh = "Công nghệ thông tin", ConHoatDong = true });
        _db.ChuyenNganhs.Add(new ChuyenNganh { MaChuyenNganh = 1, MaNganh = 1, TenChuyenNganh = "Lập trình Web", ConHoatDong = true });

        // Training Programs
        _db.ChuongTrinhDaoTaos.Add(new ChuongTrinhDaoTao { MaChuongTrinh = 1, MaChuyenNganh = 1, TenChuongTrinh = "Kỹ thuật phần mềm", ConHoatDong = true });

        // Subject
        _db.DanhMucMonHocs.Add(new DanhMucMonHoc
        {
            MaMonHoc = 1,
            MaCodeMonHoc = "WEB101",
            TenMonHoc = "Lập trình Web Cơ bản",
            SoTinChi = 3,
            MaNganh = 1,
            MaChuyenNganh = 1,
            ConHoatDong = true
        });
        _db.MonHocChuyenNganhs.Add(new MonHocChuyenNganh { MaMonHoc = 1, MaChuyenNganh = 1 });

        // Terms
        _db.HocKys.Add(new HocKy
        {
            MaHocKy = 10,
            MaDonVi = CampusA,
            TenHocKy = "HK1 2027 HCM",
            MaCodeHocKy = "HK1_2027_HCM",
            NgayBatDau = new DateOnly(2027, 9, 1),
            NgayKetThuc = new DateOnly(2027, 12, 31),
            DaKhoa = false
        });
        _db.HocKys.Add(new HocKy
        {
            MaHocKy = 12,
            MaDonVi = CampusA,
            TenHocKy = "HK2 2027 HCM",
            MaCodeHocKy = "HK2_2027_HCM",
            NgayBatDau = new DateOnly(2028, 1, 1),
            NgayKetThuc = new DateOnly(2028, 5, 31),
            DaKhoa = false
        });
        _db.HocKys.Add(new HocKy
        {
            MaHocKy = 20,
            MaDonVi = CampusB,
            TenHocKy = "HK1 2027 Dong Nai",
            MaCodeHocKy = "HK1_2027_DN",
            NgayBatDau = new DateOnly(2027, 9, 1),
            NgayKetThuc = new DateOnly(2027, 12, 31),
            DaKhoa = false
        });

        // Blocks
        _db.Blocks.Add(new Block { MaBlock = 101, MaHocKy = 10, TenBlock = "Block 1 HCM", ThuTuBlock = 1, NgayBatDau = new DateOnly(2027, 9, 1), NgayKetThuc = new DateOnly(2027, 10, 31) });
        _db.Blocks.Add(new Block { MaBlock = 102, MaHocKy = 12, TenBlock = "Block 1 HK2 HCM", ThuTuBlock = 1, NgayBatDau = new DateOnly(2028, 1, 1), NgayKetThuc = new DateOnly(2028, 2, 28) });
        _db.Blocks.Add(new Block { MaBlock = 201, MaHocKy = 20, TenBlock = "Block 1 DN", ThuTuBlock = 1, NgayBatDau = new DateOnly(2027, 9, 1), NgayKetThuc = new DateOnly(2027, 10, 31) });

        // Credit mappings
        _db.QuyDoiTinChis.Add(new QuyDoiTinChi { MaQuyDoi = 1, SoTinChi = 3, SoBlockHoc = 1, SoBuoiMoiTuan = 2, SoCaMoiBuoi = 1 });

        // Shifts
        _db.CaHocs.Add(new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", GioBatDau = new TimeOnly(7, 30), GioKetThuc = new TimeOnly(9, 30), ThuTu = 1, ConHoatDong = true });

        // Classes
        _db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = 11, MaDonVi = CampusA, MaChuongTrinh = 1, MaCodeLop = "WD1901", TenLop = "Lớp WD1901 HCM", ConHoatDong = true });
        _db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = 12, MaDonVi = CampusA, MaChuongTrinh = 1, MaCodeLop = "WD1903", TenLop = "Lớp WD1903 HCM", ConHoatDong = true });
        _db.LopHanhChinhs.Add(new LopHanhChinh { MaLop = 21, MaDonVi = CampusB, MaChuongTrinh = 1, MaCodeLop = "WD1902", TenLop = "Lớp WD1902 DN", ConHoatDong = true });

        // Staff User
        _db.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 101,
            MaDonVi = CampusA,
            HoTen = "Giáo vụ HCM",
            Email = "staff_101@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            TrangThai = UserStatuses.DbActive
        });

        // Teachers
        _db.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 301,
            MaDonVi = CampusA,
            HoTen = "Giảng viên HCM",
            Email = "gv_hcm@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            TrangThai = UserStatuses.DbActive
        });
        _db.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh { MaGiaoVien = 301, MaChuyenNganh = 1 });
        _db.GiaoVienMonHocs.Add(new GiaoVienMonHoc { MaGiaoVien = 301, MaMonHoc = 1, ConHoatDong = true });

        _db.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 303,
            MaDonVi = CampusA,
            HoTen = "Giảng viên Bị Khóa HCM",
            Email = "gv_locked_hcm@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            TrangThai = UserStatuses.DbLocked
        });
        _db.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh { MaGiaoVien = 303, MaChuyenNganh = 1 });
        _db.GiaoVienMonHocs.Add(new GiaoVienMonHoc { MaGiaoVien = 303, MaMonHoc = 1, ConHoatDong = true });

        _db.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 302,
            MaDonVi = CampusB,
            HoTen = "Giảng viên Đồng Nai",
            Email = "gv_dn@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Teacher),
            TrangThai = UserStatuses.DbActive
        });
        _db.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh { MaGiaoVien = 302, MaChuyenNganh = 1 });
        _db.GiaoVienMonHocs.Add(new GiaoVienMonHoc { MaGiaoVien = 302, MaMonHoc = 1, ConHoatDong = true });

        // Rooms & Buildings
        _db.ToaNhas.Add(new ToaNha { MaToaNha = 1, MaDonVi = CampusA, MaCodeToaNha = "T1_HCM", TenToaNha = "Tòa A HCM", ConHoatDong = true });
        _db.ToaNhas.Add(new ToaNha { MaToaNha = 2, MaDonVi = CampusB, MaCodeToaNha = "T1_DN", TenToaNha = "Tòa B DN", ConHoatDong = true });

        _db.Tangs.Add(new Tang { MaTang = 1, MaToaNha = 1, TenTang = "Tầng 1 HCM", ThuTuTang = 1 });
        _db.Tangs.Add(new Tang { MaTang = 2, MaToaNha = 2, TenTang = "Tầng 1 DN", ThuTuTang = 1 });

        _db.PhongHocs.Add(new PhongHoc { MaPhong = 101, MaDonVi = CampusA, MaToaNha = 1, MaTang = 1, MaCodePhong = "P101", TenPhong = "Phòng 101 HCM", TrangThaiPhong = "hoat_dong", SucChua = 40, LoaiPhong = "ly_thuyet" });
        _db.PhongHocs.Add(new PhongHoc { MaPhong = 201, MaDonVi = CampusB, MaToaNha = 2, MaTang = 2, MaCodePhong = "P201", TenPhong = "Phòng 201 DN", TrangThaiPhong = "hoat_dong", SucChua = 40, LoaiPhong = "ly_thuyet" });

        // Existing Courses
        _db.KhoaHocs.Add(new KhoaHoc
        {
            MaKhoaHoc = 1001,
            MaDonVi = CampusA,
            MaMonHoc = 1,
            MaGiaoVien = 301,
            MaHocKy = 10,
            MaLop = 11,
            TieuDe = "Khóa học Web HCM",
            TrangThai = "nhap",
            NgayTao = DateTime.UtcNow
        });
        _db.KhoaHocs.Add(new KhoaHoc
        {
            MaKhoaHoc = 2001,
            MaDonVi = CampusB,
            MaMonHoc = 1,
            MaGiaoVien = 302,
            MaHocKy = 20,
            MaLop = 21,
            TieuDe = "Khóa học Web DN",
            TrangThai = "nhap",
            NgayTao = DateTime.UtcNow
        });

        // Existing Published Schedule in Campus B
        _db.ThoiKhoaBieus.Add(new ThoiKhoaBieu
        {
            MaTkb = 2001,
            MaKhoaHoc = 2001,
            MaPhong = 201,
            MaCaHoc = 1,
            ThuTrongTuan = 2,
            TrangThai = "da_xuat_ban",
            NgayTao = DateTime.UtcNow
        });

        _db.SaveChanges();
    }

    private CourseService CreateCourseService()
    {
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var eligibilityService = new CourseTeacherEligibilityService(_db, scoringOptions);
        var contextService = new AcademicSchedulingContextService(_db);
        return new CourseService(_db, _httpContextAccessor, new Mock<IAuditLogService>().Object, eligibilityService, contextService);
    }

    private SmartTimetableService CreateSmartTimetableService()
    {
        var contextService = new AcademicSchedulingContextService(_db);
        var scoringOptions = Options.Create(new SmartTimetableScoringOptions());
        var scoringService = new ScheduleCandidateScoringService(scoringOptions);
        var solver = new GeneticTimetableSolver(scoringService, scoringOptions);
        var capacityService = new CourseCapacityService(_db);

        return new SmartTimetableService(
            _db,
            _httpContextAccessor,
            new Mock<IAuditLogService>().Object,
            NullLogger<SmartTimetableService>.Instance,
            contextService,
            scoringService,
            solver,
            new Mock<IScheduleNotificationService>().Object,
            scoringOptions,
            capacityService
        );
    }

    private BlockService CreateBlockService()
    {
        return new BlockService(_db, _httpContextAccessor);
    }

    private CourseAssignmentSuggestionService CreateSuggestionService()
    {
        var mockWorkload = new Mock<ITeacherAcademicWorkloadService>();
        mockWorkload.Setup(w => w.GetWorkloadsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, TeacherWorkloadDto>());

        var mockPref = new Mock<ITeachingPreferenceCoverageService>();
        mockPref.Setup(p => p.EvaluateCoveragesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<PlannedTeachingSlotDto>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, PreferenceCoverageDto>());

        return new CourseAssignmentSuggestionService(_db, mockWorkload.Object, mockPref.Object);
    }

    private ThoiKhoaBieuService CreateThoiKhoaBieuService()
    {
        var contextService = new AcademicSchedulingContextService(_db);
        var capacityService = new CourseCapacityService(_db);
        var conflictService = new ScheduleConflictService(_db, _httpContextAccessor, capacityService);

        return new ThoiKhoaBieuService(
            _db,
            _httpContextAccessor,
            new Mock<IAuditLogService>().Object,
            conflictService,
            contextService,
            capacityService
        );
    }

    // =========================================================================
    // 1. HTTP PIPELINE MIDDLEWARE & INTEGRATION TESTS (TestTier: HTTP Middleware & Integration Pipeline)
    // =========================================================================

    [Test]
    public async Task HttpPipeline_QueryCampusB_BlockedByCampusScopeMiddleware_ZeroMutation()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 101,
            Role = AuthRoles.AcademicStaff,
            CampusId = CampusA,
            Email = "staff@lms.local"
        };
        context.Request.QueryString = new QueryString("?maDonVi=2");

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CampusScopeMiddleware(next);
        await middleware.InvokeAsync(context, _db);

        Assert.That(nextCalled, Is.False, "Pipeline must halt and not call next delegate");
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(responseBody, Does.Contain("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task HttpPipeline_HeaderCampusB_BlockedByCampusScopeMiddleware_ZeroMutation()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 101,
            Role = AuthRoles.AcademicStaff,
            CampusId = CampusA,
            Email = "staff@lms.local"
        };
        context.Request.Headers["X-Campus-Id"] = "2";

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CampusScopeMiddleware(next);
        await middleware.InvokeAsync(context, _db);

        Assert.That(nextCalled, Is.False, "Pipeline must halt and not call next delegate");
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(responseBody, Does.Contain("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task HttpPipeline_RouteCampusB_BlockedByCampusScopeMiddleware_ZeroMutation()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 101,
            Role = AuthRoles.AcademicStaff,
            CampusId = CampusA,
            Email = "staff@lms.local"
        };
        context.Request.RouteValues["campusId"] = "2";

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CampusScopeMiddleware(next);
        await middleware.InvokeAsync(context, _db);

        Assert.That(nextCalled, Is.False, "Pipeline must halt and not call next delegate");
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(responseBody, Does.Contain("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task HttpPipeline_CampusA_AllowedByCampusScopeMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 101,
            Role = AuthRoles.AcademicStaff,
            CampusId = CampusA,
            Email = "staff@lms.local"
        };
        context.Request.QueryString = new QueryString("?maDonVi=1");

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CampusScopeMiddleware(next);
        await middleware.InvokeAsync(context, _db);

        Assert.That(nextCalled, Is.True, "Authorized campus must pass to next delegate");
    }

    [Test]
    public async Task HttpPipeline_RealApplicationBuilder_SimulatedJwtToTerminalController_BlockedWith403ForbiddenCampus_ZeroControllerExecution()
    {
        // Prove end-to-end pipeline: JWT Auth -> CurrentUserContext -> CampusScopeMiddleware -> Terminal Controller (never executed)
        var services = new ServiceCollection();
        services.AddSingleton(_db);
        var sp = services.BuildServiceProvider();

        var app = new ApplicationBuilder(sp);
        var controllerExecuted = false;

        // 1. Simulated JWT Middleware: populates CurrentUserContext for AcademicStaff Campus A
        app.Use(new Func<HttpContext, RequestDelegate, Task>(async (ctx, next) =>
        {
            ctx.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = 101,
                Role = AuthRoles.AcademicStaff,
                CampusId = CampusA,
                Email = "staff_hcm@lms.local"
            };
            await next(ctx);
        }));

        // 2. CampusScopeMiddleware
        app.UseMiddleware<CampusScopeMiddleware>();

        // 3. Terminal Endpoint / Controller
        app.Use(new Func<HttpContext, RequestDelegate, Task>(async (ctx, next) =>
        {
            controllerExecuted = true;
            await ctx.Response.WriteAsync("Controller executed successfully");
        }));

        var pipeline = app.Build();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = sp;
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Path = "/api/thoi-khoa-bieu";
        httpContext.Request.QueryString = new QueryString("?maDonVi=2"); // Trying to access Campus B

        await pipeline(httpContext);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(controllerExecuted, Is.False, "Terminal controller must NOT execute when foreign campus is targeted");

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        Assert.That(body, Does.Contain("FORBIDDEN_CAMPUS"));
    }

    // =========================================================================
    // 2. READ ISOLATION TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task ReadIsolation_AcademicTermService_ReturnsOnlyStaffCampusTerms()
    {
        var service = new AcademicTermService(_db, _httpContextAccessor);
        var result = await service.GetTermsAsync(new AcademicTermQueryParameters { PageIndex = 1, PageSize = 20 });

        Assert.That(result.Items.All(t => t.MaDonVi == CampusA), Is.True);
        Assert.That(result.Items.Any(t => t.MaHocKy == 10), Is.True);
        Assert.That(result.Items.Any(t => t.MaHocKy == 20), Is.False);
    }

    [Test]
    public void ReadIsolation_AcademicTermService_QueryForeignCampus_ThrowsForbiddenCampus()
    {
        var service = new AcademicTermService(_db, _httpContextAccessor);
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetTermsAsync(new AcademicTermQueryParameters { MaDonVi = CampusB });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task ReadIsolation_CourseService_ReturnsOnlyStaffCampusCourses()
    {
        var service = CreateCourseService();
        var result = await service.GetAsync(new KhoaHocQueryParameters { PageIndex = 1, PageSize = 20 });

        Assert.That(result.Items.All(c => c.MaDonVi == CampusA), Is.True);
        Assert.That(result.Items.Any(c => c.MaKhoaHoc == 1001), Is.True);
        Assert.That(result.Items.Any(c => c.MaKhoaHoc == 2001), Is.False);
    }

    [Test]
    public async Task ReadIsolation_LopHanhChinhService_ReturnsOnlyStaffCampusClasses()
    {
        var service = new LopHanhChinhService(_db, _httpContextAccessor);
        var result = (await service.GetByChuyenNganhAsync(1)).ToList();

        Assert.That(result.Any(l => l.MaLop == 11), Is.True);
        Assert.That(result.Any(l => l.MaLop == 21), Is.False);
    }

    [Test]
    public async Task ReadIsolation_RoomService_ReturnsOnlyStaffCampusRooms()
    {
        var service = new RoomService(_db, _httpContextAccessor);
        var result = await service.GetRoomsAsync(new RoomQueryParameters { PageIndex = 1, PageSize = 20 });

        Assert.That(result.Items.All(r => r.MaDonVi == CampusA), Is.True);
        Assert.That(result.Items.Any(r => r.MaPhong == 101), Is.True);
        Assert.That(result.Items.Any(r => r.MaPhong == 201), Is.False);
    }

    [Test]
    public void ReadIsolation_RoomService_QueryForeignCampus_ThrowsForbiddenCampus()
    {
        var service = new RoomService(_db, _httpContextAccessor);
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetRoomsAsync(new RoomQueryParameters { MaDonVi = CampusB });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    // =========================================================================
    // 3. COURSE MUTATION CROSS-CAMPUS TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task CourseMutation_CreateWithForeignTerm_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var initialCount = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 20, // Foreign Term Campus B
                MaLop = 11,
                MaGiaoVien = 301,
                TieuDe = "Khóa học thử nghiệm chéo kỳ"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(initialCount));
    }

    [Test]
    public async Task CourseMutation_CreateWithForeignClass_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var initialCount = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 10,
                MaLop = 21, // Foreign Class Campus B
                MaGiaoVien = 301,
                TieuDe = "Khóa học thử nghiệm chéo lớp"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(initialCount));
    }

    [Test]
    public async Task CourseMutation_CreateWithForeignTeacher_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var initialCount = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 10,
                MaLop = 11,
                MaGiaoVien = 302, // Foreign Teacher Campus B
                TieuDe = "Khóa học thử nghiệm chéo GV"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(initialCount));
    }

    [Test]
    public async Task CourseMutation_CreateWithForeignBlock_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var initialCount = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 10,
                MaLop = 11,
                MaGiaoVien = 301,
                MaBlockBatDau = 201, // Foreign Block Campus B
                TieuDe = "Khóa học thử nghiệm chéo Block"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(initialCount));
    }

    [Test]
    public async Task CourseMutation_StaffCampusA_UpdateCourseCampusB_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var originalCourseB = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 2001);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.UpdateAsync(2001, new UpdateKhoaHocRequest
            {
                MaHocKy = 20,
                MaLop = 21,
                MaGiaoVien = 302,
                TieuDe = "Hacked Title",
                TrangThai = "nhap"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));

        var afterCourseB = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 2001);
        Assert.That(afterCourseB.TieuDe, Is.EqualTo(originalCourseB.TieuDe));
    }

    // =========================================================================
    // 4. TEACHER ASSIGNMENT CROSS-CAMPUS & BULK ASSIGN TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task TeacherAssignment_AssignTeacherCampusB_ToCourseCampusA_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var originalCourseA = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 1001);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.UpdateAsync(1001, new UpdateKhoaHocRequest
            {
                MaHocKy = 10,
                MaLop = 11,
                MaGiaoVien = 302, // Foreign Teacher Campus B
                TieuDe = originalCourseA.TieuDe,
                TrangThai = "nhap"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));

        var afterCourseA = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 1001);
        Assert.That(afterCourseA.MaGiaoVien, Is.EqualTo(originalCourseA.MaGiaoVien), "Old assignment must remain intact");
    }

    [Test]
    public async Task TeacherAssignment_StaffCampusA_AssignTeacherToCourseCampusB_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var originalCourseB = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 2001);

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.UpdateAsync(2001, new UpdateKhoaHocRequest
            {
                MaHocKy = 20,
                MaLop = 21,
                MaGiaoVien = 301, // Staff A's Teacher
                TieuDe = originalCourseB.TieuDe,
                TrangThai = "nhap"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));

        var afterCourseB = await _db.KhoaHocs.AsNoTracking().FirstAsync(c => c.MaKhoaHoc == 2001);
        Assert.That(afterCourseB.MaGiaoVien, Is.EqualTo(originalCourseB.MaGiaoVien), "Course B must not be altered");
    }

    [Test]
    public async Task BulkAssign_StaffA_AssignTeacherCampusB_ToCourseCampusA_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.BulkAssignAsync(new BulkAssignCoursesRequest
            {
                MaMonHoc = 1,
                MaGiaoVien = 302, // Teacher Campus B
                MaHocKy = 10,
                MaLopIds = new List<int> { 11 }
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore), "0 mutation must occur on foreign teacher bulk-assign");
    }

    [Test]
    public async Task BulkAssign_StaffA_AssignTeacherCampusA_ToCourseCampusB_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.BulkAssignAsync(new BulkAssignCoursesRequest
            {
                MaMonHoc = 1,
                MaGiaoVien = 301,
                MaHocKy = 20, // Term Campus B
                MaLopIds = new List<int> { 21 } // Class Campus B
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore), "0 mutation must occur on foreign course bulk-assign");
    }

    [Test]
    public async Task BulkAssign_MixedClasses_FirstValidCampusA_SecondForeignCampusB_AtomicFailure_ZeroCoursesCreated()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        // Class 12 is valid Campus A, Class 21 is foreign Campus B
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.BulkAssignAsync(new BulkAssignCoursesRequest
            {
                MaMonHoc = 1,
                MaGiaoVien = 301,
                MaHocKy = 10,
                MaLopIds = new List<int> { 12, 21 }
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore), "Batch must fail atomically: Class 12 must NOT be created when Class 21 is foreign");
    }

    [Test]
    public async Task BulkAssign_AllCampusA_SucceedsNormally()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        var result = await service.BulkAssignAsync(new BulkAssignCoursesRequest
        {
            MaMonHoc = 1,
            MaGiaoVien = 301,
            MaHocKy = 10,
            MaLopIds = new List<int> { 12 } // Valid new class in Campus A
        });

        Assert.That(result.CreatedCount, Is.EqualTo(1));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore + 1));
    }

    // =========================================================================
    // 5. TEACHER SUGGESTIONS ISOLATION TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task TeacherSuggestions_OnlyReturnsCampusATeachers_ZeroTraceOfCampusBTeachers()
    {
        var suggestionService = CreateSuggestionService();

        var result = await suggestionService.GetSuggestionsAsync(new CourseAssignmentSuggestionRequestDto
        {
            MaMonHoc = 1,
            MaHocKy = 10,
            MaLopIds = new List<int> { 11 }
        }, campusId: CampusA);

        // 1. Campus A eligible teacher 301 must be in Candidates
        Assert.That(result.Candidates.Any(c => c.MaGiaoVien == 301), Is.True);

        // 2. Campus A locked teacher 303 must be in ExcludedCandidates with TEACHER_LOCKED
        Assert.That(result.ExcludedCandidates.Any(e => e.MaGiaoVien == 303 && e.ReasonCode == "TEACHER_LOCKED"), Is.True);

        // 3. Foreign teacher 302 from Campus B must NEVER appear in Candidates or ExcludedCandidates
        Assert.That(result.Candidates.Any(c => c.MaGiaoVien == 302), Is.False, "Foreign teacher must not appear in candidates");
        Assert.That(result.ExcludedCandidates.Any(e => e.MaGiaoVien == 302), Is.False, "Foreign teacher must not appear in excluded candidates");
        Assert.That(result.Candidates.Any(c => c.HoTen.Contains("Đồng Nai")), Is.False);
        Assert.That(result.ExcludedCandidates.Any(e => e.HoTen.Contains("Đồng Nai")), Is.False);
    }

    // =========================================================================
    // 6. BLOCK SCOPE & VALIDATION TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task BlockScope_StaffA_GetBlocksTermA_Succeeds()
    {
        var service = CreateBlockService();
        var blocks = await service.GetByTermIdAsync(10);

        Assert.That(blocks.Count, Is.EqualTo(1));
        Assert.That(blocks[0].MaBlock, Is.EqualTo(101));
    }

    [Test]
    public void BlockScope_StaffA_GetBlocksTermB_ThrowsForbiddenCampus()
    {
        var service = CreateBlockService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetByTermIdAsync(20); // Term 20 belongs to Campus B
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task BlockScope_CreateCourseA_WithForeignBlockB_ThrowsForbiddenCampus_ZeroMutation()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 10,
                MaLop = 11,
                MaGiaoVien = 301,
                MaBlockBatDau = 201, // Block 201 belongs to Campus B
                TieuDe = "Khóa học sai Block ngoại vi"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore));
    }

    [Test]
    public async Task BlockScope_CreateCourseA_WithSameCampusDifferentTermBlock_ThrowsBlockTermMismatch_ZeroMutation()
    {
        var service = CreateCourseService();
        var countBefore = await _db.KhoaHocs.CountAsync();

        // Block 102 belongs to Campus A, but to Term 12 (different term from Term 10)
        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CreateAsync(new CreateKhoaHocRequest
            {
                MaDonVi = CampusA,
                MaMonHoc = 1,
                MaHocKy = 10,
                MaLop = 11,
                MaGiaoVien = 301,
                MaBlockBatDau = 102, // Block of same campus but wrong term
                TieuDe = "Khóa học sai Block cùng cơ sở"
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(ex.ErrorCode, Is.EqualTo("BLOCK_TERM_MISMATCH"));
        Assert.That(await _db.KhoaHocs.CountAsync(), Is.EqualTo(countBefore));
    }

    // =========================================================================
    // 7. CURRENT-JOB & SCHEDULING READ ENDPOINTS TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public async Task CurrentJob_AcademicStaffA_ReadsOwnCampusRunningJob_Success()
    {
        var draftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 501,
            DraftId = draftId,
            MaHocKy = 10,
            MaDonVi = CampusA,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var service = CreateSmartTimetableService();
        var job = await service.GetCurrentGenerationJobAsync(10);

        Assert.That(job, Is.Not.Null);
        Assert.That(job!.DraftId, Is.EqualTo(draftId));
        Assert.That(job.MaDonVi, Is.EqualTo(CampusA));
    }

    [Test]
    public async Task CurrentJob_NewestJobAtCampusB_HiddenFromStaffA()
    {
        // Job at Campus B created more recently
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 601,
            DraftId = Guid.NewGuid(),
            MaHocKy = 20,
            MaDonVi = CampusB,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow.AddMinutes(10)
        });
        await _db.SaveChangesAsync();

        var service = CreateSmartTimetableService();
        // Staff A requests current job for term 10 (Campus A)
        var job = await service.GetCurrentGenerationJobAsync(10);

        Assert.That(job, Is.Null, "Foreign campus job must not be returned");
    }

    [Test]
    public void CurrentJob_QueryForeignTerm_ThrowsForbiddenCampus()
    {
        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetCurrentGenerationJobAsync(20); // Term 20 belongs to Campus B
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public async Task CurrentJob_CompletedOrPublishedJob_NotReturnedAsRunningDraft()
    {
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 701,
            DraftId = Guid.NewGuid(),
            MaHocKy = 10,
            MaDonVi = CampusA,
            NguoiYeuCau = 101,
            TrangThai = "da_xuat_ban", // Completed / Published
            NgayTao = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var service = CreateSmartTimetableService();
        var job = await service.GetCurrentGenerationJobAsync(10);

        Assert.That(job, Is.Null, "Published job must not be returned as an active draft generation job");
    }

    [Test]
    public async Task CurrentJob_MultipleDraftJobs_DeterministicLatestSelection()
    {
        var olderDraft = Guid.NewGuid();
        var newerDraft = Guid.NewGuid();

        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 801,
            DraftId = olderDraft,
            MaHocKy = 10,
            MaDonVi = CampusA,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow.AddMinutes(-10)
        });
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 802,
            DraftId = newerDraft,
            MaHocKy = 10,
            MaDonVi = CampusA,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var service = CreateSmartTimetableService();
        var job = await service.GetCurrentGenerationJobAsync(10);

        Assert.That(job, Is.Not.Null);
        Assert.That(job!.DraftId, Is.EqualTo(newerDraft), "Must deterministically select the latest draft");
    }

    [Test]
    public async Task CurrentJob_GetJob_ZeroMutation()
    {
        var countBefore = await _db.ScheduleGenerationJobs.CountAsync();

        var service = CreateSmartTimetableService();
        await service.GetCurrentGenerationJobAsync(10);

        var countAfter = await _db.ScheduleGenerationJobs.CountAsync();
        Assert.That(countAfter, Is.EqualTo(countBefore), "GET current-job must have 0 mutation");
    }

    [Test]
    public void SchedulingRead_GetDraft_ForeignDraftId_ThrowsForbiddenCampus()
    {
        var foreignDraftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 901,
            DraftId = foreignDraftId,
            MaHocKy = 20,
            MaDonVi = CampusB,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        _db.SaveChanges();

        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetDraftAsync(foreignDraftId);
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void SchedulingRead_GetProgress_ForeignDraftId_ThrowsForbiddenCampus()
    {
        var foreignDraftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 902,
            DraftId = foreignDraftId,
            MaHocKy = 20,
            MaDonVi = CampusB,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        _db.SaveChanges();

        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetGenerationProgressAsync(foreignDraftId);
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void SchedulingRead_CheckConflictsBatch_ForeignTerm_ThrowsForbiddenCampus()
    {
        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CheckConflictsAsync(new ConflictCheckBatchRequest
            {
                MaHocKy = 20, // Term Campus B
                MaDonVi = CampusA,
                Items = new List<ConflictCheckItem>()
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void SchedulingRead_CheckConflictsBatch_ForeignDraft_ThrowsForbiddenCampus()
    {
        var foreignDraftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 903,
            DraftId = foreignDraftId,
            MaHocKy = 20,
            MaDonVi = CampusB,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        _db.SaveChanges();

        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.CheckConflictsAsync(new ConflictCheckBatchRequest
            {
                DraftId = foreignDraftId,
                MaHocKy = 10,
                MaDonVi = CampusA,
                Items = new List<ConflictCheckItem>()
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void SchedulingRead_GetPublishedSchedule_ForeignScheduleId_ThrowsForbiddenCampus()
    {
        var service = CreateThoiKhoaBieuService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GetByIdAsync(2001); // Schedule 2001 belongs to Campus B
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    // =========================================================================
    // 8. SCHEDULING MUTATION CROSS-CAMPUS TESTS (TestTier: Service + InMemory DB)
    // =========================================================================

    [Test]
    public void Scheduling_GenerateDraft_ForeignCampus_ThrowsForbiddenCampus()
    {
        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.GenerateAsync(new GenerateTimetableRequest
            {
                MaDonVi = CampusB, // Foreign Campus
                MaHocKy = 20
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void Scheduling_ListDrafts_ForeignCampus_ThrowsForbiddenCampus()
    {
        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.ListDraftsAsync(CampusB, 20);
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }

    [Test]
    public void Scheduling_PublishDraft_ForeignCampus_ThrowsForbiddenCampus_ZeroMutation()
    {
        var foreignDraftId = Guid.NewGuid();
        _db.ScheduleGenerationJobs.Add(new ScheduleGenerationJob
        {
            MaJob = 777,
            DraftId = foreignDraftId,
            MaHocKy = 20,
            MaDonVi = CampusB,
            NguoiYeuCau = 101,
            TrangThai = "draft",
            NgayTao = DateTime.UtcNow
        });
        _db.SaveChanges();

        var service = CreateSmartTimetableService();

        var ex = Assert.ThrowsAsync<ApiException>(async () =>
        {
            await service.PublishAsync(new PublishTimetableRequest
            {
                DraftId = foreignDraftId
            });
        });

        Assert.That(ex!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        Assert.That(ex.ErrorCode, Is.EqualTo("FORBIDDEN_CAMPUS"));
    }
}

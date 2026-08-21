using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Backend.Services.Bgh;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests;

public class BghPassFailControllerTests
{
    [Test]
    public async Task AtRiskStudents_ShouldFilterBySemesterAndReturnActualRiskSubject()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var failedTerm = new HocKy
        {
            MaHocKy = 1,
            MaDonVi = 1,
            MaCodeHocKy = "HK-FAILED",
            TenHocKy = "Học kỳ có môn rớt",
            NamHoc = "2026",
            ThuTuTrongNam = 1,
            NgayBatDau = new DateOnly(2026, 1, 1),
            NgayKetThuc = new DateOnly(2026, 4, 30)
        };
        var passedTerm = new HocKy
        {
            MaHocKy = 2,
            MaDonVi = 1,
            MaCodeHocKy = "HK-PASSED",
            TenHocKy = "Học kỳ đã đạt",
            NamHoc = "2026",
            ThuTuTrongNam = 2,
            NgayBatDau = new DateOnly(2026, 5, 1),
            NgayKetThuc = new DateOnly(2026, 8, 31)
        };
        var subject = new DanhMucMonHoc
        {
            MaMonHoc = 1,
            MaCodeMonHoc = "DBI",
            TenMonHoc = "Cơ sở dữ liệu",
            SoTinChi = 3,
            ConHoatDong = true
        };
        context.DonVis.Add(new DonVi { MaDonVi = 1, TenDonVi = "Campus 1", CapDonVi = "co_so", ConHoatDong = true });
        context.HocKys.AddRange(failedTerm, passedTerm);
        context.DanhMucMonHocs.Add(subject);
        context.NguoiDungs.Add(CreateStudent(1, 1, 0));
        context.DiemSos.AddRange(
            CreateGrade(1, 1, 1, subject, failedTerm, 3m),
            CreateGrade(2, 1, 1, subject, passedTerm, 8m));
        await context.SaveChangesAsync();

        var controller = CreatePrincipalController(context, 1);
        var failedAction = await controller.GetAtRiskStudents(semesterId: failedTerm.MaHocKy);
        var failedResponse = (failedAction.Result as OkObjectResult)?.Value as ApiResponseDto<AtRiskReportDto>;
        var passedAction = await controller.GetAtRiskStudents(semesterId: passedTerm.MaHocKy);
        var passedResponse = (passedAction.Result as OkObjectResult)?.Value as ApiResponseDto<AtRiskReportDto>;

        Assert.Multiple(() =>
        {
            Assert.That(failedResponse?.Data?.TotalAtRisk, Is.EqualTo(1));
            Assert.That(failedResponse?.Data?.Students.Single().RiskSubjectName, Is.EqualTo("Cơ sở dữ liệu"));
            Assert.That(passedResponse?.Data?.TotalAtRisk, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task PassFailFiltersAndReport_ShouldFollowProgramHierarchyAndCampusScope()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var major = new NganhDaoTao { MaNganh = 1, MaCodeNganh = "CNTT", TenNganh = "Công nghệ thông tin", ConHoatDong = true };
        var specialization = new ChuyenNganh { MaChuyenNganh = 11, MaNganh = major.MaNganh, TenChuyenNganh = "Phát triển phần mềm", ConHoatDong = true, NganhDaoTao = major };
        var program = CreateProgram(101, specialization, "CT-CNTT-01");
        var subject = new DanhMucMonHoc { MaMonHoc = 501, MaCodeMonHoc = "CSDL", TenMonHoc = "Cơ sở dữ liệu", SoTinChi = 3, ConHoatDong = true };
        var subjectOutsideProgram = new DanhMucMonHoc { MaMonHoc = 502, MaCodeMonHoc = "TINVP", TenMonHoc = "Tin học văn phòng", SoTinChi = 2, ConHoatDong = true };
        var programSubject = CreateProgramSubject(1001, program, subject);
        var semester = new HocKy
        {
            MaHocKy = 301,
            MaDonVi = 1,
            MaCodeHocKy = "HK1",
            TenHocKy = "Học kỳ 1",
            NamHoc = "2026",
            ThuTuTrongNam = 1,
            NgayBatDau = new DateOnly(2026, 1, 1),
            NgayKetThuc = new DateOnly(2026, 4, 30)
        };

        context.NganhDaoTaos.Add(major);
        context.ChuyenNganhs.Add(specialization);
        context.ChuongTrinhDaoTaos.Add(program);
        context.DanhMucMonHocs.AddRange(subject, subjectOutsideProgram);
        context.MonHocTrongChuongTrinhs.Add(programSubject);
        context.HocKys.Add(semester);
        context.LopHanhChinhs.Add(CreateClass(401, 1, program));
        context.NguoiDungs.AddRange(
            CreateStudent(1, 1, 401),
            CreateStudent(2, 1, 401),
            CreateStudent(3, 2, 401));
        context.DiemSos.AddRange(
            CreateGrade(1, 1, 1, subject, semester, 8m),
            CreateGrade(2, 1, 2, subject, semester, 3m),
            CreateGrade(3, 2, 3, subject, semester, 2m),
            CreateGrade(4, 1, 1, subjectOutsideProgram, semester, 9m));
        await context.SaveChangesAsync();

        var controller = CreatePrincipalController(context, 1);

        var filterAction = await controller.GetPassFailFilterOptions(major.MaNganh, specialization.MaChuyenNganh, programSubject.MaChuongTrinhMonHoc);
        var filterResult = filterAction.Result as OkObjectResult;
        var filterResponse = filterResult?.Value as ApiResponseDto<PassFailFilterOptionsDto>;

        Assert.That(filterResponse?.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(filterResponse!.Data!.Majors.Select(item => item.Id), Is.EquivalentTo(new[] { major.MaNganh }));
            Assert.That(filterResponse.Data.Specializations.Select(item => item.Id), Is.EquivalentTo(new[] { specialization.MaChuyenNganh }));
            Assert.That(filterResponse.Data.ProgramSubjects.Select(item => item.Id), Is.EquivalentTo(new[] { programSubject.MaChuongTrinhMonHoc }));
            Assert.That(filterResponse.Data.Semesters.Select(item => item.Id), Is.EquivalentTo(new[] { semester.MaHocKy }));
        });

        var reportAction = await controller.GetPassFailRates(major.MaNganh, specialization.MaChuyenNganh, programSubject.MaChuongTrinhMonHoc, semester.MaHocKy);
        var reportResult = reportAction.Result as OkObjectResult;
        var reportResponse = reportResult?.Value as ApiResponseDto<PassFailReportDto>;

        Assert.That(reportResponse?.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(reportResponse!.Data!.TotalResults, Is.EqualTo(2));
            Assert.That(reportResponse.Data.TotalPass, Is.EqualTo(1));
            Assert.That(reportResponse.Data.TotalFail, Is.EqualTo(1));
            Assert.That(reportResponse.Data.SemesterTrend, Has.Count.EqualTo(1));
            Assert.That(reportResponse.Data.OverallPassRate, Is.EqualTo(50));
            Assert.That(reportResponse.Data.OverallFailRate, Is.EqualTo(50));
        });
    }

    private static BghAcademicController CreatePrincipalController(ApplicationDbContext context, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 99,
            Email = "principal@test.local",
            Role = AuthRoles.Principal,
            CampusId = campusId,
            Status = "hoat_dong"
        };

        var cache = new BghPerformanceCache(
            new MemoryCache(new MemoryCacheOptions { SizeLimit = 1_000 }),
            NullLogger<BghPerformanceCache>.Instance);
        return new BghAcademicController(context, cache)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static ChuongTrinhDaoTao CreateProgram(int id, ChuyenNganh specialization, string code)
    {
        return new ChuongTrinhDaoTao
        {
            MaChuongTrinh = id,
            MaChuyenNganh = specialization.MaChuyenNganh,
            MaKhoaTuyenSinh = 1,
            MaCodeChuongTrinh = code,
            TenChuongTrinh = code,
            Version = "1.0",
            SoHocKy = 6,
            ThoiGianDaoTaoThang = 36,
            TrangThai = "active",
            ConHoatDong = true,
            ChuyenNganh = specialization
        };
    }

    private static MonHocTrongChuongTrinh CreateProgramSubject(int id, ChuongTrinhDaoTao program, DanhMucMonHoc subject)
    {
        return new MonHocTrongChuongTrinh
        {
            MaChuongTrinhMonHoc = id,
            MaChuongTrinh = program.MaChuongTrinh,
            MaMonHoc = subject.MaMonHoc,
            HocKyDuKien = 1,
            SoTinChi = 3,
            LoaiMonHoc = "bat_buoc",
            BatBuoc = true,
            ConHoatDong = true,
            ChuongTrinhDaoTao = program,
            DanhMucMonHoc = subject
        };
    }

    private static LopHanhChinh CreateClass(int id, int campusId, ChuongTrinhDaoTao program)
    {
        return new LopHanhChinh
        {
            MaLop = id,
            MaDonVi = campusId,
            MaCodeLop = $"L{id}",
            TenLop = $"Lớp {id}",
            MaChuongTrinh = program.MaChuongTrinh,
            ConHoatDong = true,
            ChuongTrinh = program
        };
    }

    private static NguoiDung CreateStudent(int id, int campusId, int classId)
    {
        return new NguoiDung
        {
            MaNguoiDung = id,
            MaDonVi = campusId,
            MaLop = classId,
            Email = $"student{id}@test.local",
            HoTen = $"Student {id}",
            VaiTroChinh = "hoc_sinh",
            TrangThai = "hoat_dong",
            NgayTao = DateTime.UtcNow
        };
    }

    private static DiemSo CreateGrade(int id, int campusId, int studentId, DanhMucMonHoc subject, HocKy semester, decimal gpa)
    {
        return new DiemSo
        {
            MaDiemSo = id,
            MaDonVi = campusId,
            MaHocSinh = studentId,
            MaMonHoc = subject.MaMonHoc,
            MaHocKy = semester.MaHocKy,
            GpaMonHoc = gpa,
            TrangThai = gpa >= 4 ? "dat" : "rot",
            MonHoc = subject,
            HocKy = semester
        };
    }
}

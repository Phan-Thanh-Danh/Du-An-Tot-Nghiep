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

public class BghReportAndEvaluationLogicTests
{
    [Test]
    public async Task AcademicReport_ShouldPersistListRestoreAndDeleteSavedFilter()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.DonVis.Add(new DonVi
        {
            MaDonVi = 1,
            TenDonVi = "Cơ sở kiểm thử",
            CapDonVi = "co_so",
            ConHoatDong = true
        });
        context.NguoiDungs.Add(CreateUser(99, 1, "principal@test.local", "Ban giám hiệu", AuthRoles.Principal));
        context.HocKys.Add(CreateSemester(12, "Học kỳ báo cáo", new DateOnly(2026, 1, 1)));
        await context.SaveChangesAsync();

        var controller = CreateAcademicController(context, 99, 1);
        var createAction = await controller.CreateAcademicReport(new SaveAcademicReportRequest
        {
            Name = "Báo cáo học kỳ kiểm thử",
            ReportType = "subject",
            CampusId = 999,
            SemesterId = 12
        });
        var createResponse = (createAction.Result as OkObjectResult)?.Value
            as ApiResponseDto<SavedAcademicReportResultDto>;

        Assert.That(createResponse?.Data, Is.Not.Null);
        var savedId = createResponse!.Data!.SavedReport.Id;
        Assert.Multiple(() =>
        {
            Assert.That(savedId, Is.GreaterThan(0));
            Assert.That(createResponse.Data.SavedReport.Name, Is.EqualTo("Báo cáo học kỳ kiểm thử"));
            Assert.That(createResponse.Data.SavedReport.ReportType, Is.EqualTo("subject"));
            Assert.That(createResponse.Data.SavedReport.CampusId, Is.EqualTo(1),
                "BGH cấp cơ sở chỉ được lưu bộ lọc của cơ sở mình.");
            Assert.That(createResponse.Data.SavedReport.SemesterId, Is.EqualTo(12));
            Assert.That(context.XuatBaoCaos.Single().ThamSoJson, Does.Contain("\"reportType\":\"subject\""));
        });

        var listAction = await controller.GetSavedAcademicReports();
        var listResponse = (listAction.Result as OkObjectResult)?.Value
            as ApiResponseDto<List<SavedAcademicReportDto>>;
        Assert.That(listResponse?.Data, Has.Count.EqualTo(1));

        var detailAction = await controller.GetSavedAcademicReport(savedId);
        var detailResponse = (detailAction.Result as OkObjectResult)?.Value
            as ApiResponseDto<SavedAcademicReportResultDto>;
        Assert.Multiple(() =>
        {
            Assert.That(detailResponse?.Data?.SavedReport.Id, Is.EqualTo(savedId));
            Assert.That(detailResponse?.Data?.Report.Filter.ReportType, Is.EqualTo("subject"));
            Assert.That(detailResponse?.Data?.Report.Filter.CampusId, Is.EqualTo(1));
        });

        var deleteAction = await controller.DeleteSavedAcademicReport(savedId);
        Assert.That(deleteAction.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(await context.XuatBaoCaos.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task TeacherRanking_ShouldCalculateSentimentAndTrendFromRealSemesterScores()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var campus = new DonVi
        {
            MaDonVi = 1,
            TenDonVi = "Cơ sở kiểm thử",
            CapDonVi = "co_so",
            ConHoatDong = true
        };
        var teacher = CreateUser(10, 1, "teacher@test.local", "Giảng viên kiểm thử", "giao_vien");
        var semester1 = CreateSemester(1, "Học kỳ 1", new DateOnly(2026, 1, 1));
        var semester2 = CreateSemester(2, "Học kỳ 2", new DateOnly(2026, 5, 1));

        context.DonVis.Add(campus);
        context.NguoiDungs.Add(teacher);
        context.HocKys.AddRange(semester1, semester2);
        context.DanhGiaGiaoViens.AddRange(
            CreateEvaluation(1, teacher, semester1, 2),
            CreateEvaluation(2, teacher, semester1, 3),
            CreateEvaluation(3, teacher, semester1, 3),
            CreateEvaluation(4, teacher, semester2, 4),
            CreateEvaluation(5, teacher, semester2, 5),
            CreateEvaluation(6, teacher, semester2, 5));
        await context.SaveChangesAsync();

        var controller = CreateEvaluationController(context, 99, 1);
        var rankingAction = await controller.GetEvaluationRanking();
        var rankingResponse = (rankingAction.Result as OkObjectResult)?.Value
            as ApiResponseDto<List<TeacherRankingDto>>;
        var ranking = rankingResponse?.Data?.Single();

        Assert.That(ranking, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(ranking!.Positive, Is.EqualTo(50));
            Assert.That(ranking.Negative, Is.EqualTo(50));
            Assert.That(ranking.Trend, Is.EqualTo("up"));
            Assert.That(ranking.TrendDelta, Is.EqualTo(2));
            Assert.That(ranking.PreviousSemesterRating, Is.EqualTo(2.67).Within(0.01));
            Assert.That(ranking.LatestSemesterRating, Is.EqualTo(4.67).Within(0.01));
        });

        var overviewAction = await controller.GetEvaluationOverview();
        var overviewResponse = (overviewAction.Result as OkObjectResult)?.Value
            as ApiResponseDto<EvalOverviewDto>;
        Assert.Multiple(() =>
        {
            Assert.That(overviewResponse?.Data?.PositivePercentage, Is.EqualTo(50));
            Assert.That(overviewResponse?.Data?.NegativePercentage, Is.EqualTo(50));
        });
    }

    private static BghAcademicController CreateAcademicController(
        ApplicationDbContext context,
        int userId,
        int campusId)
    {
        var httpContext = CreateHttpContext(userId, campusId);
        var cache = new BghPerformanceCache(
            new MemoryCache(new MemoryCacheOptions { SizeLimit = 1_000 }),
            NullLogger<BghPerformanceCache>.Instance);
        return new BghAcademicController(context, cache)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static BghEvaluationController CreateEvaluationController(
        ApplicationDbContext context,
        int userId,
        int campusId) => new(context)
    {
        ControllerContext = new ControllerContext { HttpContext = CreateHttpContext(userId, campusId) }
    };

    private static DefaultHttpContext CreateHttpContext(int userId, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = userId,
            Email = "principal@test.local",
            Role = AuthRoles.Principal,
            CampusId = campusId,
            Status = "hoat_dong"
        };
        return httpContext;
    }

    private static NguoiDung CreateUser(int id, int campusId, string email, string name, string role) => new()
    {
        MaNguoiDung = id,
        MaDonVi = campusId,
        Email = email,
        HoTen = name,
        VaiTroChinh = role,
        TrangThai = "hoat_dong",
        NgayTao = DateTime.UtcNow
    };

    private static HocKy CreateSemester(int id, string name, DateOnly startDate) => new()
    {
        MaHocKy = id,
        MaDonVi = 1,
        MaCodeHocKy = $"HK-{id}",
        TenHocKy = name,
        NamHoc = "2026",
        ThuTuTrongNam = id,
        NgayBatDau = startDate,
        NgayKetThuc = startDate.AddMonths(4)
    };

    private static DanhGiaGiaoVien CreateEvaluation(
        int id,
        NguoiDung teacher,
        HocKy semester,
        int score) => new()
    {
        MaDanhGia = id,
        MaGiaoVien = teacher.MaNguoiDung,
        MaHocKy = semester.MaHocKy,
        MaCauHoiDg = 1,
        DiemSo = score,
        NgayTao = DateTime.UtcNow,
        GiaoVien = teacher,
        HocKy = semester
    };
}

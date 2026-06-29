using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD_DL_Report")]
public class RD_DL_ReportTests : ApiTestBase
{
    [Test]
    public async Task SuperAdmin_CanGetOverviewReport()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.CampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalRewards"), Is.GreaterThanOrEqualTo(4));
        Assert.That(GetInt32(data, "totalDisciplineAppeals"), Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public async Task CampusAdmin_OnlySeesInScopeRewardDisciplineData()
    {
        var seed = await SeedReportDataAsync();
        using var client = await CreateClientAsync(seed.CampusAdminEmail);

        using var response = await client.GetAsync("/api/admin/reward-discipline/reports/overview");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalRewards"), Is.GreaterThanOrEqualTo(4));
        Assert.That(GetInt32(data, "totalRewards"), Is.LessThan(10));
        Assert.That(GetInt32(data, "totalActiveDisciplineRecords"), Is.EqualTo(1));
    }

    [Test]
    public async Task OutOfScopeMaDonVi_ShouldReturnForbidden()
    {
        var seed = await SeedReportDataAsync();
        using var client = await CreateClientAsync(seed.CampusAdminEmail);

        using var response = await client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.OtherCampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Overview_ReturnsRewardCountsByStatus()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.CampusId}");

        using var root = await GetRootAsync(response);
        var rewardByStatus = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "rewardByStatus");
        Assert.That(FindStatusCount(rewardByStatus, RewardDisciplineConstants.RewardStatuses.Issued), Is.EqualTo(1));
        Assert.That(FindStatusCount(rewardByStatus, RewardDisciplineConstants.RewardStatuses.Cancelled), Is.EqualTo(1));
    }

    [Test]
    public async Task Overview_ReturnsDisciplineCountsByStatus()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.CampusId}");

        using var root = await GetRootAsync(response);
        var disciplineByStatus = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "disciplineByStatus");
        Assert.That(FindStatusCount(disciplineByStatus, RewardDisciplineConstants.DisciplineStatuses.Active), Is.EqualTo(1));
        Assert.That(FindStatusCount(disciplineByStatus, RewardDisciplineConstants.DisciplineStatuses.Removed), Is.EqualTo(1));
    }

    [Test]
    public async Task RewardReport_CountsIssuedCanceledRestoredRewardsCorrectly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/rewards?maDonVi={seed.CampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalIssuedRewards"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "totalCanceledRewards"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "totalRestoredRewards"), Is.EqualTo(1));
    }

    [Test]
    public async Task DisciplineReport_CountsActiveExpiredRemovedRecordsCorrectly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/discipline?maDonVi={seed.CampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "activeRecords"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "expiredRecords"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "removedEffectRecords"), Is.EqualTo(1));
    }

    [Test]
    public async Task CertificateReport_CountsGeneratedFailedCertificatesCorrectly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/certificates?maDonVi={seed.CampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalCertificatesGenerated"), Is.EqualTo(2));
        Assert.That(GetInt32(data, "totalCertificatesFailed"), Is.EqualTo(1));
    }

    [Test]
    public async Task AppealReport_CountsPendingAcceptedRejectedAppealsCorrectly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/appeals?maDonVi={seed.CampusId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "pendingAppeals"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "acceptedAppeals"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "rejectedAppeals"), Is.EqualTo(1));
    }

    [Test]
    public async Task TrendReport_GroupsByMonthCorrectly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/trends?maDonVi={seed.CampusId}&metric=rewards&groupBy=month");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var points = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "points");
        Assert.That(points.EnumerateArray().Any(x => GetRequiredString(x, "label") == "2026-01" && GetInt32(x, "value") >= 2), Is.True);
        Assert.That(points.EnumerateArray().Any(x => GetRequiredString(x, "label") == "2026-02" && GetInt32(x, "value") >= 2), Is.True);
    }

    [Test]
    public async Task TrendReport_InvalidDateRange_ShouldReturnBadRequest()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/trends?maDonVi={seed.CampusId}&metric=rewards&groupBy=month&fromDate=2026-03-01&toDate=2026-02-01");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task TopStudentsReport_DoesNotLeakDisciplineDescriptionEvidenceInternalNotes()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/top-students?maDonVi={seed.CampusId}&mode=balanced");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("Sensitive violation description"));
        Assert.That(body, Does.Not.Contain("secret-evidence"));
        Assert.That(body, Does.Not.Contain("internal-note-secret"));
    }

    [Test]
    public async Task StudentRole_CannotAccessAdminReports()
    {
        var seed = await SeedReportDataAsync();
        using var client = await CreateClientAsync(seed.StudentEmail);

        using var response = await client.GetAsync("/api/admin/reward-discipline/reports/overview");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UnknownStatusFilter_ShouldReturnBadRequest()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/rewards?maDonVi={seed.CampusId}&trangThai=khong_ton_tai");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ReportQueries_UseScopedDataAndDoNotIncludeOtherCampusRecords()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.CampusId}");

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalRewards"), Is.EqualTo(4));
    }

    [Test]
    public async Task RewardsFilter_BySemester_ShouldOnlyReturnSelectedSemester()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/rewards?maDonVi={seed.CampusId}&maHocKy={seed.TermId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetInt32(GetRequiredProperty(root.RootElement, "data"), "totalIssuedRewards"), Is.EqualTo(1));
    }

    [Test]
    public async Task CertificateReport_RecentFailuresReturnSafeErrorOnly()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/certificates?maDonVi={seed.CampusId}");

        using var root = await GetRootAsync(response);
        var failures = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "recentFailedCertificates");
        var serialized = failures.GetRawText();
        Assert.That(serialized, Does.Contain("safe pdf error"));
        Assert.That(serialized, Does.Not.Contain("System.Exception"));
        Assert.That(serialized, Does.Not.Contain("stack trace"));
    }

    [Test]
    public async Task Overview_LatestEventsDoNotReturnSensitiveFields()
    {
        var seed = await SeedReportDataAsync();

        using var response = await Client.GetAsync($"/api/admin/reward-discipline/reports/overview?maDonVi={seed.CampusId}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("Sensitive violation description"));
        Assert.That(body, Does.Not.Contain("internal-note-secret"));
        Assert.That(body, Does.Not.Contain("secret-evidence"));
    }

    private async Task<HttpClient> CreateClientAsync(string email)
    {
        var client = new HttpClient { BaseAddress = BaseUri };
        using var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            Assert.Fail($"Login {email} thất bại. {await DescribeResponseAsync(loginResponse)}");
        }

        using var root = await GetRootAsync(loginResponse);
        var token = GetOptionalString(root.RootElement, "accessToken");
        if (string.IsNullOrWhiteSpace(token) && HasProperty(root.RootElement, "data"))
        {
            token = GetOptionalString(GetRequiredProperty(root.RootElement, "data"), "accessToken");
        }

        Assert.That(token, Is.Not.Null.And.Not.Empty, $"Login {email} không trả accessToken.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<ReportSeed> SeedReportDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var now = DateTime.UtcNow;
        var campus = new DonVi
        {
            TenDonVi = $"RD-DL Report Campus {suffix}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = now
        };
        var otherCampus = new DonVi
        {
            TenDonVi = $"RD-DL Other Campus {suffix}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = now
        };
        db.DonVis.AddRange(campus, otherCampus);
        await db.SaveChangesAsync();

        var campusAdmin = new NguoiDung
        {
            MaDonVi = campus.MaDonVi,
            Email = $"rddl.report.admin.{suffix}@lms.local",
            HoTen = $"RD DL Campus Admin {suffix}",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            NgayTao = now
        };
        var student = new NguoiDung
        {
            MaDonVi = campus.MaDonVi,
            Email = $"rddl.report.student.{suffix}@lms.local",
            HoTen = $"RD DL Student {suffix}",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            NgayTao = now
        };
        var studentTwo = new NguoiDung
        {
            MaDonVi = campus.MaDonVi,
            Email = $"rddl.report.student2.{suffix}@lms.local",
            HoTen = $"RD DL Student 2 {suffix}",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            NgayTao = now
        };
        var otherStudent = new NguoiDung
        {
            MaDonVi = otherCampus.MaDonVi,
            Email = $"rddl.report.otherstudent.{suffix}@lms.local",
            HoTen = $"RD DL Other Student {suffix}",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            NgayTao = now
        };
        db.NguoiDungs.AddRange(campusAdmin, student, studentTwo, otherStudent);
        await db.SaveChangesAsync();

        var term = new HocKy
        {
            MaDonVi = campus.MaDonVi,
            MaCodeHocKy = $"RDL{suffix}",
            TenHocKy = $"Học kỳ RD-DL {suffix}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = new DateOnly(2026, 1, 1),
            NgayKetThuc = new DateOnly(2026, 6, 30)
        };
        var otherTerm = new HocKy
        {
            MaDonVi = otherCampus.MaDonVi,
            MaCodeHocKy = $"RDO{suffix}",
            TenHocKy = $"Học kỳ khác {suffix}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = new DateOnly(2026, 1, 1),
            NgayKetThuc = new DateOnly(2026, 6, 30)
        };
        db.HocKys.AddRange(term, otherTerm);
        await db.SaveChangesAsync();

        var campaign = new DotKhenThuong
        {
            MaDonVi = campus.MaDonVi,
            MaHocKy = term.MaHocKy,
            TenDot = $"Top 100 {suffix}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Approved,
            NguoiTao = campusAdmin.MaNguoiDung,
            NguoiDuyet = campusAdmin.MaNguoiDung,
            NgayTao = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            NgayDuyet = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
        };
        var otherCampaign = new DotKhenThuong
        {
            MaDonVi = otherCampus.MaDonVi,
            MaHocKy = otherTerm.MaHocKy,
            TenDot = $"Other Top 100 {suffix}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Approved,
            NguoiTao = campusAdmin.MaNguoiDung,
            NgayTao = now
        };
        db.DotKhenThuongs.AddRange(campaign, otherCampaign);
        await db.SaveChangesAsync();

        db.UngVienKhenThuongs.Add(new UngVienKhenThuong
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = campus.MaDonVi,
            MaHocKy = term.MaHocKy,
            DiemXet = 9.1m,
            TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.ApprovedForReward,
            NgayTao = now
        });

        db.KhenThuongs.AddRange(
            CreateReward(campus.MaDonVi, student.MaNguoiDung, term.MaHocKy, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardStatuses.Issued, "issued", new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), url: "cert-1.pdf", issued: true),
            CreateReward(campus.MaDonVi, student.MaNguoiDung, term.MaHocKy, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardStatuses.Cancelled, "cancelled", new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc), daHuy: true),
            CreateReward(campus.MaDonVi, studentTwo.MaNguoiDung, term.MaHocKy, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardStatuses.PdfGenerated, "restored", new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), url: "cert-2.pdf", restored: true),
            CreateReward(campus.MaDonVi, studentTwo.MaNguoiDung, term.MaHocKy, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardStatuses.PdfFailed, "failed", new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc), pdfError: "safe pdf error"),
            CreateReward(otherCampus.MaDonVi, otherStudent.MaNguoiDung, otherTerm.MaHocKy, otherCampaign.MaDotKhenThuong, RewardDisciplineConstants.RewardStatuses.Issued, "other", now, url: "other.pdf", issued: true)
        );

        var active = CreateDiscipline(campus.MaDonVi, student.MaNguoiDung, term.MaHocKy, RewardDisciplineConstants.DisciplineStatuses.Active, RewardDisciplineConstants.DisciplineLevels.Minor, "Active discipline", new DateOnly(2026, 1, 5));
        var expired = CreateDiscipline(campus.MaDonVi, student.MaNguoiDung, term.MaHocKy, RewardDisciplineConstants.DisciplineStatuses.Expired, RewardDisciplineConstants.DisciplineLevels.Moderate, "Expired discipline", new DateOnly(2026, 2, 5));
        var removed = CreateDiscipline(campus.MaDonVi, studentTwo.MaNguoiDung, term.MaHocKy, RewardDisciplineConstants.DisciplineStatuses.Removed, RewardDisciplineConstants.DisciplineLevels.Severe, "Removed discipline", new DateOnly(2026, 3, 5));
        var rejected = CreateDiscipline(campus.MaDonVi, studentTwo.MaNguoiDung, term.MaHocKy, RewardDisciplineConstants.DisciplineStatuses.Rejected, RewardDisciplineConstants.DisciplineLevels.Minor, "Rejected discipline", new DateOnly(2026, 3, 8));
        var other = CreateDiscipline(otherCampus.MaDonVi, otherStudent.MaNguoiDung, otherTerm.MaHocKy, RewardDisciplineConstants.DisciplineStatuses.Active, RewardDisciplineConstants.DisciplineLevels.Severe, "Other discipline", new DateOnly(2026, 1, 6));
        db.HoSoKyLuats.AddRange(active, expired, removed, rejected, other);
        await db.SaveChangesAsync();

        db.KhieuNaiKyLuats.AddRange(
            CreateAppeal(active.MaKyLuat, student.MaNguoiDung, campus.MaDonVi, RewardDisciplineConstants.DisciplineAppealStatuses.Pending, now.AddDays(-4)),
            CreateAppeal(expired.MaKyLuat, student.MaNguoiDung, campus.MaDonVi, RewardDisciplineConstants.DisciplineAppealStatuses.Accepted, now.AddDays(-3), now.AddDays(-2)),
            CreateAppeal(removed.MaKyLuat, studentTwo.MaNguoiDung, campus.MaDonVi, RewardDisciplineConstants.DisciplineAppealStatuses.Rejected, now.AddDays(-2), now.AddDays(-1)),
            CreateAppeal(other.MaKyLuat, otherStudent.MaNguoiDung, otherCampus.MaDonVi, RewardDisciplineConstants.DisciplineAppealStatuses.Pending, now.AddDays(-1))
        );

        await db.SaveChangesAsync();
        return new ReportSeed(campus.MaDonVi, otherCampus.MaDonVi, term.MaHocKy, campusAdmin.Email, student.Email);
    }

    private static KhenThuong CreateReward(
        int campusId,
        int studentId,
        int termId,
        int campaignId,
        string status,
        string suffix,
        DateTime capLuc,
        string? url = null,
        bool daHuy = false,
        bool issued = false,
        bool restored = false,
        string? pdfError = null)
    {
        return new KhenThuong
        {
            MaDonVi = campusId,
            MaHocSinh = studentId,
            MaHocKy = termId,
            MaDotKhenThuong = campaignId,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            TrangThai = status,
            HoTenSnapshot = $"Reward Student {suffix}",
            MssvSnapshot = $"SV{suffix}",
            TenHocKySnapshot = "2026",
            DanhHieuSnapshot = $"Danh hiệu {suffix}",
            DiemXet = 9.0m,
            XepHang = 1,
            UrlChungTu = string.Empty,
            UrlPdfBangKhen = url,
            NgaySinhPdf = url is not null || pdfError is not null ? capLuc.AddHours(1) : null,
            LoiSinhPdf = pdfError,
            SoLanSinhPdf = url is not null || pdfError is not null ? 1 : 0,
            CapLuc = capLuc,
            NgayCapNhat = capLuc,
            NgayCap = issued ? capLuc.AddDays(1) : null,
            DaHuy = daHuy,
            LyDoHuy = daHuy ? "test cancel" : null,
            NgayHuy = daHuy ? capLuc.AddDays(2) : null,
            GhiChuVongDoi = restored ? "Được khôi phục lúc test" : null
        };
    }

    private static HoSoKyLuat CreateDiscipline(
        int campusId,
        int studentId,
        int termId,
        string status,
        string level,
        string title,
        DateOnly ngayViPham)
    {
        return new HoSoKyLuat
        {
            MaDonVi = campusId,
            MaHocSinh = studentId,
            MaHocKy = termId,
            TieuDe = title,
            LoaiKyLuat = "hoc_tap",
            MucDoViPham = level,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder,
            TrangThai = status,
            MoTa = "Sensitive violation description",
            CanCuXuLy = "Quy chế",
            GhiChuNoiBo = "internal-note-secret",
            ChungTuJson = """{"secret":"secret-evidence"}""",
            NgayViPham = ngayViPham,
            NgayTao = ngayViPham.ToDateTime(TimeOnly.MinValue),
            NguoiTao = studentId,
            NgayDuyet = ngayViPham.ToDateTime(TimeOnly.MinValue).AddDays(1),
            NgayApDung = ngayViPham.ToDateTime(TimeOnly.MinValue).AddDays(2),
            NgayGoKyLuat = status == RewardDisciplineConstants.DisciplineStatuses.Removed ? ngayViPham.ToDateTime(TimeOnly.MinValue).AddDays(5) : null,
            DaGoKyLuat = status == RewardDisciplineConstants.DisciplineStatuses.Removed,
            NgayCapNhat = ngayViPham.ToDateTime(TimeOnly.MinValue).AddDays(5)
        };
    }

    private static KhieuNaiKyLuat CreateAppeal(
        int recordId,
        int studentId,
        int campusId,
        string status,
        DateTime createdAt,
        DateTime? resolvedAt = null)
    {
        return new KhieuNaiKyLuat
        {
            MaHoSoKyLuat = recordId,
            MaHocSinh = studentId,
            MaDonVi = campusId,
            LyDoKhieuNai = "Sensitive appeal reason should not appear in report",
            ChungTuJson = """{"secret":"secret-evidence"}""",
            TrangThai = status,
            NgayTao = createdAt,
            NgayXuLy = resolvedAt,
            NgayCapNhat = resolvedAt
        };
    }

    private static int FindStatusCount(JsonElement array, string status)
    {
        foreach (var item in array.EnumerateArray())
        {
            if (string.Equals(GetRequiredString(item, "status"), status, StringComparison.OrdinalIgnoreCase))
            {
                return GetInt32(item, "count");
            }
        }

        Assert.Fail($"Không tìm thấy status {status}.");
        return -1;
    }

    private sealed record ReportSeed(
        int CampusId,
        int OtherCampusId,
        int TermId,
        string CampusAdminEmail,
        string StudentEmail);
}

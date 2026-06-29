using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD6")]
public class RD6_CertificatePdfGenerationTests : ApiTestBase
{
    private const string AdminEmail = "admin@lms.local";

    private const string ValidConfigJson = """
        {
          "fields": [
            { "key": "hoTen", "x": 1754, "y": 600, "fontSize": 42, "align": "center", "color": "#111111", "bold": true },
            { "key": "mssv", "x": 1754, "y": 760, "fontSize": 20, "align": "center", "color": "#333333", "bold": false },
            { "key": "tenHocKy", "x": 1754, "y": 900, "fontSize": 24, "align": "center", "color": "#333333", "bold": false },
            { "key": "danhHieu", "x": 1754, "y": 1050, "fontSize": 28, "align": "center", "color": "#111111", "bold": true },
            { "key": "xepHang", "x": 1754, "y": 1200, "fontSize": 22, "align": "center", "color": "#111111", "bold": true },
            { "key": "diemXet", "x": 1754, "y": 1320, "fontSize": 20, "align": "center", "color": "#333333", "bold": false },
            { "key": "ngayCap", "x": 3100, "y": 1500, "fontSize": 18, "align": "right", "color": "#333333", "bold": false }
          ]
        }
        """;

    private const string InvalidRenderConfigJson = """
        {
          "fields": [
            { "key": "unsupported", "x": 100, "y": 100, "fontSize": 20, "align": "center", "color": "#111111", "bold": false }
          ]
        }
        """;

    [Test]
    public async Task GenerateCertificates_ShouldCreatePdfFilesAndUpdateRewards()
    {
        ValidateCertificateStorageRoot();
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 2);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "successCount"), Is.EqualTo(2));
        Assert.That(GetInt32(data, "failedCount"), Is.EqualTo(0));

        await using var verifyDb = CreateDbContext();
        var rewards = await verifyDb.KhenThuongs.AsNoTracking()
            .Where(x => x.MaDotKhenThuong == seed.CampaignId)
            .ToListAsync();
        Assert.That(rewards, Has.Count.EqualTo(2));
        Assert.That(rewards.All(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfGenerated), Is.True);
        Assert.That(rewards.All(x => !string.IsNullOrWhiteSpace(x.UrlPdfBangKhen)), Is.True);
        Assert.That(rewards.All(x => x.NgaySinhPdf.HasValue), Is.True);
        Assert.That(rewards.All(x => x.SoLanSinhPdf == 1), Is.True);
        Assert.That(rewards.All(x => x.LoiSinhPdf == null), Is.True);
        Assert.That(rewards.All(x => CertificateFileExists(x.UrlPdfBangKhen!)), Is.True);
    }

    [Test]
    public async Task GenerateCertificates_ShouldRejectCampaignNotApproved()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1, campaignStatus: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GenerateCertificates_ShouldRejectCampaignWithoutRewards()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 0);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GenerateCertificates_ShouldUseOverrideActiveTemplate()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);
        var overrideTemplate = await CreateTemplateAsync(db, seed.AdminId, active: true);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { maMauBangKhen = overrideTemplate.MaMauBangKhen });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var reward = await verifyDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == seed.CampaignId);
        Assert.That(reward.MaMauBangKhen, Is.EqualTo(overrideTemplate.MaMauBangKhen));
    }

    [Test]
    public async Task GenerateCertificates_ShouldRejectInactiveOverrideTemplate()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);
        var inactiveTemplate = await CreateTemplateAsync(db, seed.AdminId, active: false);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { maMauBangKhen = inactiveTemplate.MaMauBangKhen });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GenerateCertificates_ShouldBeIdempotentWithoutForce()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);

        using var first = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(first));
        await using var afterFirstDb = CreateDbContext();
        var rewardAfterFirst = await afterFirstDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == seed.CampaignId);

        using var second = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(second));
        using var root = await GetRootAsync(second);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "successCount"), Is.EqualTo(0));
        Assert.That(GetInt32(data, "skippedCount"), Is.EqualTo(1));

        await using var verifyDb = CreateDbContext();
        var rewardAfterSecond = await verifyDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaKhenThuong == rewardAfterFirst.MaKhenThuong);
        Assert.That(rewardAfterSecond.UrlPdfBangKhen, Is.EqualTo(rewardAfterFirst.UrlPdfBangKhen));
        Assert.That(rewardAfterSecond.SoLanSinhPdf, Is.EqualTo(1));
    }

    [Test]
    public async Task RegenerateCertificates_ShouldCreateNewPdfAndIncrementAttempt()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);

        using var first = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(first));
        await using var afterFirstDb = CreateDbContext();
        var before = await afterFirstDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == seed.CampaignId);
        await Task.Delay(5);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/regenerate",
            new { reason = "RD6 test regenerate" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var after = await verifyDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaKhenThuong == before.MaKhenThuong);
        Assert.That(after.UrlPdfBangKhen, Is.Not.EqualTo(before.UrlPdfBangKhen));
        Assert.That(after.SoLanSinhPdf, Is.EqualTo(2));
    }

    [Test]
    public async Task GenerateCertificates_OnlyFailed_ShouldGenerateFailedOrMissingOnly()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 2);

        using var first = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(first));

        await using (var mutateDb = CreateDbContext())
        {
            var failed = await mutateDb.KhenThuongs.FirstAsync(x => x.MaKhenThuong == seed.RewardIds[1]);
            failed.TrangThai = RewardDisciplineConstants.RewardStatuses.PdfFailed;
            failed.UrlPdfBangKhen = null;
            failed.LoiSinhPdf = "Simulated failure";
            await mutateDb.SaveChangesAsync();
        }

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { onlyFailed = true });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "successCount"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "skippedCount"), Is.EqualTo(1));
    }

    [Test]
    public async Task GenerateCertificates_ShouldContinueBatchWhenOneRewardFails()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 2);
        var invalidTemplate = await CreateTemplateAsync(db, seed.AdminId, active: true, configJson: InvalidRenderConfigJson);
        var failingReward = await db.KhenThuongs.FirstAsync(x => x.MaKhenThuong == seed.RewardIds[1]);
        failingReward.MaMauBangKhen = invalidTemplate.MaMauBangKhen;
        await db.SaveChangesAsync();

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "successCount"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "failedCount"), Is.EqualTo(1));

        await using var verifyDb = CreateDbContext();
        var failed = await verifyDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaKhenThuong == failingReward.MaKhenThuong);
        Assert.That(failed.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardStatuses.PdfFailed));
        Assert.That(failed.LoiSinhPdf, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ListCertificates_ShouldReturnPagedFilteredResults()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 2);
        using var generate = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(generate.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(generate));

        using var response = await Client.GetAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates?trangThaiPdf=da_sinh_pdf&pageIndex=1&pageSize=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "totalItems"), Is.EqualTo(2));
        Assert.That(GetDataItems(root.RootElement).GetArrayLength(), Is.EqualTo(1));
    }

    [Test]
    public async Task DownloadCertificate_ShouldReturnPdfWithSecureHeaders()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);
        using var generate = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        Assert.That(generate.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(generate));

        using var response = await Client.GetAsync($"api/admin/rewards/{seed.RewardIds[0]}/certificate/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
        Assert.That(response.Headers.TryGetValues("X-Content-Type-Options", out var nosniff), Is.True);
        Assert.That(nosniff!.Single(), Is.EqualTo("nosniff"));
        Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        Assert.That(response.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.That(bytes.Length, Is.GreaterThan(100));
        Assert.That(System.Text.Encoding.ASCII.GetString(bytes.Take(5).ToArray()), Is.EqualTo("%PDF-"));
    }

    [Test]
    public async Task NonSuperAdminCannotGenerateOrRegenerateCertificates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);
        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);

        using var generate = await adminClient.PostAsJsonAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate", new { });
        using var regenerate = await adminClient.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/regenerate",
            new { reason = "not allowed" });

        Assert.That(generate.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(generate));
        Assert.That(regenerate.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(regenerate));
    }

    [Test]
    public async Task AdminCannotReadGlobalCampaignCertificates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1, globalCampaign: true);
        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);

        using var list = await adminClient.GetAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates");
        using var download = await adminClient.GetAsync($"api/admin/rewards/{seed.RewardIds[0]}/certificate/download");

        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(list));
        Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(download));
    }

    [Test]
    public async Task GenerateCertificates_ShouldNotDuplicateRewards()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 2);
        var beforeCount = await db.KhenThuongs.CountAsync(x => x.MaDotKhenThuong == seed.CampaignId);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { forceRegenerate = true });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var afterCount = await verifyDb.KhenThuongs.CountAsync(x => x.MaDotKhenThuong == seed.CampaignId);
        Assert.That(afterCount, Is.EqualTo(beforeCount));
    }

    [Test]
    public async Task DownloadCertificate_ShouldReturn404WhenPdfMissing()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);

        using var response = await Client.GetAsync($"api/admin/rewards/{seed.RewardIds[0]}/certificate/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GenerateCertificates_ShouldWriteAuditLog()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { ghiChu = "RD6 audit" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var exists = await verifyDb.NhatKyKiemToans.AsNoTracking().AnyAsync(x =>
            x.LoaiDoiTuong == "DotKhenThuong" &&
            x.MaDoiTuong == seed.CampaignId.ToString() &&
            x.HanhDong == RewardDisciplineConstants.RewardAuditActions.GenerateRewardCertificates);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task UnauthorizedCannotListCertificates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateApprovedCampaignSeedAsync(db, rewardCount: 1);
        using var anonymous = new HttpClient { BaseAddress = BaseUri };

        using var response = await anonymous.GetAsync($"api/admin/reward-campaigns/{seed.CampaignId}/certificates");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var token = GetRequiredString(root.RootElement, "accessToken");
        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<RD6Seed> CreateApprovedCampaignSeedAsync(
        ApplicationDbContext db,
        int rewardCount,
        string campaignStatus = RewardDisciplineConstants.RewardCampaignStatuses.Approved,
        bool globalCampaign = false)
    {
        var adminId = await GetSuperAdminIdAsync(db);
        var organization = new DonVi
        {
            TenDonVi = $"RD6 Org {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true
        };
        db.DonVis.Add(organization);
        await db.SaveChangesAsync();

        var term = new HocKy
        {
            MaDonVi = organization.MaDonVi,
            TenHocKy = $"RD6 Term {Guid.NewGuid():N}",
            MaCodeHocKy = $"RD6_{Guid.NewGuid().ToString("N")[..8]}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(4))
        };
        db.HocKys.Add(term);
        await db.SaveChangesAsync();

        var template = await CreateTemplateAsync(db, adminId, active: true);
        var campaign = new DotKhenThuong
        {
            MaDonVi = globalCampaign ? null : organization.MaDonVi,
            MaHocKy = term.MaHocKy,
            TenDot = $"RD6 approved campaign {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            TrangThai = campaignStatus,
            SoLuongToiDa = Math.Max(1, rewardCount),
            MaMauBangKhen = template.MaMauBangKhen,
            NguoiTao = adminId,
            NguoiDuyet = adminId,
            NgayDuyet = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();

        var rewardIds = new List<int>();
        for (var index = 1; index <= rewardCount; index++)
        {
            var student = await CreateStudentAsync(db, organization.MaDonVi, index);
            var reward = new KhenThuong
            {
                MaDonVi = organization.MaDonVi,
                MaHocSinh = student.MaNguoiDung,
                MaHocKy = term.MaHocKy,
                MaDotKhenThuong = campaign.MaDotKhenThuong,
                MaMauBangKhen = template.MaMauBangKhen,
                LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
                TrangThai = RewardDisciplineConstants.RewardStatuses.Approved,
                GpaDatDuoc = 9.0m + index / 100m,
                DiemXet = 9.0m + index / 100m,
                XepHang = index,
                UrlChungTu = string.Empty,
                HoTenSnapshot = student.HoTen,
                MssvSnapshot = $"SVRD6{index:000000}",
                TenHocKySnapshot = term.TenHocKy,
                DanhHieuSnapshot = "Top 100 học kỳ",
                CapLuc = DateTime.UtcNow,
                NguoiCap = adminId,
                NguoiDuyet = adminId,
                NgayCapNhat = DateTime.UtcNow,
                DaHuy = false
            };
            db.KhenThuongs.Add(reward);
            await db.SaveChangesAsync();
            rewardIds.Add(reward.MaKhenThuong);
        }

        return new RD6Seed(campaign.MaDotKhenThuong, template.MaMauBangKhen, adminId, rewardIds);
    }

    private static async Task<MauBangKhen> CreateTemplateAsync(
        ApplicationDbContext db,
        int adminId,
        bool active,
        string? configJson = null)
    {
        var template = new MauBangKhen
        {
            TenMau = $"RD6 template {Guid.NewGuid():N}",
            LoaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            FileNenUrl = "https://example.test/rd6-template.png",
            ChieuRong = 3508,
            ChieuCao = 2480,
            HuongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
            CauHinhJson = configJson ?? ValidConfigJson,
            ConHoatDong = active,
            NguoiTao = adminId,
            NgayTao = DateTime.UtcNow
        };
        db.MauBangKhens.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    private static async Task<NguoiDung> CreateStudentAsync(ApplicationDbContext db, int maDonVi, int index)
    {
        var student = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"RD6 Student {index} {Guid.NewGuid():N}",
            Email = $"rd6_student_{Guid.NewGuid():N}@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();
        return student;
    }

    private static async Task<int> GetSuperAdminIdAsync(ApplicationDbContext db)
    {
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == "superadmin@lms.local")
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static bool CertificateFileExists(string url)
    {
        var fileName = Path.GetFileName(url.Replace('\\', '/'));
        return File.Exists(Path.Combine(GetCertificateStorageRoot(), fileName));
    }

    private static void ValidateCertificateStorageRoot()
    {
        _ = GetCertificateStorageRoot();
        var provider = Environment.GetEnvironmentVariable("CertificateStorage__Provider");
        if (!string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("RD6 tests yêu cầu CertificateStorage__Provider=Local.");
        }
    }

    private static string GetCertificateStorageRoot()
    {
        var root = Environment.GetEnvironmentVariable("CertificateStorage__LocalRoot");
        if (string.IsNullOrWhiteSpace(root))
        {
            Assert.Fail("Thiếu env var CertificateStorage__LocalRoot cho RD6 tests.");
        }

        var fullPath = Path.GetFullPath(root!);
        var leaf = new DirectoryInfo(fullPath).Name;
        if (!leaf.Contains("LMS_TEST", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("CertificateStorage__LocalRoot phải là thư mục isolated có tên chứa LMS_TEST.");
        }

        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record RD6Seed(int CampaignId, int TemplateId, int AdminId, IReadOnlyList<int> RewardIds);
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD2")]
public class RD2_RewardCampaignTop100Tests : ApiTestBase
{
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string AdminEmail = "admin@lms.local";
    private const string CampusAdminEmail = "campusadmin.hcm@lms.local";

    [Test]
    public async Task CreateTop100Campaign_ShouldSucceed()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var templateId = await CreateTemplateAsync(db, seed.AdminId, active: true);

        using var response = await Client.PostAsJsonAsync("api/admin/reward-campaigns/top100", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = $"RD2 Top 100 {Guid.NewGuid():N}",
            soLuongToiDa = 100,
            tieuChiXetJson = new { minGpa = 8.5, rankBy = "gpa" },
            maMauBangKhen = templateId,
            ghiChu = "RD2 create success"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Draft));
        Assert.That(GetInt32(data, "soLuongToiDa"), Is.EqualTo(100));
        Assert.That(GetInt32(data, "maMauBangKhen"), Is.EqualTo(templateId));
    }

    [Test]
    public async Task ListRewardCampaigns_ShouldReturnPagedItems()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);

        using var response = await Client.GetAsync($"api/admin/reward-campaigns?maHocKy={seed.MaHocKy}&pageIndex=1&pageSize=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement);
        Assert.That(items.EnumerateArray().Any(x => GetInt32(x, "maDotKhenThuong") == campaign.MaDotKhenThuong), Is.True);
    }

    [Test]
    public async Task DetailRewardCampaign_ShouldReturnDetail()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, criteriaJson: """{"minGpa":8.7}""");

        using var response = await Client.GetAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maDotKhenThuong"), Is.EqualTo(campaign.MaDotKhenThuong));
        Assert.That(GetRequiredProperty(data, "hocKy").ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(GetRequiredProperty(data, "donVi").ValueKind, Is.EqualTo(JsonValueKind.Object));
    }

    [Test]
    public async Task CreateTop100Campaign_DuplicateActive_ShouldReturn409()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        await CreateCampaignAsync(db, seed);

        using var response = await Client.PostAsJsonAsync("api/admin/reward-campaigns/top100", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = $"RD2 duplicate {Guid.NewGuid():N}",
            soLuongToiDa = 100
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateTop100Campaign_InvalidCriteriaJson_ShouldReturn400()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);

        using var response = await Client.PostAsJsonAsync("api/admin/reward-campaigns/top100", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = $"RD2 invalid criteria {Guid.NewGuid():N}",
            soLuongToiDa = 100,
            tieuChiXetJson = new[] { "not-object" }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateTop100Campaign_InactiveTemplate_ShouldReturn400()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var templateId = await CreateTemplateAsync(db, seed.AdminId, active: false);

        using var response = await Client.PostAsJsonAsync("api/admin/reward-campaigns/top100", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = $"RD2 inactive template {Guid.NewGuid():N}",
            soLuongToiDa = 100,
            maMauBangKhen = templateId
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UpdateDraftCampaign_ShouldSucceed()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);

        using var response = await Client.PutAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = $"RD2 updated {Guid.NewGuid():N}",
            soLuongToiDa = 88,
            tieuChiXetJson = new { minGpa = 8.8 },
            ghiChu = "Updated by RD2 test"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "soLuongToiDa"), Is.EqualTo(88));
        Assert.That(GetRequiredString(data, "ghiChu"), Is.EqualTo("Updated by RD2 test"));
    }

    [Test]
    public async Task UpdateNonDraftCampaign_ShouldReturn409()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, status: RewardDisciplineConstants.RewardCampaignStatuses.Evaluating);

        using var response = await Client.PutAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = "RD2 should fail",
            soLuongToiDa = 50
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CancelCampaign_ShouldSetCancelledAndAudit()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, status: RewardDisciplineConstants.RewardCampaignStatuses.Approved);

        using var response = await Client.PatchAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/cancel", new
        {
            lyDoHuy = "RD2 cancel reason"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var updated = await verifyDb.DotKhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong);
        Assert.That(updated.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Cancelled));
        Assert.That(updated.GhiChu, Does.Contain("RD2 cancel reason"));

        var hasAudit = await verifyDb.NhatKyKiemToans.AsNoTracking()
            .AnyAsync(x =>
                x.LoaiDoiTuong == "DotKhenThuong" &&
                x.MaDoiTuong == campaign.MaDotKhenThuong.ToString() &&
                x.HanhDong == "CANCEL_REWARD_CAMPAIGN");
        Assert.That(hasAudit, Is.True);
    }

    [Test]
    public async Task CancelPublishedCampaign_ShouldReturn400()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, status: RewardDisciplineConstants.RewardCampaignStatuses.Published);

        using var response = await Client.PatchAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/cancel", new
        {
            lyDoHuy = "Không được hủy đã công bố"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CampusAdmin_ShouldNotSeeGlobalCampaign()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var globalCampaign = await CreateCampaignAsync(db, seed with { MaDonVi = null });

        using var campusAdmin = await CreateAuthenticatedClientAsync(CampusAdminEmail);
        using var response = await campusAdmin.GetAsync($"api/admin/reward-campaigns/{globalCampaign.MaDotKhenThuong}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AdminCreateTop100Campaign_ShouldReturn403()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        using var admin = await CreateAuthenticatedClientAsync(AdminEmail);

        using var response = await admin.PostAsJsonAsync("api/admin/reward-campaigns/top100", new
        {
            maHocKy = seed.MaHocKy,
            maDonVi = seed.MaDonVi,
            tenDot = "RD2 admin forbidden",
            soLuongToiDa = 100
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UnauthenticatedList_ShouldReturn401()
    {
        using var anonymous = new HttpClient { BaseAddress = BaseUri };

        using var response = await anonymous.GetAsync("api/admin/reward-campaigns");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await LoginAsync(email);
        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> LoginAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return GetRequiredString(root.RootElement, "accessToken");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<RD2Seed> CreateSeedAsync(ApplicationDbContext db)
    {
        var admin = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == SuperAdminEmail)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstOrDefaultAsync();
        Assert.That(admin, Is.Not.Null, "Thiếu seed superadmin@lms.local.");

        var campus = await db.DonVis.AsNoTracking()
            .Where(x => x.ConHoatDong && x.MaDonVi == admin!.MaDonVi)
            .Select(x => new { x.MaDonVi })
            .FirstOrDefaultAsync();
        if (campus is null)
        {
            campus = await db.DonVis.AsNoTracking()
                .Where(x => x.ConHoatDong)
                .OrderBy(x => x.MaDonVi)
                .Select(x => new { x.MaDonVi })
                .FirstOrDefaultAsync();
        }

        Assert.That(campus, Is.Not.Null, "Thiếu đơn vị đang hoạt động để tạo dữ liệu RD2.");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var term = new HocKy
        {
            MaDonVi = campus!.MaDonVi,
            MaCodeHocKy = $"RD2{suffix}",
            TenHocKy = $"RD2 học kỳ {suffix}",
            NamHoc = $"RD2-{suffix}",
            ThuTuTrongNam = 1,
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3)),
            DaKhoa = false
        };

        db.HocKys.Add(term);
        await db.SaveChangesAsync();
        return new RD2Seed(admin!.MaNguoiDung, campus.MaDonVi, term.MaHocKy);
    }

    private static async Task<int> CreateTemplateAsync(ApplicationDbContext db, int adminId, bool active)
    {
        var template = new MauBangKhen
        {
            TenMau = $"RD2 mẫu Top 100 {Guid.NewGuid():N}",
            LoaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            FileNenUrl = "https://example.test/rd2-certificate.png",
            ChieuRong = 3508,
            ChieuCao = 2480,
            HuongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
            CauHinhJson = """{"fields":[{"key":"studentName","x":100,"y":100}]}""",
            ConHoatDong = active,
            NguoiTao = adminId,
            NgayTao = DateTime.UtcNow
        };

        db.MauBangKhens.Add(template);
        await db.SaveChangesAsync();
        return template.MaMauBangKhen;
    }

    private static async Task<DotKhenThuong> CreateCampaignAsync(
        ApplicationDbContext db,
        RD2Seed seed,
        string status = RewardDisciplineConstants.RewardCampaignStatuses.Draft,
        string? criteriaJson = null)
    {
        var campaign = new DotKhenThuong
        {
            MaHocKy = seed.MaHocKy,
            MaDonVi = seed.MaDonVi,
            TenDot = $"RD2 Top 100 direct {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            TieuChiXetJson = criteriaJson,
            TrangThai = status,
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        };

        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    private sealed record RD2Seed(int AdminId, int? MaDonVi, int MaHocKy);
}

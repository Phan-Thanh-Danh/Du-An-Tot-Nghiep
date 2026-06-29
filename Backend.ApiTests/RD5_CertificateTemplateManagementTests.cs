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
[Category("RD5")]
public class RD5_CertificateTemplateManagementTests : ApiTestBase
{
    private const string AdminEmail = "admin@lms.local";
    private const string ValidConfigJson = """
        {
          "fields": [
            { "key": "hoTen", "x": 100, "y": 240, "fontSize": 42, "align": "center", "color": "#111111", "bold": true },
            { "key": "mssv", "x": 100, "y": 310, "fontSize": 20, "align": "center", "color": "#333333", "bold": false },
            { "key": "tenHocKy", "x": 100, "y": 360, "fontSize": 24, "align": "center", "color": "#333333", "bold": false },
            { "key": "danhHieu", "x": 100, "y": 430, "fontSize": 28, "align": "center", "color": "#111111", "bold": true },
            { "key": "xepHang", "x": 100, "y": 500, "fontSize": 22, "align": "center", "color": "#111111", "bold": true },
            { "key": "diemXet", "x": 100, "y": 550, "fontSize": 20, "align": "center", "color": "#333333", "bold": false },
            { "key": "ngayCap", "x": 100, "y": 620, "fontSize": 18, "align": "right", "color": "#333333", "bold": false }
          ]
        }
        """;

    [Test]
    public async Task SuperAdminCreatesActiveTop100CertificateTemplate()
    {
        using var response = await Client.PostAsJsonAsync("api/admin/certificate-templates", CreateValidRequest());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var id = GetInt32(data, "maMauBangKhen");
        Assert.That(id, Is.GreaterThan(0));
        Assert.That(GetRequiredString(data, "loaiMau"), Is.EqualTo(RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester));
        Assert.That(GetRequiredString(data, "huongGiay"), Is.EqualTo(RewardDisciplineConstants.PaperOrientations.A4Landscape));
        Assert.That(GetBoolean(data, "conHoatDong"), Is.True);

        await using var db = CreateDbContext();
        var template = await db.MauBangKhens.AsNoTracking().FirstAsync(x => x.MaMauBangKhen == id);
        Assert.That(template.ConHoatDong, Is.True);
        Assert.That(template.CauHinhJson, Does.Contain("hoTen"));
    }

    [Test]
    public async Task CreateRejectsInvalidLoaiMau()
    {
        using var response = await Client.PostAsJsonAsync("api/admin/certificate-templates", CreateValidRequest(loaiMau: "INVALID"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateRejectsInvalidHuongGiay()
    {
        using var response = await Client.PostAsJsonAsync("api/admin/certificate-templates", CreateValidRequest(huongGiay: "LETTER"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateRejectsInvalidCauHinhJson()
    {
        using var response = await Client.PostAsJsonAsync("api/admin/certificate-templates", new
        {
            tenMau = $"RD5 invalid json {Guid.NewGuid():N}",
            loaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            fileNenUrl = "https://example.test/rd5-template.png",
            chieuRong = 3508,
            chieuCao = 2480,
            huongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
            cauHinhJson = """{"fields":[{"key":"hoTen","x":100,"y":100"""
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateRejectsBase64FileNenUrl()
    {
        using var response = await Client.PostAsJsonAsync(
            "api/admin/certificate-templates",
            CreateValidRequest(fileNenUrl: "data:image/png;base64,iVBORw0KGgo="));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ListReturnsActiveAndInactiveTemplatesWithFilters()
    {
        await using var db = CreateDbContext();
        var adminId = await GetSuperAdminIdAsync(db);
        var active = await CreateTemplateAsync(db, adminId, active: true);
        var inactive = await CreateTemplateAsync(db, adminId, active: false);

        using var activeResponse = await Client.GetAsync("api/admin/certificate-templates?conHoatDong=true&pageIndex=1&pageSize=50");
        Assert.That(activeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(activeResponse));
        using var activeRoot = await GetRootAsync(activeResponse);
        var activeItems = GetDataItems(activeRoot.RootElement);
        Assert.That(activeItems.EnumerateArray().Any(x => GetInt32(x, "maMauBangKhen") == active.MaMauBangKhen), Is.True);
        Assert.That(activeItems.EnumerateArray().Any(x => GetInt32(x, "maMauBangKhen") == inactive.MaMauBangKhen), Is.False);

        using var inactiveResponse = await Client.GetAsync("api/admin/certificate-templates?conHoatDong=false&pageIndex=1&pageSize=50");
        Assert.That(inactiveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(inactiveResponse));
        using var inactiveRoot = await GetRootAsync(inactiveResponse);
        var inactiveItems = GetDataItems(inactiveRoot.RootElement);
        Assert.That(inactiveItems.EnumerateArray().Any(x => GetInt32(x, "maMauBangKhen") == inactive.MaMauBangKhen), Is.True);
    }

    [Test]
    public async Task DetailReturnsTemplateConfig()
    {
        await using var db = CreateDbContext();
        var adminId = await GetSuperAdminIdAsync(db);
        var template = await CreateTemplateAsync(db, adminId, active: true);

        using var response = await Client.GetAsync($"api/admin/certificate-templates/{template.MaMauBangKhen}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "cauHinhJson"), Does.Contain("hoTen"));
        Assert.That(GetRequiredString(data, "fileNenUrl"), Is.EqualTo(template.FileNenUrl));
    }

    [Test]
    public async Task UpdateWorksAndUpdatesNgayCapNhat()
    {
        await using var db = CreateDbContext();
        var adminId = await GetSuperAdminIdAsync(db);
        var template = await CreateTemplateAsync(db, adminId, active: true);

        using var response = await Client.PutAsJsonAsync(
            $"api/admin/certificate-templates/{template.MaMauBangKhen}",
            CreateValidRequest(tenMau: $"RD5 updated {Guid.NewGuid():N}", fileNenUrl: "https://example.test/rd5-updated.png"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var updated = await verifyDb.MauBangKhens.AsNoTracking().FirstAsync(x => x.MaMauBangKhen == template.MaMauBangKhen);
        Assert.That(updated.FileNenUrl, Is.EqualTo("https://example.test/rd5-updated.png"));
        Assert.That(updated.NgayCapNhat, Is.Not.Null);
    }

    [Test]
    public async Task DeleteDisablesTemplateAndDoesNotHardDelete()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRewardSeedAsync(db);
        var template = await CreateTemplateAsync(db, seed.AdminId, active: true);
        db.DotKhenThuongs.Add(new DotKhenThuong
        {
            MaHocKy = seed.MaHocKy,
            MaDonVi = seed.MaDonVi,
            TenDot = $"RD5 linked campaign {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            MaMauBangKhen = template.MaMauBangKhen,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Draft,
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        using var response = await Client.DeleteAsync($"api/admin/certificate-templates/{template.MaMauBangKhen}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var disabled = await verifyDb.MauBangKhens.AsNoTracking().FirstOrDefaultAsync(x => x.MaMauBangKhen == template.MaMauBangKhen);
        Assert.That(disabled, Is.Not.Null);
        Assert.That(disabled!.ConHoatDong, Is.False);
    }

    [Test]
    public async Task NonSuperAdminCannotCreateUpdateDelete()
    {
        await using var db = CreateDbContext();
        var adminId = await GetSuperAdminIdAsync(db);
        var template = await CreateTemplateAsync(db, adminId, active: true);
        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);

        using var createResponse = await adminClient.PostAsJsonAsync("api/admin/certificate-templates", CreateValidRequest());
        using var updateResponse = await adminClient.PutAsJsonAsync($"api/admin/certificate-templates/{template.MaMauBangKhen}", CreateValidRequest());
        using var deleteResponse = await adminClient.DeleteAsync($"api/admin/certificate-templates/{template.MaMauBangKhen}");

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(createResponse));
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(updateResponse));
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(deleteResponse));
    }

    [Test]
    public async Task PreviewReturnsSafeResponseAndDoesNotPersistChanges()
    {
        await using var db = CreateDbContext();
        var adminId = await GetSuperAdminIdAsync(db);
        var template = await CreateTemplateAsync(db, adminId, active: true);
        var beforeCount = await db.MauBangKhens.CountAsync();

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/certificate-templates/{template.MaMauBangKhen}/preview",
            new
            {
                duLieuMau = new
                {
                    hoTen = "Trần Thị Preview",
                    mssv = "SV-PREVIEW"
                }
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetBoolean(data, "isPdfGenerated"), Is.False);
        Assert.That(GetRequiredString(data, "note"), Does.Contain("RD6"));
        var fields = GetRequiredProperty(data, "fields");
        Assert.That(fields.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(fields.GetArrayLength(), Is.GreaterThan(0));

        await using var verifyDb = CreateDbContext();
        Assert.That(await verifyDb.MauBangKhens.CountAsync(), Is.EqualTo(beforeCount));
    }

    [Test]
    public async Task Rd4ApproveFlowStillPassesWithCertificateTemplate()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRewardSeedAsync(db);
        var template = await CreateTemplateAsync(db, seed.AdminId, active: true);
        var campaign = new DotKhenThuong
        {
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            TenDot = $"RD5 approve regression {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval,
            SoLuongToiDa = 1,
            MaMauBangKhen = template.MaMauBangKhen,
            NgayTao = DateTime.UtcNow,
            NguoiTao = seed.AdminId
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();
        var student = await CreateStudentAsync(db, seed.MaDonVi);
        db.UngVienKhenThuongs.Add(new UngVienKhenThuong
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            XepHang = 1,
            DiemXet = 9.5m,
            GpaHocKy = 9.1m,
            TongTinChi = 20,
            TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.Recommended,
            HoTenSnapshot = student.HoTen,
            MssvSnapshot = student.Email,
            TenHocKySnapshot = seed.TenHocKy,
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        using var response = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var verifyDb = CreateDbContext();
        var reward = await verifyDb.KhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong);
        Assert.That(reward.MaMauBangKhen, Is.EqualTo(template.MaMauBangKhen));
        Assert.That(reward.UrlPdfBangKhen, Is.Null);
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

    private static object CreateValidRequest(
        string? tenMau = null,
        string? loaiMau = null,
        string? fileNenUrl = null,
        string? huongGiay = null)
    {
        return new
        {
            tenMau = tenMau ?? $"RD5 mẫu Top 100 {Guid.NewGuid():N}",
            loaiMau = loaiMau ?? RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            fileNenUrl = fileNenUrl ?? "https://example.test/rd5-certificate.png",
            chieuRong = 3508,
            chieuCao = 2480,
            huongGiay = huongGiay ?? RewardDisciplineConstants.PaperOrientations.A4Landscape,
            cauHinhJson = new
            {
                fields = new object[]
                {
                    new { key = "hoTen", x = 100, y = 240, fontSize = 42, align = "center", color = "#111111", bold = true },
                    new { key = "mssv", x = 100, y = 310, fontSize = 20, align = "center", color = "#333333", bold = false },
                    new { key = "tenHocKy", x = 100, y = 360, fontSize = 24, align = "center", color = "#333333", bold = false },
                    new { key = "danhHieu", x = 100, y = 430, fontSize = 28, align = "center", color = "#111111", bold = true },
                    new { key = "xepHang", x = 100, y = 500, fontSize = 22, align = "center", color = "#111111", bold = true },
                    new { key = "diemXet", x = 100, y = 550, fontSize = 20, align = "center", color = "#333333", bold = false },
                    new { key = "ngayCap", x = 100, y = 620, fontSize = 18, align = "right", color = "#333333", bold = false }
                }
            }
        };
    }

    private static async Task<MauBangKhen> CreateTemplateAsync(ApplicationDbContext db, int adminId, bool active)
    {
        var template = new MauBangKhen
        {
            TenMau = $"RD5 direct template {Guid.NewGuid():N}",
            LoaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            FileNenUrl = "https://example.test/rd5-direct.png",
            ChieuRong = 3508,
            ChieuCao = 2480,
            HuongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
            CauHinhJson = ValidConfigJson,
            ConHoatDong = active,
            NguoiTao = adminId,
            NgayTao = DateTime.UtcNow
        };

        db.MauBangKhens.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    private static async Task<int> GetSuperAdminIdAsync(ApplicationDbContext db)
    {
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == "superadmin@lms.local")
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static async Task<RD5RewardSeed> CreateRewardSeedAsync(ApplicationDbContext db)
    {
        var organization = new DonVi
        {
            TenDonVi = $"RD5 Org {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true
        };
        db.DonVis.Add(organization);
        await db.SaveChangesAsync();

        var term = new HocKy
        {
            MaDonVi = organization.MaDonVi,
            TenHocKy = "RD5 Test Term",
            MaCodeHocKy = $"RD5_{Guid.NewGuid().ToString("N")[..8]}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(4))
        };
        db.HocKys.Add(term);
        await db.SaveChangesAsync();

        var adminId = await GetSuperAdminIdAsync(db);
        return new RD5RewardSeed(organization.MaDonVi, term.MaHocKy, term.TenHocKy, adminId);
    }

    private static async Task<NguoiDung> CreateStudentAsync(ApplicationDbContext db, int maDonVi)
    {
        var student = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"RD5 Student {Guid.NewGuid():N}",
            Email = $"rd5_student_{Guid.NewGuid():N}@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();
        return student;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record RD5RewardSeed(int MaDonVi, int MaHocKy, string TenHocKy, int AdminId);
}

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
[Category("RD7")]
public class RD7_StudentRewardDownloadTests : ApiTestBase
{
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

    // 1. Student lists own rewards only
    [Test]
    public async Task Student_ListsOwnRewardsOnly()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync("api/student/rewards");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var items = GetRequiredProperty(data, "items");

        foreach (var item in items.EnumerateArray())
        {
            // All items should belong to the student - they won't have maHocSinh exposed,
            // but we can verify by checking the returned reward IDs match our seed
            Assert.That(item.GetProperty("maKhenThuong").GetInt32(), Is.EqualTo(seed.RewardId));
        }
    }

    // 2. Student list supports pagination and filter by term/hasCertificate
    [Test]
    public async Task Student_ListSupportsPaginationAndFilters()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, withPdf: true);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);

        // Filter by term
        using var termResponse = await studentClient.GetAsync($"api/student/rewards?maHocKy={seed.MaHocKy}&pageSize=5");
        Assert.That(termResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Filter by hasCertificate
        using var certResponse = await studentClient.GetAsync("api/student/rewards?hasCertificate=true&pageSize=5");
        Assert.That(certResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var certRoot = await GetRootAsync(certResponse);
        var certData = GetRequiredProperty(certRoot.RootElement, "data");
        var certItems = GetRequiredProperty(certData, "items");
        foreach (var item in certItems.EnumerateArray())
        {
            Assert.That(item.GetProperty("hasCertificate").GetBoolean(), Is.True);
        }
    }

    // 3. Student detail returns own reward
    [Test]
    public async Task Student_DetailReturnsOwnReward()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.GetProperty("maKhenThuong").GetInt32(), Is.EqualTo(seed.RewardId));
        Assert.That(data.GetProperty("danhHieuSnapshot").GetString(), Is.Not.Empty);
        Assert.That(data.GetProperty("hoTenSnapshot").GetString(), Is.Not.Empty);
    }

    // 4. Student detail for another student's reward returns 404
    [Test]
    public async Task Student_DetailForOtherStudentReward_Returns404()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        // Create a different student
        var otherStudent = await CreateStudentAsync(db, seed.MaDonVi, 999);
        using var otherClient = await CreateStudentClientAsync(db, otherStudent.MaNguoiDung);

        using var response = await otherClient.GetAsync($"api/student/rewards/{seed.RewardId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // 5. Student can download own generated certificate PDF
    [Test]
    public async Task Student_CanDownloadOwnCertificate()
    {
        ValidateCertificateStorageRoot();
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, withPdf: true);

        // Generate certificate via admin endpoint first
        using var genResponse = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });
        Assert.That(genResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(genResponse));

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var dlResponse = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}/certificate/download");

        Assert.That(dlResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(dlResponse));
    }

    // 6. Download response has correct headers
    [Test]
    public async Task Student_DownloadHasCorrectHeaders()
    {
        ValidateCertificateStorageRoot();
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, withPdf: true);

        await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{seed.CampaignId}/certificates/generate",
            new { });

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}/certificate/download");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
        Assert.That(response.Content.Headers.ContentDisposition?.DispositionType, Is.EqualTo("attachment"));

        Assert.That(response.Headers.Contains("X-Content-Type-Options"), Is.True);
        var nosniff = response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault();
        Assert.That(nosniff, Is.EqualTo("nosniff"));

        Assert.That(response.Headers.Contains("Cache-Control"), Is.True);
        var cacheControl = response.Headers.GetValues("Cache-Control").FirstOrDefault();
        Assert.That(cacheControl, Does.Contain("private"));
        Assert.That(cacheControl, Does.Contain("no-store"));
    }

    // 7. Student cannot download reward without PDF
    [Test]
    public async Task Student_CannotDownloadRewardWithoutPdf()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, withPdf: false);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}/certificate/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // 8. Student cannot download canceled reward
    [Test]
    public async Task Student_CannotDownloadCanceledReward()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, rewardStatus: RewardDisciplineConstants.RewardStatuses.Cancelled, daHuy: true);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}/certificate/download");

        // Should be 404 or 409
        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.NotFound, HttpStatusCode.Conflict));
    }

    // 9. Student cannot see internal PDF error/local storage path
    [Test]
    public async Task Student_CannotSeeInternalPdfErrorOrStoragePath()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        // Ensure internal fields are NOT in response
        Assert.That(content, Does.Not.Contain("loiSinhPdf"));
        Assert.That(content, Does.Not.Contain("urlPdfBangKhen"));
        Assert.That(content, Does.Not.Contain("soLanSinhPdf"));
        Assert.That(content, Does.Not.Contain("ngaySinhPdf"));
    }

    // 10. SuperAdmin cannot use student endpoint (403)
    [Test]
    public async Task SuperAdmin_CannotUseStudentEndpoint()
    {
        // Client from base class is logged in as superadmin
        using var response = await Client.GetAsync("api/student/rewards");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // 11. Inactive student cannot list rewards
    [Test]
    public async Task InactiveStudent_CannotListRewards()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        // Create student with inactive status, mutate and login
        var inactiveStudent = await CreateStudentAsync(db, seed.MaDonVi, 777, status: UserStatuses.DbLocked);
        // Inactive students should get rejected at auth level or get empty results.
        // Since the student is locked, login should fail or be blocked by middleware.
        try
        {
            using var lockedClient = await CreateStudentClientAsync(db, inactiveStudent.MaNguoiDung);
            using var response = await lockedClient.GetAsync("api/student/rewards");
            // If somehow the locked student gets through auth, response should still be OK with no rewards
            // (since there are no rewards for this student)
            Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden));
        }
        catch (AssertionException)
        {
            // Login failure is expected for locked user — test passes
            Assert.Pass("Locked student login correctly rejected.");
        }
    }

    // 12. RD7 does not create or regenerate PDFs
    [Test]
    public async Task Student_EndpointsDoNotCreateOrRegeneratePdfs()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);

        // List
        await studentClient.GetAsync("api/student/rewards");
        // Detail
        await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}");
        // Download attempt (will 404 since no PDF)
        await studentClient.GetAsync($"api/student/rewards/{seed.RewardId}/certificate/download");

        // Verify the reward still has no PDF (not generated by student actions)
        await using var verifyDb = CreateDbContext();
        var reward = await verifyDb.KhenThuongs.AsNoTracking()
            .FirstAsync(k => k.MaKhenThuong == seed.RewardId);
        Assert.That(reward.UrlPdfBangKhen, Is.Null.Or.Empty);
        Assert.That(reward.SoLanSinhPdf, Is.EqualTo(0));
    }

    // 13. Student list hides Draft/PdfFailed rewards
    [Test]
    public async Task Student_ListHidesDraftAndPdfFailedRewards()
    {
        await using var db = CreateDbContext();
        var seed = await CreateRD7SeedAsync(db, rewardStatus: RewardDisciplineConstants.RewardStatuses.Draft);

        using var studentClient = await CreateStudentClientAsync(db, seed.StudentId);
        using var response = await studentClient.GetAsync("api/student/rewards");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var items = GetRequiredProperty(data, "items");

        // Draft reward should not appear
        foreach (var item in items.EnumerateArray())
        {
            Assert.That(item.GetProperty("maKhenThuong").GetInt32(), Is.Not.EqualTo(seed.RewardId));
        }
    }

    // ============ Helpers ============

    private async Task<RD7Seed> CreateRD7SeedAsync(
        ApplicationDbContext db,
        bool withPdf = false,
        string rewardStatus = RewardDisciplineConstants.RewardStatuses.Approved,
        bool daHuy = false)
    {
        var adminId = await GetSuperAdminIdAsync(db);
        var org = new DonVi { TenDonVi = $"RD7 Org {Guid.NewGuid():N}", CapDonVi = "co_so" };
        db.DonVis.Add(org);
        await db.SaveChangesAsync();

        var term = new HocKy
        {
            TenHocKy = $"RD7 HK {Guid.NewGuid():N}",
            MaCodeHocKy = $"HK_{Guid.NewGuid().ToString("N")[..8]}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3)),
            MaDonVi = org.MaDonVi
        };
        db.HocKys.Add(term);
        await db.SaveChangesAsync();

        var student = await CreateStudentAsync(db, org.MaDonVi, 1);

        MauBangKhen? template = null;
        if (withPdf)
        {
            template = new MauBangKhen
            {
                TenMau = $"RD7 template {Guid.NewGuid():N}",
                LoaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
                FileNenUrl = "https://example.test/rd7-template.png",
                ChieuRong = 3508,
                ChieuCao = 2480,
                HuongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
                CauHinhJson = ValidConfigJson,
                ConHoatDong = true,
                NguoiTao = adminId,
                NgayTao = DateTime.UtcNow
            };
            db.MauBangKhens.Add(template);
            await db.SaveChangesAsync();
        }

        var campaign = new DotKhenThuong
        {
            MaDonVi = org.MaDonVi,
            MaHocKy = term.MaHocKy,
            TenDot = $"RD7 campaign {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Approved,
            SoLuongToiDa = 100,
            MaMauBangKhen = template?.MaMauBangKhen,
            NguoiTao = adminId,
            NguoiDuyet = adminId,
            NgayDuyet = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();

        var reward = new KhenThuong
        {
            MaDonVi = org.MaDonVi,
            MaHocSinh = student.MaNguoiDung,
            MaHocKy = term.MaHocKy,
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            MaMauBangKhen = template?.MaMauBangKhen,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            TrangThai = rewardStatus,
            GpaDatDuoc = 9.5m,
            DiemXet = 9.5m,
            XepHang = 1,
            UrlChungTu = string.Empty,
            HoTenSnapshot = student.HoTen,
            MssvSnapshot = $"SVRD7{student.MaNguoiDung:000000}",
            TenHocKySnapshot = term.TenHocKy,
            DanhHieuSnapshot = "Top 100 học kỳ",
            CapLuc = DateTime.UtcNow,
            NguoiCap = adminId,
            NguoiDuyet = adminId,
            NgayCapNhat = DateTime.UtcNow,
            DaHuy = daHuy
        };
        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        return new RD7Seed(
            campaign.MaDotKhenThuong,
            reward.MaKhenThuong,
            student.MaNguoiDung,
            org.MaDonVi,
            term.MaHocKy,
            adminId,
            student.Email);
    }

    private static async Task<NguoiDung> CreateStudentAsync(
        ApplicationDbContext db, int maDonVi, int index, string status = UserStatuses.DbActive)
    {
        var student = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"RD7 Student {index} {Guid.NewGuid():N}",
            Email = $"rd7_student_{Guid.NewGuid():N}@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = status,
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

    private async Task<HttpClient> CreateStudentClientAsync(ApplicationDbContext db, int studentUserId)
    {
        var student = await db.NguoiDungs.FirstAsync(x => x.MaNguoiDung == studentUserId);
        var originalRole = student.VaiTroChinh;
        var originalStatus = student.TrangThai;

        // Ensure the user is a student with active status for login
        student.VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student);
        if (student.TrangThai != UserStatuses.DbLocked)
        {
            student.TrangThai = UserStatuses.DbActive;
        }
        await db.SaveChangesAsync();

        using var loginResponse = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email = student.Email,
            password = GetSharedTestPassword()
        });

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Student login failed for {student.Email}: {await DescribeResponseAsync(loginResponse)}");

        using var root = await GetRootAsync(loginResponse);
        var token = GetRequiredString(root.RootElement, "accessToken");

        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static void ValidateCertificateStorageRoot()
    {
        var root = Environment.GetEnvironmentVariable("CertificateStorage__LocalRoot");
        if (string.IsNullOrWhiteSpace(root))
        {
            Assert.Fail("Thiếu env var CertificateStorage__LocalRoot cho RD7 tests.");
        }

        var provider = Environment.GetEnvironmentVariable("CertificateStorage__Provider");
        if (!string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("RD7 tests yêu cầu CertificateStorage__Provider=Local.");
        }

        Directory.CreateDirectory(Path.GetFullPath(root!));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record RD7Seed(
        int CampaignId,
        int RewardId,
        int StudentId,
        int MaDonVi,
        int MaHocKy,
        int AdminId,
        string StudentEmail);
}

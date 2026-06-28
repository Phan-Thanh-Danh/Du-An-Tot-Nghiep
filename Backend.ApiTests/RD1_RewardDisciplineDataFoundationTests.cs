using System.Net;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD1")]
public class RD1_RewardDisciplineDataFoundationTests : ApiTestBase
{
    [Test]
    public async Task SchemaOptions_ShouldReturnRewardCampaignOptions()
    {
        using var response = await Client.GetAsync("api/reward-discipline/schema/options");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        AssertOptionExists(data, "rewardCampaignTypes", RewardDisciplineConstants.RewardCampaignTypes.Top100Semester);
        AssertOptionExists(data, "rewardCampaignStatuses", RewardDisciplineConstants.RewardCampaignStatuses.Draft);
        AssertOptionExists(data, "rewardCampaignStatuses", RewardDisciplineConstants.RewardCampaignStatuses.Published);
    }

    [Test]
    public async Task SchemaOptions_ShouldReturnCertificateTemplateOptions()
    {
        using var response = await Client.GetAsync("api/reward-discipline/schema/options");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");

        AssertOptionExists(data, "certificateTemplateTypes", RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester);
        AssertOptionExists(data, "paperOrientations", RewardDisciplineConstants.PaperOrientations.A4Landscape);
        AssertOptionExists(data, "paperOrientations", RewardDisciplineConstants.PaperOrientations.A4Portrait);
    }

    [Test]
    public async Task SchemaOptions_ShouldReturnRewardOptions()
    {
        using var response = await Client.GetAsync("api/reward-discipline/schema/options");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");

        AssertOptionExists(data, "rewardTypes", RewardDisciplineConstants.RewardTypes.Top100Semester);
        AssertOptionExists(data, "rewardTypes", RewardDisciplineConstants.RewardTypes.AcademicLegacy);
        AssertOptionExists(data, "rewardStatuses", RewardDisciplineConstants.RewardStatuses.PdfGenerated);
        AssertOptionExists(data, "rewardStatuses", RewardDisciplineConstants.RewardStatuses.Cancelled);
    }

    [Test]
    public async Task SchemaOptions_ShouldReturnDisciplineOptions()
    {
        using var response = await Client.GetAsync("api/reward-discipline/schema/options");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");

        AssertOptionExists(data, "disciplineLevels", RewardDisciplineConstants.DisciplineLevels.Minor);
        AssertOptionExists(data, "disciplineLevels", RewardDisciplineConstants.DisciplineLevels.Severe);
        AssertOptionExists(data, "disciplineStatuses", RewardDisciplineConstants.DisciplineStatuses.Active);
        AssertOptionExists(data, "disciplineActions", RewardDisciplineConstants.DisciplineActions.Warning);
    }

    [Test]
    public async Task Model_ShouldContainRewardDisciplineEntities()
    {
        await using var db = CreateDbContext();

        Assert.That(db.Model.FindEntityType(typeof(DotKhenThuong)), Is.Not.Null);
        Assert.That(db.Model.FindEntityType(typeof(MauBangKhen)), Is.Not.Null);
        Assert.That(db.Model.FindEntityType(typeof(KhenThuong)), Is.Not.Null);
        Assert.That(db.Model.FindEntityType(typeof(HoSoKyLuat)), Is.Not.Null);
    }

    [Test]
    public async Task DotKhenThuong_WithValidJson_ShouldPersist()
    {
        await using var db = CreateDbContext();
        var seed = await GetSeedAsync(db);

        var dot = new DotKhenThuong
        {
            MaHocKy = seed.MaHocKy,
            MaDonVi = seed.MaDonVi,
            TenDot = $"RD1 Top 100 cancelled {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            TieuChiXetJson = """{"minGpa":8.5,"rankBy":"gpa"}""",
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Cancelled,
            NguoiTao = seed.AdminId
        };

        db.DotKhenThuongs.Add(dot);
        await db.SaveChangesAsync();

        Assert.That(dot.MaDotKhenThuong, Is.GreaterThan(0));
    }

    [Test]
    public async Task DotKhenThuong_WithInvalidJson_ShouldFail()
    {
        await using var db = CreateDbContext();
        var seed = await GetSeedAsync(db);

        db.DotKhenThuongs.Add(new DotKhenThuong
        {
            MaHocKy = seed.MaHocKy,
            MaDonVi = seed.MaDonVi,
            TenDot = $"RD1 invalid json {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            SoLuongToiDa = 100,
            TieuChiXetJson = "{not-json",
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Cancelled,
            NguoiTao = seed.AdminId
        });

        Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [Test]
    public async Task MauBangKhen_WithValidJson_ShouldPersist()
    {
        await using var db = CreateDbContext();
        var seed = await GetSeedAsync(db);

        var template = new MauBangKhen
        {
            TenMau = $"RD1 mẫu bằng khen {Guid.NewGuid():N}",
            LoaiMau = RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester,
            FileNenUrl = "https://example.test/certificate.png",
            ChieuRong = 3508,
            ChieuCao = 2480,
            HuongGiay = RewardDisciplineConstants.PaperOrientations.A4Landscape,
            CauHinhJson = """{"fields":[{"key":"studentName","x":100,"y":100}]}""",
            ConHoatDong = true,
            NguoiTao = seed.AdminId
        };

        db.MauBangKhens.Add(template);
        await db.SaveChangesAsync();

        Assert.That(template.MaMauBangKhen, Is.GreaterThan(0));
    }

    [Test]
    public async Task KhenThuong_WithExpandedFoundationFields_ShouldPersist()
    {
        await using var db = CreateDbContext();
        var seed = await GetSeedAsync(db);

        var reward = new KhenThuong
        {
            MaDonVi = seed.MaDonVi,
            MaHocSinh = seed.StudentId,
            MaHocKy = seed.MaHocKy,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            TrangThai = RewardDisciplineConstants.RewardStatuses.PdfGenerated,
            GpaDatDuoc = 9.10m,
            DiemXet = 9.1234m,
            XepHang = 1,
            UrlChungTu = string.Empty,
            UrlPdfBangKhen = "https://example.test/reward.pdf",
            HoTenSnapshot = "Nguyen Van RD1",
            MssvSnapshot = $"RD1{Random.Shared.Next(100000, 999999)}",
            TenHocKySnapshot = "Học kỳ RD1",
            DanhHieuSnapshot = "Top 100 học kỳ",
            NguoiCap = seed.AdminId,
            NguoiDuyet = seed.AdminId
        };

        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        Assert.That(reward.MaKhenThuong, Is.GreaterThan(0));
    }

    [Test]
    public async Task HoSoKyLuat_WithExpandedFoundationFields_ShouldPersist()
    {
        await using var db = CreateDbContext();
        var seed = await GetSeedAsync(db);

        var record = new HoSoKyLuat
        {
            MaHocSinh = seed.StudentId,
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            LoaiKyLuat = "vi_pham_noi_quy",
            MucDoViPham = RewardDisciplineConstants.DisciplineLevels.Moderate,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Warning,
            TrangThai = RewardDisciplineConstants.DisciplineStatuses.Active,
            MoTa = "RD1 disciplinary foundation test",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayHieuLuc = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NguoiTao = seed.AdminId,
            NguoiDuyet = seed.AdminId,
            ChungTuJson = """[{"fileName":"bien-ban.pdf","size":12345}]""",
            LoaiDoiTuongLienKet = "manual",
            MaDoiTuongLienKet = 1
        };

        db.HoSoKyLuats.Add(record);
        await db.SaveChangesAsync();

        Assert.That(record.MaKyLuat, Is.GreaterThan(0));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<RD1Seed> GetSeedAsync(ApplicationDbContext db)
    {
        var admin = await db.NguoiDungs
            .AsNoTracking()
            .Where(x => x.Email == "superadmin@lms.local")
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstOrDefaultAsync();
        Assert.That(admin, Is.Not.Null, "Thiếu seed superadmin@lms.local.");

        var student = await db.NguoiDungs
            .AsNoTracking()
            .Where(x => x.Email == "student.cntt01@lms.local")
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstOrDefaultAsync();
        Assert.That(student, Is.Not.Null, "Thiếu seed student.cntt01@lms.local.");

        var termId = await db.HocKys
            .AsNoTracking()
            .Where(x => x.MaDonVi == student!.MaDonVi)
            .OrderByDescending(x => x.NamHoc)
            .ThenByDescending(x => x.ThuTuTrongNam)
            .Select(x => (int?)x.MaHocKy)
            .FirstOrDefaultAsync();
        Assert.That(termId, Is.Not.Null, $"Thiếu HocKy seed cho đơn vị {student!.MaDonVi}.");

        return new RD1Seed(admin!.MaNguoiDung, student!.MaNguoiDung, student.MaDonVi, termId!.Value);
    }

    private static void AssertOptionExists(System.Text.Json.JsonElement data, string collectionName, string expectedValue)
    {
        var options = GetRequiredProperty(data, collectionName);
        var found = options.EnumerateArray().Any(option =>
            string.Equals(GetRequiredString(option, "value"), expectedValue, StringComparison.OrdinalIgnoreCase));

        Assert.That(found, Is.True, $"Thiếu option {expectedValue} trong {collectionName}.");
    }

    private sealed record RD1Seed(int AdminId, int StudentId, int MaDonVi, int MaHocKy);
}

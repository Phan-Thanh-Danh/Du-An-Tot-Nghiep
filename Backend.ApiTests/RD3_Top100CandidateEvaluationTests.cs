using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD3")]
public class RD3_Top100CandidateEvaluationTests : ApiTestBase
{
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string AdminEmail = "admin@lms.local";
    
    // 1. SuperAdmin evaluate campaign creates candidate list ordered by DiemXet desc.
    [Test]
    public async Task EvaluateCampaign_ShouldCreateCandidatesOrderedByDiemXet()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":5.0, "excludeActiveDiscipline":true}""");

        // Create 2 students with grades
        var student1 = await CreateStudentWithGradeAsync(db, seed, gpa: 8.5m, credits: 10, "STU1");
        var student2 = await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU2");

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        var candidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong && c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended).OrderBy(c => c.XepHang).ToListAsync();
        
        Assert.That(candidates, Has.Count.EqualTo(2));
        Assert.That(candidates[0].MaHocSinh, Is.EqualTo(student2.MaNguoiDung)); // GPA 9.0 is rank 1
        Assert.That(candidates[1].MaHocSinh, Is.EqualTo(student1.MaNguoiDung)); // GPA 8.5 is rank 2
        
        Assert.That(await db.KhenThuongs.AnyAsync(k => k.MaDotKhenThuong == campaign.MaDotKhenThuong), Is.False, "RD3 should not create KhenThuong");
    }

    // 2. Evaluation respects SoLuongToiDa.
    [Test]
    public async Task EvaluateCampaign_ShouldRespectMaxCandidates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, criteriaJson: """{"minGpa":5.0}""");

        await CreateStudentWithGradeAsync(db, seed, gpa: 8.5m, credits: 10, "STU1");
        await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU2");

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        var candidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong).ToListAsync();
        Assert.That(candidates.Count(c => c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended), Is.EqualTo(1));
        Assert.That(candidates.Count(c => c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Excluded && c.LyDoLoai == RewardDisciplineConstants.CandidateExclusionReasons.OutOfScope), Is.EqualTo(1));
    }

    // 3. Tie-breaker stable by GPA/credits.
    [Test]
    public async Task EvaluateCampaign_TieBreaker_ShouldOrderByCreditsAndId()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":5.0}""");

        // Same GPA, diff credits
        var student1 = await CreateStudentWithGradeAsync(db, seed, gpa: 8.0m, credits: 10, "STU1");
        var student2 = await CreateStudentWithGradeAsync(db, seed, gpa: 8.0m, credits: 12, "STU2");

        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });

        var candidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong && c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended).OrderBy(c => c.XepHang).ToListAsync();
        Assert.That(candidates[0].MaHocSinh, Is.EqualTo(student2.MaNguoiDung)); // More credits = higher rank
        Assert.That(candidates[1].MaHocSinh, Is.EqualTo(student1.MaNguoiDung));
    }

    // 4. Student with active discipline is excluded.
    [Test]
    public async Task EvaluateCampaign_ActiveDiscipline_ShouldExcludeStudent()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":5.0, "excludeActiveDiscipline":true}""");

        var student = await CreateStudentWithGradeAsync(db, seed, gpa: 9.5m, credits: 15, "STU1");
        db.HoSoKyLuats.Add(new HoSoKyLuat
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            LoaiKyLuat = "GianLan",
            MucDoViPham = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reprimand,
            TrangThai = RewardDisciplineConstants.DisciplineStatuses.Active,
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var candidate = await db.UngVienKhenThuongs.FirstOrDefaultAsync(c => c.MaHocSinh == student.MaNguoiDung);
        Assert.That(candidate, Is.Not.Null);
        Assert.That(candidate!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.Excluded));
        Assert.That(candidate.LyDoLoai, Is.EqualTo(RewardDisciplineConstants.CandidateExclusionReasons.ActiveDiscipline));
    }

    // 5. Inactive student is excluded.
    [Test]
    public async Task EvaluateCampaign_InactiveStudent_ShouldExcludeStudent()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":5.0, "requireActiveStudent":true}""");

        var student = await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 15, "STU1", status: "bi_khoa");

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });
        
        var candidate = await db.UngVienKhenThuongs.FirstOrDefaultAsync(c => c.MaHocSinh == student.MaNguoiDung);
        Assert.That(candidate!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.Excluded));
        Assert.That(candidate.LyDoLoai, Is.EqualTo(RewardDisciplineConstants.CandidateExclusionReasons.InactiveStudent));
    }

    // 6. Student without enough grades is excluded.
    [Test]
    public async Task EvaluateCampaign_NoGrades_ShouldExcludeStudent()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":5.0}""");

        var student = new NguoiDung { MaDonVi = seed.MaDonVi, HoTen = "NoGrade", Email = $"nograde_{Guid.NewGuid():N}@test", VaiTroChinh = "hoc_sinh", TrangThai = "hoat_dong" };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });
        
        var candidate = await db.UngVienKhenThuongs.FirstOrDefaultAsync(c => c.MaHocSinh == student.MaNguoiDung);
        Assert.That(candidate!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.Excluded));
        Assert.That(candidate.LyDoLoai, Is.EqualTo(RewardDisciplineConstants.CandidateExclusionReasons.MissingGrades));
    }

    // 7. GPA below min criteria is excluded.
    [Test]
    public async Task EvaluateCampaign_LowGpa_ShouldExcludeStudent()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10, criteriaJson: """{"minGpa":8.0}""");

        var student = await CreateStudentWithGradeAsync(db, seed, gpa: 7.9m, credits: 15, "STU1");

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });
        
        var candidate = await db.UngVienKhenThuongs.FirstOrDefaultAsync(c => c.MaHocSinh == student.MaNguoiDung);
        Assert.That(candidate!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.Excluded));
        Assert.That(candidate.LyDoLoai, Is.EqualTo(RewardDisciplineConstants.CandidateExclusionReasons.LowGpa));
    }

    // 8. Campaign status changes.
    [Test]
    public async Task EvaluateCampaign_ShouldChangeCampaignStatus()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        Assert.That(campaign.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Draft));

        var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });
        var responseString = await response.Content.ReadAsStringAsync();
        TestContext.WriteLine($"Response: {response.StatusCode} - {responseString}");
        
        // Ensure to clear tracker to fetch fresh data
        db.ChangeTracker.Clear();
        var updatedCampaign = await db.DotKhenThuongs.FindAsync(campaign.MaDotKhenThuong);
        Assert.That(updatedCampaign!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Evaluating));
    }

    // 9. dryRun does not persist.
    [Test]
    public async Task EvaluateCampaign_DryRun_ShouldNotPersist()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU1");

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { dryRun = true });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var candidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong).ToListAsync();
        Assert.That(candidates, Is.Empty);
        
        var updatedCampaign = await db.DotKhenThuongs.FindAsync(campaign.MaDotKhenThuong);
        Assert.That(updatedCampaign!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Draft));
    }

    // 10. Re-evaluate without force returns 409.
    [Test]
    public async Task EvaluateCampaign_ReevaluateWithoutForce_ShouldReturn409()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU1");

        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });
        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    // 11. Re-evaluate with force replaces candidates.
    [Test]
    public async Task EvaluateCampaign_ReevaluateWithForce_ShouldReplaceCandidates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        var student1 = await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU1");

        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });
        var firstCandidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong).ToListAsync();
        Assert.That(firstCandidates, Has.Count.EqualTo(1));

        // Add another student
        var student2 = await CreateStudentWithGradeAsync(db, seed, gpa: 9.5m, credits: 10, "STU2");

        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = true });
        
        var newCandidates = await db.UngVienKhenThuongs.Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong).ToListAsync();
        Assert.That(newCandidates, Has.Count.EqualTo(2));
        Assert.That(newCandidates.Any(c => c.MaUngVienKhenThuong == firstCandidates[0].MaUngVienKhenThuong), Is.False); // old id replaced
    }

    // 12. Cannot evaluate da_huy/da_duyet.
    [Test]
    public async Task EvaluateCampaign_InvalidStatus_ShouldReturnBadRequest()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, status: RewardDisciplineConstants.RewardCampaignStatuses.Approved);

        using var response = await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = true });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // 13. GET candidates paginates.
    [Test]
    public async Task GetCandidates_ShouldPaginate()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        await CreateStudentWithGradeAsync(db, seed, gpa: 9.0m, credits: 10, "STU1");
        await CreateStudentWithGradeAsync(db, seed, gpa: 8.5m, credits: 10, "STU2");
        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false });

        using var response = await Client.GetAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates?pageIndex=1&pageSize=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var items = GetRequiredProperty(data, "items");
        Assert.That(items.GetArrayLength(), Is.EqualTo(1));
    }

    // 14. GET excluded-candidates.
    [Test]
    public async Task GetExcludedCandidates_ShouldReturnList()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 10);
        await CreateStudentWithGradeAsync(db, seed, gpa: 3.0m, credits: 10, "STU1"); // will be excluded due to low GPA
        await Client.PostAsJsonAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/evaluate", new { forceReevaluate = false, includeExcluded = true });

        using var response = await Client.GetAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/excluded-candidates?pageIndex=1&pageSize=10");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var items = GetRequiredProperty(data, "items");
        Assert.That(items.GetArrayLength(), Is.EqualTo(1));
    }

    // Helpers
    private async Task<NguoiDung> CreateStudentWithGradeAsync(ApplicationDbContext db, RD3Seed seed, decimal gpa, int credits, string suffix, string status = "hoat_dong")
    {
        var student = new NguoiDung
        {
            MaDonVi = seed.MaDonVi,
            HoTen = $"Student {suffix}",
            Email = $"student_{Guid.NewGuid():N}_{suffix}@lms.local",
            VaiTroChinh = "hoc_sinh",
            TrangThai = status,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();

        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = $"SUBJ_{Guid.NewGuid().ToString("N").Substring(0, 8)}_{suffix}",
            TenMonHoc = "Subject",
            SoTinChi = credits,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);
        await db.SaveChangesAsync();

        var grade = new DiemSo
        {
            MaHocSinh = student.MaNguoiDung,
            MaHocKy = seed.MaHocKy,
            MaMonHoc = subject.MaMonHoc,
            MaDonVi = seed.MaDonVi,
            GpaMonHoc = gpa,
            TrangThai = "dat",
            DaKhoa = true,
            NamNhapHoc = 2026
        };
        db.DiemSos.Add(grade);
        await db.SaveChangesAsync();

        return student;
    }

    private async Task<DotKhenThuong> CreateCampaignAsync(ApplicationDbContext db, RD3Seed seed, int maxCandidates = 100, string status = RewardDisciplineConstants.RewardCampaignStatuses.Draft, string? criteriaJson = null)
    {
        var campaign = new DotKhenThuong
        {
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            TenDot = $"Test Campaign {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            TrangThai = status,
            SoLuongToiDa = maxCandidates,
            TieuChiXetJson = criteriaJson ?? """{"minGpa": 8.0, "excludeActiveDiscipline": true}""",
            NgayTao = DateTime.UtcNow,
            NguoiTao = seed.AdminId
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    private async Task<RD3Seed> CreateSeedAsync(ApplicationDbContext db)
    {
        var donVi = new DonVi { TenDonVi = "Test Org", CapDonVi = "co_so" };
        db.DonVis.Add(donVi);
        await db.SaveChangesAsync();

        var hocKy = new HocKy { TenHocKy = "Test HK", MaCodeHocKy = $"HK_{Guid.NewGuid().ToString("N").Substring(0, 8)}", NamHoc = "2026-2027", ThuTuTrongNam = 1, NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date), NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(3)), MaDonVi = donVi.MaDonVi };
        db.HocKys.Add(hocKy);
        await db.SaveChangesAsync();

        var adminEmail = $"admin_{Guid.NewGuid():N}@lms.local";
        var admin = new NguoiDung { MaDonVi = donVi.MaDonVi, HoTen = "Admin", Email = adminEmail, VaiTroChinh = "sieu_quan_tri", TrangThai = "hoat_dong" };
        db.NguoiDungs.Add(admin);
        await db.SaveChangesAsync();

        return new RD3Seed(donVi.MaDonVi, hocKy.MaHocKy, admin.MaNguoiDung);
    }

    private sealed record RD3Seed(int MaDonVi, int MaHocKy, int AdminId);

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }
}

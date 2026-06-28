using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD4")]
public class RD4_Top100ApprovalAdjustmentTests : ApiTestBase
{
    private const string AdminEmail = "admin@lms.local";

    [Test]
    public async Task ApprovalSummary_ShouldReturnCounts()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        await CreateCandidateAsync(db, campaign, seed, 2, RewardDisciplineConstants.RewardCandidateStatuses.Excluded);

        using var response = await Client.GetAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approval-summary");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "selectedCount"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "excludedCount"), Is.EqualTo(1));
    }

    [Test]
    public async Task AdjustCandidate_ShouldUpdateStatusRankScoreAndAudit()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var candidate = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PatchAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/{candidate.MaUngVienKhenThuong}",
            new
            {
                trangThai = RewardDisciplineConstants.RewardCandidateStatuses.Reserve,
                xepHang = 5,
                diemXet = 8.75m,
                lyDoDieuChinh = "Điều chỉnh kiểm thử",
                ghiChuDieuChinh = "Ghi chú kiểm thử"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var updated = await db.UngVienKhenThuongs.AsNoTracking().FirstAsync(x => x.MaUngVienKhenThuong == candidate.MaUngVienKhenThuong);
        Assert.That(updated.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.Reserve));
        Assert.That(updated.XepHang, Is.EqualTo(5));
        Assert.That(updated.DiemXet, Is.EqualTo(8.75m));
        Assert.That(updated.NguoiDieuChinh, Is.Not.Null);
        Assert.That(await HasAuditAsync(db, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardAuditActions.AdjustRewardCandidate), Is.True);
    }

    [Test]
    public async Task AdjustCandidate_WithoutReasonForChange_ShouldReturnBadRequest()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var candidate = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PatchAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/{candidate.MaUngVienKhenThuong}",
            new { trangThai = RewardDisciplineConstants.RewardCandidateStatuses.Excluded });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ManualAddCandidate_ShouldCreateManualCandidate()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var student = await CreateStudentAsync(db, seed.MaDonVi, "Manual");

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/manual-add",
            new
            {
                maHocSinh = student.MaNguoiDung,
                diemXet = 9.25m,
                xepHang = 3,
                lyDoThemBatBuoc = "Bổ sung thủ công kiểm thử"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var candidate = await db.UngVienKhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong && x.MaHocSinh == student.MaNguoiDung);
        Assert.That(candidate.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded));
        Assert.That(candidate.GhiChuDieuChinh, Is.EqualTo("Bổ sung thủ công kiểm thử"));
        Assert.That(await HasAuditAsync(db, campaign.MaDotKhenThuong, RewardDisciplineConstants.RewardAuditActions.ManualAddRewardCandidate), Is.True);
    }

    [Test]
    public async Task ManualAddCandidate_Duplicate_ShouldReturnConflict()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var candidate = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/manual-add",
            new
            {
                maHocSinh = candidate.MaHocSinh,
                lyDoThemBatBuoc = "Trùng ứng viên"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ManualAddCandidate_ActiveDiscipline_ShouldReturnBadRequest()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var student = await CreateStudentAsync(db, seed.MaDonVi, "Discipline");
        db.HoSoKyLuats.Add(new HoSoKyLuat
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            LoaiKyLuat = "kiem_thu",
            MucDoViPham = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder,
            TrangThai = RewardDisciplineConstants.DisciplineStatuses.Active,
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/manual-add",
            new
            {
                maHocSinh = student.MaNguoiDung,
                lyDoThemBatBuoc = "Có kỷ luật"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ReorderCandidates_ShouldUpdateRanks()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var first = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        var second = await CreateCandidateAsync(db, campaign, seed, 2, RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/reorder",
            new
            {
                items = new[]
                {
                    new { candidateId = first.MaUngVienKhenThuong, xepHang = 2 },
                    new { candidateId = second.MaUngVienKhenThuong, xepHang = 1 }
                }
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var ranks = await db.UngVienKhenThuongs.AsNoTracking()
            .Where(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong)
            .ToDictionaryAsync(x => x.MaUngVienKhenThuong, x => x.XepHang);
        Assert.That(ranks[first.MaUngVienKhenThuong], Is.EqualTo(2));
        Assert.That(ranks[second.MaUngVienKhenThuong], Is.EqualTo(1));
    }

    [Test]
    public async Task ReorderCandidates_DuplicateRank_ShouldReturnBadRequest()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        var first = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        var second = await CreateCandidateAsync(db, campaign, seed, 2, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PostAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/reorder",
            new
            {
                items = new[]
                {
                    new { candidateId = first.MaUngVienKhenThuong, xepHang = 1 },
                    new { candidateId = second.MaUngVienKhenThuong, xepHang = 1 }
                }
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task SubmitForApproval_ShouldChangeStatus()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/submit-for-approval", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var updated = await db.DotKhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong);
        Assert.That(updated.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval));
    }

    [Test]
    public async Task ApproveCampaign_ShouldCreateRewardsAndMarkCandidates()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 2, status: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        await CreateCandidateAsync(db, campaign, seed, 2, RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded);

        using var response = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var rewards = await db.KhenThuongs.AsNoTracking().Where(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong).ToListAsync();
        Assert.That(rewards, Has.Count.EqualTo(2));
        Assert.That(rewards.All(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Approved), Is.True);
        Assert.That(rewards.All(x => x.UrlChungTu == string.Empty), Is.True);
        Assert.That(rewards.All(x => x.UrlPdfBangKhen == null), Is.True);
        Assert.That(rewards.All(x => x.LoaiKhenThuong == RewardDisciplineConstants.RewardTypes.Top100Semester), Is.True);
        Assert.That(rewards.All(x => x.DanhHieuSnapshot == "Top 100 học kỳ"), Is.True);
        Assert.That(rewards.All(x => x.MaHocKy == campaign.MaHocKy), Is.True);
        Assert.That(rewards.All(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong), Is.True);
        Assert.That(rewards.All(x => x.XepHang > 0), Is.True);
        Assert.That(rewards.All(x => x.DiemXet > 0), Is.True);
        Assert.That(rewards.All(x => x.HoTenSnapshot != null), Is.True);
        Assert.That(rewards.All(x => x.MssvSnapshot != null), Is.True);
        Assert.That(rewards.All(x => x.TenHocKySnapshot != null), Is.True);
        Assert.That(rewards.All(x => x.DaHuy == false), Is.True);
        Assert.That(await db.UngVienKhenThuongs.AsNoTracking().CountAsync(x =>
            x.MaDotKhenThuong == campaign.MaDotKhenThuong &&
            x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.ApprovedForReward), Is.EqualTo(2));

        // Campaign moves to da_duyet and sets approver/time
        var updated = await db.DotKhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong);
        Assert.That(updated.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Approved));
        Assert.That(updated.NguoiDuyet, Is.Not.Null);
        Assert.That(updated.NgayDuyet, Is.Not.Null);
    }

    [Test]
    public async Task ApproveCampaign_Twice_ShouldReturnConflictAndNoDuplicateRewards()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, status: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        using var firstResponse = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(firstResponse));

        using var secondResponse = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(secondResponse));
        Assert.That(await db.KhenThuongs.AsNoTracking().CountAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong), Is.EqualTo(1));
    }

    [Test]
    public async Task ApproveCampaign_OverMaxCandidates_ShouldReturnConflict()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, status: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        await CreateCandidateAsync(db, campaign, seed, 2, RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded);

        using var response = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
        Assert.That(await db.KhenThuongs.AsNoTracking().AnyAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong), Is.False);
    }

    [Test]
    public async Task ApproveCampaign_FromEvaluating_ShouldCreateRewardsAndSetApproved()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, status: RewardDisciplineConstants.RewardCampaignStatuses.Evaluating);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);

        using var response = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        db.ChangeTracker.Clear();
        var updatedCampaign = await db.DotKhenThuongs.AsNoTracking().FirstAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong);
        Assert.That(updatedCampaign.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardCampaignStatuses.Approved));
        Assert.That(await db.KhenThuongs.AsNoTracking().CountAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong), Is.EqualTo(1));
    }

    [Test]
    public async Task Mutation_Anonymous_ShouldReturnUnauthorized()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed);
        using var anonymous = new HttpClient { BaseAddress = BaseUri };

        using var response = await anonymous.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task NonSuperAdmin_CannotApproveCampaign()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, status: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);
        await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);

        using var response = await adminClient.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AdjustCandidate_AfterApproved_ShouldReturnConflict()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, maxCandidates: 1, status: RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval);
        var candidate = await CreateCandidateAsync(db, campaign, seed, 1, RewardDisciplineConstants.RewardCandidateStatuses.Recommended);
        using var firstResponse = await Client.PostAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approve", null);
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(firstResponse));

        using var response = await Client.PatchAsJsonAsync(
            $"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/candidates/{candidate.MaUngVienKhenThuong}",
            new { trangThai = RewardDisciplineConstants.RewardCandidateStatuses.Excluded, lyDoDieuChinh = "Sau duyệt" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Admin_CannotViewOutOfScopeApprovalSummary()
    {
        await using var db = CreateDbContext();
        var seed = await CreateSeedAsync(db);
        var campaign = await CreateCampaignAsync(db, seed, isGlobalCampaign: true);
        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);

        using var response = await adminClient.GetAsync($"api/admin/reward-campaigns/{campaign.MaDotKhenThuong}/approval-summary");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
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

    private static async Task<bool> HasAuditAsync(ApplicationDbContext db, int campaignId, string action)
    {
        return await db.NhatKyKiemToans.AsNoTracking().AnyAsync(x =>
            x.LoaiDoiTuong == "DotKhenThuong" &&
            x.MaDoiTuong == campaignId.ToString() &&
            x.HanhDong == action);
    }

    private static async Task<UngVienKhenThuong> CreateCandidateAsync(
        ApplicationDbContext db,
        DotKhenThuong campaign,
        RD4Seed seed,
        int rank,
        string status)
    {
        var student = await CreateStudentAsync(db, seed.MaDonVi, $"Rank{rank}");
        var candidate = new UngVienKhenThuong
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            XepHang = rank,
            DiemXet = 10m - rank / 10m,
            GpaHocKy = 9m,
            TongTinChi = 20,
            TrangThai = status,
            LyDoLoai = status == RewardDisciplineConstants.RewardCandidateStatuses.Excluded
                ? RewardDisciplineConstants.CandidateExclusionReasons.LowGpa
                : null,
            HoTenSnapshot = student.HoTen,
            MssvSnapshot = student.Email,
            TenHocKySnapshot = seed.TenHocKy,
            NguoiTao = seed.AdminId,
            NgayTao = DateTime.UtcNow
        };
        db.UngVienKhenThuongs.Add(candidate);
        await db.SaveChangesAsync();
        return candidate;
    }

    private static async Task<NguoiDung> CreateStudentAsync(ApplicationDbContext db, int maDonVi, string suffix)
    {
        var student = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"RD4 Student {suffix}",
            Email = $"rd4_student_{Guid.NewGuid():N}_{suffix}@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();
        return student;
    }

    private static async Task<DotKhenThuong> CreateCampaignAsync(
        ApplicationDbContext db,
        RD4Seed seed,
        int maxCandidates = 100,
        string status = RewardDisciplineConstants.RewardCampaignStatuses.Evaluating,
        bool isGlobalCampaign = false)
    {
        var campaign = new DotKhenThuong
        {
            MaDonVi = isGlobalCampaign ? null : seed.MaDonVi,
            MaHocKy = seed.MaHocKy,
            TenDot = $"RD4 Top100 {Guid.NewGuid():N}",
            LoaiDot = RewardDisciplineConstants.RewardCampaignTypes.Top100Semester,
            TrangThai = status,
            SoLuongToiDa = maxCandidates,
            TieuChiXetJson = """{"minGpa":8.0}""",
            NgayTao = DateTime.UtcNow,
            NguoiTao = seed.AdminId
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    private static async Task<RD4Seed> CreateSeedAsync(ApplicationDbContext db)
    {
        var organization = new DonVi
        {
            TenDonVi = $"RD4 Org {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true
        };
        db.DonVis.Add(organization);
        await db.SaveChangesAsync();

        var term = new HocKy
        {
            MaDonVi = organization.MaDonVi,
            TenHocKy = "RD4 Test Term",
            MaCodeHocKy = $"RD4_{Guid.NewGuid().ToString("N")[..8]}",
            NamHoc = "2026-2027",
            ThuTuTrongNam = 1,
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            NgayKetThuc = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(4))
        };
        db.HocKys.Add(term);
        await db.SaveChangesAsync();

        var admin = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == "superadmin@lms.local")
            .Select(x => new { x.MaNguoiDung })
            .FirstAsync();

        return new RD4Seed(organization.MaDonVi, term.MaHocKy, term.TenHocKy, admin.MaNguoiDung);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record RD4Seed(int MaDonVi, int MaHocKy, string TenHocKy, int AdminId);
}

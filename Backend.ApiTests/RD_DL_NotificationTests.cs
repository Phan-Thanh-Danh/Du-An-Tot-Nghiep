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
[Category("RD_DL_Notification")]
public class RD_DL_NotificationTests : ApiTestBase
{
    private string GetDbConnectionString()
    {
        return GetSharedTestConnectionString();
    }

    private string GetTestPassword()
    {
        return GetSharedTestPassword();
    }

    private async Task<HttpClient> CreateClientAsync(string email)
    {
        var client = new HttpClient { BaseAddress = BaseUri };
        var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
        {
            email = email,
            password = GetTestPassword()
        });

        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var token = json.RootElement.TryGetProperty("accessToken", out var accessToken)
            ? accessToken.GetString()
            : json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<(int CampusId, int TermId, int SuperAdminId, string SuperAdminEmail, int CampusAdminId, string CampusAdminEmail, int StudentId, string StudentEmail)> SeedDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var superAdminRole = AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin);
        var campusAdminRole = AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin);
        var adminRole = AuthRoles.ToDatabaseCode(AuthRoles.Admin);
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        var superAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == superAdminRole);
        var student = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == studentRole);
        var campus = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == student!.MaDonVi);
        var term = await db.HocKys.FirstOrDefaultAsync();
        var campusAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == campusAdminRole && u.MaDonVi == campus!.MaDonVi);
        if (campusAdmin == null) campusAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == adminRole);

        return (
            campus!.MaDonVi,
            term!.MaHocKy,
            superAdmin!.MaNguoiDung, superAdmin.Email,
            campusAdmin!.MaNguoiDung, campusAdmin.Email,
            student!.MaNguoiDung, student.Email
        );
    }

    [Test]
    public async Task ResendRewardNotification_WithoutSuperAdminRole_ReturnsForbidden()
    {
        var seed = await SeedDataAsync();
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var reward = new KhenThuong
        {
            MaDonVi = seed.CampusId,
            MaHocKy = seed.TermId,
            MaHocSinh = seed.StudentId,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            DanhHieuSnapshot = "Test",
            TrangThai = RewardDisciplineConstants.RewardStatuses.Issued,
            CapLuc = DateTime.UtcNow
        };
        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        using var client = await CreateClientAsync(seed.CampusAdminEmail);

        var request = new { Reason = "Lý do hợp lệ để gửi lại thông báo" };
        var response = await client.PostAsJsonAsync($"/api/admin/rewards/{reward.MaKhenThuong}/notifications/resend", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ResendRewardNotification_WithShortReason_ReturnsBadRequest()
    {
        var seed = await SeedDataAsync();
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var reward = new KhenThuong
        {
            MaDonVi = seed.CampusId,
            MaHocKy = seed.TermId,
            MaHocSinh = seed.StudentId,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            DanhHieuSnapshot = "Test",
            TrangThai = RewardDisciplineConstants.RewardStatuses.Issued,
            CapLuc = DateTime.UtcNow
        };
        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        using var client = await CreateClientAsync(seed.SuperAdminEmail);

        var request = new { Reason = "Ngắn" };
        var response = await client.PostAsJsonAsync($"/api/admin/rewards/{reward.MaKhenThuong}/notifications/resend", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ResendRewardNotification_WithSuperAdminRoleAndValidReason_ReturnsOkAndCreatesNotification()
    {
        var seed = await SeedDataAsync();
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var reward = new KhenThuong
        {
            MaDonVi = seed.CampusId,
            MaHocKy = seed.TermId,
            MaHocSinh = seed.StudentId,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            DanhHieuSnapshot = "Test Reward Title",
            TrangThai = RewardDisciplineConstants.RewardStatuses.Issued,
            CapLuc = DateTime.UtcNow
        };
        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        using var client = await CreateClientAsync(seed.SuperAdminEmail);

        var request = new { Reason = "Gửi lại do sinh viên chưa nhận được email, reason dài hơn 10 ky tu" };
        var response = await client.PostAsJsonAsync($"/api/admin/rewards/{reward.MaKhenThuong}/notifications/resend", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var tb = await db.ThongBaos
            .Include(x => x.NguoiNhans)
            .FirstOrDefaultAsync(x => x.MaDoiTuongLienKet == reward.MaKhenThuong && x.LoaiDoiTuongLienKet == RewardDisciplineConstants.NotificationRefTypes.Reward);
            
        Assert.That(tb, Is.Not.Null);
        Assert.That(tb!.LoaiThongBao, Is.EqualTo(NotificationConstants.Types.RewardDiscipline));
        Assert.That(tb.NguoiNhans.Any(x => x.MaNguoiNhan == seed.StudentId), Is.True);
    }
}

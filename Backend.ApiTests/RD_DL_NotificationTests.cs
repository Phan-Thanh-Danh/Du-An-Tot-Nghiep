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
        return Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=LMS_Test;Trusted_Connection=True;MultipleActiveResultSets=true";
    }

    private string GetTestPassword()
    {
        return Environment.GetEnvironmentVariable("LMS_TEST_PASSWORD") ?? "T0P_S3cr3t_2024";
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
        var token = json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<(int CampusId, int SuperAdminId, string SuperAdminEmail, int CampusAdminId, string CampusAdminEmail, int StudentId, string StudentEmail)> SeedDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var campus = await db.DonVis.FirstOrDefaultAsync();

        var superAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.SuperAdmin);
        var campusAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.CampusAdmin && u.MaDonVi == campus!.MaDonVi);
        if (campusAdmin == null) campusAdmin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.Admin);
        
        var student = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.Student && u.MaDonVi == campus!.MaDonVi);

        return (
            campus!.MaDonVi, 
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
            MaHocSinh = seed.StudentId,
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
            MaHocSinh = seed.StudentId,
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
            MaHocSinh = seed.StudentId,
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
        Assert.That(tb!.LoaiThongBao, Is.EqualTo("reward_discipline"));
        Assert.That(tb.NguoiNhans.Any(x => x.MaNguoiNhan == seed.StudentId), Is.True);
    }
}

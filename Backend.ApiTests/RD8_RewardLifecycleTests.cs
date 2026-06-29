using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Constants;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Backend.Data;

namespace Backend.ApiTests;

[TestFixture]
[Category("RD8")]
public class RD8_RewardLifecycleTests : ApiTestBase
{
    private async Task<(int CampaignId, int RewardId, int StudentId)> SeedDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING") ?? "Server=(localdb)\\mssqllocaldb;Database=LMS_Test;Trusted_Connection=True;MultipleActiveResultSets=true");
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var campus = await db.DonVis.FirstOrDefaultAsync();
        var student = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.Student);
        var term = await db.HocKys.FirstOrDefaultAsync();

        var campaign = new DotKhenThuong
        {
            MaDonVi = campus!.MaDonVi,
            TenDot = "Đợt test RD8",
            LoaiDot = RewardDisciplineConstants.RewardTypes.Top100Semester,
            MaHocKy = term!.MaHocKy,
            TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Approved
        };
        db.DotKhenThuongs.Add(campaign);
        await db.SaveChangesAsync();

        var reward = new KhenThuong
        {
            MaDonVi = campus.MaDonVi,
            MaHocSinh = student!.MaNguoiDung,
            MaHocKy = term.MaHocKy,
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
            TrangThai = RewardDisciplineConstants.RewardStatuses.Approved,
            HoTenSnapshot = "Test",
            MssvSnapshot = "123",
            DiemXet = 3.5m,
            DaHuy = false
        };
        db.KhenThuongs.Add(reward);
        await db.SaveChangesAsync();

        return (campaign.MaDotKhenThuong, reward.MaKhenThuong, student.MaNguoiDung);
    }

    private async Task<HttpClient> CreateStudentClientAsync(int studentUserId)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING") ?? "Server=(localdb)\\mssqllocaldb;Database=LMS_Test;Trusted_Connection=True;MultipleActiveResultSets=true");
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        
        var student = await db.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == studentUserId);
        Assert.That(student, Is.Not.Null);

        var client = new HttpClient { BaseAddress = BaseUri };
        var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
        {
            email = student!.Email,
            password = Environment.GetEnvironmentVariable("LMS_TEST_PASSWORD") ?? "T0P_S3cr3t_2024"
        });

        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var token = json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    [Test]
    public async Task GetRewards_ReturnsPagedList()
    {
        var seed = await SeedDataAsync();

        using var response = await Client.GetAsync($"/api/admin/rewards?maDotKhenThuong={seed.CampaignId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<PagedResultDto<AdminRewardListItemDto>>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.Not.Null);
    }

    [Test]
    public async Task GetRewardDetail_ValidId_ReturnsDetail()
    {
        var seed = await SeedDataAsync();

        using var response = await Client.GetAsync($"/api/admin/rewards/{seed.RewardId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<AdminRewardDetailDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.Not.Null);
        Assert.That(result.Data!.MaKhenThuong, Is.EqualTo(seed.RewardId));
    }

    [Test]
    public async Task CancelReward_ValidRequest_Success()
    {
        var seed = await SeedDataAsync();

        var request = new CancelRewardRequest { Reason = "Lý do hủy hợp lệ test" };
        using var response = await Client.PostAsJsonAsync($"/api/admin/rewards/{seed.RewardId}/cancel", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detailRes = await Client.GetAsync($"/api/admin/rewards/{seed.RewardId}");
        var body = await detailRes.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<ApiResponseDto<AdminRewardDetailDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(detail!.Data!.DaHuy, Is.True);
        Assert.That(detail.Data.LyDoHuy, Is.EqualTo("Lý do hủy hợp lệ test"));
    }

    [Test]
    public async Task RestoreReward_AfterCancel_Success()
    {
        var seed = await SeedDataAsync();

        await Client.PostAsJsonAsync($"/api/admin/rewards/{seed.RewardId}/cancel", new CancelRewardRequest { Reason = "Cancel before restore" });

        var request = new RestoreRewardRequest { Reason = "Lý do khôi phục hợp lệ test" };
        using var response = await Client.PostAsJsonAsync($"/api/admin/rewards/{seed.RewardId}/restore", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detailRes = await Client.GetAsync($"/api/admin/rewards/{seed.RewardId}");
        var body = await detailRes.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<ApiResponseDto<AdminRewardDetailDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(detail!.Data!.DaHuy, Is.False);
        Assert.That(detail.Data.LyDoHuy, Is.Null);
    }

    [Test]
    public async Task MarkIssued_ValidRequest_Success()
    {
        var seed = await SeedDataAsync();

        var request = new MarkRewardIssuedRequest { Note = "Đã phát tại hội trường" };
        using var response = await Client.PostAsJsonAsync($"/api/admin/rewards/{seed.RewardId}/mark-issued", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detailRes = await Client.GetAsync($"/api/admin/rewards/{seed.RewardId}");
        var body = await detailRes.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<ApiResponseDto<AdminRewardDetailDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(detail!.Data!.TrangThai, Is.EqualTo(RewardDisciplineConstants.RewardStatuses.Issued));
    }

    [Test]
    public async Task Student_CannotDownloadCancelledReward()
    {
        var seed = await SeedDataAsync();
        
        await Client.PostAsJsonAsync($"/api/admin/rewards/{seed.RewardId}/cancel", new CancelRewardRequest { Reason = "Cancel to test student" });

        using var studentClient = await CreateStudentClientAsync(seed.StudentId);
        using var response = await studentClient.GetAsync($"/api/student/rewards/{seed.RewardId}/certificate/download");
        
        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.NotFound, HttpStatusCode.Conflict));
    }
}

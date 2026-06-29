using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("DL2")]
public class DL2_DisciplineApprovalEffectTests : ApiTestBase
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

    private async Task<(int CampusId, int AdminId, string AdminEmail, int SuperAdminId, string SuperAdminEmail, int StudentId, string StudentEmail, int TermId)> SeedDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var campus = await db.DonVis.FirstOrDefaultAsync();
        var term = await db.HocKys.FirstOrDefaultAsync();

        var superAdminRole = AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin);
        var studentRole = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        var adminSuffix = Guid.NewGuid().ToString("N");
        var admin = new NguoiDung
        {
            MaDonVi = campus!.MaDonVi,
            HoTen = $"DL2 SuperAdmin {adminSuffix}",
            Email = $"dl2.superadmin.{adminSuffix}@lms.local",
            VaiTroChinh = superAdminRole,
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetTestPassword()),
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(admin);
        await db.SaveChangesAsync();
        
        var superAdmin = admin;

        Assert.That(campus, Is.Not.Null, "Thiếu đơn vị test cho DL2.");
        Assert.That(term, Is.Not.Null, "Thiếu học kỳ test cho DL2.");
        Assert.That(admin, Is.Not.Null, "Thiếu admin/campus admin test cho DL2.");
        Assert.That(superAdmin, Is.Not.Null, "Thiếu super admin test cho DL2.");
        var student = await CreateStudentAsync(db, campus!.MaDonVi);

        return (
            campus!.MaDonVi, 
            admin!.MaNguoiDung, admin.Email, 
            superAdmin!.MaNguoiDung, superAdmin.Email,
            student!.MaNguoiDung, student.Email, 
            term!.MaHocKy
        );
    }

    private async Task<NguoiDung> CreateStudentAsync(ApplicationDbContext db, int maDonVi)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var student = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"DL2 Student {suffix}",
            Email = $"dl2.student.{suffix}@lms.local",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetTestPassword()),
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(student);
        await db.SaveChangesAsync();
        return student;
    }

    private async Task<int> CreateAndSubmitRecord(HttpClient client, int studentId, int termId)
    {
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = studentId,
            MaHocKy = termId,
            TieuDe = "Test record DL2",
            MoTaViPham = "Test mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var res = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(res));
        var data = await res.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        var submitResponse = await client.PostAsync($"/api/admin/discipline-records/{data!.MaHoSoKyLuat}/submit", null);
        Assert.That(submitResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(submitResponse));
        return data.MaHoSoKyLuat;
    }

    [Test]
    public async Task GetPendingRecords_Works()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        await CreateAndSubmitRecord(client, seed.StudentId, seed.TermId);

        var response = await client.GetAsync("/api/admin/discipline-records/pending-approval");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var data = await response.Content.ReadFromJsonAsync<PagedResultDto<DisciplineRecordListItemDto>>();
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Items, Is.Not.Empty);
        
        foreach(var item in data.Items)
        {
            Assert.That(item.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.PendingApproval));
        }
    }

    [Test]
    public async Task Approve_MovesToApproved()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var recordId = await CreateAndSubmitRecord(client, seed.StudentId, seed.TermId);

        using var superAdminClient = await CreateClientAsync(seed.SuperAdminEmail);
        var req = new DisciplineApprovalRequest
        {
            DecisionNote = "Approved by DL2 test",
            EffectiveFrom = DateTime.UtcNow,
            EffectiveTo = DateTime.UtcNow.AddDays(30)
        };

        var response = await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/approve", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var data = await response.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Approved));
    }

    [Test]
    public async Task Reject_MovesToRejected()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var recordId = await CreateAndSubmitRecord(client, seed.StudentId, seed.TermId);

        using var superAdminClient = await CreateClientAsync(seed.SuperAdminEmail);
        var req = new DisciplineRejectRequest
        {
            Reason = "Rejected by DL2 test"
        };

        var response = await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/reject", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var data = await response.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Rejected));
    }

    [Test]
    public async Task Activate_MovesToActive()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var recordId = await CreateAndSubmitRecord(client, seed.StudentId, seed.TermId);

        using var superAdminClient = await CreateClientAsync(seed.SuperAdminEmail);
        
        // Approve first
        var approveReq = new DisciplineApprovalRequest { DecisionNote = "Approved" };
        await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/approve", approveReq);

        var activateReq = new DisciplineActivateRequest
        {
            EffectiveFrom = DateTime.UtcNow,
            Note = "Activated"
        };
        var response = await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/activate", activateReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var data = await response.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Active));
    }

    [Test]
    public async Task Expire_MovesToExpired_WhenSuperAdmin()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var recordId = await CreateAndSubmitRecord(client, seed.StudentId, seed.TermId);

        using var superAdminClient = await CreateClientAsync(seed.SuperAdminEmail);
        
        await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/approve", new DisciplineApprovalRequest());
        await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/activate", new DisciplineActivateRequest());

        var expireReq = new DisciplineExpireRequest
        {
            Reason = "Term ended"
        };
        var response = await superAdminClient.PostAsJsonAsync($"/api/admin/discipline-records/{recordId}/expire", expireReq);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var data = await response.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Expired));
    }

    [Test]
    public async Task StudentCannotAccessWorkflowApis()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.StudentEmail);
        
        var res = await client.PostAsJsonAsync("/api/admin/discipline-records/1/approve", new DisciplineApprovalRequest());
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

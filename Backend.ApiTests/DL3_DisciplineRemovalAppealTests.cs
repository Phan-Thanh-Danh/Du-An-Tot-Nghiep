using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[Category("DL3")]
public class DL3_DisciplineRemovalAppealTests : ApiTestBase
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

    private async Task<(int CampusId, int AdminId, string AdminEmail, int StudentId, string StudentEmail, int OtherStudentId, string OtherStudentEmail, int TermId)> SeedDataAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var campus = await db.DonVis.FirstOrDefaultAsync();
        var term = await db.HocKys.FirstOrDefaultAsync();

        var admin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.CampusAdmin && u.MaDonVi == campus!.MaDonVi);
        if (admin == null) admin = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTroChinh == AuthRoles.Admin);
        
        var students = await db.NguoiDungs
            .Where(u => u.VaiTroChinh == AuthRoles.Student && u.MaDonVi == campus!.MaDonVi)
            .Take(2)
            .ToListAsync();

        return (
            CampusId: campus!.MaDonVi,
            AdminId: admin!.MaNguoiDung,
            AdminEmail: admin.Email,
            StudentId: students[0].MaNguoiDung,
            StudentEmail: students[0].Email,
            OtherStudentId: students[1].MaNguoiDung,
            OtherStudentEmail: students[1].Email,
            TermId: term!.MaHocKy
        );
    }

    private HttpClient _adminClient = null!;
    private HttpClient _studentClient = null!;
    private int _studentId;
    
    [SetUp]
    public async Task SetupAsync()
    {
        var data = await SeedDataAsync();
        _adminClient = await CreateClientAsync(data.AdminEmail);
        _studentClient = await CreateClientAsync(data.StudentEmail);
        _studentId = data.StudentId;
    }

    [Test]
    public async Task Admin_CanRemoveEffect_OfActiveDiscipline()
    {
        var createReq = new CreateDisciplineRecordRequest
        {
            MaHocSinh = _studentId,
            TieuDe = "DL3 Remove Effect Test",
            MoTaViPham = "Testing removal",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var createRes = await _adminClient.PostAsJsonAsync("/api/admin/discipline-records", createReq);
        createRes.EnsureSuccessStatusCode();
        var record = await createRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        int id = record!.MaHoSoKyLuat;

        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/submit", new { });
        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/approve", new { decision = "duyet", reason = "ok" });
        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/activate", new { });

        var removeReq = new { reason = "Test remove", removalNote = "Removed for test" };
        var removeRes = await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/remove-effect", removeReq);
        
        Assert.That(removeRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await removeRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        Assert.That(updated!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Removed));
    }

    [Test]
    public async Task Admin_CanVoid_ApprovedDiscipline()
    {
        var createReq = new CreateDisciplineRecordRequest
        {
            MaHocSinh = _studentId,
            TieuDe = "DL3 Void Test",
            MoTaViPham = "Testing void",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var createRes = await _adminClient.PostAsJsonAsync("/api/admin/discipline-records", createReq);
        var record = await createRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        int id = record!.MaHoSoKyLuat;

        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/submit", new { });
        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/approve", new { decision = "duyet", reason = "ok" });

        var voidReq = new { reason = "Test void", internalNote = "Voided for test" };
        var voidRes = await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/void-approved", voidReq);
        
        Assert.That(voidRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await voidRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        Assert.That(updated!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Cancelled));
    }

    [Test]
    public async Task Student_CanAppeal_AndAdmin_CanResolve()
    {
        var createReq = new CreateDisciplineRecordRequest
        {
            MaHocSinh = _studentId,
            TieuDe = "DL3 Appeal Test",
            MoTaViPham = "Testing appeal",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var createRes = await _adminClient.PostAsJsonAsync("/api/admin/discipline-records", createReq);
        var record = await createRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        int id = record!.MaHoSoKyLuat;

        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/submit", new { });
        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/approve", new { decision = "duyet", reason = "ok" });
        await _adminClient.PostAsJsonAsync($"/api/admin/discipline-records/{id}/activate", new { });

        var appealReq = new { reason = "I am innocent" };
        var appealRes = await _studentClient.PostAsJsonAsync($"/api/student/discipline-records/{id}/appeals", appealReq);
        
        Assert.That(appealRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var appeal = await appealRes.Content.ReadFromJsonAsync<DisciplineAppealListItemDto>();
        Assert.That(appeal!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineAppealStatuses.Pending));
        int appealId = appeal.MaKhieuNaiKyLuat;

        var resolveReq = new { decision = "chap_nhan", reason = "Ok, we checked", removeEffect = true };
        var resolveRes = await _adminClient.PostAsJsonAsync($"/api/admin/discipline-appeals/{appealId}/resolve", resolveReq);
        
        Assert.That(resolveRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var resolved = await resolveRes.Content.ReadFromJsonAsync<DisciplineAppealDetailDto>();
        Assert.That(resolved!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineAppealStatuses.Accepted));
        
        var getRecordRes = await _adminClient.GetAsync($"/api/admin/discipline-records/{id}");
        var finalRecord = await getRecordRes.Content.ReadFromJsonAsync<DisciplineRecordDetailDto>();
        Assert.That(finalRecord!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Removed));
    }
}

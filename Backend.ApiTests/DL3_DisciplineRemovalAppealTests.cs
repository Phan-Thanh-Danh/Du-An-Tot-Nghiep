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
[Category("DL3")]
public class DL3_DisciplineRemovalAppealTests : ApiTestBase
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

    private async Task<(int CampusId, int AdminId, string AdminEmail, int StudentId, string StudentEmail, int OtherStudentId, string OtherStudentEmail, int TermId)> SeedDataAsync()
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
            HoTen = $"DL3 SuperAdmin {adminSuffix}",
            Email = $"dl3.superadmin.{adminSuffix}@lms.local",
            VaiTroChinh = superAdminRole,
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetTestPassword()),
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(admin);
        await db.SaveChangesAsync();
        
        Assert.That(campus, Is.Not.Null, "Thiếu đơn vị test cho DL3.");
        Assert.That(term, Is.Not.Null, "Thiếu học kỳ test cho DL3.");
        Assert.That(admin, Is.Not.Null, "Thiếu admin/campus admin test cho DL3.");

        var students = new List<NguoiDung>();
        for (var i = 0; i < 2; i++)
        {
            var suffix = Guid.NewGuid().ToString("N");
            var student = new NguoiDung
            {
                MaDonVi = campus!.MaDonVi,
                HoTen = $"DL3 Student {i + 1} {suffix}",
                Email = $"dl3.student.{suffix}@lms.local",
                VaiTroChinh = studentRole,
                TrangThai = UserStatuses.DbActive,
                MatKhauHash = PasswordHelper.HashPassword(GetTestPassword()),
                NgayTao = DateTime.UtcNow
            };
            db.NguoiDungs.Add(student);
            students.Add(student);
        }

        await db.SaveChangesAsync();

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

        var voidReq = new { reason = "Test void with valid reason", internalNote = "Voided for test" };
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

        var appealReq = new { reason = "I am innocent and request a formal review" };
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

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
[Category("DL1")]
public class DL1_DisciplineRecordCreationTests : ApiTestBase
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

        var campusAdminRole = AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin);

        Assert.That(campus, Is.Not.Null, "Thiếu đơn vị test cho DL1.");
        Assert.That(term, Is.Not.Null, "Thiếu học kỳ test cho DL1.");

        var admin = await CreateUserAsync(db, campus!.MaDonVi, campusAdminRole, "dl1.campusadmin");
        var student = await CreateStudentAsync(db, campus.MaDonVi, "dl1.student");

        // Get a student in another campus (or just fall back if only 1 campus exists)
        var otherCampus = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi != campus.MaDonVi);
        NguoiDung otherStudent;
        if (otherCampus != null)
        {
            otherStudent = await CreateStudentAsync(db, otherCampus.MaDonVi, "dl1.otherstudent");
        }
        else
        {
            otherStudent = student;
        }

        return (
            campus!.MaDonVi, 
            admin!.MaNguoiDung, admin.Email, 
            student!.MaNguoiDung, student.Email, 
            otherStudent!.MaNguoiDung, otherStudent.Email,
            term!.MaHocKy
        );
    }

    private async Task<NguoiDung> CreateStudentAsync(ApplicationDbContext db, int maDonVi, string prefix)
    {
        return await CreateUserAsync(db, maDonVi, AuthRoles.ToDatabaseCode(AuthRoles.Student), prefix);
    }

    private async Task<NguoiDung> CreateUserAsync(ApplicationDbContext db, int maDonVi, string role, string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var user = new NguoiDung
        {
            MaDonVi = maDonVi,
            HoTen = $"{prefix} {suffix}",
            Email = $"{prefix}.{suffix}@lms.local",
            VaiTroChinh = role,
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetTestPassword()),
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Test]
    public async Task AdminCreatesDraftDisciplineRecordForStudentInScope_ReturnsCreated()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            MaHocKy = seed.TermId,
            TieuDe = "Vi pham 1",
            MoTaViPham = "Mo ta vi pham detail",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };

        var response = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DisciplineRecordResultDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Draft));

        // verify audit
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        
        var audit = await db.NhatKyKiemToans
            .OrderByDescending(x => x.ThoiDiemThayDoi)
            .FirstOrDefaultAsync(x => x.NguoiThayDoi == seed.AdminId && x.HanhDong == RewardDisciplineConstants.DisciplineAuditActions.CreateDisciplineRecord);
        Assert.That(audit, Is.Not.Null);
    }

    [Test]
    public async Task CreateRejectsOutOfScopeStudent()
    {
        var seed = await SeedDataAsync();
        if (seed.StudentId == seed.OtherStudentId) Assert.Ignore("Only 1 campus in DB, skipping out of scope test.");

        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.OtherStudentId,
            TieuDe = "Vi pham",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var response = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateRejectsInvalidSeverity()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            TieuDe = "Vi pham",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = "invalid_severity",
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var response = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateRejectsUnsafeEvidenceJson()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            TieuDe = "Vi pham",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder,
            EvidenceJson = JsonSerializer.Deserialize<JsonElement>("""{"password": "123"}""")
        };
        var response = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ListReturnsOnlyRecordsInAdminScope()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var response = await client.GetAsync("/api/admin/discipline-records");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var data = await response.Content.ReadFromJsonAsync<PagedResultDto<DisciplineRecordListItemDto>>();
        Assert.That(data, Is.Not.Null);
        
        // Use regex or Contains in case Admin role covers all, but CampusAdmin covers only 1
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        var admin = await db.NguoiDungs.FindAsync(seed.AdminId);
        
        if (admin!.VaiTroChinh == AuthRoles.CampusAdmin)
        {
            foreach (var item in data!.Items)
            {
                Assert.That(item.MaDonVi, Is.EqualTo(seed.CampusId));
            }
        }
    }

    [Test]
    public async Task UpdateDraftWorks()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        // Create draft
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            TieuDe = "Draft to update",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var res = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(res));
        var data = await res.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.MaHoSoKyLuat, Is.GreaterThan(0));
        
        // Update
        var upd = new UpdateDisciplineRecordRequest
        {
            TieuDe = "Updated Title",
            MoTaViPham = "Updated desc",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Moderate,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Warning
        };
        var updRes = await client.PutAsJsonAsync($"/api/admin/discipline-records/{data!.MaHoSoKyLuat}", upd);
        Assert.That(updRes.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(updRes));
    }

    [Test]
    public async Task SubmitDraftMovesToChoDuyet()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            TieuDe = "Draft to submit",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var res = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(res));
        var data = await res.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.MaHoSoKyLuat, Is.GreaterThan(0));

        var subRes = await client.PostAsync($"/api/admin/discipline-records/{data!.MaHoSoKyLuat}/submit", null);
        Assert.That(subRes.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(subRes));
        
        var subData = await subRes.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(subData!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.PendingApproval));
    }

    [Test]
    public async Task CancelRequiresReasonAndCancelsSuccessfully()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.AdminEmail);
        
        var req = new CreateDisciplineRecordRequest
        {
            MaHocSinh = seed.StudentId,
            TieuDe = "Draft to cancel",
            MoTaViPham = "Mo ta",
            NgayViPham = DateOnly.FromDateTime(DateTime.UtcNow),
            MucDoKyLuat = RewardDisciplineConstants.DisciplineLevels.Minor,
            HinhThucXuLy = RewardDisciplineConstants.DisciplineActions.Reminder
        };
        var res = await client.PostAsJsonAsync("/api/admin/discipline-records", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(res));
        var data = await res.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(data!.MaHoSoKyLuat, Is.GreaterThan(0));

        // Missing reason
        var cancelReq = new CancelDisciplineRecordRequest { Reason = "" };
        var failCancelRes = await client.PostAsJsonAsync($"/api/admin/discipline-records/{data!.MaHoSoKyLuat}/cancel", cancelReq);
        Assert.That(failCancelRes.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Valid cancel
        cancelReq.Reason = "Invalid record, cancelling it";
        var cancelRes = await client.PostAsJsonAsync($"/api/admin/discipline-records/{data.MaHoSoKyLuat}/cancel", cancelReq);
        Assert.That(cancelRes.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(cancelRes));

        var cancelData = await cancelRes.Content.ReadFromJsonAsync<DisciplineRecordResultDto>();
        Assert.That(cancelData!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Cancelled));

        // Verify it was not hard deleted
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetDbConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        
        var recordInDb = await db.HoSoKyLuats.FindAsync(data.MaHoSoKyLuat);
        Assert.That(recordInDb, Is.Not.Null);
        Assert.That(recordInDb!.TrangThai, Is.EqualTo(RewardDisciplineConstants.DisciplineStatuses.Cancelled));
    }

    [Test]
    public async Task StudentCannotAccessAdminApis()
    {
        var seed = await SeedDataAsync();
        using var client = await CreateClientAsync(seed.StudentEmail);
        
        var res = await client.GetAsync("/api/admin/discipline-records");
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}

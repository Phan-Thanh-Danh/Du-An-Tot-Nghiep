using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using Backend.Helpers;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class NT_TemplateManagementTests : ApiTestBase
{
    private new string GetSharedTestConnectionString()
    {
        return Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING")
               ?? "Server=(localdb)\\mssqllocaldb;Database=LMS_TEST_DB;Trusted_Connection=True;MultipleActiveResultSets=true";
    }

    private new string GetSharedTestPassword()
    {
        return Environment.GetEnvironmentVariable("LMS_TEST_PASSWORD") ?? "T0P_S3cr3t_2024";
    }

    private async Task<HttpClient> CreateClientAsync(string email)
    {
        var client = new HttpClient { BaseAddress = BaseUri };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = GetSharedTestPassword() });
        if (!loginResponse.IsSuccessStatusCode)
        {
            var bodyText = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed for {email}: {loginResponse.StatusCode}. Body: {bodyText}");
        }
        var body = await loginResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var token = json.RootElement.TryGetProperty("accessToken", out var accessToken)
            ? accessToken.GetString()
            : json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<NguoiDung> CreateUserAsync(ApplicationDbContext db, int? maDonVi, string role, string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var user = new NguoiDung
        {
            MaDonVi = maDonVi ?? 1, // fallback to 1 if null, but superadmin can have null DonVi if DB allows.
            HoTen = $"{prefix} {suffix}",
            Email = $"{prefix}.{suffix}@lms.local",
            VaiTroChinh = role,
            TrangThai = UserStatuses.DbActive,
            MatKhauHash = PasswordHelper.HashPassword(GetSharedTestPassword()),
            NgayTao = DateTime.UtcNow
        };
        // Let's assume MaDonVi is required for users in this DB schema.
        if (maDonVi.HasValue) user.MaDonVi = maDonVi.Value;
        else
        {
            var firstDonVi = await db.DonVis.FirstOrDefaultAsync();
            if (firstDonVi != null) user.MaDonVi = firstDonVi.MaDonVi;
        }

        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private async Task<(int campusId, HttpClient superAdmin, HttpClient campusAdmin, HttpClient student)> SetupTestEnvAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        
        var campus = await db.DonVis.FirstOrDefaultAsync();
        Assert.That(campus, Is.Not.Null, "Missing DonVi for tests.");
        
        var sa = await CreateUserAsync(db, null, AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), "nt.superadmin");
        var ca = await CreateUserAsync(db, campus.MaDonVi, AuthRoles.ToDatabaseCode(AuthRoles.CampusAdmin), "nt.campusadmin");
        var st = await CreateUserAsync(db, campus.MaDonVi, AuthRoles.ToDatabaseCode(AuthRoles.Student), "nt.student");

        return (
            campus.MaDonVi,
            await CreateClientAsync(sa.Email),
            await CreateClientAsync(ca.Email),
            await CreateClientAsync(st.Email)
        );
    }

    [Test]
    public async Task SuperAdminCanCreateAndPreviewNotificationTemplate()
    {
        var (campusId, superAdmin, _, _) = await SetupTestEnvAsync();

        // 1. Create Template
        var req = new CreateNotificationTemplateRequest
        {
            MaMau = $"TEST_TPL_{Guid.NewGuid():N}",
            TenMau = "Test Template",
            LoaiThongBao = NotificationConstants.Types.General,
            TieuDeMau = "Hello {{name}}",
            NoiDungMau = "Welcome to {{campusName}}, please review your {{document}}.",
            MaDonVi = null,
            DangHoatDong = true
        };

        var res = await superAdmin.PostAsJsonAsync("/api/admin/notification-templates", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var data = await res.Content.ReadFromJsonAsync<ApiResponseDto<NotificationTemplateDetailDto>>();
        Assert.That(data!.Data, Is.Not.Null);
        var tplId = data!.Data!.MaMauThongBao;

        // 2. Preview Template
        var previewReq = new PreviewNotificationTemplateRequest
        {
            Variables = new Dictionary<string, string>
            {
                { "name", "John Doe" },
                { "campusName", "Main Campus" }
            }
        };

        var previewRes = await superAdmin.PostAsJsonAsync($"/api/admin/notification-templates/{tplId}/preview", previewReq);
        Assert.That(previewRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var previewData = await previewRes.Content.ReadFromJsonAsync<ApiResponseDto<PreviewNotificationTemplateResultDto>>();
        Assert.That(previewData!.Data, Is.Not.Null);
        Assert.That(previewData!.Data!.RenderedTitle, Is.EqualTo("Hello John Doe"));
        Assert.That(previewData!.Data!.RenderedBody, Is.EqualTo("Welcome to Main Campus, please review your {{document}}."));
        Assert.That(previewData!.Data!.MissingVariables, Contains.Item("document"));
        Assert.That(previewData!.Data!.DetectedPlaceholders, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task StudentCannotAccessAdminTemplates()
    {
        var (_, _, _, student) = await SetupTestEnvAsync();
        var res = await student.GetAsync("/api/admin/notification-templates");
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task DuplicateMaMauInSameScopeIsRejected()
    {
        var (campusId, superAdmin, _, _) = await SetupTestEnvAsync();
        var maMau = $"DUP_TPL_{Guid.NewGuid():N}";
        var req = new CreateNotificationTemplateRequest
        {
            MaMau = maMau,
            TenMau = "Duplicate Test 1",
            LoaiThongBao = NotificationConstants.Types.General,
            TieuDeMau = "Title",
            NoiDungMau = "Body",
            MaDonVi = campusId
        };

        var res1 = await superAdmin.PostAsJsonAsync("/api/admin/notification-templates", req);
        Assert.That(res1.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var req2 = new CreateNotificationTemplateRequest
        {
            MaMau = maMau,
            TenMau = "Duplicate Test 2",
            LoaiThongBao = NotificationConstants.Types.General,
            TieuDeMau = "Title 2",
            NoiDungMau = "Body 2",
            MaDonVi = campusId
        };
        var res2 = await superAdmin.PostAsJsonAsync("/api/admin/notification-templates", req2);
        Assert.That(res2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AdminDeactivatesTemplateWithoutDeleting()
    {
        var (campusId, superAdmin, _, _) = await SetupTestEnvAsync();
        var req = new CreateNotificationTemplateRequest
        {
            MaMau = $"DEL_TPL_{Guid.NewGuid():N}",
            TenMau = "Delete Test",
            LoaiThongBao = NotificationConstants.Types.General,
            TieuDeMau = "Title",
            NoiDungMau = "Body"
        };
        var res = await superAdmin.PostAsJsonAsync("/api/admin/notification-templates", req);
        var data = await res.Content.ReadFromJsonAsync<ApiResponseDto<NotificationTemplateDetailDto>>();
        var tplId = data!.Data!.MaMauThongBao;

        var delRes = await superAdmin.DeleteAsync($"/api/admin/notification-templates/{tplId}");
        Assert.That(delRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // It should be hard deleted if not system template. Wait, logic says:
        // System template -> deactivate. Others -> hard delete.
        // Let's create a system template to test deactivation.
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);
        var sysTpl = new MauThongBao { MaMau = $"SYS_{Guid.NewGuid():N}", LoaiSuKien = "system", KenhGui = "in_app", MauNoiDung = "body", LaHeThong = true, DangHoatDong = true };
        db.MauThongBaos.Add(sysTpl);
        await db.SaveChangesAsync();

        var sysDelRes = await superAdmin.DeleteAsync($"/api/admin/notification-templates/{sysTpl.MaMauTb}");
        Assert.That(sysDelRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var getSysRes = await superAdmin.GetAsync($"/api/admin/notification-templates/{sysTpl.MaMauTb}");
        Assert.That(getSysRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sysData = await getSysRes.Content.ReadFromJsonAsync<ApiResponseDto<NotificationTemplateDetailDto>>();
        Assert.That(sysData!.Data!.DangHoatDong, Is.False, "System template should be deactivated instead of deleted");
    }
}

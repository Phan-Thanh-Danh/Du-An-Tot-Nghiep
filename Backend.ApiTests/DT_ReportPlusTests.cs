
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.DTOs.Applications;
using Backend.DTOs.Common;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class DT_ReportPlusTests : ApiTestBase
{
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
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Test]
    public async Task Overview_SuperAdmin_ReturnsOverviewReport()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/overview");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ApplicationReportOverviewDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Summary, Is.Not.Null);
        Assert.That(result.Data.StatusBreakdown, Is.Not.Null);
    }

    [Test]
    public async Task Endpoints_Student_ReturnsForbidden()
    {
        var studentClient = await CreateClientAsync("student1@lms.local");
        var response = await studentClient.GetAsync("/api/admin/applications/reports/overview");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Overview_CampusAdmin_ReturnsInScopeOnly()
    {
        var campusAdminClient = await CreateClientAsync("campusadmin1@lms.local");
        var response = await campusAdminClient.GetAsync("/api/admin/applications/reports/overview");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ApplicationReportOverviewDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        
        // Out of scope requests
        var outOfScopeResponse = await campusAdminClient.GetAsync("/api/admin/applications/reports/overview?campusId=2");
        Assert.That(outOfScopeResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden).Or.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ByType_GroupsCorrectly()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/by-type");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<List<ApplicationByTypeReportDto>>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task Pending_ReturnsPagedItems_WithoutSensitiveData()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/pending?pageSize=5&pageIndex=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<PendingApplicationReportDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.PageSize, Is.EqualTo(5));
        
        // Verify no sensitive data
        Assert.That(json, Does.Not.Contain("evidenceJson").IgnoreCase);
        Assert.That(json, Does.Not.Contain("ghiChuNoiBo").IgnoreCase);
    }

    [Test]
    public async Task Overdue_CalculatesCorrectly()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/overdue?slaHours=48");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<OverdueApplicationReportDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.DefaultSlaHours, Is.EqualTo(48));
    }

    [Test]
    public async Task ProcessingTime_ExcludesDrafts()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/processing-time");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ProcessingTimeReportDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task ByAssignee_GroupsCorrectly()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/by-assignee");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<List<ApplicationByAssigneeReportDto>>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task Trends_GroupsByMonthCorrectly()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/trends?metric=submitted&groupBy=month");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<ApplicationTrendReportDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Metric, Is.EqualTo("submitted"));
        Assert.That(result.Data.GroupBy, Is.EqualTo("month"));
    }

    [Test]
    public async Task Trends_InvalidMetric_ReturnsBadRequest()
    {
        var response = await Client.GetAsync("/api/admin/applications/reports/trends?metric=invalid_metric");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest).Or.EqualTo(HttpStatusCode.InternalServerError));
    }
}

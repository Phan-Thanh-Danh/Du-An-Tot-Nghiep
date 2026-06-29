using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.DTOs.Common;
using Backend.DTOs.Notification;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class NT_SpecializedNotificationsTests : ApiTestBase
{
    [Test]
    public async Task GetCategories_ReturnsSuccess()
    {
        var response = await Client.GetAsync("/api/admin/specialized-notifications/categories");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<SpecializedNotificationCategoryDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Categories, Contains.Item("hoc_phi"));
        Assert.That(result.Data.Categories, Contains.Item("hoc_vu"));
        Assert.That(result.Data.Categories, Contains.Item("khan_cap"));
        Assert.That(result.Data.Categories, Contains.Item("bao_tri"));
    }

    [Test]
    public async Task PreviewRecipients_AllStudents_ReturnsActiveStudents()
    {
        var request = new PreviewSpecializedRecipientsRequest
        {
            Target = new SpecializedNotificationTargetDto
            {
                TargetType = "all_students"
            }
        };

        var response = await Client.PostAsJsonAsync("/api/admin/specialized-notifications/preview-recipients", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<PreviewSpecializedRecipientsResultDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Success, Is.True);
        Assert.That(result.Data!.TotalRecipients, Is.GreaterThan(0));
        Assert.That(result.Data.SampleRecipients.All(r => r.VaiTroChinh == "sinh_vien"), Is.True);
    }

    [Test]
    public async Task PreviewRecipients_Department_ReturnsUnsupported()
    {
        var request = new PreviewSpecializedRecipientsRequest
        {
            Target = new SpecializedNotificationTargetDto
            {
                TargetType = "department"
            }
        };

        var response = await Client.PostAsJsonAsync("/api/admin/specialized-notifications/preview-recipients", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<PreviewSpecializedRecipientsResultDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data!.UnsupportedFilters, Contains.Item("department"));
    }

    [Test]
    public async Task SendTuitionNotification_Success_And_Idempotent()
    {
        var request = new SendTuitionNotificationRequest
        {
            Target = new SpecializedNotificationTargetDto
            {
                TargetType = "all_students"
            },
            Title = "Tuition Fee Reminder",
            Body = "Please pay your tuition fees by the due date.",
            AuditNote = "Sending tuition reminder to all students.",
            ClientRequestId = "test_tuition_" + Guid.NewGuid().ToString()
        };

        // First send
        var response = await Client.PostAsJsonAsync("/api/admin/specialized-notifications/tuition", request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponseDto<SpecializedNotificationSendResultDto>>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
        Assert.That(result!.Success, Is.True);
        Assert.That(result.Message.ToLower(), Does.Contain("thành công"));

        // Second send (Idempotent)
        var response2 = await Client.PostAsJsonAsync("/api/admin/specialized-notifications/tuition", request);
        response2.EnsureSuccessStatusCode();
        var json2 = await response2.Content.ReadAsStringAsync();
        var result2 = JsonSerializer.Deserialize<ApiResponseDto<SpecializedNotificationSendResultDto>>(
            json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
        Assert.That(result2!.Success, Is.True);
        Assert.That(result2.Message.ToLower(), Does.Contain("idempotent"));
    }
}

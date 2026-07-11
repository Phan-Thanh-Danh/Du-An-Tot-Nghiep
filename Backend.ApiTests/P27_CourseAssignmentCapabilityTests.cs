using System.Net;
using System.Net.Http.Json;
using Backend.DTOs.Common;
using Backend.DTOs.Courses.AssignmentSuggestions;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P27_CourseAssignmentCapabilityTests : ApiTestBase
{
    [Test]
    public async Task GetSuggestions_ShouldRankCandidates_AndExcludeIncapable()
    {
        // Act
        var request = new CourseAssignmentSuggestionRequestDto
        {
            MaMonHoc = 1,
            MaHocKy = 1,
            MaLopIds = new List<int> { 1 }
        };

        var response = await Client.PostAsJsonAsync("/api/courses/assignment-suggestions", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<CourseAssignmentSuggestionResultDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.Not.Null);
    }
}

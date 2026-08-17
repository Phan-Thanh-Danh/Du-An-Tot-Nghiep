using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

public class R2PresignedUrlTest
{
    [Test]
    public void GetPresignedStreamUrl_WithPublicDomain_ReturnsEncodedDirectPlaybackUrl()
    {
        var settings = new R2StorageSettings
        {
            Endpoint = "https://account.example.invalid",
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret-key",
            BucketName = "test-bucket",
            PublicDomain = "https://media.example"
        };
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());
        environment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());
        var service = new R2StorageService(
            settings,
            NullLogger<R2StorageService>.Instance,
            environment.Object);

        var playbackUrl = service.GetPresignedStreamUrl(
            "videos/2026/08/15/CNTT/C#/BÀI 1/Bài mở đầu.mp4");

        Assert.That(playbackUrl, Is.EqualTo(
            "https://media.example/videos/2026/08/15/CNTT/C%23/B%C3%80I%201/B%C3%A0i%20m%E1%BB%9F%20%C4%91%E1%BA%A7u.mp4"));
    }
}

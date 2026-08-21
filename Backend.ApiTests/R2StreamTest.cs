using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

public class R2StreamTest
{
    [Test]
    public async Task GetFileStreamAsync_WithLocalFallback_ReturnsSeekableVideoStream()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"lms-storage-test-{Guid.NewGuid():N}");
        var storageKey = "videos/local-video.mp4";
        var localPath = Path.Combine(tempRoot, "uploads", "videos", "local-video.mp4");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await File.WriteAllBytesAsync(localPath, new byte[] { 0, 1, 2, 3, 4 });

            var environment = new Mock<IWebHostEnvironment>();
            environment.Setup(x => x.WebRootPath).Returns(tempRoot);
            environment.Setup(x => x.ContentRootPath).Returns(tempRoot);
            var service = new R2StorageService(
                new R2StorageSettings(),
                NullLogger<R2StorageService>.Instance,
                environment.Object);

            var (stream, contentType, contentLength) = await service.GetFileStreamAsync(storageKey);
            await using (stream)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(stream.CanSeek, Is.True);
                    Assert.That(contentType, Is.EqualTo("video/mp4"));
                    Assert.That(contentLength, Is.EqualTo(5));
                });
            }
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class StorageControllerSeekingTest
    {
        [Test]
        public async Task StreamFile_WhenDirectR2UrlIsAvailable_RedirectsBrowserToObjectStorage()
        {
            const string key = "videos/2026/08/15/CNTT/SQL/BAI-1/video.mp4";
            const string playbackUrl = "https://media.example/videos/2026/08/15/CNTT/SQL/BAI-1/video.mp4";
            var storage = new Mock<IR2StorageService>();
            storage.Setup(x => x.GetPresignedStreamUrl(key, null)).Returns(playbackUrl);
            var controller = new StorageController(storage.Object);

            var result = await controller.StreamFile(key, default);

            Assert.That(result, Is.TypeOf<RedirectResult>());
            var redirectResult = (RedirectResult)result;
            Assert.That(redirectResult.Url, Is.EqualTo(playbackUrl));
            storage.Verify(x => x.GetFileStreamAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task StreamFile_WhenUsingLocalSeekableFile_EnablesRangeProcessing()
        {
            const string key = "videos/local-video.mp4";
            var bytes = Encoding.UTF8.GetBytes("local seekable video test data");
            var stream = new MemoryStream(bytes);
            var storage = new Mock<IR2StorageService>();
            storage.Setup(x => x.GetPresignedStreamUrl(key, null)).Returns((string?)null);
            storage.Setup(x => x.GetFileStreamAsync(key, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((stream, "video/mp4", (long?)bytes.Length));
            var controller = new StorageController(storage.Object);

            var result = await controller.StreamFile(key, default);

            Assert.That(result, Is.TypeOf<FileStreamResult>());
            Assert.That(((FileStreamResult)result).EnableRangeProcessing, Is.True);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TestStreamLesson69And70
    {
        [Test]
        public async Task TestStorageStreamForLesson69And70()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var l69 = await db.BaiHocs.FirstOrDefaultAsync(b => b.MaBaiHoc == 69);
            var l70 = await db.BaiHocs.FirstOrDefaultAsync(b => b.MaBaiHoc == 70);

            TestContext.Progress.WriteLine($"Lesson 69 Url: {l69?.UrlTapTin}");
            TestContext.Progress.WriteLine($"Lesson 70 Url: {l70?.UrlTapTin}");

            var r2Settings = new R2StorageSettings
            {
                AccountId = "dummy",
                AccessKeyId = "dummy",
                SecretAccessKey = "dummy",
                Endpoint = "https://dummy.r2.cloudflarestorage.com",
                BucketName = "lms-media",
                PublicDomain = "https://media.lms.local"
            };

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            var storageService = new R2StorageService(
                r2Settings,
                NullLogger<R2StorageService>.Instance,
                mockEnv.Object
            );

            var controller = new StorageController(storageService);

            // Test key parsing for lesson 69
            var key69 = "videos/2026/08/15/CNTT/SQL/BÀI 3/Cách tạo Bảng (Table) và thao tác cấu trúc dữ liệu.mp4";
            var directUrl69 = storageService.GetPresignedStreamUrl(key69);
            TestContext.Progress.WriteLine($"Direct URL 69: {directUrl69}");

            var res69 = await controller.StreamFile(key69, CancellationToken.None);
            TestContext.Progress.WriteLine($"Stream result 69: {res69.GetType().Name}");
            if (res69 is RedirectResult redir69)
            {
                TestContext.Progress.WriteLine($"Redirect URL 69: {redir69.Url}");
            }
        }
    }
}

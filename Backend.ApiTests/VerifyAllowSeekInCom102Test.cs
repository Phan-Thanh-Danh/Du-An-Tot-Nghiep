using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.StudentCourse;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class VerifyAllowSeekInCom102Test
    {
        [Test]
        public async Task VerifyCom102AllowSeekResponse()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var env = new Moq.Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(AppContext.BaseDirectory);
            env.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);

            var settings = new R2StorageSettings
            {
                Endpoint = "https://87934b0fb36afe0a6b19db75efc7fe24.r2.cloudflarestorage.com",
                AccessKeyId = "872e796be9c27223e4d2b7fe48afd75e",
                SecretAccessKey = "46a0c09da41ff2f0a7cc7aacad3bb8ed6c418eb4530617862c860d248bf2e28b",
                BucketName = "aet-lms-media"
            };
            var storageService = new R2StorageService(settings, NullLogger<R2StorageService>.Instance, env.Object);

            var controller = new StudentCoursesController();
            var httpContext = new DefaultHttpContext();
            var studentUser = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email.Contains("p12test_student01"));
            Assert.That(studentUser, Is.Not.Null);

            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = studentUser.MaNguoiDung,
                Email = studentUser.Email,
                Role = "Student"
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.GetCourseDetail("COM102", db, storageService);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiRes = okResult.Value as Backend.DTOs.Common.ApiResponseDto<CourseDetailResponseDto>;
            Assert.That(apiRes, Is.Not.Null);
            Assert.That(apiRes.Data, Is.Not.Null);

            var allLessons = apiRes.Data.Lessons.SelectMany(ch => ch.Lessons).ToList();
            TestContext.Progress.WriteLine($"Total lessons returned: {allLessons.Count}");
            for (int i = 0; i < allLessons.Count; i++)
            {
                var l = allLessons[i];
                TestContext.Progress.WriteLine($"Lesson {i + 1}: ID={l.Id}, Title='{l.Title}', AllowSeek={l.AllowSeek}");
            }
        }
    }
}

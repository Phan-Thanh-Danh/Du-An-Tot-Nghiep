using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.StudentCourse;
using Backend.Models;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class SqlLessonDetailInspectorTest
    {
        [Test]
        public async Task InspectSqlLessonDetail()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var sqlLesson = await db.BaiHocs
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.MonHoc)
                .FirstOrDefaultAsync(b => b.TieuDe.Contains("Giới thiệu tổng quan về SQL"));

            Assert.That(sqlLesson, Is.Not.Null);
            TestContext.Progress.WriteLine($"Lesson ID: {sqlLesson.MaBaiHoc}");
            TestContext.Progress.WriteLine($"Title: {sqlLesson.TieuDe}");
            TestContext.Progress.WriteLine($"Type: {sqlLesson.LoaiBaiHoc}");
            TestContext.Progress.WriteLine($"Duration: {sqlLesson.ThoiLuongGiay}");
            TestContext.Progress.WriteLine($"UrlTapTin: {sqlLesson.UrlTapTin}");
            TestContext.Progress.WriteLine($"Subject Code: {sqlLesson.Chuong?.MonHoc?.MaCodeMonHoc}");

            var settings = new R2StorageSettings
            {
                Endpoint = "https://account.example.invalid",
                AccessKeyId = "test-access-key",
                SecretAccessKey = "test-secret-key",
                BucketName = "test-bucket",
                PublicDomain = "https://media.example"
            };

            var env = new Moq.Mock<IWebHostEnvironment>();
            var storageService = new R2StorageService(settings, NullLogger<R2StorageService>.Instance, env.Object);

            var controller = new StudentCoursesController();
            var httpContext = new DefaultHttpContext();
            var studentUser = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "p12test_student01@lms.local");
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

            var firstChapter = apiRes.Data.Lessons.FirstOrDefault();
            Assert.That(firstChapter, Is.Not.Null);
            TestContext.Progress.WriteLine($"First Chapter: {firstChapter.Chapter} - {firstChapter.Title}");

            var firstLesson = firstChapter.Lessons.FirstOrDefault();
            Assert.That(firstLesson, Is.Not.Null);
            TestContext.Progress.WriteLine($"First Lesson ID: {firstLesson.Id}");
            TestContext.Progress.WriteLine($"First Lesson Title: {firstLesson.Title}");
            TestContext.Progress.WriteLine($"First Lesson Duration: {firstLesson.Duration}");
            TestContext.Progress.WriteLine($"First Lesson Url: {firstLesson.Url}");
        }
    }
}

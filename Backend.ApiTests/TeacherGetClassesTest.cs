using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Services.Grading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TeacherGetClassesTest
    {
        [Test]
        public async Task TestGetClassesForLecturer()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(lecturer, Is.Not.Null);

            var controller = new TeacherClassesController(db, Moq.Mock.Of<IGradeAggregationService>());
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = lecturer.MaNguoiDung,
                Email = lecturer.Email,
                Role = "Teacher"
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.GetClasses();
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            TestContext.Progress.WriteLine($"GetClasses response for lecturer ID={lecturer.MaNguoiDung}: {json}");

            // Also check how many students are in LopHanhChinh ID=2
            var studentsInClass2 = await db.NguoiDungs
                .Where(u => u.MaLop == 2)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Students in Class 2 (SD1902): Count={studentsInClass2.Count}");
            foreach (var s in studentsInClass2)
            {
                TestContext.Progress.WriteLine($" - ID={s.MaNguoiDung}, Name={s.HoTen}, Email={s.Email}, VaiTroChinh={s.VaiTroChinh}");
            }
        }

        [Test]
        public async Task TestGetCoursesWithProgress()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == 15);
            Assert.That(lecturer, Is.Not.Null);

            var controller = new TeacherClassesController(db, Moq.Mock.Of<IGradeAggregationService>());
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = lecturer.MaNguoiDung,
                Email = lecturer.Email,
                Role = "Teacher"
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.GetCourses();
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            TestContext.Progress.WriteLine($"GetCourses response: {json}");

            var progressResult = await controller.GetClassProgress(3184);
            var okProgress = progressResult.Result as OkObjectResult;
            Assert.That(okProgress, Is.Not.Null);

            var progressJson = System.Text.Json.JsonSerializer.Serialize(okProgress.Value);
            TestContext.Progress.WriteLine($"GetClassProgress(3184) response: {progressJson}");

            // Test CourseService
            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor
            {
                HttpContext = httpContext
            };
            var courseService = new Backend.Services.Courses.CourseService(
                db,
                httpContextAccessor,
                Moq.Mock.Of<Backend.Services.Audit.IAuditLogService>(),
                Moq.Mock.Of<Backend.Services.Courses.ICourseTeacherEligibilityService>(),
                Moq.Mock.Of<Backend.Services.AcademicSchedulingContext.IAcademicSchedulingContextService>());
            var coursesPaged = await courseService.GetAsync(new Backend.DTOs.Courses.KhoaHocQueryParameters { PageSize = 50 });
            var pagedJson = System.Text.Json.JsonSerializer.Serialize(coursesPaged);
            TestContext.Progress.WriteLine($"CourseService.GetAsync response: {pagedJson}");
        }
    }
}

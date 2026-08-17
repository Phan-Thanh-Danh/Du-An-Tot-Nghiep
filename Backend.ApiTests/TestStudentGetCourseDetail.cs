using System;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentCourse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TestStudentGetCourseDetail
    {
        [Test]
        public async Task TestGetCourseDetailCom102()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // Find student
            var student = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email != null && u.Email.Contains("student"));

            Assert.That(student, Is.Not.Null, "Student should exist");
            TestContext.Progress.WriteLine($"Testing with Student: ID={student!.MaNguoiDung}, Email={student.Email}, MaLop={student.MaLop}");

            var controller = new StudentCoursesController();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = student.MaNguoiDung,
                Email = student.Email ?? "",
                Role = "Student",
                CampusId = student.MaDonVi,
                Status = "hoat_dong"
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            try
            {
                var result = await controller.GetCourseDetail("COM102", db, null!);
                if (result.Result is OkObjectResult okObj)
                {
                    TestContext.Progress.WriteLine($"OK Value: {okObj.Value}");
                    if (okObj.Value is ApiResponseDto<CourseDetailResponseDto> apiRes)
                    {
                        TestContext.Progress.WriteLine($"Course Title: {apiRes.Data?.Course?.Title}");
                        TestContext.Progress.WriteLine($"Chapters Count: {apiRes.Data?.Lessons?.Count}");
                        foreach (var ch in apiRes.Data?.Lessons ?? new())
                        {
                            TestContext.Progress.WriteLine($"  Chapter {ch.Chapter}: {ch.Title}, Lessons: {ch.Lessons?.Count}");
                            foreach (var l in ch.Lessons ?? new())
                            {
                                TestContext.Progress.WriteLine($"    Lesson {l.Id}: {l.Title} (Url: {l.Url})");
                            }
                        }
                    }
                }
                else if (result.Result is ObjectResult objRes)
                {
                    TestContext.Progress.WriteLine($"ObjectResult Status: {objRes.StatusCode}, Value: {objRes.Value}");
                }
                else
                {
                    TestContext.Progress.WriteLine($"Result: {result.Result}");
                }
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"EXCEPTION: {ex.GetType().FullName}: {ex.Message}");
                TestContext.Progress.WriteLine($"STACKTRACE: {ex.StackTrace}");
                throw;
            }
        }
    }
}

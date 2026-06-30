using Backend.DTOs.Common;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/courses")]
public class StudentCoursesController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Student")]
    public ActionResult<ApiResponseDto<List<CourseProgressDto>>> GetCourses()
    {
        var courses = new List<CourseProgressDto>
        {
            new() { Id = "ctdl", Name = "Cấu trúc dữ liệu & Giải thuật", Code = "CTDL101", Lecturer = "TS. Nguyễn Minh Khoa", Progress = 72, Completed = 9, Total = 12, Status = "Cần tiếp tục", StatusVariant = "warning" },
            new() { Id = "web", Name = "Lập trình Web nâng cao", Code = "LTW301", Lecturer = "ThS. Lê Phương Mai", Progress = 86, Completed = 12, Total = 14, Status = "Sắp hoàn thành", StatusVariant = "success" },
            new() { Id = "db", Name = "Hệ quản trị CSDL", Code = "HQTCSDL401", Lecturer = "ThS. Trần Quốc Việt", Progress = 100, Completed = 15, Total = 15, Status = "Hoàn thành", StatusVariant = "neutral" }
        };

        return Ok(ApiResponseDto<List<CourseProgressDto>>.Ok(courses));
    }
}

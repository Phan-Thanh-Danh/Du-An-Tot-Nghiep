using Backend.DTOs.Common;
using Backend.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = "Student")]
public class StudentDashboardController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponseDto<StudentDashboardDto>> GetDashboard()
    {
        var dashboardData = new StudentDashboardDto
        {
            Student = new StudentInfoDto
            {
                Name = "Nguyễn Văn An",
                Code = "SV2026001",
                ClassName = "SE1601",
                Semester = "HK2 2025-2026",
            },
            WeekProgress = 68,
            FocusSummary = new FocusSummaryDto
            {
                ClassesToday = 3,
                AssignmentsDue = 4,
                CompletedThisWeek = 11,
                NearestDeadline = "23:59 hôm nay",
                Gpa = "8.2",
            },
            Kpis = new List<KpiDto>
            {
                new() { Id = "courses", Label = "Khóa học đang học", Value = "6", Trend = "+2 khóa mới kỳ này", Tone = "blue", Route = "/student/courses" },
                new() { Id = "assignments", Label = "Bài tập cần xử lý", Value = "4", Trend = "2 bài sắp đến hạn", Tone = "amber", Route = "/student/assignments" },
                new() { Id = "gpa", Label = "GPA học kỳ", Value = "8.2", Trend = "+0.4 so với kỳ trước", Tone = "violet", Route = "/student/grades" },
                new() { Id = "attendance", Label = "Chuyên cần", Value = "92%", Trend = "2 buổi vắng", Tone = "teal", Route = "/student/attendance" }
            },
            Courses = new List<CourseProgressDto>
            {
                new() { Id = "ctdl", Name = "Cấu trúc dữ liệu & Giải thuật", Code = "CTDL101", Lecturer = "TS. Nguyễn Minh Khoa", Progress = 72, Completed = 9, Total = 12, Status = "Cần tiếp tục", StatusVariant = "warning" },
                new() { Id = "web", Name = "Lập trình Web nâng cao", Code = "LTW301", Lecturer = "ThS. Lê Phương Mai", Progress = 86, Completed = 12, Total = 14, Status = "Sắp hoàn thành", StatusVariant = "success" },
                new() { Id = "db", Name = "Hệ quản trị CSDL", Code = "HQTCSDL401", Lecturer = "ThS. Trần Quốc Việt", Progress = 54, Completed = 7, Total = 13, Status = "Đang học", StatusVariant = "info" },
                new() { Id = "math", Name = "Toán rời rạc", Code = "TRR201", Lecturer = "TS. Phạm Thu Hà", Progress = 61, Completed = 8, Total = 13, Status = "Đang học", StatusVariant = "info" }
            },
            Assignments = new List<AssignmentDto>
            {
                new() { Id = "a1", Title = "Phân tích độ phức tạp thuật toán", Course = "Cấu trúc dữ liệu", Deadline = "Hôm nay · 23:59", Status = "Sắp đến hạn", Variant = "warning", Priority = "high" },
                new() { Id = "a2", Title = "Bài tập thực hành Layout", Course = "Lập trình Web", Deadline = "Ngày mai · 17:00", Status = "Chưa làm", Variant = "danger", Priority = "high" },
                new() { Id = "a3", Title = "Thiết kế ERD cơ bản", Course = "Hệ quản trị CSDL", Deadline = "3 ngày nữa", Status = "Đang làm", Variant = "info", Priority = "medium" },
                new() { Id = "a4", Title = "Chứng minh đồ thị", Course = "Toán rời rạc", Deadline = "Tuần sau", Status = "Chưa làm", Variant = "neutral", Priority = "low" }
            },
            Schedule = new List<ScheduleDto>
            {
                new() { Id = "s1", Course = "Cấu trúc dữ liệu", Code = "CTDL101", Time = "07:30 - 09:45", Room = "P.402 - Tòa Alpha", Type = "Lý thuyết", TypeVariant = "info", Status = "Đang diễn ra", StatusVariant = "success" },
                new() { Id = "s2", Course = "Lập trình Web", Code = "LTW301", Time = "10:00 - 12:15", Room = "Lab 2 - Tòa Beta", Type = "Thực hành", TypeVariant = "warning", Status = "Sắp tới", StatusVariant = "secondary" },
                new() { Id = "s3", Course = "Hệ quản trị CSDL", Code = "HQTCSDL401", Time = "13:30 - 15:45", Room = "P.201 - Tòa Alpha", Type = "Lý thuyết", TypeVariant = "info", Status = "Chiều nay", StatusVariant = "secondary" }
            },
            Grades = new List<GradeDto>
            {
                new() { Id = "g1", Course = "Kiến trúc máy tính", Code = "ARC201", ExamType = "Thi cuối kỳ", Score = 8.5, Date = "12/03/2026", Status = "passed" },
                new() { Id = "g2", Course = "Cấu trúc dữ liệu", Code = "CTDL101", ExamType = "Bài tập 1", Score = 9.0, Date = "10/03/2026", Status = "passed" },
                new() { Id = "g3", Course = "Lập trình Web", Code = "LTW301", ExamType = "Quiz 1", Score = 7.5, Date = "08/03/2026", Status = "passed" }
            },
            Tuition = new TuitionDto
            {
                TotalDue = "15.400.000",
                Deadline = "15/04/2026",
                Progress = 0,
                Status = "Chưa thanh toán",
                StatusVariant = "danger"
            },
            Registration = new RegistrationDto
            {
                Semester = "HK3 2025-2026",
                StartDate = "20/05/2026",
                Status = "Sắp mở",
                Action = "Xem trước lịch"
            },
            Attendance = new AttendanceHealthDto
            {
                Score = 92,
                Status = "Tốt",
                Tone = "teal",
                Advice = "Tiếp tục duy trì tiến độ học tập và đi học đầy đủ bạn nhé!",
                Risks = new List<AttendanceRiskDto>
                {
                    new() { Id = "r1", Course = "Toán rời rạc", Code = "TRR201", Absent = 2, Limit = 3, Percent = 66 }
                }
            },
            Notifications = new List<NotificationDto>
            {
                new() { Id = "n1", Title = "Thông báo nộp học phí HK2", Content = "Hạn chót nộp học phí HK2 2025-2026 là ngày 15/04/2026...", Time = "2 giờ trước", Category = "Tài chính", Unread = true },
                new() { Id = "n2", Title = "Đổi phòng học môn Cấu trúc dữ liệu", Content = "Lớp CTDL101 chiều nay chuyển sang học tại P.405 Tòa Alpha.", Time = "5 giờ trước", Category = "Học vụ", Unread = true },
                new() { Id = "n3", Title = "Mở đăng ký môn học HK3", Content = "Hệ thống sẽ mở cổng đăng ký môn học HK3 vào ngày 20/05...", Time = "Hôm qua", Category = "Học vụ", Unread = false }
            }
        };

        return Ok(ApiResponseDto<StudentDashboardDto>.Ok(dashboardData));
    }
}

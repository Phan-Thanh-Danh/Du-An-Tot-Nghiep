namespace Backend.DTOs.StudentDashboard;

public class StudentDashboardDto
{
    public StudentInfoDto Student { get; set; } = new();
    public int WeekProgress { get; set; }
    public FocusSummaryDto FocusSummary { get; set; } = new();
    public List<KpiDto> Kpis { get; set; } = new();
    public List<CourseProgressDto> Courses { get; set; } = new();
    public List<AssignmentDto> Assignments { get; set; } = new();
    public List<ScheduleDto> Schedule { get; set; } = new();
    public List<GradeDto> Grades { get; set; } = new();
    public TuitionDto Tuition { get; set; } = new();
    public RegistrationDto Registration { get; set; } = new();
    public AttendanceHealthDto Attendance { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class StudentInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
}

public class FocusSummaryDto
{
    public int ClassesToday { get; set; }
    public int AssignmentsDue { get; set; }
    public int CompletedThisWeek { get; set; }
    public string NearestDeadline { get; set; } = string.Empty;
    public string Gpa { get; set; } = string.Empty;
}

public class KpiDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
}

public class CourseProgressDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Lecturer { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int Completed { get; set; }
    public int Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusVariant { get; set; } = string.Empty;
}

public class AssignmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class ScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeVariant { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusVariant { get; set; } = string.Empty;
}

public class GradeDto
{
    public string Id { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TuitionDto
{
    public string TotalDue { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusVariant { get; set; } = string.Empty;
}

public class RegistrationDto
{
    public string Semester { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class AttendanceHealthDto
{
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
    public List<AttendanceRiskDto> Risks { get; set; } = new();
}

public class AttendanceRiskDto
{
    public string Id { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Absent { get; set; }
    public int Limit { get; set; }
    public int Percent { get; set; }
}

public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Unread { get; set; }
}

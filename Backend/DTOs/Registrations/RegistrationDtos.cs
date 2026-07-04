using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Registrations;

public class RegistrationPeriodDto
{
    public int Id { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public DateTime OpenDate { get; set; }
    public DateTime CloseDate { get; set; }
    public DateOnly? WithdrawDeadline { get; set; }
    public int MaxCredits { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int ClassCount { get; set; }
}

public class UpsertRegistrationPeriodRequest
{
    [Required]
    public int MaHocKy { get; set; }

    public int? MaDonVi { get; set; }

    [Required]
    public DateTime OpenDate { get; set; }

    [Required]
    public DateTime CloseDate { get; set; }

    [Range(1, 60)]
    public int MaxCredits { get; set; } = 24;
}

public class CourseSectionRegistrationDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    public int MaMonHoc { get; set; }
    public int? MaKhoaHoc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Semester { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Enrolled { get; set; }
    public int Waitlist { get; set; }
    public int MinEnroll { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RegistrationStatus { get; set; } = string.Empty;
    public int? RegistrationId { get; set; }
    public int? WaitlistPosition { get; set; }
}

public class UpdateCourseSectionCapacityRequest
{
    [Range(1, 1000)]
    public int Capacity { get; set; }

    public string? Reason { get; set; }
}

public class CourseSectionStatusRequest
{
    public string? Reason { get; set; }
}

public class StudentEnrollmentRequest
{
    [Required]
    public int MaLopHocPhan { get; set; }
}

public class StudentRegistrationDto
{
    public int Id { get; set; }
    public int MaLopHocPhan { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Semester { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminRegistrationResultDto
{
    public int Id { get; set; }
    public int MaHocSinh { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int MaLopHocPhan { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime RegisteredAt { get; set; }
}

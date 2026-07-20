namespace Backend.DTOs.Grading;

// === Response cho GET classes/{id}/grades/v2 ===

public class ClassGradesSummaryDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<GradeTypeColumnDto> GradeColumns { get; set; } = new();
    public List<StudentGradeSummaryDto> Students { get; set; } = new();
}

public class GradeTypeColumnDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int ColumnCount { get; set; }
}

public class StudentGradeSummaryDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Dictionary<string, decimal?> TypeGrades { get; set; } = new();
    public decimal? DiemQuaTrinh { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? GpaMonHoc { get; set; }
    public string? TrangThai { get; set; }
    public bool DaKhoa { get; set; }
}

// === Response cho GET classes/{id}/grades/{studentId}/detail ===

public class StudentGradeDetailDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<GradeTypeDetailDto> GradeTypes { get; set; } = new();
    public decimal? DiemQuaTrinh { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? GpaMonHoc { get; set; }
    public string? TrangThai { get; set; }
    public bool DaKhoa { get; set; }
}

public class GradeTypeDetailDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal? AverageGrade { get; set; }
    public List<GradeItemDto> Items { get; set; } = new();
}

public class GradeItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal? Grade { get; set; }
}

// === Request cho POST unlock ===

public class UnlockGradeRequest
{
    public string LyDo { get; set; } = string.Empty;
}

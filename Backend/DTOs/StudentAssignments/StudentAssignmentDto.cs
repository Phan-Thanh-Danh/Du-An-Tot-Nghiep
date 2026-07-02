namespace Backend.DTOs.StudentAssignments;

public class StudentAssignmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class StudentAssignmentDetailDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string DeadlineDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubmissionRulesDto Rules { get; set; } = new();
    public List<SubmissionHistoryDto> Submissions { get; set; } = new();
}

public class SubmissionRulesDto
{
    public List<string> AllowedFormats { get; set; } = new();
    public int MaxSizeMB { get; set; }
    public int MaxAttempts { get; set; }
    public int CurrentAttempt { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class SubmissionHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public int Attempt { get; set; }
    public string SubmittedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public bool OnTime { get; set; }
    public string TimeLabel { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsLatest { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}

public class AssignmentSubmissionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SubmissionHistoryDto? Submission { get; set; }
}

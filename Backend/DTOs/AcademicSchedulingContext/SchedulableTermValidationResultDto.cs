namespace Backend.DTOs.AcademicSchedulingContext;

public class SchedulableTermValidationResultDto
{
    public bool IsValid { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonMessage { get; set; } = string.Empty;
    public int? ValidSchedulableTermId { get; set; }
}

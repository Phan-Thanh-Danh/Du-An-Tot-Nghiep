namespace Backend.Services.Storage;

public class ApplicationEvidenceStorageOptions
{
    public const string SectionName = "ApplicationEvidenceStorage";

    public string Provider { get; set; } = "Local";
    public string LocalRoot { get; set; } = Path.Combine(Path.GetTempPath(), "LMS_APPLICATION_EVIDENCE");
}

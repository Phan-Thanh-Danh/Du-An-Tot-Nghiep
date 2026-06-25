namespace Backend.Services.Storage;

public class ApplicationEvidenceStorageOptions
{
    public const string SectionName = "ApplicationEvidenceStorage";

    public string Provider { get; set; } = "R2";
    public string LocalRoot { get; set; } = string.Empty;
}

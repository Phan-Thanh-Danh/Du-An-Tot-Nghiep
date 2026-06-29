namespace Backend.Services.RewardDiscipline;

public class CertificateStorageOptions
{
    public const string SectionName = "CertificateStorage";

    public string Provider { get; set; } = "Local";
    public string? LocalRoot { get; set; }
    public string PublicBasePath { get; set; } = "/certificates";
}

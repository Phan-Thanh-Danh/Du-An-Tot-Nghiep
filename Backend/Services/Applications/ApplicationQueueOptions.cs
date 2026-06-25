namespace Backend.Services.Applications;

public class ApplicationQueueOptions
{
    public const string SectionName = "ApplicationQueue";

    public int SlaWarningBeforeHours { get; set; } = 24;
}

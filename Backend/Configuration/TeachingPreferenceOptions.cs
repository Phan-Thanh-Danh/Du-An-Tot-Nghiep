namespace Backend.Configuration;

public class TeachingPreferenceOptions
{
    public const string SectionName = "TeachingPreference";

    public int OpenDaysBeforeTermStart { get; set; } = 30;
    public int DeadlineDaysBeforeTermStart { get; set; } = 14;
}

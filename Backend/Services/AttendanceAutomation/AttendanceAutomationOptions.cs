namespace Backend.Services.AttendanceAutomation;

public class AttendanceAutomationOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
    public int BatchSize { get; set; } = 100;
    public bool AutoSubmitEnabled { get; set; } = true;
    public bool AutoLockEnabled { get; set; } = true;
    public int LockAfterSubmitMinutes { get; set; } = 10;
}

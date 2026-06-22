namespace Backend.Services.LearningProgress;

public class LearningProgressOptions
{
    public const string SectionName = "LearningProgress";

    /// <summary>
    /// Số giây dự kiến giữa các lần gửi heartbeat. Mặc định 60s.
    /// </summary>
    public int HeartbeatExpectedSeconds { get; set; } = 60;

    /// <summary>
    /// Sai số cho phép (giây) khi nhận heartbeat.
    /// </summary>
    public int HeartbeatToleranceSeconds { get; set; } = 30;

    /// <summary>
    /// Tối đa số lần gửi heartbeat được cộng dồn (trong trường hợp offline sync).
    /// </summary>
    public int MaxBatchHeartbeats { get; set; } = 5;

    /// <summary>
    /// Phần trăm cần đạt để coi như hoàn thành video (ví dụ: 90%).
    /// </summary>
    public decimal VideoCompletionPercent { get; set; } = 90;

    /// <summary>
    /// Phần trăm cuộn (scroll) cần đạt để coi như hoàn thành slide (ví dụ: 90%).
    /// </summary>
    public decimal SlideCompletionPercent { get; set; } = 90;

    /// <summary>
    /// Số giây tối thiểu cần đọc văn bản/tài liệu để được đánh dấu hoàn thành.
    /// </summary>
    public int MinReadingSecondsForCompletion { get; set; } = 60;
}

using System.Text.Json;

namespace Backend.DTOs.Applications;

public class AdminApplicationProcessRequest
{
    public string RowVersion { get; set; } = string.Empty;
}

public class AdminApplicationRecordProcessingResultRequest
{
    public string Outcome { get; set; } = string.Empty;
    public string PublicNote { get; set; } = string.Empty;
    public string? InternalNote { get; set; }
    public JsonElement? Result { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

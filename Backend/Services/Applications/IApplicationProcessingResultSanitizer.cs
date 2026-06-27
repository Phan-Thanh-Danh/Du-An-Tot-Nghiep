using System.Text.Json;

namespace Backend.Services.Applications;

public interface IApplicationProcessingResultSanitizer
{
    JsonElement Sanitize(JsonElement? result);
}

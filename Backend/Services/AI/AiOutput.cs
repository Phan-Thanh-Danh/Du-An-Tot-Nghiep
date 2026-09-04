using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Exceptions;

namespace Backend.Services.AI;

internal static class AiOutput
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static T Parse<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new JsonException(); }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new ApiException(502, "AI trả về nội dung chưa hợp lệ. Vui lòng thử lại; chưa có thay đổi nào được áp dụng.");
        }
    }

    public static object Schema(object properties, params string[] required) => new
    {
        type = "object", additionalProperties = false, properties, required
    };
}

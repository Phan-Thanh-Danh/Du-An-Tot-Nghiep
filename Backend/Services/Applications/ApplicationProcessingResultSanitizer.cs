using System.Text.Json;
using Backend.Exceptions;

namespace Backend.Services.Applications;

public class ApplicationProcessingResultSanitizer : IApplicationProcessingResultSanitizer
{
    private const int MaxSerializedBytes = 16 * 1024;
    private const int MaxDepth = 5;
    private const int MaxProperties = 50;
    private const int MaxArrayItems = 100;
    private const int MaxStringLength = 2000;

    private static readonly string[] SensitiveKeyFragments =
    [
        "password",
        "token",
        "secret",
        "connectionstring",
        "storagekey",
        "filehash",
        "localpath",
        "bucket",
        "credential",
        "apikey",
        "privatekey"
    ];

    public JsonElement Sanitize(JsonElement? result)
    {
        if (!result.HasValue ||
            result.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            using var emptyDocument = JsonDocument.Parse("{}");
            return emptyDocument.RootElement.Clone();
        }

        if (result.Value.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý phải là JSON object.");
        }

        ValidateElement(result.Value, depth: 1);
        var raw = result.Value.GetRawText();
        if (System.Text.Encoding.UTF8.GetByteCount(raw) > MaxSerializedBytes)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý vượt quá dung lượng cho phép.");
        }

        using var document = JsonDocument.Parse(raw);
        return document.RootElement.Clone();
    }

    private static void ValidateElement(JsonElement element, int depth)
    {
        if (depth > MaxDepth)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý vượt quá độ sâu cho phép.");
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                ValidateObject(element, depth);
                break;
            case JsonValueKind.Array:
                ValidateArray(element, depth);
                break;
            case JsonValueKind.String:
                ValidateString(element.GetString());
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý chứa giá trị không hợp lệ.");
        }
    }

    private static void ValidateObject(JsonElement element, int depth)
    {
        var count = 0;
        foreach (var property in element.EnumerateObject())
        {
            if (++count > MaxProperties)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý có quá nhiều thuộc tính.");
            }

            if (IsSensitiveKey(property.Name))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý chứa khóa không được phép.");
            }

            ValidateElement(property.Value, depth + 1);
        }
    }

    private static void ValidateArray(JsonElement element, int depth)
    {
        var count = 0;
        foreach (var item in element.EnumerateArray())
        {
            if (++count > MaxArrayItems)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý có mảng quá dài.");
            }

            ValidateElement(item, depth + 1);
        }
    }

    private static void ValidateString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (value.Length > MaxStringLength ||
            value.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("</script", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý chứa chuỗi không hợp lệ.");
        }
    }

    private static bool IsSensitiveKey(string key)
    {
        var normalized = key
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveKeyFragments.Any(fragment => normalized.Contains(fragment, StringComparison.Ordinal));
    }
}

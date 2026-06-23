using System.Text.Json;
using Backend.Constants;
using Backend.Exceptions;

namespace Backend.Services.Applications;

public class ApplicationTemplateValidator : IApplicationTemplateValidator
{
    private const int MaxJsonLength = 64 * 1024;
    private const int MaxFields = 50;
    private const int MaxFieldKeyLength = 80;
    private const int MaxFieldLabelLength = 200;
    private const int MaxAllowedTextLength = 5000;

    private static readonly string[] ForbiddenTokens =
    [
        "<script",
        "</script",
        "<iframe",
        "javascript:",
        "select ",
        "insert ",
        "update ",
        "delete ",
        "drop ",
        "alter ",
        "exec ",
        "execute ",
        "from ",
        "where ",
        "table",
        "query"
    ];

    public void Validate(string configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn không được rỗng.");
        }

        if (configurationJson.Length > MaxJsonLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn vượt quá dung lượng cho phép.");
        }

        EnsureNoForbiddenTokens(configurationJson);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(configurationJson, new JsonDocumentOptions
            {
                MaxDepth = 16,
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });
        }
        catch (JsonException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn không phải JSON hợp lệ.");
        }

        using (document)
        {
            ValidateDocument(document);
        }
    }

    private static void ValidateDocument(JsonDocument document)
    {
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("fields", out var fields) ||
            fields.ValueKind != JsonValueKind.Array)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn phải có root.fields dạng mảng.");
        }

        if (fields.GetArrayLength() is < 1 or > MaxFields)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn phải có 1-50 field.");
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields.EnumerateArray())
        {
            ValidateField(field, keys);
        }
    }

    private static void ValidateField(JsonElement field, HashSet<string> keys)
    {
        if (field.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Mỗi field trong mẫu đơn phải là object.");
        }

        var key = GetRequiredString(field, "key", MaxFieldKeyLength);
        if (!keys.Add(key))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field key '{key}' bị trùng.");
        }

        _ = GetRequiredString(field, "label", MaxFieldLabelLength);
        var type = GetRequiredString(field, "type", 40);
        if (!ApplicationFieldTypes.All.Contains(type))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field type '{type}' không hợp lệ.");
        }

        if (type.Equals(ApplicationFieldTypes.RelatedEntity, StringComparison.OrdinalIgnoreCase))
        {
            if (!field.TryGetProperty("relatedEntity", out var relatedEntity) ||
                relatedEntity.ValueKind == JsonValueKind.Null)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Field type related_entity phải có relatedEntity.");
            }

            var value = GetStringValue(relatedEntity, "relatedEntity", 80);
            if (!ApplicationRelatedEntities.All.Contains(value))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Related entity '{value}' không hợp lệ.");
            }
        }
        else if (field.TryGetProperty("relatedEntity", out var relatedEntity) &&
                 relatedEntity.ValueKind != JsonValueKind.Null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "relatedEntity chỉ được dùng với field type related_entity.");
        }

        if (type.Equals(ApplicationFieldTypes.Select, StringComparison.OrdinalIgnoreCase) ||
            type.Equals(ApplicationFieldTypes.Multiselect, StringComparison.OrdinalIgnoreCase))
        {
            if (!field.TryGetProperty("options", out var options) ||
                options.ValueKind == JsonValueKind.Null)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Field select/multiselect phải có options.");
            }

            ValidateOptions(options);
        }
        else if (field.TryGetProperty("options", out var options) && options.ValueKind != JsonValueKind.Null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "options chỉ được dùng cho select/multiselect.");
        }

        if (field.TryGetProperty("maxLength", out var maxLength) &&
            maxLength.ValueKind != JsonValueKind.Null &&
            (!maxLength.TryGetInt32(out var length) || length <= 0 || length > MaxAllowedTextLength))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "maxLength của field không hợp lệ.");
        }

        ValidateOptionalBoolean(field, "required");
        ValidateOptionalBoolean(field, "evidenceRequired");
    }

    private static void ValidateOptions(JsonElement options)
    {
        if (options.ValueKind != JsonValueKind.Array || options.GetArrayLength() == 0 || options.GetArrayLength() > 50)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "options phải là mảng có 1-50 phần tử.");
        }

        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in options.EnumerateArray())
        {
            if (option.ValueKind != JsonValueKind.Object)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Mỗi option phải là object.");
            }

            var value = GetRequiredString(option, "value", 100);
            _ = GetRequiredString(option, "label", 200);
            if (!values.Add(value))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Option value '{value}' bị trùng.");
            }
        }
    }

    private static void ValidateOptionalBoolean(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{propertyName}' phải là boolean.");
        }
    }

    private static string GetRequiredString(JsonElement element, string propertyName, int maxLength)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Thiếu field '{propertyName}' trong cấu hình mẫu đơn.");
        }

        return GetStringValue(property, propertyName, maxLength);
    }

    private static string GetStringValue(JsonElement property, string propertyName, int maxLength)
    {
        if (property.ValueKind != JsonValueKind.String)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{propertyName}' phải là chuỗi.");
        }

        var value = property.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{propertyName}' không được rỗng.");
        }

        if (value.Length > maxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{propertyName}' vượt quá độ dài cho phép.");
        }

        EnsureNoForbiddenTokens(value);
        return value;
    }

    private static void EnsureNoForbiddenTokens(string value)
    {
        var normalized = value.ToLowerInvariant();
        if (ForbiddenTokens.Any(normalized.Contains))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cấu hình mẫu đơn chứa nội dung không được phép.");
        }
    }
}

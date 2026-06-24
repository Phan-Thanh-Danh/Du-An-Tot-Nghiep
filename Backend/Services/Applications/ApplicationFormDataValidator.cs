using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Backend.Constants;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services.Applications;

public class ApplicationFormDataValidator : IApplicationFormDataValidator
{
    private const int MaxJsonLength = 64 * 1024;
    private const int MaxDepth = 16;

    public ApplicationFormDataValidationResult Validate(
        MauDonTu template,
        string? formDataJson,
        ApplicationFormValidationMode mode)
    {
        var fields = ReadTemplateFields(template);
        using var formDocument = ParseFormData(formDataJson);
        var root = formDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu biểu mẫu phải là JSON object.");
        }

        var provided = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in root.EnumerateObject())
        {
            if (!provided.TryAdd(property.Name, property.Value))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{property.Name}' bị trùng.");
            }

            if (!fields.ContainsKey(property.Name))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{property.Name}' không thuộc mẫu đơn.");
            }
        }

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var relatedEntities = new List<ApplicationRelatedEntityReference>();
        var normalizedFields = new List<(string Key, object? Value, ApplicationFieldDefinition Field)>();
        var requiresEvidence = template.BatBuocMinhChung || fields.Values.Any(x => x.EvidenceRequired);

        foreach (var field in fields.Values)
        {
            if (!provided.TryGetValue(field.Key, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                if (mode == ApplicationFormValidationMode.Submit && field.Required)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Thiếu field bắt buộc '{field.Key}'.");
                }

                continue;
            }

            var normalized = NormalizeFieldValue(field, value, mode);
            if (normalized.ShouldKeep)
            {
                values[field.Key] = normalized.Value;
                normalizedFields.Add((field.Key, normalized.Value, field));
                if (field.Type.Equals(ApplicationFieldTypes.RelatedEntity, StringComparison.OrdinalIgnoreCase) &&
                    normalized.Value is int id &&
                    !string.IsNullOrWhiteSpace(field.RelatedEntity))
                {
                    relatedEntities.Add(new ApplicationRelatedEntityReference
                    {
                        FieldKey = field.Key,
                        RelatedEntity = field.RelatedEntity,
                        Id = id
                    });
                }
            }
            else if (mode == ApplicationFormValidationMode.Submit && field.Required)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Field bắt buộc '{field.Key}' không được rỗng.");
            }
        }

        return new ApplicationFormDataValidationResult
        {
            NormalizedJson = SerializeNormalized(normalizedFields),
            Values = values,
            Fields = fields,
            RelatedEntities = relatedEntities,
            RequiresEvidence = requiresEvidence,
            ProvidedFieldKeys = values.Keys.ToList()
        };
    }

    private static IReadOnlyDictionary<string, ApplicationFieldDefinition> ReadTemplateFields(MauDonTu template)
    {
        try
        {
            using var document = JsonDocument.Parse(template.CauHinhJson, new JsonDocumentOptions
            {
                MaxDepth = MaxDepth,
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });

            var root = document.RootElement;
            var fieldsElement = root.GetProperty("fields");
            var fields = new Dictionary<string, ApplicationFieldDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in fieldsElement.EnumerateArray())
            {
                var key = GetRequiredString(item, "key");
                var type = GetRequiredString(item, "type");
                var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (item.TryGetProperty("options", out var optionsElement) &&
                    optionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var option in optionsElement.EnumerateArray())
                    {
                        var value = GetRequiredString(option, "value");
                        options[value] = value;
                    }
                }

                fields[key] = new ApplicationFieldDefinition
                {
                    Key = key,
                    Label = GetRequiredString(item, "label"),
                    Type = type,
                    Required = GetOptionalBoolean(item, "required"),
                    EvidenceRequired = GetOptionalBoolean(item, "evidenceRequired"),
                    MaxLength = GetOptionalInt(item, "maxLength"),
                    RelatedEntity = GetOptionalString(item, "relatedEntity"),
                    Options = options
                };
            }

            return fields;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Cấu hình mẫu đơn không hợp lệ.");
        }
    }

    private static JsonDocument ParseFormData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            json = "{}";
        }

        if (Encoding.UTF8.GetByteCount(json) > MaxJsonLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu biểu mẫu vượt quá dung lượng cho phép.");
        }

        try
        {
            return JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = MaxDepth,
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });
        }
        catch (JsonException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dữ liệu biểu mẫu không phải JSON hợp lệ.");
        }
    }

    private static NormalizedFieldValue NormalizeFieldValue(
        ApplicationFieldDefinition field,
        JsonElement value,
        ApplicationFormValidationMode mode)
    {
        switch (field.Type)
        {
            case ApplicationFieldTypes.Text:
            case ApplicationFieldTypes.Textarea:
                if (value.ValueKind != JsonValueKind.String)
                {
                    throw InvalidFieldType(field.Key);
                }

                var text = value.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new NormalizedFieldValue(false, null);
                }

                if (field.MaxLength.HasValue && text.Length > field.MaxLength.Value)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{field.Key}' vượt quá độ dài cho phép.");
                }

                return new NormalizedFieldValue(true, text);

            case ApplicationFieldTypes.Number:
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var number))
                {
                    throw InvalidFieldType(field.Key);
                }

                return new NormalizedFieldValue(true, number);

            case ApplicationFieldTypes.Date:
                if (value.ValueKind != JsonValueKind.String ||
                    !DateOnly.TryParseExact(value.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    throw InvalidFieldType(field.Key);
                }

                return new NormalizedFieldValue(true, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            case ApplicationFieldTypes.DateTime:
                if (value.ValueKind != JsonValueKind.String ||
                    !DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    throw InvalidFieldType(field.Key);
                }

                return new NormalizedFieldValue(true, dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));

            case ApplicationFieldTypes.Select:
                if (value.ValueKind != JsonValueKind.String)
                {
                    throw InvalidFieldType(field.Key);
                }

                var selected = value.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(selected))
                {
                    return new NormalizedFieldValue(false, null);
                }

                if (!field.Options.TryGetValue(selected, out var canonical))
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Giá trị của field '{field.Key}' không nằm trong options.");
                }

                return new NormalizedFieldValue(true, canonical);

            case ApplicationFieldTypes.Multiselect:
                if (value.ValueKind != JsonValueKind.Array)
                {
                    throw InvalidFieldType(field.Key);
                }

                var values = new List<string>();
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        throw InvalidFieldType(field.Key);
                    }

                    var itemValue = item.GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(itemValue))
                    {
                        continue;
                    }

                    if (!field.Options.TryGetValue(itemValue, out var itemCanonical))
                    {
                        throw new ApiException(StatusCodes.Status400BadRequest, $"Giá trị của field '{field.Key}' không nằm trong options.");
                    }

                    if (!set.Add(itemCanonical))
                    {
                        throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{field.Key}' có giá trị bị trùng.");
                    }

                    values.Add(itemCanonical);
                }

                return values.Count == 0
                    ? new NormalizedFieldValue(false, null)
                    : new NormalizedFieldValue(true, values);

            case ApplicationFieldTypes.Boolean:
                if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                {
                    throw InvalidFieldType(field.Key);
                }

                return new NormalizedFieldValue(true, value.GetBoolean());

            case ApplicationFieldTypes.RelatedEntity:
                if (value.ValueKind != JsonValueKind.Number ||
                    !value.TryGetInt32(out var id) ||
                    id <= 0)
                {
                    throw InvalidFieldType(field.Key);
                }

                return new NormalizedFieldValue(true, id);

            default:
                throw InvalidFieldType(field.Key);
        }
    }

    private static string SerializeNormalized(IReadOnlyList<(string Key, object? Value, ApplicationFieldDefinition Field)> fields)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }))
        {
            writer.WriteStartObject();
            foreach (var (key, value, _) in fields)
            {
                writer.WritePropertyName(key);
                WriteValue(writer, value);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case int integer:
                writer.WriteNumberValue(integer);
                break;
            case decimal number:
                writer.WriteNumberValue(number);
                break;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                break;
            case IReadOnlyList<string> strings:
                writer.WriteStartArray();
                foreach (var item in strings)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
                break;
            default:
                JsonSerializer.Serialize(writer, value);
                break;
        }
    }

    private static ApiException InvalidFieldType(string fieldKey)
    {
        return new ApiException(StatusCodes.Status400BadRequest, $"Field '{fieldKey}' không đúng kiểu dữ liệu.");
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).GetString()?.Trim() ?? string.Empty;
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()?.Trim()
            : null;
    }

    private static bool GetOptionalBoolean(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind is JsonValueKind.True or JsonValueKind.False &&
               property.GetBoolean();
    }

    private static int? GetOptionalInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value)
            ? value
            : null;
    }

    private readonly record struct NormalizedFieldValue(bool ShouldKeep, object? Value);
}

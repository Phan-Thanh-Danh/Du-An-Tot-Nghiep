using System.Text.Json;
using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public static class ApplicationTimelineMetadataSanitizer
{
    private const int MaxListItems = 100;
    private const int MaxTextLength = 100;
    private const int MaxFileCount = 1000;

    private static readonly string[] AllowedOperations =
    [
        "upload_evidence",
        "delete_evidence"
    ];

    public static AdminApplicationTimelineMetadataDto? Sanitize(string? snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(snapshotJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var root = document.RootElement;
            var metadata = new AdminApplicationTimelineMetadataDto
            {
                Operation = ReadAllowedOperation(root),
                FromAssigneeId = ReadPositiveIntOrNull(root, "fromAssigneeId"),
                ToAssigneeId = ReadPositiveIntOrNull(root, "toAssigneeId"),
                ReasonProvided = ReadBooleanOrNull(root, "reasonProvided"),
                TemplateAssigned = ReadBooleanOrNull(root, "templateAssigned"),
                ChangedFields = ReadStringArray(root, "changedFields"),
                AttachmentIds = ReadPositiveIntArray(root, "attachmentIds"),
                AttachmentId = ReadPositiveIntOrNull(root, "attachmentId"),
                FileCount = ReadFileCount(root)
            };

            return HasAnyValue(metadata) ? metadata : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadAllowedOperation(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "operation", out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return AllowedOperations.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    private static int? ReadPositiveIntOrNull(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value) &&
               value > 0
            ? value
            : null;
    }

    private static int? ReadFileCount(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "fileCount", out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value) &&
               value is >= 0 and <= MaxFileCount
            ? value
            : null;
    }

    private static bool? ReadBooleanOrNull(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = item.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (value.Length > MaxTextLength)
            {
                value = value[..MaxTextLength];
            }

            if (seen.Add(value))
            {
                result.Add(value);
            }

            if (result.Count == MaxListItems)
            {
                break;
            }
        }

        return result;
    }

    private static IReadOnlyList<int> ReadPositiveIntArray(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<int>();
        var seen = new HashSet<int>();
        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Number &&
                item.TryGetInt32(out var value) &&
                value > 0 &&
                seen.Add(value))
            {
                result.Add(value);
            }

            if (result.Count == MaxListItems)
            {
                break;
            }
        }

        return result;
    }

    private static bool HasAnyValue(AdminApplicationTimelineMetadataDto metadata)
    {
        return metadata.Operation is not null ||
               metadata.FromAssigneeId.HasValue ||
               metadata.ToAssigneeId.HasValue ||
               metadata.ReasonProvided.HasValue ||
               metadata.TemplateAssigned.HasValue ||
               metadata.ChangedFields.Count > 0 ||
               metadata.AttachmentIds.Count > 0 ||
               metadata.AttachmentId.HasValue ||
               metadata.FileCount.HasValue;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}

using System.Text.Json;
using Backend.Constants;
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
        "delete_evidence",
        "auto_process",
        "manual_processing_result"
    ];

    private static readonly string[] AllowedHandlers =
    [
        "confirmation_request",
        "manual_required_fallback"
    ];

    private static readonly string[] SensitiveFieldTerms =
    [
        "password",
        "connectionstring",
        "storagekey",
        "filehash",
        "tenfileluu",
        "localpath",
        "bucket",
        "token",
        "secret",
        "formdata",
        "fieldvalues",
        "raw"
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
            var operation = ReadAllowedOperation(root);
            var legacyOperation = operation is null ? ReadLegacyEvidenceOperation(root) : null;
            var metadata = new AdminApplicationTimelineMetadataDto
            {
                Operation = operation ?? legacyOperation,
                FromAssigneeId = ReadPositiveIntOrNull(root, "fromAssigneeId"),
                ToAssigneeId = ReadPositiveIntOrNull(root, "toAssigneeId"),
                ReasonProvided = ReadBooleanOrNull(root, "reasonProvided"),
                TemplateAssigned = ReadBooleanOrNull(root, "templateAssigned"),
                ChangedFields = ReadStringArray(root, "changedFields"),
                AttachmentIds = ReadPositiveIntArray(root, "attachmentIds"),
                AttachmentId = ReadPositiveIntOrNull(root, "attachmentId") ?? ReadLegacyAttachmentId(root, legacyOperation),
                FileCount = ReadFileCount(root) ?? ReadLegacyFileCount(root, legacyOperation),
                Decision = ReadAllowedDecision(root),
                PreviousAssigneeId = ReadPositiveIntOrNull(root, "previousAssigneeId"),
                ProcessorId = ReadPositiveIntOrNull(root, "processorId"),
                Handler = ReadAllowedHandler(root),
                ProcessingStatusFrom = ReadAllowedProcessingStatus(root, "processingStatusFrom"),
                ProcessingStatusTo = ReadAllowedProcessingStatus(root, "processingStatusTo"),
                Outcome = ReadAllowedProcessingStatus(root, "outcome")
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

    private static string? ReadLegacyEvidenceOperation(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "attachmentAction", out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return property.GetString()?.Trim().ToLowerInvariant() switch
        {
            "upload" => "upload_evidence",
            "delete" => "delete_evidence",
            _ => null
        };
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

    private static int? ReadLegacyFileCount(JsonElement root, string? legacyOperation)
    {
        if (legacyOperation != "upload_evidence" ||
            !TryGetPropertyIgnoreCase(root, "count", out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value) &&
               value is >= 0 and <= MaxFileCount
            ? value
            : null;
    }

    private static int? ReadLegacyAttachmentId(JsonElement root, string? legacyOperation)
    {
        if (legacyOperation != "delete_evidence" ||
            !TryGetPropertyIgnoreCase(root, "maTep", out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value) &&
               value > 0
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
        var inspected = 0;
        foreach (var item in property.EnumerateArray())
        {
            if (inspected++ == MaxListItems)
            {
                break;
            }

            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = item.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(value) || IsSensitiveMetadataField(value))
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
        var inspected = 0;
        foreach (var item in property.EnumerateArray())
        {
            if (inspected++ == MaxListItems)
            {
                break;
            }

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

    private static bool IsSensitiveMetadataField(string value)
    {
        var normalized = value.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveFieldTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal));
    }

    private static string? ReadAllowedDecision(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "decision", out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return value is "request_supplement" or "approve" or "reject"
            ? value
            : null;
    }

    private static string? ReadAllowedHandler(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "handler", out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return AllowedHandlers.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadAllowedProcessingStatus(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return ApplicationProcessingStatuses.All.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
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
               metadata.FileCount.HasValue ||
               metadata.Decision is not null ||
               metadata.PreviousAssigneeId.HasValue ||
               metadata.ProcessorId.HasValue ||
               metadata.Handler is not null ||
               metadata.ProcessingStatusFrom is not null ||
               metadata.ProcessingStatusTo is not null ||
               metadata.Outcome is not null;
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

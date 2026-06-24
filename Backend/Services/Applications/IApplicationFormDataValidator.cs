using Backend.Models;

namespace Backend.Services.Applications;

public enum ApplicationFormValidationMode
{
    Draft,
    Submit
}

public sealed class ApplicationFormDataValidationResult
{
    public string NormalizedJson { get; init; } = "{}";
    public IReadOnlyDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
    public IReadOnlyDictionary<string, ApplicationFieldDefinition> Fields { get; init; } = new Dictionary<string, ApplicationFieldDefinition>();
    public IReadOnlyList<ApplicationRelatedEntityReference> RelatedEntities { get; init; } = [];
    public bool RequiresEvidence { get; init; }
    public IReadOnlyList<string> ProvidedFieldKeys { get; init; } = [];
}

public sealed class ApplicationFieldDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public bool EvidenceRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? RelatedEntity { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class ApplicationRelatedEntityReference
{
    public string FieldKey { get; init; } = string.Empty;
    public string RelatedEntity { get; init; } = string.Empty;
    public int Id { get; init; }
}

public interface IApplicationFormDataValidator
{
    ApplicationFormDataValidationResult Validate(
        MauDonTu template,
        string? formDataJson,
        ApplicationFormValidationMode mode);
}

using System.Text.Json;
using Backend.Constants;
using Backend.Models;

namespace Backend.Services.Applications;

public class ConfirmationApplicationPostApprovalHandler : IApplicationPostApprovalHandler
{
    private const string HandlerName = "confirmation_request";

    public bool CanHandle(string applicationType)
    {
        return string.Equals(applicationType, ApplicationTypes.Confirmation, StringComparison.OrdinalIgnoreCase);
    }

    public Task<ApplicationPostApprovalOutcome> ProcessAsync(
        DonTu application,
        ApplicationActorContext actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(application.DuLieuBieuMau) ? "{}" : application.DuLieuBieuMau);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Task.FromResult(Failed("invalid_confirmation_form"));
            }

            var confirmationType = ReadString(document.RootElement, "loai_xac_nhan");
            var copyCount = ReadInt(document.RootElement, "so_ban");
            if (string.IsNullOrWhiteSpace(confirmationType) || copyCount is < 1 or > 5)
            {
                return Task.FromResult(Failed("invalid_confirmation_form"));
            }

            using var dataDocument = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                confirmationType,
                copyCount
            }));

            return Task.FromResult(new ApplicationPostApprovalOutcome
            {
                Outcome = ApplicationProcessingStatuses.Recorded,
                Handler = HandlerName,
                PublicNote = "Yêu cầu xác nhận đã được ghi nhận.",
                Data = dataDocument.RootElement.Clone()
            });
        }
        catch (JsonException)
        {
            return Task.FromResult(Failed("invalid_confirmation_form"));
        }
    }

    private static ApplicationPostApprovalOutcome Failed(string code)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            errorCode = code
        }));

        return new ApplicationPostApprovalOutcome
        {
            Outcome = ApplicationProcessingStatuses.Failed,
            Handler = HandlerName,
            PublicNote = "Việc xử lý yêu cầu chưa hoàn tất. Bộ phận phụ trách sẽ kiểm tra lại.",
            InternalCode = code,
            Data = document.RootElement.Clone()
        };
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return TryGetPropertyIgnoreCase(root, propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()?.Trim()
            : null;
    }

    private static int? ReadInt(JsonElement root, string propertyName)
    {
        return TryGetPropertyIgnoreCase(root, propertyName, out var property) &&
               property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt32(out var value)
            ? value
            : null;
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

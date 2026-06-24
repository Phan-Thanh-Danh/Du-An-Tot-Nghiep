using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationEvidenceValidator
{
    Task ValidateAsync(
        DonTu application,
        MauDonTu template,
        ApplicationFormDataValidationResult formData,
        CancellationToken cancellationToken = default);
}

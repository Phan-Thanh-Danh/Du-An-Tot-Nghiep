using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationReferenceValidator
{
    Task ValidateAsync(
        NguoiDung student,
        ApplicationFormDataValidationResult formData,
        CancellationToken cancellationToken = default);
}

using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationApprovalPreconditionValidator
{
    Task ValidateAsync(DonTu application, CancellationToken cancellationToken = default);
}
